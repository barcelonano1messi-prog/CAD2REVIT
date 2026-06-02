using Autodesk.Revit.DB;
using Cad2Revit.Helpers;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cad2Revit.Converter
{
    public class CadReader
    {
        private Document _doc;
        public List<string> DanhSachLayerTimThay =
            new List<string>();

        public CadReader(Document doc)
        {
            _doc = doc;
        }

        public KetQuaDocCAD DocCadTuImport()
        {
            KetQuaDocCAD ketQua = new KetQuaDocCAD();

            ImportInstance cadImport = TimCadImport();

            if (cadImport == null)
            {
                ketQua.ThanhCong = false;
                ketQua.ThongBaoLoi =
                    "Không tìm thấy file CAD!\n" +
                    "Hãy Insert CAD trước.";

                return ketQua;
            }

            Options tuyChon = new Options();
            tuyChon.ComputeReferences = false;

            GeometryElement geoElement =
                cadImport.get_Geometry(tuyChon);

            if (geoElement == null)
            {
                ketQua.ThanhCong = false;
                ketQua.ThongBaoLoi =
                    "Không đọc được geometry CAD.";

                return ketQua;
            }

            Transform transformCad =
                CadCoordinateHelper.LayTransformImport(cadImport);

            DocGeometry(
                geoElement,
                ketQua.DanhSachDuong,
                transformCad);

            ketQua.CadImportId = cadImport.Id;

            CadCoordinateHelper.GhiLogHeToaDo(
                ketQua,
                transformCad);

            ketQua.ThanhCong = true;

            return ketQua;
        }

        private ImportInstance TimCadImport()
        {
            var imports = new FilteredElementCollector(_doc)
                .OfClass(typeof(ImportInstance))
                .WhereElementIsNotElementType()
                .Cast<ImportInstance>()
                .OrderByDescending(i => i.Id.IntegerValue)
                .ToList();

            if (imports.Count == 0)
                return null;

            ImportInstance newest = imports.First();
            if (imports.Count > 1)
            {
                List<ElementId> oldIds = imports
                    .Skip(1)
                    .Select(i => i.Id)
                    .ToList();

                try
                {
                    using (Transaction tx = new Transaction(_doc, "Xóa file CAD cũ"))
                    {
                        tx.Start();
                        _doc.Delete(oldIds);
                        tx.Commit();
                    }
                }
                catch
                {
                    // Nếu xóa CAD cũ thất bại thì vẫn tiếp tục dùng CAD mới nhất.
                }
            }

            return newest;
        }

        /// <summary>
        /// Đọc geometry CAD (có xử lý transform)
        /// </summary>
        private void DocGeometry(
            GeometryElement geoElement,
            List<CadLine> danhSach,
            Transform transformHienTai)
        {
            foreach (GeometryObject doiTuong in geoElement)
            {
                // CAD block / nested geometry
                if (doiTuong is GeometryInstance geoInstance)
                {
                    Transform transformMoi =
                        transformHienTai.Multiply(geoInstance.Transform);

                    GeometryElement geoSym =
                        geoInstance.GetSymbolGeometry();

                    if (geoSym != null)
                    {
                        DocGeometry(
                            geoSym,
                            danhSach,
                            transformMoi);
                    }
                }

                // LINE
                else if (doiTuong is Line line)
                {
                    XuLyLine(
                        line,
                        danhSach,
                        transformHienTai);
                }

                // POLYLINE
                else if (doiTuong is PolyLine polyLine)
                {
                    XuLyPolyLine(
                        polyLine,
                        danhSach,
                        transformHienTai);
                }

                // ARC
                else if (doiTuong is Arc arc)
                {
                    XuLyArc(
                        arc,
                        danhSach,
                        transformHienTai);
                }
            }
        }

        private void XuLyLine(
            Line line,
            List<CadLine> danhSach,
            Transform transform)
        {
            try
            {
                string tenLayer = LayTenLayer(line);

                if (!DanhSachLayerTimThay.Contains(tenLayer))
                    DanhSachLayerTimThay.Add(tenLayer);

                LoaiCauKien loai =
                    LayerMapper.XacDinhLoai(tenLayer);

                XYZ diemDau =
                    transform.OfPoint(
                        line.GetEndPoint(0));

                XYZ diemCuoi =
                    transform.OfPoint(
                        line.GetEndPoint(1));

                CadLine cadLine = new CadLine();
                cadLine.DiemDau = diemDau;
                cadLine.DiemCuoi = diemCuoi;
                cadLine.TenLayer = tenLayer;
                cadLine.Loai = loai;

                danhSach.Add(cadLine);
            }
            catch (Exception ex)
            {
            }
        }

        private void XuLyPolyLine(
            PolyLine polyLine,
            List<CadLine> danhSach,
            Transform transform)
        {
            try
            {
                IList<XYZ> dsDiem =
                    polyLine.GetCoordinates();

                if (dsDiem == null ||
                    dsDiem.Count < 2)
                    return;

                string tenLayer =
                    LayTenLayer(polyLine);

                if (!DanhSachLayerTimThay.Contains(tenLayer))
                    DanhSachLayerTimThay.Add(tenLayer);

                LoaiCauKien loai =
                    LayerMapper.XacDinhLoai(tenLayer);

                for (int i = 0; i < dsDiem.Count - 1; i++)
                {
                    XYZ diemDau =
                        transform.OfPoint(dsDiem[i]);

                    XYZ diemCuoi =
                        transform.OfPoint(dsDiem[i + 1]);

                    if (diemDau.IsAlmostEqualTo(diemCuoi))
                        continue;

                    CadLine cadLine = new CadLine();
                    cadLine.DiemDau = diemDau;
                    cadLine.DiemCuoi = diemCuoi;
                    cadLine.TenLayer = tenLayer;
                    cadLine.Loai = loai;

                    danhSach.Add(cadLine);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void XuLyArc(
            Arc arc,
            List<CadLine> danhSach,
            Transform transform)
        {
            try
            {
                string tenLayer = LayTenLayer(arc);

                if (!DanhSachLayerTimThay.Contains(tenLayer))
                    DanhSachLayerTimThay.Add(tenLayer);

                LoaiCauKien loai =
                    LayerMapper.XacDinhLoai(tenLayer);

                XYZ diemDau =
                    transform.OfPoint(
                        arc.GetEndPoint(0));

                XYZ diemCuoi =
                    transform.OfPoint(
                        arc.GetEndPoint(1));

                CadLine cadLine = new CadLine();
                cadLine.DiemDau = diemDau;
                cadLine.DiemCuoi = diemCuoi;
                cadLine.TenLayer = tenLayer;
                cadLine.Loai = loai;

                danhSach.Add(cadLine);
            }
            catch (Exception ex)
            {
            }
        }

        private string LayTenLayer(GeometryObject geoObj)
        {
            try
            {
                ElementId styleId =
                    geoObj.GraphicsStyleId;

                if (styleId == ElementId.InvalidElementId)
                    return "UNKNOWN";

                GraphicsStyle graphicsStyle =
                    _doc.GetElement(styleId)
                    as GraphicsStyle;

                if (graphicsStyle == null)
                    return "UNKNOWN";

                Category category =
                    graphicsStyle.GraphicsStyleCategory;

                if (category == null)
                    return "UNKNOWN";

                return category.Name;
            }
            catch
            {
                return "UNKNOWN";
            }
        }
    }
}