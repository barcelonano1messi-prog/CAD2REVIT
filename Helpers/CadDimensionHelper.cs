using Autodesk.Revit.DB;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cad2Revit.Helpers
{
    /// <summary>
    /// Đọc kích thước cột: ưu tiên tên layer (500X600, 400X500), sau đó DIM layer KT.
    /// </summary>
    public static class CadDimensionHelper
    {
        private const double BanKinhGanDimMm = 2800;
        private const double TolHuongMm = 80;
        private const double DimMinMm = 150;
        private const double DimMaxMm = 12000;
        private const double TolCapChuThichMm = 50;
        private const double CotDimMinMm = 350;
        private const double CotDimMaxMm = 950;

        private static readonly Tuple<double, double>[] CapCotTheoChuThiet =
        {
            Tuple.Create(500.0, 600.0),
            Tuple.Create(400.0, 500.0),
        };

        private static readonly Regex RegexKichThuoc =
            new Regex(
                @"(\d{2,4})\s*[xX×]\s*(\d{2,4})",
                RegexOptions.Compiled);

        private class DoanDim
        {
            public double GiaTriMm { get; set; }
            public bool TheoPhuongX { get; set; }
            public XYZ Tam { get; set; }
        }

        public static string TomTatKichThuocCot(IList<DiemCot> ds)
        {
            if (ds == null || ds.Count == 0)
                return "";

            var nhom = ds
                .GroupBy(c =>
                    string.Format(
                        "{0:0.#}×{1:0.#} ({2})",
                        c.RongMm,
                        c.SauMm,
                        c.TenLayer ?? "?"))
                .Select(g => g.Count() + "× " + g.Key)
                .ToList();

            return "Tiết diện cột CAD: " + string.Join("; ", nhom);
        }

        public static void ApDungKichThuocTuBanVe(
            KetQuaPhanTich phanTich,
            IList<CadLine> tatCaDuong)
        {
            if (phanTich == null || tatCaDuong == null)
                return;

            var doanDim = TrichDoanDim(tatCaDuong);

            if (phanTich.DanhSachDiemCot != null)
            {
                foreach (DiemCot cot in phanTich.DanhSachDiemCot)
                {
                    ApDungKichThuocCot(cot, doanDim, tatCaDuong);
                }

                string chiTiet = TomTatKichThuocCot(phanTich.DanhSachDiemCot);
                if (!string.IsNullOrEmpty(chiTiet))
                {
                    phanTich.TomTat = string.IsNullOrEmpty(phanTich.TomTat)
                        ? chiTiet
                        : phanTich.TomTat + "\r\n" + chiTiet;
                }
            }

            if (phanTich.DanhSachDuongDam != null)
            {
                foreach (DuongDam dam in phanTich.DanhSachDuongDam)
                {
                    ApDungKichThuocDam(dam, doanDim);
                }
            }
        }

        /// <summary>Đọc y nguyên thứ tự trong chuỗi (500X600 → 500, 600).</summary>
        public static Tuple<double, double> LayKichThuocTuTen(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
                return null;

            Match m = RegexKichThuoc.Match(ten);
            if (!m.Success)
                return null;

            double a = double.Parse(m.Groups[1].Value);
            double b = double.Parse(m.Groups[2].Value);

            if (a < 100 && b < 100)
            {
                a *= 10;
                b *= 10;
            }

            return Tuple.Create(a, b);
        }

        public static bool LaLayerChiTietCot(string tenLayer)
        {
            if (string.IsNullOrWhiteSpace(tenLayer))
                return false;

            string ten = tenLayer.ToUpperInvariant();
            return ten.Contains("CHI TIET") ||
                ten.Contains("CHITIET") ||
                ten.Contains("DETAIL");
        }

        public static bool LaLayerBienCot(string tenLayer)
        {
            if (string.IsNullOrWhiteSpace(tenLayer))
                return false;

            string ten = tenLayer.ToUpperInvariant();
            return ten.Contains("BIEN") ||
                ten.Contains("BIÊN") ||
                ten.Contains("MAT BANG") ||
                ten.Contains("MATBANG");
        }

        public static Tuple<double, double> MacDinhTheoLoaiLayer(string tenLayer)
        {
            if (LaLayerBienCot(tenLayer))
                return CapCotTheoChuThiet[1];

            if (LaLayerChiTietCot(tenLayer))
                return CapCotTheoChuThiet[0];

            return null;
        }

        private static void ApDungKichThuocCot(
            DiemCot cot,
            List<DoanDim> doanDim,
            IList<CadLine> tatCaDuong)
        {
            // 1) Tên layer cột — chuẩn nhất (COT_500X600, 400X500, …)
            var tuTenLayer = LayKichThuocTuTen(cot.TenLayer);
            if (tuTenLayer != null)
            {
                GanKichThuoc(cot, tuTenLayer);
                return;
            }

            // 2) Cặp DIM KT gần cột (ưu tiên 500+600 hoặc 400+500 theo loại bản)
            var tuCapDim = TimCapChuThichTuDim(cot, doanDim, cot.TenLayer);
            if (tuCapDim != null)
            {
                GanKichThuoc(cot, tuCapDim);
                return;
            }

            // 3) DIM ngang + dọc gần nhất (đúng số trên bản vẽ)
            var tuHuong = LayKichThuocDimTheoHuong(cot, doanDim);
            if (tuHuong != null)
            {
                GanKichThuoc(cot, tuHuong);
                return;
            }

            // 4) Layer DIM/KT gần có kích thước trong tên
            var tuLayerDimGan = LayKichThuocTuLayerDimGan(cot, tatCaDuong);
            if (tuLayerDimGan != null)
            {
                GanKichThuoc(cot, tuLayerDimGan);
                return;
            }

            // 5) Mặc định chi tiết / biên
            var mac = MacDinhTheoLoaiLayer(cot.TenLayer);
            if (mac != null)
                GanKichThuoc(cot, mac);
        }

        private static void GanKichThuoc(
            DiemCot cot,
            Tuple<double, double> kt)
        {
            cot.RongMm = kt.Item1;
            cot.SauMm = kt.Item2;
        }

        private static List<DoanDim> TrichDoanDim(IList<CadLine> duong)
        {
            var ketQua = new List<DoanDim>();
            double tolHuong = UnitHelper.MmSangFeet(TolHuongMm);

            foreach (CadLine ln in duong)
            {
                if (!LaLayerDim(ln.TenLayer))
                    continue;

                double daiMm = UnitHelper.FeetSangMm(ln.ChieuDaiFeet());
                if (daiMm < DimMinMm || daiMm > DimMaxMm)
                    continue;

                XYZ dau = ln.DiemDau;
                XYZ cuoi = ln.DiemCuoi;
                double dx = Math.Abs(cuoi.X - dau.X);
                double dy = Math.Abs(cuoi.Y - dau.Y);

                bool theoX = dx >= dy;
                if (Math.Max(dx, dy) < tolHuong)
                    continue;

                ketQua.Add(new DoanDim
                {
                    GiaTriMm = daiMm,
                    TheoPhuongX = theoX,
                    Tam = new XYZ(
                        (dau.X + cuoi.X) * 0.5,
                        (dau.Y + cuoi.Y) * 0.5,
                        0)
                });
            }

            return ketQua;
        }

        private static Tuple<double, double> TimCapChuThichTuDim(
            DiemCot cot,
            List<DoanDim> doanDim,
            string tenLayer)
        {
            if (doanDim == null || doanDim.Count == 0)
                return null;

            double banKinh = UnitHelper.MmSangFeet(BanKinhGanDimMm);
            XYZ tam = new XYZ(cot.X, cot.Y, 0);

            var giaTri = doanDim
                .Where(d => d.Tam.DistanceTo(tam) <= banKinh)
                .Select(d => d.GiaTriMm)
                .ToList();

            if (giaTri.Count == 0)
                return null;

            foreach (Tuple<double, double> cap in LayThuTuCap(tenLayer))
            {
                bool coA = giaTri.Any(v => GanChuThich(v, cap.Item1));
                bool coB = giaTri.Any(v => GanChuThich(v, cap.Item2));

                if (coA && coB)
                    return cap;
            }

            return null;
        }

        private static IEnumerable<Tuple<double, double>> LayThuTuCap(
            string tenLayer)
        {
            if (LaLayerBienCot(tenLayer))
            {
                yield return CapCotTheoChuThiet[1];
                yield return CapCotTheoChuThiet[0];
            }
            else
            {
                yield return CapCotTheoChuThiet[0];
                yield return CapCotTheoChuThiet[1];
            }
        }

        private static Tuple<double, double> LayKichThuocDimTheoHuong(
            DiemCot cot,
            List<DoanDim> doanDim)
        {
            if (doanDim == null || doanDim.Count == 0)
                return null;

            double banKinh = UnitHelper.MmSangFeet(BanKinhGanDimMm);
            XYZ tam = new XYZ(cot.X, cot.Y, 0);

            DoanDim dimX = null;
            DoanDim dimY = null;
            double ganX = double.MaxValue;
            double ganY = double.MaxValue;

            foreach (DoanDim d in doanDim)
            {
                if (!LaKichThuocCotHopLe(d.GiaTriMm))
                    continue;

                double kc = d.Tam.DistanceTo(tam);
                if (kc > banKinh)
                    continue;

                if (d.TheoPhuongX && kc < ganX)
                {
                    ganX = kc;
                    dimX = d;
                }
                else if (!d.TheoPhuongX && kc < ganY)
                {
                    ganY = kc;
                    dimY = d;
                }
            }

            if (dimX == null || dimY == null)
                return null;

            return Tuple.Create(dimX.GiaTriMm, dimY.GiaTriMm);
        }

        private static Tuple<double, double> LayKichThuocTuLayerDimGan(
            DiemCot cot,
            IList<CadLine> duong)
        {
            if (duong == null)
                return null;

            double banKinh = UnitHelper.MmSangFeet(1200);
            XYZ tam = new XYZ(cot.X, cot.Y, 0);
            Tuple<double, double> ganNhat = null;
            double kcGanNhat = double.MaxValue;

            foreach (CadLine ln in duong)
            {
                if (!LaLayerDim(ln.TenLayer))
                    continue;

                var kt = LayKichThuocTuTen(ln.TenLayer);
                if (kt == null)
                    continue;

                double kc = KhoangCachDenDiem(ln, tam);
                if (kc > banKinh)
                    continue;

                if (kc < kcGanNhat)
                {
                    kcGanNhat = kc;
                    ganNhat = kt;
                }
            }

            return ganNhat;
        }

        private static bool LaKichThuocCotHopLe(double mm)
        {
            return mm >= CotDimMinMm && mm <= CotDimMaxMm;
        }

        private static bool GanChuThich(double giaTri, double chuThich)
        {
            return Math.Abs(giaTri - chuThich) <= TolCapChuThichMm;
        }

        private static void ApDungKichThuocDam(
            DuongDam dam,
            List<DoanDim> doanDim)
        {
            var tuTen = LayKichThuocTuTen(dam.TenLayer);
            if (tuTen != null)
            {
                dam.RongMm = tuTen.Item1;
                dam.CaoMm = tuTen.Item2;
                return;
            }

            if (doanDim == null || doanDim.Count == 0)
                return;

            XYZ tam = new XYZ(
                (dam.DiemDau.X + dam.DiemCuoi.X) * 0.5,
                (dam.DiemDau.Y + dam.DiemCuoi.Y) * 0.5,
                0);

            double banKinh = UnitHelper.MmSangFeet(BanKinhGanDimMm);
            bool damTheoX =
                Math.Abs(dam.DiemCuoi.X - dam.DiemDau.X) >=
                Math.Abs(dam.DiemCuoi.Y - dam.DiemDau.Y);

            foreach (DoanDim d in doanDim
                .Where(x => x.TheoPhuongX == damTheoX)
                .OrderBy(x => x.Tam.DistanceTo(tam)))
            {
                if (d.Tam.DistanceTo(tam) > banKinh)
                    continue;

                if (damTheoX)
                    dam.RongMm = d.GiaTriMm;
                else
                    dam.CaoMm = d.GiaTriMm;

                break;
            }
        }

        private static bool LaLayerDim(string tenLayer)
        {
            if (string.IsNullOrWhiteSpace(tenLayer))
                return false;

            string ten = tenLayer.ToUpperInvariant();
            return ten.Contains("DIM") ||
                ten.Contains("DIMENSION") ||
                ten.Contains("KICH THUOC") ||
                ten.Contains("KICHTHUOC") ||
                ten.Contains("KT ") ||
                ten.EndsWith("_KT") ||
                ten.Contains("ANNOTATION") ||
                ten.Contains("KTS");
        }

        private static double KhoangCachDenDiem(CadLine ln, XYZ p)
        {
            return KhoangCachDiemLenDuong(p, ln.DiemDau, ln.DiemCuoi);
        }

        private static double KhoangCachDiemLenDuong(XYZ p, XYZ a, XYZ b)
        {
            XYZ ab = b - a;
            double len = ab.GetLength();
            if (len < 1e-9)
                return p.DistanceTo(a);

            XYZ ap = p - a;
            double t = ap.DotProduct(ab) / (len * len);
            t = Math.Max(0, Math.Min(1, t));
            XYZ gan = a + t * ab;
            return p.DistanceTo(gan);
        }
    }
}
