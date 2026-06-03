using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Cad2Revit.Helpers;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cad2Revit.Converter
{
    public class ElementCreator
    {
        private readonly Document _doc;
        private readonly ConversionSettings _caiDat;
        private readonly KetQuaPhanTich _phanTich;
        private readonly RevitTcvnHelper _tcvn;
        private readonly KetQuaDocCAD _ketQuaCAD;

        private readonly List<Line> _danhSachTuongDaTao = new List<Line>();
        private readonly HashSet<string> _cotDaTaoTheoTang = new HashSet<string>();
        private readonly HashSet<string> _damDaTaoTheoTang = new HashSet<string>();
        private readonly HashSet<string> _sanDaTaoTheoTang = new HashSet<string>();

        private List<Level> _cacLevel;
        private Level _levelMai;
        private readonly List<Grid> _gridDaTao = new List<Grid>();

        public ElementCreator(
            Document doc,
            ConversionSettings caiDat,
            KetQuaPhanTich phanTich,
            KetQuaDocCAD ketQuaCAD)
        {
            _doc = doc;
            _caiDat = caiDat ?? new ConversionSettings();
            _phanTich = phanTich ?? new KetQuaPhanTich();
            _ketQuaCAD = ketQuaCAD;
            _tcvn = new RevitTcvnHelper(doc);
        }

        public string TaoTatCaCauKien(KetQuaDocCAD ketQuaCAD)
        {
            // BƯỚC 0: XÓA TOÀN BỘ NỘI DUNG (TRƯỚC KHI TẠO BẤT CỨ GÌ)
            if (!_caiDat.SuDungGridRevitCoSan)
            {
                CadCleanupHelper.XoaTatCaGrid(_doc);       // Xóa tất cả Grid nếu ta sẽ tạo grid CAD2Revit mới
            }

            // BƯỚC 1: ChuẩnHoá tọa độ
            CadCoordinateHelper.ChuanHoaVeGocKienTruc(
                ketQuaCAD,
                _phanTich);

            StructuralGridSystem.ApDungChuanRevit(
                _phanTich,
                ketQuaCAD,
                _caiDat.SuDungGridRevitCoSan ? _doc : null);

            // Xóa kết quả chạy trước (nếu có CAD2Revit cũ)
            CadCleanupHelper.XoaKetQuaLanTruoc(
                _doc,
                _phanTich.LuoiTruc,
                _ketQuaCAD?.DanhSachDuong);

            if (_caiDat.SuDungLevelRevitCoSan)
            {
                _cacLevel = RevitLevelHelper.LayDanhSachLevel(
                    _doc,
                    _caiDat);
            }
            else
            {
                _cacLevel = TaoHoacLayDanhSachLevel(_caiDat.SoTang);
            }

            _levelMai = null;

            if (_cacLevel == null || _cacLevel.Count == 0)
            {
                return "LỖI: Không có Level Revit.\n" +
                    "Tạo Level \"Tầng 1\", \"Tầng 2\"… trong Revit (như template) " +
                    "hoặc tắt SuDungLevelRevitCoSan.";
            }

            _phanTich.SoTang = _cacLevel.Count;

            if (_caiDat.TaoSanMai)
                ChuanBiLevelMai();

            // BƯỚC 2+3: TẠO LƯỚI TRỤC VÀ SÀN TRƯỚC
            var gridFloorBuilder = new GridFloorBuilder(
                _doc,
                _caiDat,
                _phanTich,
                ketQuaCAD,
                _cacLevel,
                _levelMai);
            gridFloorBuilder.CreateGridAndFloors();

            int soTuong = 0;
            int soCot = 0;
            int soDam = 0;

            // BƯỚC 4: VẼ CẤU KIỆN (Cột, Dầm, Tường) - Bắt đầu sau khi Grid và Floor đã xong
            for (int tang = 0; tang < _cacLevel.Count; tang++)
            {
                Level level = _cacLevel[tang];
                Level levelTren = LayLevelTrenChoTang(tang);

                double chieuCaoTangFeet = LayChieuCaoTangFeet(tang);

                if (_caiDat.ConvertColumns &&
                    _phanTich.DanhSachDiemCot != null &&
                    levelTren != null)
                {
                    var cotTang = StructuralGridSystem.LayDiemCotChoTang(
                        _phanTich.LuoiTruc,
                        _phanTich.DanhSachDiemCot,
                        tang,
                        _cacLevel.Count);

                    foreach (DiemCot dc in cotTang)
                    {
                        if (TaoCotTheoTang(
                            dc,
                            level,
                            levelTren,
                            tang,
                            chieuCaoTangFeet))
                        {
                            soCot++;
                        }
                    }
                }

                if (_caiDat.ConvertBeams && _phanTich.DanhSachDuongDam != null)
                {
                    double caoTrinhDam = LayCaoTrinhDam(tang);

                    foreach (DuongDam dd in _phanTich.DanhSachDuongDam)
                    {
                        if (!DamThuocPhamViTang(dd, tang))
                            continue;

                        if (TaoDamTheoTang(dd, tang, caoTrinhDam))
                            soDam++;
                    }
                }

                foreach (CadLine duong in ketQuaCAD.DanhSachDuong)
                {
                    if (duong.Loai == LoaiCauKien.Tuong && _caiDat.ConvertWalls)
                    {
                        if (TaoTuong(duong, level, chieuCaoTangFeet))
                            soTuong++;
                    }
                }
            }

            CadCoordinateHelper.DichCadImportVeGoc(
                _doc,
                ketQuaCAD);

            if (!_caiDat.SuDungGridRevitCoSan)
                ThuPhamViGridVaLevel(_cacLevel, _levelMai);
            else if (!_caiDat.SuDungLevelRevitCoSan)
                ThuPhamViLevelRevitCoSan(_cacLevel, _levelMai);

            string ketQua = "done";

            return ketQua;
        }

        private void ChuanBiLevelMai()
        {
            if (_cacLevel == null || _cacLevel.Count == 0)
                return;

            if (_caiDat.SuDungLevelRevitCoSan)
            {
                _levelMai = RevitLevelHelper.LayLevelMai(
                    _doc,
                    _cacLevel,
                    _caiDat);
                return;
            }

            Level tren = _cacLevel.Last();
            int chiSoTren = _cacLevel.Count - 1;
            double caoMai = tren.Elevation +
                UnitHelper.MmSangFeet(_caiDat.LayChieuCaoTang(chiSoTren));

            _levelMai = LayHoacTaoLevelMai(caoMai);
        }

        /// <summary>
        /// Tầng trên cùng: cột nối tới Level mái (đóng khung tầng 4).
        /// </summary>
        private Level LayLevelTrenChoTang(int tang)
        {
            if (tang + 1 < _cacLevel.Count)
                return _cacLevel[tang + 1];

            if (tang == _cacLevel.Count - 1 && _levelMai != null)
                return _levelMai;

            return null;
        }

        /// <summary>
        /// Đáy (tầng 0 / Level thấp nhất): không tạo sàn.
        /// Từ tầng 1 trở lên: mỗi tầng một tấm sàn trong lưới.
        /// </summary>
        private bool NenTaoSanChoTang(int tang)
        {
            if (_cacLevel == null || tang < 0 || tang >= _cacLevel.Count)
                return false;

            if (LaTangDay(tang))
                return false;

            return true;
        }

        private bool LaTangDay(int tang)
        {
            if (_caiDat.BoQuaSanTangTret && tang == 0)
                return true;

            return false;
        }

        private double LayChieuCaoTangFeet(int tang)
        {
            if (_caiDat.SuDungLevelRevitCoSan &&
                _cacLevel != null &&
                tang >= 0 &&
                tang < _cacLevel.Count)
            {
                Level duoi = _cacLevel[tang];
                Level tren = LayLevelTrenChoTang(tang);

                if (tren != null)
                    return tren.Elevation - duoi.Elevation;
            }

            return UnitHelper.MmSangFeet(_caiDat.LayChieuCaoTang(tang));
        }

        private double LayCaoTrinhDam(int tang)
        {
            if (tang + 1 < _cacLevel.Count)
                return _cacLevel[tang + 1].Elevation;

            if (_levelMai != null)
                return _levelMai.Elevation;

            Level tren = _cacLevel[tang];
            return tren.Elevation +
                UnitHelper.MmSangFeet(_caiDat.LayChieuCaoTang(tang));
        }

        private List<Level> TaoHoacLayDanhSachLevel(int soTang)
        {
            double tolFeet = UnitHelper.MmSangFeet(50);

            var levelsCoSan = new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .ToList();

            var ketQua = new List<Level>();
            double caoDo = 0;

            for (int i = 0; i < soTang; i++)
            {
                if (i > 0)
                {
                    caoDo += UnitHelper.MmSangFeet(
                        _caiDat.LayChieuCaoTang(i - 1));
                }

                // Tìm Level CAD2Revit đã tạo (ưu tiên)
                Level level = TimLevelCad2RevitTheoCaoDo(
                    levelsCoSan,
                    caoDo,
                    tolFeet);

                // Nếu không có, tạo mới
                if (level == null)
                {
                    string ten = "Tầng " + (i + 1);
                    level = Level.Create(_doc, caoDo);
                    DatTenLevel(level, ten);
                    levelsCoSan.Add(level);
                }

                ketQua.Add(level);
            }

            return ketQua;
        }

        private Level LayHoacTaoLevelMai(double caoDoFeet)
        {
            double tolFeet = UnitHelper.MmSangFeet(50);

            var levelsCoSan = new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .ToList();

            Level level = TimLevelTheoCaoDo(levelsCoSan, caoDoFeet, tolFeet);
            if (level != null)
                return level;

            level = Level.Create(_doc, caoDoFeet);
            DatTenLevel(level, "Mái");
            return level;
        }

        private static Level TimLevelCad2RevitTheoCaoDo(
            List<Level> levels,
            double caoDo,
            double tolFeet)
        {
            return levels.FirstOrDefault(l =>
                LaLevelCad2Revit(l) &&
                Math.Abs(l.Elevation - caoDo) < tolFeet);
        }

        private static Level TimLevelTheoCaoDo(
            List<Level> levels,
            double caoDo,
            double tolFeet)
        {
            Level gan = levels
                .OrderBy(l => Math.Abs(l.Elevation - caoDo))
                .FirstOrDefault(l =>
                    Math.Abs(l.Elevation - caoDo) < tolFeet);

            if (gan != null)
                return gan;

            return levels.FirstOrDefault(l =>
                LaLevelCad2Revit(l) &&
                Math.Abs(l.Elevation - caoDo) < tolFeet);
        }

        private static bool LaLevelCad2Revit(Level level)
        {
            if (level?.Name == null)
                return false;

            return level.Name.StartsWith(
                "CAD2Revit",
                StringComparison.OrdinalIgnoreCase);
        }

        private static void DatTenLevel(Level level, string ten)
        {
            if (level == null)
                return;

            try
            {
                level.Name = ten;
            }
            catch
            {
                level.Name = ten + " (" + level.Id.Value + ")";
            }
        }

        private bool TaoCotTheoTang(
            DiemCot dc,
            Level levelDuoi,
            Level levelTren,
            int tang,
            double chieuCaoTangFeet)
        {
            string key =
                tang + "_" +
                MaViTri(dc.X, dc.Y) + "_" +
                dc.RongMm + "x" + dc.SauMm;

            if (_cotDaTaoTheoTang.Contains(key))
                return false;

            try
            {
                FamilySymbol loaiCot =
                    _tcvn.TimFamilyCot(dc.RongMm, dc.SauMm);

                if (loaiCot == null)
                    return false;

                if (!loaiCot.IsActive)
                    loaiCot.Activate();

                XYZ viTri = new XYZ(dc.X, dc.Y, 0);

                FamilyInstance cot = _doc.Create.NewFamilyInstance(
                    viTri,
                    loaiCot,
                    levelDuoi,
                    StructuralType.Column);

                if (cot == null)
                    return false;

                if (levelTren != null)
                    DatChieuCaoCot(cot, levelDuoi, levelTren);
                else
                    DatChieuCaoCotBangOffset(cot, chieuCaoTangFeet);

                _tcvn.DatKichThuocFamily(cot, dc.RongMm, dc.SauMm);
                _tcvn.GanVatLieuTcvn(cot, Tcvn5574Catalog.beTongCot);

                _cotDaTaoTheoTang.Add(key);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void DatChieuCaoCotBangOffset(
            FamilyInstance cot,
            double chieuCaoFeet)
        {
            try
            {
                Parameter topOffset = cot.get_Parameter(
                    BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);

                if (topOffset != null && !topOffset.IsReadOnly)
                    topOffset.Set(chieuCaoFeet);
            }
            catch
            {
                // bỏ qua
            }
        }

        private bool DamThuocPhamViTang(DuongDam dd, int tang)
        {
            if (_phanTich.DanhSachVungSan != null &&
                _phanTich.DanhSachVungSan.Count > 0)
            {
                XYZ tamTrongVung = new XYZ(
                    (dd.DiemDau.X + dd.DiemCuoi.X) * 0.5,
                    (dd.DiemDau.Y + dd.DiemCuoi.Y) * 0.5,
                    0);

                double dungSaiTrongVung = UnitHelper.MmSangFeet(50);
                foreach (VungSan vung in _phanTich.DanhSachVungSan)
                {
                    if (vung?.DuongVien == null || vung.DuongVien.Count < 3)
                        continue;

                    if (PointInPolygon(tamTrongVung, vung.DuongVien, dungSaiTrongVung))
                        return true;
                }

                return false;
            }

            List<XYZ> DuongVien = LayDuongVienSanChoTang(tang);
            if (DuongVien == null || DuongVien.Count < 3)
                return true;

            double minX = DuongVien.Min(p => p.X);
            double maxX = DuongVien.Max(p => p.X);
            double minY = DuongVien.Min(p => p.Y);
            double maxY = DuongVien.Max(p => p.Y);
            double tol = UnitHelper.MmSangFeet(50);

            XYZ tam = new XYZ(
                (dd.DiemDau.X + dd.DiemCuoi.X) * 0.5,
                (dd.DiemDau.Y + dd.DiemCuoi.Y) * 0.5,
                0);

            return tam.X >= minX - tol && tam.X <= maxX + tol &&
                tam.Y >= minY - tol && tam.Y <= maxY + tol;
        }

        private static bool PointInPolygon(
            XYZ point,
            List<XYZ> polygon,
            double tolFeet = 0)
        {
            if (polygon == null || polygon.Count < 3)
                return false;

            double x = point.X;
            double y = point.Y;
            bool inside = false;
            int n = polygon.Count;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                double xi = polygon[i].X;
                double yi = polygon[i].Y;
                double xj = polygon[j].X;
                double yj = polygon[j].Y;

                bool intersect = ((yi > y) != (yj > y)) &&
                    (x < (xj - xi) * (y - yi) / (yj - yi + 1e-12) + xi);
                if (intersect)
                    inside = !inside;
            }

            if (inside)
                return true;

            if (tolFeet <= 0)
                return false;

            for (int i = 0; i < polygon.Count; i++)
            {
                XYZ a = polygon[i];
                XYZ b = polygon[(i + 1) % polygon.Count];
                if (PointDistanceToSegment(point, a, b) <= tolFeet)
                    return true;
            }

            return false;
        }

        private static double PointDistanceToSegment(
            XYZ p,
            XYZ a,
            XYZ b)
        {
            XYZ ab = b - a;
            double abLen2 = ab.DotProduct(ab);
            if (abLen2 <= 0)
                return p.DistanceTo(a);

            double t = ((p - a).DotProduct(ab)) / abLen2;
            t = Math.Max(0, Math.Min(1, t));
            XYZ proj = a + t * ab;
            return p.DistanceTo(proj);
        }

        private bool TaoDamTheoTang(
            DuongDam dd,
            int tang,
            double caoTrinhFeet)
        {
            string key = tang + "_" + MaDuongDam(dd);
            if (_damDaTaoTheoTang.Contains(key))
                return false;

            try
            {
                if (dd.ChieuDaiNhipMm < 400)
                    return false;

                FamilySymbol loaiDam =
                    _tcvn.TimFamilyDam(dd.RongMm, dd.CaoMm);

                if (loaiDam == null)
                {
                    return false;
                }

                if (!loaiDam.IsActive)
                    loaiDam.Activate();

                int chiSoLevelDam = tang + 1 < _cacLevel.Count
                    ? tang + 1
                    : tang;

                Level levelDam = _cacLevel[chiSoLevelDam];
                double zLevel = levelDam.Elevation;

                XYZ p1 = new XYZ(dd.DiemDau.X, dd.DiemDau.Y, zLevel);
                XYZ p2 = new XYZ(dd.DiemCuoi.X, dd.DiemCuoi.Y, zLevel);

                LamThangDuong(ref p1, ref p2);

                if (p1.IsAlmostEqualTo(p2))
                    return false;

                Line line = Line.CreateBound(p1, p2);

                FamilyInstance dam = _doc.Create.NewFamilyInstance(
                    line,
                    loaiDam,
                    levelDam,
                    StructuralType.Beam);

                if (dam == null)
                    return false;

                double offsetLen = caoTrinhFeet - zLevel;
                if (Math.Abs(offsetLen) < UnitHelper.MmSangFeet(5))
                    offsetLen = 0;

                DatOffsetDam(dam, offsetLen);

                _tcvn.DatKichThuocFamily(dam, dd.RongMm, dd.CaoMm);
                _tcvn.GanVatLieuTcvn(dam, Tcvn5574Catalog.beTongDam);

                _damDaTaoTheoTang.Add(key);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static void DatOffsetDam(
            FamilyInstance dam,
            double offsetFeet)
        {
            if (dam == null)
                return;

            try
            {
                Parameter zOff = dam.get_Parameter(
                    BuiltInParameter.Z_OFFSET_VALUE);

                if (zOff != null && !zOff.IsReadOnly)
                    zOff.Set(offsetFeet);
            }
            catch
            {
                // bỏ qua
            }
        }

        private int TaoSanChoTang(int tang, Level level)
        {
            List<XYZ> DuongVien = LayDuongVienSanChoTang(tang);
            if (DuongVien == null || DuongVien.Count < 3)
                return 0;

            string key = tang + "_" + MaVung(DuongVien);
            if (_sanDaTaoTheoTang.Contains(key))
                return 0;

            if (TaoSanTuDuongVien(
                DuongVien,
                _caiDat.BeDaySanMm,
                level.Elevation,
                level.Id,
                false,
                _phanTich.DanhSachLoThung))
            {
                _sanDaTaoTheoTang.Add(key);
                return 1;
            }

            return 0;
        }

        private List<XYZ> LayDuongVienSanChoTang(int tang)
        {
            if (_phanTich.LuoiTruc != null)
            {
                List<XYZ> theoTang = StructuralGridSystem.LayDuongVienChoTang(
                    _phanTich.LuoiTruc,
                    tang,
                    _cacLevel.Count);

                if (theoTang != null && theoTang.Count >= 3)
                    return theoTang;
            }

            VungSan vung = LayVungSanLonNhat();
            return vung?.DuongVien;
        }

        private int TaoSanMai()
        {
            if (_cacLevel.Count == 0)
                return 0;

            VungSan vung = LayVungSanLonNhat();
            if (vung == null)
            {
                return 0;
            }

            if (_levelMai == null)
                ChuanBiLevelMai();

            if (_levelMai == null)
                return 0;

            string key = "MAI_" + MaVung(vung.DuongVien);
            if (_sanDaTaoTheoTang.Contains(key))
                return 0;

            List<XYZ> DuongVienMai = LayDuongVienSanChoTang(_cacLevel.Count - 1)
                ?? vung.DuongVien;

            if (TaoSanTuDuongVien(
                DuongVienMai,
                _caiDat.BeDaySanMm,
                _levelMai.Elevation,
                _levelMai.Id,
                true,
                _phanTich.DanhSachLoThung))
            {
                _sanDaTaoTheoTang.Add(key);
                return 1;
            }

            return 0;
        }

        private VungSan LayVungSanLonNhat()
        {
            if (_phanTich.LuoiTruc?.DuongVienSan != null &&
                _phanTich.LuoiTruc.DuongVienSan.Count >= 3)
            {
                return new VungSan
                {
                    DuongVien = _phanTich.LuoiTruc.DuongVienSan,
                    BeDayMm = _caiDat.BeDaySanMm,
                    TenLayer = "CAD2Revit-Luoi"
                };
            }

            if (_phanTich.DanhSachVungSan == null ||
                _phanTich.DanhSachVungSan.Count == 0)
            {
                return null;
            }

            return _phanTich.DanhSachVungSan
                .OrderByDescending(v => DienTichVung(v.DuongVien))
                .First();
        }

        private void ThuPhamViLevelRevitCoSan(
            List<Level> levels,
            Level levelMai)
        {
            ThuPhamViGridVaLevel(levels, levelMai);
        }

        private View3D TimView3D()
        {
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(v => !v.IsTemplate && v.ViewType == ViewType.ThreeD);
        }

        private void ThuPhamViGridVaLevel(
            List<Level> levels,
            Level levelMai)
        {
            LuoiTrucKetCau luoi = _phanTich.LuoiTruc;
            if (luoi == null || levels == null || levels.Count == 0)
                return;

            View3D view3d = TimView3D();
            if (view3d == null)
            {
                return;
            }

            double margin = UnitHelper.MmSangFeet(4500);
            double minX = luoi.MinX - margin;
            double maxX = luoi.MaxX + margin;
            double minY = luoi.MinY - margin;

            int so = 0;
            var tatCa = new List<Level>(levels);
            if (levelMai != null && !tatCa.Contains(levelMai))
                tatCa.Add(levelMai);

            foreach (Level level in tatCa)
            {
                try
                {
                    double z = level.Elevation;
                    Line line = Line.CreateBound(
                        new XYZ(minX, minY, z),
                        new XYZ(maxX, minY, z));

                    level.SetDatumExtentType(
                        DatumEnds.End0,
                        view3d,
                        DatumExtentType.Model);
                    level.SetDatumExtentType(
                        DatumEnds.End1,
                        view3d,
                        DatumExtentType.Model);
                    level.SetCurveInView(
                        DatumExtentType.Model,
                        view3d,
                        line);
                    so++;
                }
                catch
                {
                }
            }

        }


        private bool TaoTuong(
            CadLine duong,
            Level level,
            double chieuCaoTangFeet)
        {
            try
            {
                if (UnitHelper.FeetSangMm(duong.ChieuDaiFeet()) < 200)
                    return false;

                XYZ p1 = ChieuLenLevel(duong.DiemDau, level.Elevation);
                XYZ p2 = ChieuLenLevel(duong.DiemCuoi, level.Elevation);
                LamThangDuong(ref p1, ref p2);

                if (p1.DistanceTo(p2) < UnitHelper.MmSangFeet(200))
                    return false;

                Line trucTuong = Line.CreateBound(p1, p2);
                if (DaCoTuongTrung(trucTuong))
                    return false;

                WallType loaiTuong = TimLoaiTuongGanNhat(
                    duong.BeDayMm ?? _phanTich.BeDayTuongMm);

                if (loaiTuong == null)
                    return false;

                Wall wall = Wall.Create(
                    _doc,
                    trucTuong,
                    loaiTuong.Id,
                    level.Id,
                    chieuCaoTangFeet,
                    0,
                    false,
                    false);

                if (wall == null)
                    return false;

                _danhSachTuongDaTao.Add(trucTuong);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void DatChieuCaoCot(
            FamilyInstance cot,
            Level levelDuoi,
            Level levelTren)
        {
            try
            {
                Parameter topLevel = cot.get_Parameter(
                    BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);

                if (topLevel != null && !topLevel.IsReadOnly)
                    topLevel.Set(levelTren.Id);

                Parameter topOffset = cot.get_Parameter(
                    BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);

                if (topOffset != null && !topOffset.IsReadOnly)
                    topOffset.Set(0);
            }
            catch
            {

            }
        }

        private bool TaoSanTuDuongVien(
            List<XYZ> DuongVien,
            double beDayMm,
            double elevationFeet,
            ElementId levelId,
            bool laSanMai,
            List<List<XYZ>> loThungs = null)
        {
            try
            {
                if (DuongVien == null || DuongVien.Count < 3)
                    return false;

                FloorType loaiSan = _tcvn.TimFloorTypeTcvn(beDayMm, laSanMai);
                if (loaiSan == null)
                    return false;

                CurveLoop loopNgoai = TaoCurveLoop(DuongVien, elevationFeet);
                if (loopNgoai == null)
                    return false;

                var loops = new List<CurveLoop> { loopNgoai };

                if (loThungs != null)
                {
                    foreach (var loThung in loThungs)
                    {
                        if (loThung == null || loThung.Count < 3)
                            continue;

                        CurveLoop loopTrong = TaoCurveLoop(loThung, elevationFeet);
                        if (loopTrong != null)
                            loops.Add(loopTrong);
                    }
                    levelId);         

                if (san == null)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static CurveLoop TaoCurveLoop(
            List<XYZ> DuongVien,
            double elevationFeet)
        {
            var curves = new List<Curve>();
            for (int i = 0; i < DuongVien.Count; i++)
            {
                XYZ a = ChieuLenCaoDo(DuongVien[i], elevationFeet);
                XYZ b = ChieuLenCaoDo(
                    DuongVien[(i + 1) % DuongVien.Count],
                    elevationFeet);

                if (a.DistanceTo(b) < UnitHelper.MmSangFeet(50))
                    continue;

                curves.Add(Line.CreateBound(a, b));
            }

            if (curves.Count < 3)
                return null;

            return CurveLoop.Create(curves);
        }

        private static XYZ ChieuLenLevel(XYZ p, double elevationFeet)
        {
            return ChieuLenCaoDo(p, elevationFeet);
        }

        private static XYZ ChieuLenCaoDo(XYZ p, double elevationFeet)
        {
            return new XYZ(p.X, p.Y, elevationFeet);
        }

        private WallType TimLoaiTuongGanNhat(double beDayMm)
        {
            double beDayFeet = UnitHelper.MmSangFeet(beDayMm);
            WallType ganNhat = null;
            double saiSoNhoNhat = double.MaxValue;

            foreach (WallType wt in new FilteredElementCollector(_doc)
                .OfClass(typeof(WallType))
                .Cast<WallType>())
            {
                if (wt.Kind != WallKind.Basic)
                    continue;

                double saiSo = Math.Abs(wt.Width - beDayFeet);
                if (saiSo < saiSoNhoNhat)
                {
                    saiSoNhoNhat = saiSo;
                    ganNhat = wt;
                }
            }

            return ganNhat;
        }

        private bool DaCoTuongTrung(Line trucTuong)
        {
            XYZ p1 = trucTuong.GetEndPoint(0);
            XYZ p2 = trucTuong.GetEndPoint(1);
            double tol = UnitHelper.MmSangFeet(10);

            foreach (Line lineCu in _danhSachTuongDaTao)
            {
                XYZ a1 = lineCu.GetEndPoint(0);
                XYZ a2 = lineCu.GetEndPoint(1);

                bool trung1 = a1.DistanceTo(p1) < tol && a2.DistanceTo(p2) < tol;
                bool trung2 = a1.DistanceTo(p2) < tol && a2.DistanceTo(p1) < tol;

                if (trung1 || trung2)
                    return true;
            }

            return false;
        }

        private static void LamThangDuong(ref XYZ p1, ref XYZ p2)
        {
            double tol = UnitHelper.MmSangFeet(20);
            double dx = Math.Abs(p2.X - p1.X);
            double dy = Math.Abs(p2.Y - p1.Y);

            if (dy < tol)
                p2 = new XYZ(p2.X, p1.Y, p2.Z);

            if (dx < tol)
                p2 = new XYZ(p1.X, p2.Y, p2.Z);
        }

        private static string MaViTri(double x, double y)
        {
            double g = UnitHelper.MmSangFeet(25);
            long bx = (long)Math.Round(x / g);
            long by = (long)Math.Round(y / g);
            return bx + ";" + by;
        }

        private static string MaDuongDam(DuongDam dd)
        {
            double g = UnitHelper.MmSangFeet(25);
            long x1 = (long)Math.Round(dd.DiemDau.X / g);
            long y1 = (long)Math.Round(dd.DiemDau.Y / g);
            long x2 = (long)Math.Round(dd.DiemCuoi.X / g);
            long y2 = (long)Math.Round(dd.DiemCuoi.Y / g);

            if (x1 > x2 || (x1 == x2 && y1 > y2))
            {
                long t = x1; x1 = x2; x2 = t;
                t = y1; y1 = y2; y2 = t;
            }

            return x1 + "," + y1 + "," + x2 + "," + y2;
        }

        private static string MaVung(List<XYZ> DuongVien)
        {
            if (DuongVien == null || DuongVien.Count == 0)
                return "";

            double minX = DuongVien.Min(p => p.X);
            double maxX = DuongVien.Max(p => p.X);
            double minY = DuongVien.Min(p => p.Y);
            double maxY = DuongVien.Max(p => p.Y);

            return Math.Round(minX) + "_" + Math.Round(minY) + "_" +
                   Math.Round(maxX) + "_" + Math.Round(maxY);
        }

        private static double DienTichVung(List<XYZ> DuongVien)
        {
            if (DuongVien == null || DuongVien.Count < 3)
                return 0;

            double s = 0;
            int n = DuongVien.Count;
            for (int i = 0; i < n; i++)
            {
                XYZ p = DuongVien[i];
                XYZ q = DuongVien[(i + 1) % n];
                s += p.X * q.Y - q.X * p.Y;
            }

            return Math.Abs(s);
        }
    }
}
