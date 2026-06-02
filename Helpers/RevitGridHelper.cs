using Autodesk.Revit.DB;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cad2Revit.Helpers
{
    /// <summary>
    /// Đọc lưới trục Revit có sẵn và căn mô hình 3D gọn trong lưới (không tự chỉnh độ dài grid).
    /// </summary>
    public static partial class RevitGridHelper
    {
        private const double TolTrungMm = 80;

        public class LuoiRevitCoSan
        {
            public List<Grid> GridDoc { get; set; } = new List<Grid>();
            public List<double> TrucX { get; set; } = new List<double>();
            public List<double> TrucY { get; set; } = new List<double>();
            public double MinX { get; set; }
            public double MaxX { get; set; }
            public double MinY { get; set; }
            public double MaxY { get; set; }
        }

        public static LuoiRevitCoSan LayLuoiRevitCoSan(Document doc)
        {
            var ketQua = new LuoiRevitCoSan();
            if (doc == null)
                return ketQua;

            var grids = new FilteredElementCollector(doc)
                .OfClass(typeof(Grid))
                .Cast<Grid>()
                .Where(g =>
                    g.Name != null &&
                    g.Name.IndexOf(
                        "CAD2Revit",
                        StringComparison.OrdinalIgnoreCase) < 0)
                .ToList();

            ketQua.GridDoc = grids;

            var trucX = new List<double>();
            var trucY = new List<double>();

            foreach (Grid g in grids)
            {
                Curve c = g.Curve;
                if (!(c is Line line))
                    continue;

                XYZ p0 = line.GetEndPoint(0);
                XYZ p1 = line.GetEndPoint(1);

                if (Math.Abs(p0.X - p1.X) < UnitHelper.MmSangFeet(50))
                    trucX.Add((p0.X + p1.X) * 0.5);
                else if (Math.Abs(p0.Y - p1.Y) < UnitHelper.MmSangFeet(50))
                    trucY.Add((p0.Y + p1.Y) * 0.5);
            }

            ketQua.TrucX = GomTruc(trucX);
            ketQua.TrucY = GomTruc(trucY);

            if (ketQua.TrucX.Count > 0)
            {
                ketQua.MinX = ketQua.TrucX.First();
                ketQua.MaxX = ketQua.TrucX.Last();
            }

            if (ketQua.TrucY.Count > 0)
            {
                ketQua.MinY = ketQua.TrucY.First();
                ketQua.MaxY = ketQua.TrucY.Last();
            }

            return ketQua;
        }

        /// <summary>
        /// Căn tâm mô hình về gốc nội bộ và snap trục theo lưới Revit template.
        /// </summary>
        public static bool CanhChinhMoHinhVaoLuoiRevit(
            KetQuaPhanTich phanTich,
            KetQuaDocCAD ketQuaCad,
            Document doc)
        {
            if (phanTich?.LuoiTruc == null ||
                doc == null ||
                phanTich.LuoiTruc.TrucX.Count < 2 ||
                phanTich.LuoiTruc.TrucY.Count < 2)
            {
                return false;
            }

            LuoiRevitCoSan revit = LayLuoiRevitCoSan(doc);
            if (revit.TrucX.Count < 2 || revit.TrucY.Count < 2)
            {
                return false;
            }

            LuoiTrucKetCau luoi = phanTich.LuoiTruc;
            double tamModelX = (luoi.MinX + luoi.MaxX) * 0.5;
            double tamModelY = (luoi.MinY + luoi.MaxY) * 0.5;

            // Căn tâm mô hình về gốc nội bộ Revit (giữa khung N/S/E/W), không kéo về trục 1/A.
            XYZ delta = new XYZ(-tamModelX, -tamModelY, 0);
            double tolFeet = UnitHelper.MmSangFeet(TolTrungMm);

            if (delta.GetLength() >= tolFeet)
            {
                CadCoordinateHelper.DichPhanTich(phanTich, delta);
                CadCoordinateHelper.DichDuongCad(ketQuaCad, delta);
                luoi.DichVeGoc(new XYZ(tamModelX, tamModelY, 0));

                if (ketQuaCad != null)
                {
                    ketQuaCad.DoiTatX += tamModelX;
                    ketQuaCad.DoiTatY += tamModelY;
                }
            }

            MapSnapTrucVeRevit(luoi, revit);

            return true;
        }

        private static void MapSnapTrucVeRevit(
            LuoiTrucKetCau luoi,
            LuoiRevitCoSan revit)
        {
            luoi.TrucX = SnapDanhSachTruc(luoi.TrucX, revit.TrucX);
            luoi.TrucY = SnapDanhSachTruc(luoi.TrucY, revit.TrucY);

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

            CapNhatDuongVienTuLuoi(luoi);
        }

        private static List<double> SnapDanhSachTruc(
            List<double> model,
            List<double> revit)
        {
            if (model == null || revit == null || model.Count == 0)
                return model;

            var ketQua = new List<double>();
            double tol = UnitHelper.MmSangFeet(TolTrungMm);

            foreach (double v in model)
            {
                double gan = revit
                    .OrderBy(r => Math.Abs(r - v))
                    .First();

                if (Math.Abs(gan - v) <= tol)
                    ketQua.Add(gan);
                else
                    ketQua.Add(v);
            }

            return GomTruc(ketQua);
        }

        private static List<double> GomTruc(List<double> giaTri)
        {
            double tol = UnitHelper.MmSangFeet(TolTrungMm);
            var sap = giaTri.OrderBy(v => v).ToList();
            var ketQua = new List<double>();

            foreach (double v in sap)
            {
                if (ketQua.Count == 0 ||
                    Math.Abs(v - ketQua[ketQua.Count - 1]) > tol)
                {
                    ketQua.Add(v);
                }
            }

            return ketQua;
        }

        private static void CapNhatDuongVienTuLuoi(LuoiTrucKetCau luoi)
        {
            if (luoi == null ||
                luoi.TrucX.Count < 2 ||
                luoi.TrucY.Count < 2)
            {
                return;
            }

            // Floor boundary should stop at column edges (no extra margin)
            double m = UnitHelper.MmSangFeet(0);
            luoi.DuongVienSan = new List<XYZ>
            {
                new XYZ(luoi.MinX - m, luoi.MinY - m, 0),
                new XYZ(luoi.MaxX + m, luoi.MinY - m, 0),
                new XYZ(luoi.MaxX + m, luoi.MaxY + m, 0),
                new XYZ(luoi.MinX - m, luoi.MaxY + m, 0)
            };
        }

        public static void MoRongGridCoSan(
            Document doc,
            LuoiTrucKetCau luoi,
            IList<Level> levels,
            Level levelMai)
            {
                if (doc == null || luoi == null || luoi.TrucX == null || luoi.TrucY == null)
                    return;

                double m = UnitHelper.MmSangFeet(4500);

                double zDuoi = levels != null && levels.Count > 0
                    ? levels[0].Elevation
                    : 0;

                double zTren = levelMai != null
                    ? levelMai.Elevation
                    : (levels != null && levels.Count > 0 ? levels[levels.Count - 1].Elevation : zDuoi);

                var views = new FilteredElementCollector(doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .Where(v => !v.IsTemplate && !v.IsAssemblyView)
                    .ToList();

                var grids = new FilteredElementCollector(doc)
                    .OfClass(typeof(Grid))
                    .Cast<Grid>()
                    .Where(g => g.Name != null && g.Name.IndexOf("CAD2Revit", StringComparison.OrdinalIgnoreCase) < 0)
                    .ToList();

                var toDelete = new List<ElementId>();
                var toCreate = new List<Tuple<string, Line>>();

                foreach (Grid g in grids)
                {
                    try
                    {
                        Curve c = g.Curve;
                        if (!(c is Line line))
                            continue;

                        XYZ p0 = line.GetEndPoint(0);
                        XYZ p1 = line.GetEndPoint(1);

                        Line newLine = null;

                        if (Math.Abs(p0.X - p1.X) < UnitHelper.MmSangFeet(50))
                        {
                            double x = (p0.X + p1.X) * 0.5;
                            newLine = Line.CreateBound(
                                new XYZ(x, luoi.MinY - m, zDuoi),
                                new XYZ(x, luoi.MaxY + m, zDuoi));
                        }
                        else if (Math.Abs(p0.Y - p1.Y) < UnitHelper.MmSangFeet(50))
                        {
                            double y = (p0.Y + p1.Y) * 0.5;
                            newLine = Line.CreateBound(
                                new XYZ(luoi.MinX - m, y, zDuoi),
                                new XYZ(luoi.MaxX + m, y, zDuoi));
                        }

                        if (newLine == null)
                            continue;

                        toDelete.Add(g.Id);
                        toCreate.Add(Tuple.Create(g.Name ?? "", newLine));
                    }
                    catch { }
                }

                if (toDelete.Count == 0)
                    return;

                try
                {
                    doc.Delete(toDelete);
                }
                catch { }

                foreach (var t in toCreate)
                {
                    try
                    {
                        Grid gg = Grid.Create(doc, t.Item2);
                        if (gg != null)
                        {
                            try { gg.Name = t.Item1; } catch { gg.Name = t.Item1 + "-" + gg.Id.Value; }
                            try { gg.SetVerticalExtents(zDuoi, zTren); } catch { }

                            foreach (View view in views)
                            {
                                try
                                {
                                    gg.SetDatumExtentType(DatumEnds.End0, view, DatumExtentType.Model);
                                    gg.SetCurveInView(DatumExtentType.Model, view, t.Item2);
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                }
            }
    }
}