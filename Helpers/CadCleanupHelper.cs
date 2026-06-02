using Autodesk.Revit.DB;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cad2Revit.Helpers
{
    public static class CadCleanupHelper
    {
        public static int XoaTatCaGrid(Document doc)
        {
            if (doc == null)
                return 0;

            var ids = new List<ElementId>();
            foreach (Grid g in new FilteredElementCollector(doc)
                .OfClass(typeof(Grid))
                .Cast<Grid>())
            {
                ids.Add(g.Id);
            }

            if (ids.Count == 0)
                return 0;

            try
            {
                doc.Delete(ids);
                return ids.Count;
            }
            catch (Exception)
            {
                return 0;
            }
        }


        public static int XoaKetQuaLanTruoc(
            Document doc,
            LuoiTrucKetCau luoi,
            List<CadLine> danhSachDuong = null)
        {
            if (doc == null)
                return 0;

            var ids = new List<ElementId>();
            ids.AddRange(LayIdGridCad2Revit(doc));
            ids.AddRange(LayIdLevelCad2Revit(doc));

            if (luoi != null)
            {
                AddRegionIds(doc, ids, luoi.MinX, luoi.MaxX, luoi.MinY, luoi.MaxY);
            }

            if (danhSachDuong != null && danhSachDuong.Count > 0)
            {
                if (TinhBienTuCad(danhSachDuong,
                    out double minX,
                    out double maxX,
                    out double minY,
                    out double maxY))
                {
                    AddRegionIds(doc, ids, minX, maxX, minY, maxY);
                }
            }

            if (TinhBienTuGridCad2Revit(doc,
                out double gridMinX,
                out double gridMaxX,
                out double gridMinY,
                out double gridMaxY))
            {
                AddRegionIds(doc, ids, gridMinX, gridMaxX, gridMinY, gridMaxY);
            }

            ids = ids.Distinct().ToList();
            if (ids.Count == 0)
                return 0;

            try
            {
                doc.Delete(ids);
                return ids.Count;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static void AddRegionIds(
            Document doc,
            List<ElementId> ids,
            double minX,
            double maxX,
            double minY,
            double maxY)
        {
            double pad = UnitHelper.MmSangFeet(2000);
            minX -= pad;
            maxX += pad;
            minY -= pad;
            maxY += pad;

            ids.AddRange(LayIdTrongVung(
                doc,
                BuiltInCategory.OST_StructuralColumns,
                minX,
                maxX,
                minY,
                maxY));
            ids.AddRange(LayIdTrongVung(
                doc,
                BuiltInCategory.OST_Columns,
                minX,
                maxX,
                minY,
                maxY));
            ids.AddRange(LayIdTrongVung(
                doc,
                BuiltInCategory.OST_StructuralFraming,
                minX,
                maxX,
                minY,
                maxY));
            ids.AddRange(LayIdTrongVung(
                doc,
                BuiltInCategory.OST_Floors,
                minX,
                maxX,
                minY,
                maxY));
            ids.AddRange(LayIdTrongVung(
                doc,
                BuiltInCategory.OST_Walls,
                minX,
                maxX,
                minY,
                maxY));
        }

        private static bool TinhBienTuCad(
            List<CadLine> lines,
            out double minX,
            out double maxX,
            out double minY,
            out double maxY)
        {
            minX = double.MaxValue;
            minY = double.MaxValue;
            maxX = double.MinValue;
            maxY = double.MinValue;

            foreach (CadLine ln in lines)
            {
                if (ln?.DiemDau != null)
                {
                    minX = Math.Min(minX, ln.DiemDau.X);
                    minY = Math.Min(minY, ln.DiemDau.Y);
                    maxX = Math.Max(maxX, ln.DiemDau.X);
                    maxY = Math.Max(maxY, ln.DiemDau.Y);
                }

                if (ln?.DiemCuoi != null)
                {
                    minX = Math.Min(minX, ln.DiemCuoi.X);
                    minY = Math.Min(minY, ln.DiemCuoi.Y);
                    maxX = Math.Max(maxX, ln.DiemCuoi.X);
                    maxY = Math.Max(maxY, ln.DiemCuoi.Y);
                }
            }

            return minX < maxX && minY < maxY;
        }

        private static bool TinhBienTuGridCad2Revit(
            Document doc,
            out double minX,
            out double maxX,
            out double minY,
            out double maxY)
        {
            minX = double.MaxValue;
            minY = double.MaxValue;
            maxX = double.MinValue;
            maxY = double.MinValue;
            bool found = false;

            foreach (Grid g in new FilteredElementCollector(doc)
                .OfClass(typeof(Grid))
                .Cast<Grid>())
            {
                if (g.Name == null ||
                    g.Name.IndexOf("CAD2Revit", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                Curve curve = g.Curve;
                if (curve == null)
                    continue;

                IEnumerable<XYZ> points = curve.Tessellate();
                foreach (XYZ p in points)
                {
                    minX = Math.Min(minX, p.X);
                    minY = Math.Min(minY, p.Y);
                    maxX = Math.Max(maxX, p.X);
                    maxY = Math.Max(maxY, p.Y);
                    found = true;
                }
            }

            return found && minX < maxX && minY < maxY;
        }

        private static IEnumerable<ElementId> LayIdGridCad2Revit(Document doc)
        {
            var ketQua = new List<ElementId>();
            foreach (Grid g in new FilteredElementCollector(doc)
                .OfClass(typeof(Grid))
                .Cast<Grid>())
            {
                if (g.Name != null &&
                    g.Name.IndexOf("CAD2Revit", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ketQua.Add(g.Id);
                }
            }

            return ketQua;
        }

        private static IEnumerable<ElementId> LayIdLevelCad2Revit(Document doc)
        {
            var ketQua = new List<ElementId>();
            foreach (Level l in new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>())
            {
                if (l.Name != null &&
                    l.Name.StartsWith("CAD2Revit", StringComparison.OrdinalIgnoreCase))
                {
                    ketQua.Add(l.Id);
                }
            }

            return ketQua;
        }

        private static IEnumerable<ElementId> LayTatCaCauKienTheoCategory(
            Document doc,
            BuiltInCategory cat)
        {
            var ketQua = new List<ElementId>();

            foreach (Element el in new FilteredElementCollector(doc)
                .OfCategory(cat)
                .WhereElementIsNotElementType())
            {
                ketQua.Add(el.Id);
            }

            return ketQua;
        }

        private static IEnumerable<ElementId> LayIdTrongVung(
            Document doc,
            BuiltInCategory cat,
            double minX,
            double maxX,
            double minY,
            double maxY)
        {
            var ketQua = new List<ElementId>();

            foreach (Element el in new FilteredElementCollector(doc)
                .OfCategory(cat)
                .WhereElementIsNotElementType())
            {
                BoundingBoxXYZ bb = el.get_BoundingBox(null);
                if (bb == null)
                    continue;

                if (bb.Max.X >= minX && bb.Min.X <= maxX &&
                    bb.Max.Y >= minY && bb.Min.Y <= maxY)
                {
                    ketQua.Add(el.Id);
                }
            }

            return ketQua;
        }
    }
}
