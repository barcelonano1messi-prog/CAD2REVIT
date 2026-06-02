using Autodesk.Revit.DB;
using Cad2Revit.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cad2Revit.Converter
{
    public class RevitTcvnHelper
    {
        private readonly Document _doc;
        private readonly Dictionary<string, Material> _vatLieuCache =
            new Dictionary<string, Material>();

        private readonly Dictionary<string, FamilySymbol> _symbolCotCache =
            new Dictionary<string, FamilySymbol>();

        private readonly Dictionary<string, FamilySymbol> _symbolDamCache =
            new Dictionary<string, FamilySymbol>();

        private const double TolKhoiTaoTypeMm = 3;

        public RevitTcvnHelper(Document doc)
        {
            _doc = doc;
        }

        public FamilySymbol TimFamilyCot(double rongMm, double sauMm)
        {
            return TimHoacTaoSymbol(
                BuiltInCategory.OST_StructuralColumns,
                rongMm,
                sauMm,
                "cột",
                _symbolCotCache,
                LaFamilyDam: false);
        }

        public FamilySymbol TimFamilyDam(double rongMm, double caoMm)
        {
            return TimHoacTaoSymbol(
                BuiltInCategory.OST_StructuralFraming,
                rongMm,
                caoMm,
                "dầm",
                _symbolDamCache,
                LaFamilyDam: true);
        }
        public FloorType TimFloorTypeTcvn(double beDayMm, bool laSanMai = false)
        {
            double beDayFeet = UnitHelper.MmSangFeet(beDayMm);
            FloorType generic = TimFloorGenericMacDinh(beDayFeet, laSanMai);
            if (generic != null)
            {
                return ChinhDoDayFloorTypeIfNeeded(generic, beDayFeet);
            }

            var ungVien = new FilteredElementCollector(_doc)
                .OfClass(typeof(FloorType))
                .Cast<FloorType>()
                .Where(ft => LaFloorTypeSanPhuHop(ft, laSanMai))
                .Select(ft => new
                {
                    Type = ft,
                    DoDay = LayDoDayFloor(ft),  
                    Diem = TinhDiemFloor(ft, beDayFeet) 
                })
                .OrderByDescending(x => x.Diem)
                .ToList();

            if (ungVien.Count > 0)
            {
                FloorType chon = ungVien[0].Type;
                return ChinhDoDayFloorTypeIfNeeded(chon, beDayFeet);
            }

            FloorType macDinh = new FilteredElementCollector(_doc)
                .OfClass(typeof(FloorType))
                .Cast<FloorType>()
                .Where(ft => LaFloorTypeSanPhuHop(ft, laSanMai))
                .OrderBy(ft => Math.Abs(LayDoDayFloor(ft) - beDayFeet))
                .FirstOrDefault();

            if (macDinh != null)
            {
                return ChinhDoDayFloorTypeIfNeeded(macDinh, beDayFeet);
            }

            return null;
        }
        private FloorType ChinhDoDayFloorTypeIfNeeded(
            FloorType floorType,
            double beDayFeet)
        {
            if (floorType == null)
                return null;

            // ============ KIỂM TRA: ĐỘ DÀY CÓ PHẢI BẰNG KHÔNG? ============
            double currentThickness = LayDoDayFloor(floorType);
            
            // Nếu độ dày hiện tại gần bằng độ dày cần → dùng luôn
            if (Math.Abs(currentThickness - beDayFeet) < UnitHelper.MmSangFeet(1))
                return floorType;

            try
            {
                string newName = floorType.Name + " - " + Math.Round(UnitHelper.FeetSangMm(beDayFeet)) + "mm";

                FloorType newType = null;
                try
                {
                    newType = floorType.Duplicate(newName) as FloorType;
                }
                catch
                {
                    newType = null;
                }

                if (newType == null)
                    return floorType;

                CompoundStructure cs = newType.GetCompoundStructure();
                if (cs != null)
                {
                    double currentWidth = cs.GetWidth();
                    if (currentWidth > 0)
                    {
                        double scale = beDayFeet / currentWidth;
                        
                        for (int i = 0; i < cs.LayerCount; i++)
                        {
                            double oldLayerWidth = cs.GetLayerWidth(i);
                            cs.SetLayerWidth(i, oldLayerWidth * scale);
                        }
                        newType.SetCompoundStructure(cs);
                    }
                }

                return newType;
            }
            catch (Exception ex)
            {
                return floorType;
            }
        }

        public void GanVatLieuTcvn(Element element, string macBeTong)
        {
            if (element == null)
                return;

            Material vatLieu = TimHoacTaoVatLieu(macBeTong);
            if (vatLieu == null)
                return;

            try
            {
                Parameter pMat = element.get_Parameter(
                    BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);

                if (pMat != null && !pMat.IsReadOnly)
                {
                    pMat.Set(vatLieu.Id);
                    return;
                }

                foreach (Parameter p in element.Parameters)
                {
                    if (p == null || p.IsReadOnly)
                        continue;

                    if (p.StorageType != StorageType.ElementId)
                        continue;

                    string ten = p.Definition?.Name ?? "";
                    if (ten.IndexOf("material", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        ten.IndexOf("vật liệu", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        ten.IndexOf("Structural Material", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        p.Set(vatLieu.Id);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void DatKichThuocFamily(
            FamilyInstance instance,
            double rongMm,
            double sauMm)
        {
            if (instance == null)
                return;

            double rFeet = UnitHelper.MmSangFeet(rongMm);
            double sFeet = UnitHelper.MmSangFeet(sauMm);

            bool okR = DatThamSoKichThuoc(instance, rFeet, true);
            bool okS = DatThamSoKichThuoc(instance, sFeet, false);

            if (!okR || !okS)
            {
                DatThamSoKichThuoc(instance, rFeet, false);
                DatThamSoKichThuoc(instance, sFeet, true);
            }

            DatBuiltIn(instance, BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, rFeet);
            DatBuiltIn(instance, BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sFeet);
        }

        /// <summary>
        /// Tìm type đúng b×h; nếu không có thì Duplicate type mẫu và sửa b, h (Type parameter).
        /// </summary>
        private FamilySymbol TimHoacTaoSymbol(
            BuiltInCategory category,
            double rongMm,
            double sauMm,
            string loaiMoTa,
            Dictionary<string, FamilySymbol> cache,
            bool LaFamilyDam)
        {
            string key = MaSymbol(rongMm, sauMm);
            if (cache.TryGetValue(key, out FamilySymbol daCo) &&
                daCo != null &&
                daCo.IsValidObject)
            {
                return daCo;
            }

            FamilySymbol khop = TimSymbolKhopKichThuoc(
                category,
                rongMm,
                sauMm,
                LaFamilyDam);

            if (khop != null)
            {
                cache[key] = khop;
                return khop;
            }

            FamilySymbol mau = TimFamilyBeTong(
                category,
                rongMm,
                sauMm,
                loaiMoTa);

            if (mau == null)
                return null;

            FamilySymbol taoMoi = TaoTypeTuMau(mau, rongMm, sauMm, loaiMoTa);
            if (taoMoi != null)
                cache[key] = taoMoi;

            return taoMoi ?? mau;
        }

        private static string MaSymbol(double rongMm, double sauMm)
        {
            return Math.Round(rongMm) + "x" + Math.Round(sauMm);
        }

        private FamilySymbol TimSymbolKhopKichThuoc(
            BuiltInCategory category,
            double rongMm,
            double sauMm,
            bool laFamilyDam)
        {
            double rongFeet = UnitHelper.MmSangFeet(rongMm);
            double sauFeet = UnitHelper.MmSangFeet(sauMm);
            double tolFeet = UnitHelper.MmSangFeet(TolKhoiTaoTypeMm);

            foreach (FamilySymbol fs in LayDanhSachSymbol(category, laFamilyDam))
            {
                double w = LayKichThuocSymbol(fs, "b", "B", "Width", "WIDTH", "Rong", "rong", "CHRONG", "Chieu rong", "CHIEU RONG", "SECTION WIDTH");
                double h = LayKichThuocSymbol(fs, "h", "H", "Depth", "DEPTH", "Height", "HEIGHT", "Cao", "cao", "CHCAO", "Chieu cao", "CHIEU CAO", "SECTION DEPTH", "SECTION HEIGHT");

                if (w <= 0 || h <= 0)
                    continue;

                bool khopThuong =
                    Math.Abs(w - rongFeet) <= tolFeet &&
                    Math.Abs(h - sauFeet) <= tolFeet;

                bool khopDao =
                    Math.Abs(w - sauFeet) <= tolFeet &&
                    Math.Abs(h - rongFeet) <= tolFeet;

                if (khopThuong || khopDao)
                    return fs;
            }

            return null;
        }

        private FamilySymbol TaoTypeTuMau(
            FamilySymbol mau,
            double rongMm,
            double sauMm,
            string loaiMoTa)
        {
            string tenMoi = string.Format(
                "{0:0.#} x {1:0.#}mm",
                rongMm,
                sauMm);

            try
            {
                FamilySymbol daTonTai = TimSymbolTheoTen(mau, tenMoi);
                if (daTonTai != null)
                {
                    DatKichThuocTrenSymbol(daTonTai, rongMm, sauMm);
                    return daTonTai;
                }

                ElementType banSao = mau.Duplicate(tenMoi);
                FamilySymbol moi = banSao as FamilySymbol;
                if (moi == null)
                    return null;

                if (!moi.IsActive)
                    moi.Activate();

                DatKichThuocTrenSymbol(moi, rongMm, sauMm);
                return moi;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private FamilySymbol TimSymbolTheoTen(
            FamilySymbol mau,
            string ten)
        {
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategoryId(mau.Category.Id)
                .Cast<FamilySymbol>()
                .FirstOrDefault(fs =>
                    fs.FamilyName == mau.FamilyName &&
                    string.Equals(fs.Name, ten, StringComparison.OrdinalIgnoreCase));
        }

        private bool DatKichThuocTrenSymbol(
            FamilySymbol fs,
            double rongMm,
            double sauMm)
        {
            if (fs == null)
                return false;

            double rFeet = UnitHelper.MmSangFeet(rongMm);
            double sFeet = UnitHelper.MmSangFeet(sauMm);

            bool okR = DatThamSoKichThuoc(fs, rFeet, true);
            bool okS = DatThamSoKichThuoc(fs, sFeet, false);

            if (!okR || !okS)
            {
                okR = DatThamSoKichThuoc(fs, rFeet, false) || okR;
                okS = DatThamSoKichThuoc(fs, sFeet, true) || okS;
            }

            return okR && okS;
        }

        private List<FamilySymbol> LayDanhSachSymbol(
            BuiltInCategory category,
            bool laFamilyDam)
        {
            var symbols = new FilteredElementCollector(_doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(category)
                .Cast<FamilySymbol>()
                .Where(fs => LaFamilyBeTong(fs))
                .Where(fs => !laFamilyDam || LaFamilyDam(fs))
                .ToList();

            if (symbols.Count == 0)
            {
                symbols = new FilteredElementCollector(_doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(category)
                    .Cast<FamilySymbol>()
                    .Where(fs => !LaFamilyBiLoaiTru(fs))
                    .Where(fs => !laFamilyDam || LaFamilyDam(fs))
                    .ToList();
            }

            return symbols;
        }

        private static void DatBuiltIn(
            Element el,
            BuiltInParameter bip,
            double giaTriFeet)
        {
            try
            {
                Parameter p = el.get_Parameter(bip);
                if (p != null &&
                    !p.IsReadOnly &&
                    p.StorageType == StorageType.Double)
                {
                    p.Set(giaTriFeet);
                }
            }
            catch
            {
                // bỏ qua
            }
        }

        private FamilySymbol TimFamilyBeTong(
            BuiltInCategory category,
            double rongMm,
            double sauMm,
            string loaiMoTa)
        {
            var symbols = new FilteredElementCollector(_doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(category)
                .Cast<FamilySymbol>()
                .Where(fs => LaFamilyBeTong(fs))
                .Where(fs => category != BuiltInCategory.OST_StructuralFraming ||
                    LaFamilyDam(fs))
                .ToList();

            if (symbols.Count == 0)
            {
                symbols = new FilteredElementCollector(_doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(category)
                    .Cast<FamilySymbol>()
                    .Where(fs => !LaFamilyBiLoaiTru(fs))
                    .Where(fs => category != BuiltInCategory.OST_StructuralFraming ||
                        LaFamilyDam(fs))
                    .ToList();
            }

            if (symbols.Count == 0)
            {
                return null;
            }

            double rongFeet = UnitHelper.MmSangFeet(rongMm);
            double sauFeet = UnitHelper.MmSangFeet(sauMm);

            var helper = this;
            FamilySymbol ganNhat = symbols
                .OrderByDescending(fs => DiemFamily(fs))
                .ThenBy(fs => helper.SaiSoKichThuoc(fs, rongFeet, sauFeet))
                .First();

            return ganNhat;
        }

        private static int DiemFamily(FamilySymbol fs)
        {
            string text = (fs.FamilyName + " " + fs.Name).ToUpper();
            int diem = 0;

            foreach (string kw in Tcvn5574Catalog.FamilyBeTong)
            {
                if (text.Contains(kw))
                    diem += 10;
            }

            if (text.Contains("METRIC") || text.Contains("MM"))
                diem += 5;

            return diem;
        }

        private double SaiSoKichThuoc(
            FamilySymbol fs,
            double rongFeet,
            double sauFeet)
        {
            double w = LayKichThuocSymbol(fs, "b", "B", "Width", "WIDTH");
            double h = LayKichThuocSymbol(fs, "h", "H", "Depth", "DEPTH");

            double saiSo = 1000.0;

            if (w > 0 && h > 0)
            {
                saiSo = Math.Abs(w - rongFeet) + Math.Abs(h - sauFeet);
            }

            string text = (fs.FamilyName + " " + fs.Name).ToUpper();
            long rongMm = (long)Math.Round(UnitHelper.FeetSangMm(rongFeet));
            long sauMm = (long)Math.Round(UnitHelper.FeetSangMm(sauFeet));

            if (text.Contains(rongMm.ToString()) &&
                text.Contains(sauMm.ToString()))
            {
                saiSo -= 50;
            }

            if (text.Replace(" ", "").Contains(
                rongMm + "X" + sauMm) ||
                text.Replace(" ", "").Contains(
                    rongMm + "×" + sauMm))
            {
                saiSo -= 30;
            }

            if (text.Contains((rongMm / 10).ToString()) &&
                text.Contains((sauMm / 10).ToString()))
            {
                saiSo -= 5;
            }

            return saiSo;
        }

        private static double LayKichThuocSymbol(
            FamilySymbol fs,
            params string[] ten)
        {
            foreach (string t in ten)
            {
                Parameter p = fs.LookupParameter(t);
                if (p != null &&
                    p.StorageType == StorageType.Double &&
                    p.HasValue)
                {
                    return p.AsDouble();
                }
            }

            return 0;
        }

        private static bool LaFamilyBeTong(FamilySymbol fs)
        {
            if (LaFamilyBiLoaiTru(fs))
                return false;

            string text = (fs.FamilyName + " " + fs.Name).ToUpper();

            return Tcvn5574Catalog.FamilyBeTong
                .Any(kw => text.Contains(kw));
        }

        private static bool LaFamilyBiLoaiTru(FamilySymbol fs)
        {
            string text = (fs.FamilyName + " " + fs.Name).ToUpper();
            return Tcvn5574Catalog.Family
                .Any(kw => text.Contains(kw));
        }

        private static bool LaFamilyDam(FamilySymbol fs)
        {
            string text = (fs.FamilyName + " " + fs.Name).ToUpper();

            if (text.Contains("BRACE") ||
                text.Contains("GIẰNG") ||
                text.Contains("GIRDER") && !text.Contains("BEAM"))
            {
                return false;
            }

            return text.Contains("BEAM") ||
                text.Contains("DẦM") ||
                text.Contains("DAM") ||
                text.Contains("RECTANGULAR") ||
                text.Contains("CONCRETE");
        }

        private static bool LaGenericKhongPhuHop(string ten)
        {
            if (string.IsNullOrEmpty(ten))
                return true;

            string t = ten.ToUpper();
            return t.Contains("GENERIC") && t.Contains("12");
        }

        private static bool LaFloorGeneric(string ten)
        {
            if (string.IsNullOrEmpty(ten) || LaGenericKhongPhuHop(ten))
                return false;

            return ten.ToUpperInvariant().Contains(
                Tcvn5574Catalog.floorTypeMacDinh);
        }

        private FloorType TimFloorGenericMacDinh(
            double beDayFeet,
            bool laSanMai)
        {
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(FloorType))
                .Cast<FloorType>()
                .Where(ft =>
                    LaFloorTypeSanPhuHop(ft, laSanMai) &&
                    LaFloorGeneric(ft.Name))
                .OrderBy(ft => Math.Abs(LayDoDayFloor(ft) - beDayFeet))
                .ThenBy(ft => ft.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }

        private static bool LaFloorTypeSanPhuHop(
            FloorType ft,
            bool laSanMai)
        {
            if (ft == null || LaGenericKhongPhuHop(ft.Name))
                return false;

            string t = ft.Name.ToUpper();

            if (t.Contains("FOUNDATION") ||
                t.Contains("MÓNG") ||
                t.Contains("MONG") ||
                t.Contains("FOOTING") ||
                t.Contains("RAFT"))
            {
                return false;
            }

            if (laSanMai &&
                (t.Contains("SLAB ON GRADE") || t.Contains("SAN NEN")))
            {
                return false;
            }

            return true;
        }

        private static int TinhDiemFloor(FloorType ft, double beDayFeet)
        {
            string ten = ft.Name.ToUpper();
            int diem = 0;

            if (LaFloorGeneric(ft.Name))
                diem += 100;

            foreach (string kw in Tcvn5574Catalog.FloorType)
            {
                if (ten.Contains(kw))
                    diem += 10;
            }

            if (LaGenericKhongPhuHop(ft.Name))
                diem -= 50;

            double saiSo = Math.Abs(LayDoDayFloor(ft) - beDayFeet);
            diem -= (int)UnitHelper.FeetSangMm(saiSo) / 10;

            return diem;
        }

        private static double LayDoDayFloor(FloorType ft)
        {
            try
            {
                CompoundStructure cs = ft.GetCompoundStructure();
                if (cs != null)
                    return cs.GetWidth();
            }
            catch
            {
                // bỏ qua
            }

            return UnitHelper.MmSangFeet(150);
        }

        private Material TimHoacTaoVatLieu(string macBeTong)
        {
            if (_vatLieuCache.TryGetValue(macBeTong, out Material cached))
                return cached;

            Material timThay = null;
            int diemCaoNhat = int.MinValue;

            foreach (Material m in new FilteredElementCollector(_doc)
                .OfClass(typeof(Material))
                .Cast<Material>())
            {
                string ten = m.Name.ToUpper();
                int diem = 0;

                if (ten.Contains(macBeTong.ToUpper()))
                    diem += 30;

                foreach (string kw in Tcvn5574Catalog.VatLieu)
                {
                    if (ten.Contains(kw))
                        diem += 5;
                }

                if (ten.Contains("TCVN") || ten.Contains("5574"))
                    diem += 15;

                if (diem > diemCaoNhat)
                {
                    diemCaoNhat = diem;
                    timThay = m;
                }
            }

            if (timThay == null)
            {
                try
                {
                    string tenMoi =
                        "BTCT " + macBeTong + " - TCVN 5574:2018";

                    ElementId idVatLieu =
                        Material.Create(_doc, tenMoi);

                    timThay = _doc.GetElement(idVatLieu) as Material;
                }
                catch (Exception ex)
                {
                }
            }

            _vatLieuCache[macBeTong] = timThay;
            return timThay;
        }

        private static bool DatThamSoKichThuoc(
            Element el,
            double giaTriFeet,
            bool laRong)
        {
            string[] ten = laRong
                ? new[]
                {
                    "b", "B", "b1", "B1", "Width", "WIDTH", "Rong", "rong",
                    "CHRONG", "Chieu rong", "CHIEU RONG", "SECTION WIDTH"
                }
                : new[]
                {
                    "h", "H", "h1", "H1", "Depth", "DEPTH", "Height", "HEIGHT", "Cao", "cao",
                    "CHCAO", "Chieu cao", "CHIEU CAO", "SECTION DEPTH", "SECTION HEIGHT", "d"
                };

            foreach (string t in ten)
            {
                Parameter p = el.LookupParameter(t);
                if (p == null ||
                    p.IsReadOnly ||
                    p.StorageType != StorageType.Double)
                {
                    continue;
                }

                try
                {
                    p.Set(giaTriFeet);
                    return true;
                }
                catch
                {
                }
            }

            return false;
        }
    }
}
