using Autodesk.Revit.DB;
using Cad2Revit.Helpers;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cad2Revit.Converter
{
    /// <summary>
    /// Ghép các đường layer sàn thành vùng khép kín — tránh tạo hàng trăm sàn chồng lấn.
    /// </summary>
    public static class FloorDuongVienBuilder
    {
        private const double TolMm = 100;
        private const double DienTichToiThieuMm2 = 2_000_000;

        public static List<VungSan> TaoVungSanTuDuong(
            List<CadLine> duongSan,
            double beDayMm)
        {
            var ketQua = new List<VungSan>();
            if (duongSan == null || duongSan.Count == 0)
                return ketQua;

            double tolFeet = UnitHelper.MmSangFeet(TolMm);
            var canh = new List<Canh>();

            foreach (CadLine d in duongSan)
            {
                if (UnitHelper.FeetSangMm(d.ChieuDaiFeet()) < 200)
                    continue;

                canh.Add(new Canh
                {
                    Dau = LamPhang(d.DiemDau),
                    Cuoi = LamPhang(d.DiemCuoi),
                    Layer = d.TenLayer
                });
            }

            var daDung = new HashSet<int>();
            var DuongVienDaCo = new List<List<XYZ>>();

            for (int i = 0; i < canh.Count; i++)
            {
                if (daDung.Contains(i))
                    continue;

                List<XYZ> DuongVien = DuyetDuongVien(canh, daDung, i, tolFeet);

                if (DuongVien == null || DuongVien.Count < 3)
                    continue;

                if (DuongVienTrungLap(DuongVien, DuongVienDaCo, tolFeet))
                    continue;

                double dienTichMm2 = DienTichMm2(DuongVien);
                if (dienTichMm2 < DienTichToiThieuMm2)
                    continue;

                DuongVienDaCo.Add(DuongVien);
                ketQua.Add(new VungSan
                {
                    DuongVien = DuongVien,
                    BeDayMm = beDayMm,
                    TenLayer = duongSan[0].TenLayer
                });
            }

            if (ketQua.Count == 0)
            {
                List<XYZ> bao = TaoBaoBoc(duongSan);
                if (bao != null && DienTichMm2(bao) >= DienTichToiThieuMm2)
                {
                    ketQua.Add(new VungSan
                    {
                        DuongVien = bao,
                        BeDayMm = beDayMm,
                        TenLayer = duongSan[0].TenLayer
                    });
                }
            }

            return LocVungKhongChong(ketQua);
        }

        private static List<VungSan> LocVungKhongChong(List<VungSan> vungs)
        {
            return vungs
                .OrderByDescending(v => DienTichMm2(v.DuongVien))
                .Where(v =>
                {
                    double s = DienTichMm2(v.DuongVien);
                    return vungs.Count(other =>
                        !ReferenceEquals(other, v) &&
                        DienTichMm2(other.DuongVien) > s * 1.05 &&
                        DuongVienTrungLap(v.DuongVien, new List<List<XYZ>> { other.DuongVien },
                            UnitHelper.MmSangFeet(TolMm))) == 0;
                })
                .ToList();
        }

        private static List<XYZ> DuyetDuongVien(
            List<Canh> canh,
            HashSet<int> daDung,
            int seedIndex,
            double tolFeet)
        {
            Canh seed = canh[seedIndex];
            daDung.Add(seedIndex);

            var DuongVien = new List<XYZ> { seed.Dau };
            XYZ hienTai = seed.Cuoi;

            if (!GanDiem(seed.Dau, seed.Cuoi, tolFeet))
                DuongVien.Add(hienTai);

            int anToan = canh.Count * 2;
            int dem = 0;

            while (dem++ < anToan)
            {
                int tiepIndex = -1;
                bool daoHuong = false;

                for (int j = 0; j < canh.Count; j++)
                {
                    if (daDung.Contains(j))
                        continue;

                    Canh c = canh[j];

                    if (GanDiem(c.Dau, hienTai, tolFeet))
                    {
                        tiepIndex = j;
                        daoHuong = false;
                        break;
                    }

                    if (GanDiem(c.Cuoi, hienTai, tolFeet))
                    {
                        tiepIndex = j;
                        daoHuong = true;
                        break;
                    }
                }

                if (tiepIndex < 0)
                    break;

                daDung.Add(tiepIndex);
                Canh tiep = canh[tiepIndex];
                XYZ diemMoi = daoHuong ? tiep.Dau : tiep.Cuoi;

                if (GanDiem(diemMoi, DuongVien[0], tolFeet))
                    break;

                if (!GanDiem(diemMoi, hienTai, tolFeet))
                    DuongVien.Add(diemMoi);

                hienTai = diemMoi;
            }

            return DuongVien.Count >= 3 ? DuongVien : null;
        }

        private static List<XYZ> TaoBaoBoc(List<CadLine> duongSan)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (CadLine d in duongSan)
            {
                CapNhatBien(d.DiemDau, ref minX, ref minY, ref maxX, ref maxY);
                CapNhatBien(d.DiemCuoi, ref minX, ref minY, ref maxX, ref maxY);
            }

            if (minX >= maxX || minY >= maxY)
                return null;

            return new List<XYZ>
            {
                new XYZ(minX, minY, 0),
                new XYZ(maxX, minY, 0),
                new XYZ(maxX, maxY, 0),
                new XYZ(minX, maxY, 0)
            };
        }

        private static void CapNhatBien(
            XYZ p,
            ref double minX,
            ref double minY,
            ref double maxX,
            ref double maxY)
        {
            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
        }

        private static bool DuongVienTrungLap(
            List<XYZ> DuongVien,
            List<List<XYZ>> daCo,
            double tolFeet)
        {
            foreach (List<XYZ> cu in daCo)
            {
                if (DuongVien.Count != cu.Count)
                    continue;

                bool trung = true;
                for (int i = 0; i < DuongVien.Count; i++)
                {
                    if (!GanDiem(DuongVien[i], cu[i], tolFeet))
                    {
                        trung = false;
                        break;
                    }
                }

                if (trung)
                    return true;
            }

            return false;
        }

        private static double DienTichMm2(List<XYZ> pts)
        {
            double s = 0;
            int n = pts.Count;
            for (int i = 0; i < n; i++)
            {
                XYZ p = pts[i];
                XYZ q = pts[(i + 1) % n];
                s += p.X * q.Y - q.X * p.Y;
            }

            double sFeet2 = Math.Abs(s) * 0.5;
            double ftToMm = UnitHelper.FeetSangMm(1);
            return sFeet2 * ftToMm * ftToMm;
        }

        private static XYZ LamPhang(XYZ p)
        {
            return new XYZ(p.X, p.Y, 0);
        }

        private static bool GanDiem(XYZ a, XYZ b, double tolFeet)
        {
            return a.DistanceTo(b) < tolFeet;
        }

        private class Canh
        {
            public XYZ Dau { get; set; }
            public XYZ Cuoi { get; set; }
            public string Layer { get; set; }
        }
    }
}
