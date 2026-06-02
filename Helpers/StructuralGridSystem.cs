using Autodesk.Revit.DB;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cad2Revit.Helpers
{

    public class LuoiTrucKetCau
    {
        public List<double> TrucX { get; set; } = new List<double>();
        public List<double> TrucY { get; set; } = new List<double>();
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public List<XYZ> DuongVienSan { get; set; }
        public List<XYZ> LoThung { get; set; }

        /// <summary>Số nhịp bỏ mép mỗi phía ở tầng trên (mái thu hẹp như hình mẫu).</summary>
        public int SoNhipBoMeTangTren { get; set; }
    }

    public static class StructuralGridSystem
    {
        private const double TolGomTrucMm = 150;
        // Khoảng kéo dài cho lưới (grid) khi tạo trục trên Revit
        private const double MarginGridMm = 4500;
        // Khoảng biên cho vùng sàn (floor) relative tới trục - để sàn chỉ đến cột biên, đặt 0
        private const double MarginFloorMm = 0;

        public static LuoiTrucKetCau ApDungChuanRevit(
            KetQuaPhanTich phanTich,
            KetQuaDocCAD ketQuaCad,
            Document doc = null)
        {
            if (phanTich?.DanhSachDiemCot == null ||
                phanTich.DanhSachDiemCot.Count < 2)
            {
                return null;
            }

            LuoiTrucKetCau luoi = TaoLuoiTuCot(phanTich.DanhSachDiemCot);
            SnapCotVeLuoi(phanTich.DanhSachDiemCot, luoi);

            double tamX = (luoi.MinX + luoi.MaxX) * 0.5;
            double tamY = (luoi.MinY + luoi.MaxY) * 0.5;
            XYZ gocLuoi = new XYZ(tamX, tamY, 0);
            XYZ delta = new XYZ(-tamX, -tamY, 0);

            CadCoordinateHelper.DichPhanTich(phanTich, delta);
            CadCoordinateHelper.DichDuongCad(ketQuaCad, delta);
            luoi.DichVeGoc(gocLuoi);

            TaoDuongVienSan(luoi, phanTich);
            XacDinhMaiThuNho(
                luoi,
                phanTich.DanhSachDiemCot,
                phanTich.SoTang);

            TaoDamTheoLuoi(luoi, phanTich);
            SnapCotVeLuoi(phanTich.DanhSachDiemCot, luoi);

            if (doc != null)
            {
                RevitGridHelper.CanhChinhMoHinhVaoLuoiRevit(
                    phanTich,
                    ketQuaCad,
                    doc);
            }

            if (ketQuaCad != null)
            {
                ketQuaCad.DoiTatX += gocLuoi.X;
                ketQuaCad.DoiTatY += gocLuoi.Y;
            }

            phanTich.LuoiTruc = luoi;
            return luoi;
        }

        public static List<XYZ> LayDuongVienChoTang(
            LuoiTrucKetCau luoi,
            int tang,
            int tongSoTang)
        {
            if (luoi == null ||
                luoi.TrucX.Count < 2 ||
                luoi.TrucY.Count < 2)
            {
                return null;
            }

            int bo = 0;
            if (tongSoTang > 0 &&
                tang == tongSoTang - 1 &&
                luoi.SoNhipBoMeTangTren > 0)
            {
                bo = luoi.SoNhipBoMeTangTren;
            }

            int i0 = bo;
            int i1 = luoi.TrucX.Count - 1 - bo;
            int j0 = bo;
            int j1 = luoi.TrucY.Count - 1 - bo;

            if (i1 <= i0 || j1 <= j0)
            {
                i0 = 0;
                i1 = luoi.TrucX.Count - 1;
                j0 = 0;
                j1 = luoi.TrucY.Count - 1;
            }

            double m = UnitHelper.MmSangFeet(MarginFloorMm);
            double minX = luoi.TrucX[i0] - m;
            double maxX = luoi.TrucX[i1] + m;
            double minY = luoi.TrucY[j0] - m;
            double maxY = luoi.TrucY[j1] + m;

            return new List<XYZ>
            {
                new XYZ(minX, minY, 0),
                new XYZ(maxX, minY, 0),
                new XYZ(maxX, maxY, 0),
                new XYZ(minX, maxY, 0)
            };
        }

        /// <summary>Cột tại tầng — tầng mái thu hẹp: không tạo cột ở mép ngoài.</summary>
        public static List<DiemCot> LayDiemCotChoTang(
            LuoiTrucKetCau luoi,
            IList<DiemCot> tatCa,
            int tang,
            int tongSoTang)
        {
            if (tatCa == null || tatCa.Count == 0 || luoi == null)
                return new List<DiemCot>();

            bool tangTrenThuNho =
                tongSoTang > 0 &&
                tang == tongSoTang - 1 &&
                luoi.SoNhipBoMeTangTren > 0 &&
                luoi.TrucX.Count >= 4 &&
                luoi.TrucY.Count >= 4;

            if (!tangTrenThuNho)
                return tatCa.ToList();

            double tol = UnitHelper.MmSangFeet(TolGomTrucMm);
            int bo = luoi.SoNhipBoMeTangTren;
            double xMin = luoi.TrucX[bo] - tol;
            double xMax = luoi.TrucX[luoi.TrucX.Count - 1 - bo] + tol;
            double yMin = luoi.TrucY[bo] - tol;
            double yMax = luoi.TrucY[luoi.TrucY.Count - 1 - bo] + tol;

            return tatCa
                .Where(c =>
                    c.X >= xMin && c.X <= xMax &&
                    c.Y >= yMin && c.Y <= yMax)
                .ToList();
        }

        private static void XacDinhMaiThuNho(
            LuoiTrucKetCau luoi,
            List<DiemCot> cot,
            int soTangYeuCau)
        {
            luoi.SoNhipBoMeTangTren = 0;

            if (soTangYeuCau < 6)
                return;

            if (luoi.TrucX.Count < 5 || luoi.TrucY.Count < 3 || cot == null)
                return;

            double tol = UnitHelper.MmSangFeet(TolGomTrucMm);
            int demGoc = 0;
            int demTrong = 0;

            foreach (DiemCot c in cot)
            {
                bool ganGocX =
                    Math.Abs(c.X - luoi.TrucX[0]) < tol ||
                    Math.Abs(c.X - luoi.TrucX[luoi.TrucX.Count - 1]) < tol;
                bool ganGocY =
                    Math.Abs(c.Y - luoi.TrucY[0]) < tol ||
                    Math.Abs(c.Y - luoi.TrucY[luoi.TrucY.Count - 1]) < tol;

                if (ganGocX && ganGocY)
                    demGoc++;
                else if (!ganGocX && !ganGocY)
                    demTrong++;
            }

            if (demGoc >= 4 && demTrong >= 4 &&
                luoi.TrucX.Count >= 5)
            {
                luoi.SoNhipBoMeTangTren = 1;
            }
        }

        private static LuoiTrucKetCau TaoLuoiTuCot(List<DiemCot> cot)
        {
            double tolFeet = UnitHelper.MmSangFeet(TolGomTrucMm);

            var luoi = new LuoiTrucKetCau
            {
                TrucX = GomTruc(cot.Select(c => c.X), tolFeet),
                TrucY = GomTruc(cot.Select(c => c.Y), tolFeet)
            };

            if (luoi.TrucX.Count > 0)
            {
                luoi.MinX = luoi.TrucX.First();
                luoi.MaxX = luoi.TrucX.Last();
            }

            if (luoi.TrucY.Count > 0)
            {
                luoi.MinY = luoi.TrucY.First();
                luoi.MaxY = luoi.TrucY.Last();
            }

            return luoi;
        }

        private static List<double> GomTruc(
            IEnumerable<double> giaTri,
            double tolFeet)
        {
            var sap = giaTri.OrderBy(v => v).ToList();
            var ketQua = new List<double>();

            foreach (double v in sap)
            {
                if (ketQua.Count == 0 ||
                    Math.Abs(v - ketQua[ketQua.Count - 1]) > tolFeet)
                {
                    ketQua.Add(v);
                }
                else
                {
                    int cuoi = ketQua.Count - 1;
                    ketQua[cuoi] = (ketQua[cuoi] + v) / 2;
                }
            }

            return ketQua;
        }

        private static void SnapCotVeLuoi(
            List<DiemCot> cot,
            LuoiTrucKetCau luoi)
        {
            foreach (DiemCot c in cot)
            {
                c.X = SnapGiaTri(c.X, luoi.TrucX);
                c.Y = SnapGiaTri(c.Y, luoi.TrucY);
            }
        }

        private static double SnapGiaTri(double v, List<double> truc)
        {
            if (truc == null || truc.Count == 0)
                return v;

            double ganNhat = truc[0];
            double saiSo = Math.Abs(v - ganNhat);

            foreach (double t in truc)
            {
                double s = Math.Abs(v - t);
                if (s < saiSo)
                {
                    saiSo = s;
                    ganNhat = t;
                }
            }

            return ganNhat;
        }

        private static void TaoDuongVienSan(
            LuoiTrucKetCau luoi,
            KetQuaPhanTich phanTich)
        {
            double m = UnitHelper.MmSangFeet(MarginFloorMm);

            luoi.DuongVienSan = new List<XYZ>
            {
                new XYZ(luoi.MinX - m, luoi.MinY - m, 0),
                new XYZ(luoi.MaxX + m, luoi.MinY - m, 0),
                new XYZ(luoi.MaxX + m, luoi.MaxY + m, 0),
                new XYZ(luoi.MinX - m, luoi.MaxY + m, 0)
            };

            luoi.LoThung = TimLoThungTuVungSan(phanTich, luoi);
        }

        private static List<XYZ> TimLoThungTuVungSan(
            KetQuaPhanTich phanTich,
            LuoiTrucKetCau luoi)
        {
            if (phanTich.DanhSachVungSan == null ||
                phanTich.DanhSachVungSan.Count < 2)
            {
                return null;
            }

            var sap = phanTich.DanhSachVungSan
                .Where(v => v.DuongVien != null && v.DuongVien.Count >= 3)
                .OrderByDescending(v => DienTich(v.DuongVien))
                .ToList();

            if (sap.Count < 2)
                return null;

            double sNgoai = DienTich(sap[0].DuongVien);

            foreach (VungSan v in sap.Skip(1))
            {
                double sTrong = DienTich(v.DuongVien);
                if (sTrong < sNgoai * 0.45 && NamTrongLuoi(v.DuongVien, luoi))
                {
                    return new List<XYZ>(v.DuongVien);
                }
            }

            return null;
        }

        private static bool NamTrongLuoi(
            List<XYZ> DuongVien,
            LuoiTrucKetCau luoi)
        {
            double minX = DuongVien.Min(p => p.X);
            double maxX = DuongVien.Max(p => p.X);
            double minY = DuongVien.Min(p => p.Y);
            double maxY = DuongVien.Max(p => p.Y);

            return minX >= luoi.MinX - UnitHelper.MmSangFeet(500) &&
                maxX <= luoi.MaxX + UnitHelper.MmSangFeet(500) &&
                minY >= luoi.MinY - UnitHelper.MmSangFeet(500) &&
                maxY <= luoi.MaxY + UnitHelper.MmSangFeet(500);
        }

        private static void TaoDamTheoLuoi(
            LuoiTrucKetCau luoi,
            KetQuaPhanTich phanTich)
        {
            if (luoi.TrucX.Count < 2 || luoi.TrucY.Count < 2)
                return;

            double rong = phanTich.DamRongMm > 0
                ? phanTich.DamRongMm
                : 200;

            double cao = phanTich.DamCaoMm > 0
                ? phanTich.DamCaoMm
                : 500;

            var ds = new List<DuongDam>();

            for (int j = 0; j < luoi.TrucY.Count; j++)
            {
                double y = luoi.TrucY[j];
                for (int i = 0; i < luoi.TrucX.Count - 1; i++)
                {
                    ThemDam(
                        ds,
                        luoi.TrucX[i],
                        y,
                        luoi.TrucX[i + 1],
                        y,
                        rong,
                        cao);
                }
            }

            for (int i = 0; i < luoi.TrucX.Count; i++)
            {
                double x = luoi.TrucX[i];
                for (int j = 0; j < luoi.TrucY.Count - 1; j++)
                {
                    ThemDam(
                        ds,
                        x,
                        luoi.TrucY[j],
                        x,
                        luoi.TrucY[j + 1],
                        rong,
                        cao);
                }
            }

            phanTich.DanhSachDuongDam = ds;
        }

        private static void ThemDam(
            List<DuongDam> ds,
            double x1,
            double y1,
            double x2,
            double y2,
            double rong,
            double cao)
        {
            if (Math.Abs(x1 - x2) < 1e-6 &&
                Math.Abs(y1 - y2) < 1e-6)
            {
                return;
            }

            ds.Add(new DuongDam
            {
                DiemDau = new XYZ(x1, y1, 0),
                DiemCuoi = new XYZ(x2, y2, 0),
                RongMm = rong,
                CaoMm = cao,
                TenLayer = "CAD2Revit-Luoi"
            });
        }

        private static XYZ Cong(XYZ p, XYZ delta)
        {
            if (p == null)
                return XYZ.Zero;

            return new XYZ(
                p.X + delta.X,
                p.Y + delta.Y,
                p.Z);
        }

        private static double DienTich(List<XYZ> DuongVien)
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

            return Math.Abs(s) * 0.5;
        }
    }

    internal static class LuoiTrucKetCauExtensions
    {
        public static void DichVeGoc(this LuoiTrucKetCau luoi, XYZ goc)
        {
            for (int i = 0; i < luoi.TrucX.Count; i++)
                luoi.TrucX[i] -= goc.X;

            for (int i = 0; i < luoi.TrucY.Count; i++)
                luoi.TrucY[i] -= goc.Y;

            luoi.MinX -= goc.X;
            luoi.MaxX -= goc.X;
            luoi.MinY -= goc.Y;
            luoi.MaxY -= goc.Y;

            if (luoi.DuongVienSan != null)
            {
                for (int i = 0; i < luoi.DuongVienSan.Count; i++)
                    luoi.DuongVienSan[i] = new XYZ(
                        luoi.DuongVienSan[i].X - goc.X,
                        luoi.DuongVienSan[i].Y - goc.Y,
                        0);
            }

            if (luoi.LoThung != null)
            {
                for (int i = 0; i < luoi.LoThung.Count; i++)
                    luoi.LoThung[i] = new XYZ(
                        luoi.LoThung[i].X - goc.X,
                        luoi.LoThung[i].Y - goc.Y,
                        0);
            }
        }
    }
}
