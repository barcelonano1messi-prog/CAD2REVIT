using Autodesk.Revit.DB;
using Cad2Revit.Helpers;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cad2Revit.Converter
{
    public class CadGeometryAnalyzer
    {
        public KetQuaPhanTich PhanTich(
            KetQuaDocCAD ketQuaCad,
            ConversionSettings caiDat)
        {
            if (caiDat == null)
                caiDat = new ConversionSettings();

            int soTang = caiDat.SoTang < 1 ? 1 : caiDat.SoTang;

            var ketQua = new KetQuaPhanTich
            {
                SoTang = soTang,
                ChieuCaoTangMm = caiDat.ChieuCaoTangDienHinhMm,
                BeDaySanMm = caiDat.BeDaySanMm
            };

            if (ketQuaCad == null ||
                ketQuaCad.DanhSachDuong == null ||
                ketQuaCad.DanhSachDuong.Count == 0)
            {
                GanMacDinh(ketQua);
                ketQua.TomTat = TaoTomTat(ketQua);
                return ketQua;
            }

            var duongTuong = LocTheoLoai(ketQuaCad, LoaiCauKien.Tuong);
            var duongCot = LocTheoLoai(ketQuaCad, LoaiCauKien.Cot);
            var duongDam = LocTheoLoai(ketQuaCad, LoaiCauKien.Dam);
            var duongSan = LocTheoLoai(ketQuaCad, LoaiCauKien.San);

            ketQua.BeDayTuongMm = TinhTuCapSongSong(
                duongTuong, 80, 500, 200);

            ketQua.DanhSachDiemCot =
                StructuralPointExtractor.TrichXuatCot(duongCot);

            ketQua.DanhSachDuongDam =
                StructuralPointExtractor.TrichXuatDam(duongDam);

            ketQua.DanhSachVungSan = FloorDuongVienBuilder.TaoVungSanTuDuong(
                duongSan,
                ketQua.BeDaySanMm);

            CadDimensionHelper.ApDungKichThuocTuBanVe(
                ketQua,
                ketQuaCad.DanhSachDuong);

            var kichThuocCot = ThongKeKichThuocCot(ketQua.DanhSachDiemCot);
            ketQua.CotRongMm = kichThuocCot.Item1;
            ketQua.CotSauMm = kichThuocCot.Item2;

            ApDungKichThuocDamNhapTay(ketQua, caiDat);

            GanBeDayLenDuong(duongTuong, ketQua.BeDayTuongMm);

            ketQua.TomTat = TaoTomTat(ketQua);
            return ketQua;
        }

        private static List<CadLine> LocTheoLoai(
            KetQuaDocCAD ketQua,
            LoaiCauKien loai)
        {
            return ketQua.DanhSachDuong
                .Where(d => d.Loai == loai)
                .ToList();
        }

        private static void GanMacDinh(KetQuaPhanTich k)
        {
            k.BeDayTuongMm = 200;
            k.CotRongMm = 300;
            k.CotSauMm = 300;
            k.DamRongMm = 200;
            k.DamCaoMm = 500;
            k.BeDaySanMm = 150;
        }

        private static string TaoTomTat(KetQuaPhanTich k)
        {
            int soCot = k.DanhSachDiemCot != null
                ? k.DanhSachDiemCot.Count
                : 0;

            int soDam = k.DanhSachDuongDam != null
                ? k.DanhSachDuongDam.Count
                : 0;

            string nhip = "";
            if (k.DanhSachDuongDam != null && k.DanhSachDuongDam.Count > 0)
            {
                var ds = k.DanhSachDuongDam
                    .Select(d => d.ChieuDaiNhipMm)
                    .OrderBy(x => x)
                    .ToList();

                nhip = string.Format(
                    " | Nhịp {0:F0}–{1:F0} mm",
                    ds.First(),
                    ds.Last());
            }

            string kichThuocCot = "";
            if (k.DanhSachDiemCot != null && k.DanhSachDiemCot.Count > 0)
            {
                var r = k.DanhSachDiemCot.Select(c => c.RongMm).OrderBy(x => x).ToList();
                var s = k.DanhSachDiemCot.Select(c => c.SauMm).OrderBy(x => x).ToList();
                kichThuocCot = string.Format(
                    " | Cột {0:F0}×{1:F0}–{2:F0}×{3:F0} mm",
                    r.First(),
                    s.First(),
                    r.Last(),
                    s.Last());
            }

            string nguonDam = k.DamRongMm > 0 && k.DamCaoMm > 0
                ? string.Format(
                    "Dầm {0:0.#}×{1:0.#} mm",
                    k.DamRongMm,
                    k.DamCaoMm)
                : "Dầm —";

            return string.Format(
                "Cột: {0} | Dầm: {1} nhịp | {2} | Sàn {3} mm{4}{5}",
                soCot,
                soDam,
                nguonDam,
                Math.Round(k.BeDaySanMm),
                nhip,
                kichThuocCot);
        }

        private static void ApDungKichThuocDamNhapTay(
            KetQuaPhanTich ketQua,
            ConversionSettings caiDat)
        {
            if (ketQua.DanhSachDuongDam == null)
                return;

            if (caiDat.UuTienKichThuocDamTuUi &&
                caiDat.DamRongMm > 0 &&
                caiDat.DamCaoMm > 0)
            {
                foreach (DuongDam d in ketQua.DanhSachDuongDam)
                {
                    d.RongMm = caiDat.DamRongMm;
                    d.CaoMm = caiDat.DamCaoMm;
                }

                ketQua.DamRongMm = caiDat.DamRongMm;
                ketQua.DamCaoMm = caiDat.DamCaoMm;
                return;
            }

            var kt = ThongKeKichThuocDam(ketQua.DanhSachDuongDam);
            ketQua.DamRongMm = kt.Item1;
            ketQua.DamCaoMm = kt.Item2;
        }

        private static Tuple<double, double> ThongKeKichThuocCot(
            List<DiemCot> ds)
        {
            if (ds == null || ds.Count == 0)
                return Tuple.Create(300.0, 300.0);

            return Tuple.Create(
                TrungVi(ds.Select(c => c.RongMm).ToList(), 300),
                TrungVi(ds.Select(c => c.SauMm).ToList(), 300));
        }

        private static Tuple<double, double> ThongKeKichThuocDam(
            List<DuongDam> ds)
        {
            if (ds == null || ds.Count == 0)
                return Tuple.Create(200.0, 500.0);

            return Tuple.Create(
                TrungVi(ds.Select(d => d.RongMm).ToList(), 200),
                TrungVi(ds.Select(d => d.CaoMm).ToList(), 500));
        }

        private static double TinhTuCapSongSong(
            List<CadLine> duong,
            double minMm,
            double maxMm,
            double macDinhMm)
        {
            var ds = TimKhoangCachSongSong(duong, minMm, maxMm);
            return TrungVi(ds, macDinhMm);
        }

        private static List<double> TimKhoangCachSongSong(
            List<CadLine> duong,
            double minMm,
            double maxMm)
        {
            var ketQua = new List<double>();

            for (int i = 0; i < duong.Count; i++)
            {
                for (int j = i + 1; j < duong.Count; j++)
                {
                    double? kc = KhoangCachGiuaHaiDuong(
                        duong[i], duong[j]);

                    if (kc.HasValue &&
                        kc.Value >= minMm &&
                        kc.Value <= maxMm)
                    {
                        ketQua.Add(kc.Value);
                    }
                }
            }

            return ketQua;
        }

        private static double? KhoangCachGiuaHaiDuong(
            CadLine a,
            CadLine b)
        {
            XYZ huongA = Huong(a);
            XYZ huongB = Huong(b);

            if (huongA == null || huongB == null)
                return null;

            double goc = Math.Abs(huongA.DotProduct(huongB));
            if (goc < 0.96)
                return null;

            XYZ p = a.DiemDau;
            XYZ q = b.DiemDau;
            XYZ ab = huongA.Normalize();

            double distFeet = Math.Abs(
                (q - p).CrossProduct(ab).Z) / ab.GetLength();

            return UnitHelper.FeetSangMm(distFeet);
        }

        private static void GanBeDayLenDuong(
            List<CadLine> duong,
            double beDayMm)
        {
            foreach (CadLine d in duong)
                d.BeDayMm = beDayMm;
        }

        private static XYZ Huong(CadLine d)
        {
            XYZ v = d.DiemCuoi - d.DiemDau;
            if (v.GetLength() < 1e-9)
                return null;
            return v.Normalize();
        }

        private static double TrungVi(List<double> ds, double macDinh)
        {
            if (ds == null || ds.Count == 0)
                return macDinh;

            var sapXep = ds.OrderBy(x => x).ToList();
            return sapXep[sapXep.Count / 2];
        }
    }
}
