using Autodesk.Revit.DB;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cad2Revit.Helpers
{
    public static class CadCoordinateHelper
    {
        public static Transform LayTransformImport(ImportInstance cadImport)
        {
            if (cadImport == null)
                return Transform.Identity;

            try
            {
                return cadImport.GetTotalTransform();
            }
            catch
            {
                return cadImport.GetTransform();
            }
        }

        private static XYZ CongDelta(XYZ p, XYZ delta)
        {
            if (p == null)
                return XYZ.Zero;

            return new XYZ(
                p.X + delta.X,
                p.Y + delta.Y,
                p.Z);
        }

        public static void GhiLogHeToaDo(
            KetQuaDocCAD ketQua,
            Transform transform)
        {
            if (ketQua == null || transform == null)
                return;

            XYZ goc = transform.OfPoint(XYZ.Zero);
            ketQua.GocX = goc.X;
            ketQua.GocY = goc.Y;

            if (ketQua.DanhSachDuong != null &&
                ketQua.DanhSachDuong.Count > 0)
            {
                double minX = double.MaxValue;
                double minY = double.MaxValue;
                double maxX = double.MinValue;
                double maxY = double.MinValue;

                foreach (CadLine ln in ketQua.DanhSachDuong)
                {
                    CapNhatBien(ln.DiemDau, ref minX, ref minY, ref maxX, ref maxY);
                    CapNhatBien(ln.DiemCuoi, ref minX, ref minY, ref maxX, ref maxY);
                }
            }
        }

        public static bool ChuanHoaVeGocKienTruc(
            KetQuaDocCAD ketQua,
            KetQuaPhanTich phanTich)
        {
            if (ketQua == null ||
                ketQua.DanhSachDuong == null ||
                ketQua.DanhSachDuong.Count == 0)
            {
                return false;
            }

            if (!TinhTamKienTruc(ketQua, phanTich, out double tamX, out double tamY))
                return false;

            XYZ delta = new XYZ(-tamX, -tamY, 0);

            foreach (CadLine ln in ketQua.DanhSachDuong)
            {
                ln.DiemDau = TruOffset(ln.DiemDau, tamX, tamY);
                ln.DiemCuoi = TruOffset(ln.DiemCuoi, tamX, tamY);
            }

            DichPhanTich(phanTich, delta);

            ketQua.DoiTatX += tamX;
            ketQua.DoiTatY += tamY;

            return true;
        }

        public static void DichPhanTich(KetQuaPhanTich phanTich, XYZ delta)
        {
            if (phanTich == null)
                return;

            if (phanTich.DanhSachDiemCot != null)
            {
                foreach (DiemCot c in phanTich.DanhSachDiemCot)
                {
                    c.X += delta.X;
                    c.Y += delta.Y;
                }
            }

            if (phanTich.DanhSachDuongDam != null)
            {
                foreach (DuongDam d in phanTich.DanhSachDuongDam)
                {
                    d.DiemDau = CongDelta(d.DiemDau, delta);
                    d.DiemCuoi = CongDelta(d.DiemCuoi, delta);
                }
            }

            if (phanTich.DanhSachVungSan != null)
            {
                foreach (VungSan v in phanTich.DanhSachVungSan)
                {
                    if (v.DuongVien == null)
                        continue;

                    for (int i = 0; i < v.DuongVien.Count; i++)
                        v.DuongVien[i] = CongDelta(v.DuongVien[i], delta);
                }
            }

            if (phanTich.DanhSachLoThung != null)
            {
                foreach (var loThung in phanTich.DanhSachLoThung)
                {
                    if (loThung == null)
                        continue;

                    for (int i = 0; i < loThung.Count; i++)
                        loThung[i] = CongDelta(loThung[i], delta);
                }
            }
        }

        public static void DichDuongCad(KetQuaDocCAD ketQuaCad, XYZ delta)
        {
            if (ketQuaCad?.DanhSachDuong == null)
                return;

            foreach (CadLine ln in ketQuaCad.DanhSachDuong)
            {
                ln.DiemDau = CongDelta(ln.DiemDau, delta);
                ln.DiemCuoi = CongDelta(ln.DiemCuoi, delta);
            }
        }

        private static bool TinhTamKienTruc(
            KetQuaDocCAD ketQua,
            KetQuaPhanTich phanTich,
            out double tamX,
            out double tamY)
        {
            tamX = 0;
            tamY = 0;

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            List<CadLine> duongKetCau = LayDuongKetCau(ketQua.DanhSachDuong);

            foreach (CadLine ln in duongKetCau)
            {
                CapNhatBien(ln.DiemDau, ref minX, ref minY, ref maxX, ref maxY);
                CapNhatBien(ln.DiemCuoi, ref minX, ref minY, ref maxX, ref maxY);
            }

            if (phanTich?.DanhSachDiemCot != null)
            {
                foreach (DiemCot c in phanTich.DanhSachDiemCot)
                {
                    minX = Math.Min(minX, c.X);
                    maxX = Math.Max(maxX, c.X);
                    minY = Math.Min(minY, c.Y);
                    maxY = Math.Max(maxY, c.Y);
                }
            }

            if (minX >= maxX || minY >= maxY)
                return false;

            tamX = (minX + maxX) * 0.5;
            tamY = (minY + maxY) * 0.5;
            return true;
        }

        public static void DichCadImportVeGoc(
            Document doc,
            KetQuaDocCAD ketQua)
        {
            if (doc == null ||
                ketQua == null ||
                ketQua.DaDichCadVeGoc ||
                ketQua.CadImportId == null ||
                ketQua.CadImportId == ElementId.InvalidElementId)
            {
                return;
            }

            if (Math.Abs(ketQua.DoiTatX) < 1e-6 &&
                Math.Abs(ketQua.DoiTatY) < 1e-6)
            {
                return;
            }

            Element cad = doc.GetElement(ketQua.CadImportId);
            if (cad == null)
                return;

            bool daGhim = cad.Pinned;
            if (daGhim)
            {
                try
                {
                    cad.Pinned = false;
                }
                catch (Exception ex)
                {
                    return;
                }
            }

            try
            {
                ElementTransformUtils.MoveElement(
                    doc,
                    ketQua.CadImportId,
                    new XYZ(-ketQua.DoiTatX, -ketQua.DoiTatY, 0));

                ketQua.DaDichCadVeGoc = true;
            }
            catch (Exception ex)
            {
                return;
            }
            finally
            {
                if (daGhim && cad != null && cad.IsValidObject)
                {
                    try
                    {
                        cad.Pinned = true;
                    }
                    catch
                    {
                        // bỏ qua — CAD đã dời, không ghim lại được
                    }
                }
            }
        }

        private static XYZ TruOffset(XYZ p, double offsetX, double offsetY)
        {
            if (p == null)
                return XYZ.Zero;

            return new XYZ(
                p.X - offsetX,
                p.Y - offsetY,
                p.Z);
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

        public static void DatCaBaGocTrungGiaoTruc(
            Document doc,
            XYZ gocKienTruc)
        {
            if (doc == null)
                return;

            if (gocKienTruc == null)
                gocKienTruc = XYZ.Zero;

            int so = 0;
            so += DatBasePointVeViTri(
                BasePoint.GetProjectBasePoint(doc),
                gocKienTruc,
                "Project Base Point") ? 1 : 0;

            so += DatBasePointVeViTri(
                BasePoint.GetSurveyPoint(doc),
                gocKienTruc,
                "Survey Point") ? 1 : 0;

        }

        private static bool DatBasePointVeViTri(
            BasePoint bp,
            XYZ dich,
            string ten)
        {
            if (bp == null)
                return false;

            try
            {
                XYZ delta = new XYZ(
                    dich.X - bp.Position.X,
                    dich.Y - bp.Position.Y,
                    0);

                if (delta.GetLength() < UnitHelper.MmSangFeet(20))
                    return false;

                bool ghim = bp.Pinned;
                if (ghim)
                    bp.Pinned = false;

                if (bp.IsShared)
                {
                    try
                    {
                        bp.Clipped = true;
                    }
                    catch
                    {
                        // bỏ qua
                    }
                }

                LocationPoint loc = bp.Location as LocationPoint;
                if (loc == null)
                    return false;

                loc.Move(delta);

                if (ghim)
                    bp.Pinned = true;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static List<CadLine> LayDuongKetCau(List<CadLine> tatCa)
        {
            if (tatCa == null || tatCa.Count == 0)
                return new List<CadLine>();

            var ketCau = tatCa
                .Where(d =>
                    d.Loai == LoaiCauKien.Cot ||
                    d.Loai == LoaiCauKien.Dam ||
                    d.Loai == LoaiCauKien.Tuong)
                .ToList();

            return ketCau.Count > 0 ? ketCau : tatCa;
        }
    }
}
