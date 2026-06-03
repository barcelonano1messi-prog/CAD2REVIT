using Autodesk.Revit.DB;
using Cad2Revit.Helpers;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cad2Revit.Converter
{
    public static class StructuralPointExtractor
    {
        private const double TolGocMm = 80;
        private const double KhoangDamKepMm = 350;
        private const double CotMinMm = 250;
        private const double CotMaxMm = 900;
        private const double DamMinMm = 400;

        private static readonly Regex RegexKichThuoc =
            new Regex(@"(\d{2,4})\s*[xX×]\s*(\d{2,4})",
                RegexOptions.Compiled);

        public static List<DiemCot> TrichXuatCot(List<CadLine> duongCot)
        {
            var ketQua = new List<DiemCot>();
            if (duongCot == null || duongCot.Count == 0)
                return ketQua;

            foreach (IGrouping<string, CadLine> nhomLayer in
                duongCot.GroupBy(d => d.TenLayer ?? ""))
            {
                var kichThuocLayer =
                    CadDimensionHelper.LayKichThuocTuTen(nhomLayer.Key) ??
                    CadDimensionHelper.MacDinhTheoLoaiLayer(nhomLayer.Key);

                double macRong = kichThuocLayer?.Item1 ?? 500;
                double macSau = kichThuocLayer?.Item2 ?? 600;

                var canh = nhomLayer
                    .Where(l =>
                    {
                        double dai = UnitHelper.FeetSangMm(l.ChieuDaiFeet());
                        return dai >= CotMinMm * 0.35 && dai <= CotMaxMm * 1.25;
                    })
                    .ToList();

                ketQua.AddRange(
                    TimCotTuHinhChuNhat(
                        canh,
                        nhomLayer.Key,
                        macRong,
                        macSau));

                ketQua.AddRange(
                    TimCotTuCumDiem(
                        canh,
                        nhomLayer.Key,
                        macRong,
                        macSau));
            }

            return LocTrungDiemCot(ketQua);
        }

        public static List<DuongDam> TrichXuatDam(List<CadLine> duongDam)
        {
            var doan = new List<DuongDam>();
            if (duongDam == null || duongDam.Count == 0)
                return doan;

            foreach (IGrouping<string, CadLine> nhomLayer in
                duongDam.GroupBy(d => d.TenLayer ?? ""))
            {
                var kt = LayKichThuocTuTenLayer(nhomLayer.Key);
                double rong = kt?.Item1 ?? 200;
                double cao = kt?.Item2 ?? 500;

                foreach (CadLine ln in nhomLayer)
                {
                    double dai = UnitHelper.FeetSangMm(ln.ChieuDaiFeet());
                    if (dai < DamMinMm)
                        continue;

                    doan.Add(new DuongDam
                    {
                        DiemDau = LamPhang(ln.DiemDau),
                        DiemCuoi = LamPhang(ln.DiemCuoi),
                        RongMm = rong,
                        CaoMm = cao,
                        TenLayer = nhomLayer.Key
                    });
                }
            }
            return LoaiBoDamSongSongTrung(LocTrungDam(doan));
        }

        private static List<DuongDam> LoaiBoDamSongSongTrung(List<DuongDam> ds)
        {
            if (ds.Count < 2)
                return ds;

            double tolFeet = UnitHelper.MmSangFeet(KhoangDamKepMm);
            var xoa = new bool[ds.Count];

            for (int i = 0; i < ds.Count; i++)
            {
                if (xoa[i])
                    continue;

                for (int j = i + 1; j < ds.Count; j++)
                {
                    if (xoa[j])
                        continue;

                    if (!CungHuongDam(
                        ds[i].DiemDau,
                        ds[i].DiemCuoi,
                        ds[j]))
                    {
                        continue;
                    }

                    double kc = KhoangCachHaiDuongSongSong(
                        ds[i],
                        ds[j]);

                    if (kc > tolFeet)
                        continue;

                    double tiLe = TyLeChongLenNhip(ds[i], ds[j]);
                    if (tiLe < 0.45)
                        continue;

                    if (ds[i].ChieuDaiNhipMm >= ds[j].ChieuDaiNhipMm)
                        xoa[j] = true;
                    else
                        xoa[i] = true;
                }
            }

            var ketQua = new List<DuongDam>();
            for (int k = 0; k < ds.Count; k++)
            {
                if (!xoa[k])
                    ketQua.Add(ds[k]);
            }

            return ketQua;
        }

        private static double KhoangCachHaiDuongSongSong(
            DuongDam a,
            DuongDam b)
        {
            return Math.Min(
                KhoangCachDiemLenDuong(
                    b.DiemDau,
                    a.DiemDau,
                    a.DiemCuoi),
                KhoangCachDiemLenDuong(
                    b.DiemCuoi,
                    a.DiemDau,
                    a.DiemCuoi));
        }

        private static double TyLeChongLenNhip(DuongDam a, DuongDam b)
        {
            XYZ huong = Huong(a.DiemDau, a.DiemCuoi);
            if (huong == null)
                return 0;

            double tA1 = (a.DiemCuoi - a.DiemDau).DotProduct(huong);
            double tB1 = (b.DiemDau - a.DiemDau).DotProduct(huong);
            double tB2 = (b.DiemCuoi - a.DiemDau).DotProduct(huong);

            double bMin = Math.Min(tB1, tB2);
            double bMax = Math.Max(tB1, tB2);
            double overlap = Math.Min(tA1, bMax) - Math.Max(0, bMin);

            if (overlap <= 0)
                return 0;

            double daiNgan = Math.Min(
                a.ChieuDaiNhipMm,
                b.ChieuDaiNhipMm);

            if (daiNgan < 1)
                return 0;

            return UnitHelper.FeetSangMm(overlap) / daiNgan;
        }

        private static List<DuongDam> LocTrungDam(List<DuongDam> ds)
        {
            var ketQua = new List<DuongDam>();
            var daCo = new HashSet<string>();

            foreach (DuongDam d in ds.OrderByDescending(x => x.ChieuDaiNhipMm))
            {
                string key = MaDam(d);
                if (daCo.Contains(key))
                    continue;

                daCo.Add(key);
                ketQua.Add(d);
            }

            return ketQua;
        }

        private static double KhoangCachDiemLenDuong(
            XYZ p,
            XYZ a,
            XYZ b)
        {
            XYZ ab = b - a;
            double len = ab.GetLength();
            if (len < 1e-9)
                return p.DistanceTo(a);

            XYZ ap = p - a;
            double t = ap.DotProduct(ab) / (len * len);
            t = Math.Max(0, Math.Min(1, t));
            XYZ ganNhat = a + t * ab;
            return p.DistanceTo(ganNhat);
        }

        private static bool CungHuongDam(XYZ dau, XYZ cuoi, DuongDam seg)
        {
            XYZ v1 = Huong(dau, cuoi);
            XYZ v2 = Huong(seg.DiemDau, seg.DiemCuoi);
            if (v1 == null || v2 == null)
                return false;

            return Math.Abs(v1.DotProduct(v2)) > 0.995;
        }

        private static XYZ Huong(XYZ a, XYZ b)
        {
            XYZ v = b - a;
            if (v.GetLength() < 1e-9)
                return null;
            return v.Normalize();
        }

        private static List<DiemCot> TimCotTuHinhChuNhat(
            List<CadLine> canh,
            string layer,
            double macRong,
            double macSau)
        {
            var ketQua = new List<DiemCot>();
            var daDung = new HashSet<int>();
            double tolFeet = UnitHelper.MmSangFeet(TolGocMm);

            for (int i = 0; i < canh.Count; i++)
            {
                if (daDung.Contains(i))
                    continue;

                var cum = new List<int> { i };
                daDung.Add(i);
                var hangDoi = new Queue<int>();
                hangDoi.Enqueue(i);

                while (hangDoi.Count > 0)
                {
                    int idx = hangDoi.Dequeue();
                    CadLine a = canh[idx];

                    for (int j = 0; j < canh.Count; j++)
                    {
                        if (daDung.Contains(j))
                            continue;

                        if (ChiaGocCot(a, canh[j], tolFeet))
                        {
                            daDung.Add(j);
                            cum.Add(j);
                            hangDoi.Enqueue(j);
                        }
                    }
                }

                if (cum.Count < 3)
                    continue;

                var diem = new List<XYZ>();
                foreach (int k in cum)
                {
                    diem.Add(LamPhang(canh[k].DiemDau));
                    diem.Add(LamPhang(canh[k].DiemCuoi));
                }

                double minX = diem.Min(p => p.X);
                double maxX = diem.Max(p => p.X);
                double minY = diem.Min(p => p.Y);
                double maxY = diem.Max(p => p.Y);

                double wMm = UnitHelper.FeetSangMm(maxX - minX);
                double hMm = UnitHelper.FeetSangMm(maxY - minY);

                if (wMm < CotMinMm * 0.45 || hMm < CotMinMm * 0.45)
                    continue;

                if (wMm > CotMaxMm * 2.2 || hMm > CotMaxMm * 2.2)
                    continue;

                double rong = Math.Min(wMm, hMm);
                double sau = Math.Max(wMm, hMm);

                var kt = CadDimensionHelper.LayKichThuocTuTen(layer) ??
                    CadDimensionHelper.MacDinhTheoLoaiLayer(layer);

                ketQua.Add(new DiemCot
                {
                    X = (minX + maxX) / 2,
                    Y = (minY + maxY) / 2,
                    RongMm = kt?.Item1 ?? macRong,
                    SauMm = kt?.Item2 ?? macSau,
                    TenLayer = layer
                });
            }

            return ketQua;
        }

        private static List<DiemCot> TimCotTuCumDiem(
            List<CadLine> canh,
            string layer,
            double macRong,
            double macSau)
        {
            var ketQua = new List<DiemCot>();
            if (canh.Count == 0)
                return ketQua;

            var diem = new List<XYZ>();
            foreach (CadLine ln in canh)
            {
                diem.Add(LamPhang(ln.DiemDau));
                diem.Add(LamPhang(ln.DiemCuoi));
            }

            double epsFeet = UnitHelper.MmSangFeet(60);
            var cum = new List<List<XYZ>>();

            foreach (XYZ p in diem)
            {
                List<XYZ> nhom = cum.FirstOrDefault(c =>
                    c.Any(q => q.DistanceTo(p) < epsFeet));

                if (nhom == null)
                    cum.Add(new List<XYZ> { p });
                else if (!nhom.Any(q => q.DistanceTo(p) < epsFeet / 2))
                    nhom.Add(p);
            }

            foreach (List<XYZ> nhom in cum)
            {
                if (nhom.Count < 2)
                    continue;

                double minX = nhom.Min(q => q.X);
                double maxX = nhom.Max(q => q.X);
                double minY = nhom.Min(q => q.Y);
                double maxY = nhom.Max(q => q.Y);

                double wMm = UnitHelper.FeetSangMm(maxX - minX);
                double hMm = UnitHelper.FeetSangMm(maxY - minY);

                if (wMm < CotMinMm * 0.35 && hMm < CotMinMm * 0.35)
                {
                    ketQua.Add(new DiemCot
                    {
                        X = nhom.Average(q => q.X),
                        Y = nhom.Average(q => q.Y),
                        RongMm = macRong,
                        SauMm = macSau,
                        TenLayer = layer
                    });
                    continue;
                }

                if (wMm > CotMaxMm * 2.5 || hMm > CotMaxMm * 2.5)
                    continue;

                var kt = CadDimensionHelper.LayKichThuocTuTen(layer) ??
                    CadDimensionHelper.MacDinhTheoLoaiLayer(layer);

                ketQua.Add(new DiemCot
                {
                    X = (minX + maxX) / 2,
                    Y = (minY + maxY) / 2,
                    RongMm = kt?.Item1 ?? macRong,
                    SauMm = kt?.Item2 ?? macSau,
                    TenLayer = layer
                });
            }

            return ketQua;
        }

        private static double TrungVi(List<double> ds, double macDinh)
        {
            if (ds == null || ds.Count == 0)
                return macDinh;

            var sapXep = ds.OrderBy(x => x).ToList();
            return sapXep[sapXep.Count / 2];
        }

        private static List<DiemCot> LocTrungDiemCot(List<DiemCot> ds)
        {
            var ketQua = new List<DiemCot>();
            double tolFeet = UnitHelper.MmSangFeet(30);

            foreach (DiemCot dc in ds.OrderByDescending(c => c.RongMm * c.SauMm))
            {
                bool trung = ketQua.Any(k =>
                    Math.Abs(k.X - dc.X) < tolFeet &&
                    Math.Abs(k.Y - dc.Y) < tolFeet);

                if (!trung)
                    ketQua.Add(dc);
            }

            return ketQua;
        }

        private static bool ChiaGocCot(CadLine a, CadLine b, double tolFeet)
        {
            return GanDiem(a.DiemDau, b.DiemDau, tolFeet) ||
                   GanDiem(a.DiemDau, b.DiemCuoi, tolFeet) ||
                   GanDiem(a.DiemCuoi, b.DiemDau, tolFeet) ||
                   GanDiem(a.DiemCuoi, b.DiemCuoi, tolFeet);
        }

        private static bool GanDiem(XYZ p, XYZ q, double tolFeet)
        {
            return LamPhang(p).DistanceTo(LamPhang(q)) < tolFeet;
        }

        private static Tuple<double, double> LayKichThuocTuTenLayer(string tenLayer)
        {
            if (string.IsNullOrWhiteSpace(tenLayer))
                return null;

            Match m = RegexKichThuoc.Match(tenLayer);
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

        private static string MaDam(DuongDam d)
        {
            XYZ p1 = d.DiemDau;
            XYZ p2 = d.DiemCuoi;
            double g = UnitHelper.MmSangFeet(25);

            long x1 = (long)Math.Round(p1.X / g);
            long y1 = (long)Math.Round(p1.Y / g);
            long x2 = (long)Math.Round(p2.X / g);
            long y2 = (long)Math.Round(p2.Y / g);

            if (x1 > x2 || (x1 == x2 && y1 > y2))
            {
                long t = x1; x1 = x2; x2 = t;
                t = y1; y1 = y2; y2 = t;
            }

            return x1 + "," + y1 + "," + x2 + "," + y2;
        }

        private static XYZ LamPhang(XYZ p)
        {
            return new XYZ(p.X, p.Y, 0);
        }
    }
}
