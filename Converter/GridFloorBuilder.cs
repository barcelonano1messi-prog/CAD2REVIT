using Autodesk.Revit.DB;
using Cad2Revit.Helpers;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cad2Revit.Converter
{
    internal class GridFloorBuilder
    {
        private readonly Document _doc;
        private readonly ConversionSettings _settings;
        private readonly KetQuaPhanTich _phanTich;
        private readonly KetQuaDocCAD _ketQuaCAD;
        private readonly List<Level> _levels;
        private readonly Level _roofLevel;
        private readonly HashSet<string> _createdFloorKeys = new HashSet<string>();

        public GridFloorBuilder(
            Document doc,
            ConversionSettings settings,
            KetQuaPhanTich phanTich,
            KetQuaDocCAD ketQuaCAD,
            List<Level> levels,
            Level roofLevel)
        {
            _doc = doc ?? throw new ArgumentNullException(nameof(doc));
            _settings = settings ?? new ConversionSettings();
            _phanTich = phanTich ?? new KetQuaPhanTich();
            _ketQuaCAD = ketQuaCAD;
            _levels = levels ?? new List<Level>();
            _roofLevel = roofLevel;
        }

        public void CreateGridAndFloors()
        {
            if (!_settings.SuDungGridRevitCoSan)
            {
                CreateGrids();
            }

            if (!_settings.ConvertFloors || _levels == null)
                return;

            for (int tang = 0; tang < _levels.Count; tang++)
            {
                if (ShouldCreateFloor(tang))
                {
                    CreateFloorForLevel(tang, _levels[tang]);
                }
            }

            if (_settings.TaoSanMai)
            {
                CreateRoofFloor();
            }
        }

        private bool ShouldCreateFloor(int tang)
        {
            if (_levels == null || tang < 0 || tang >= _levels.Count)
                return false;

            if (_settings.BoQuaSanTangTret && tang == 0)
                return false;

            return true;
        }

        private void CreateGrids()
        {
            CadCleanupHelper.XoaTatCaGrid(_doc);

            LuoiTrucKetCau luoi = _phanTich.LuoiTruc;
            if (luoi == null || luoi.TrucX.Count < 2 || luoi.TrucY.Count < 2)
                return;

            double z = _levels != null && _levels.Count > 0
                ? _levels[0].Elevation
                : 0;

            double margin = UnitHelper.MmSangFeet(4500);
            double minY = luoi.MinY - margin;
            double maxY = luoi.MaxY + margin;
            double minX = luoi.MinX - margin;
            double maxX = luoi.MaxX + margin;

            for (int i = 0; i < luoi.TrucX.Count; i++)
            {
                double x = luoi.TrucX[i];
                try
                {
                    Line line = Line.CreateBound(
                        new XYZ(x, minY, z),
                        new XYZ(x, maxY, z));

                    if (HasDuplicateGrid(line))
                        continue;

                    Grid grid = Grid.Create(_doc, line);
                    if (grid != null)
                    {
                        NameGrid(grid, GetXAxisLabel(i));
                        SetGridExtents(grid, line);
                    }
                }
                catch
                {
                }
            }

            for (int j = 0; j < luoi.TrucY.Count; j++)
            {
                double y = luoi.TrucY[j];
                try
                {
                    Line line = Line.CreateBound(
                        new XYZ(minX, y, z),
                        new XYZ(maxX, y, z));

                    if (HasDuplicateGrid(line))
                        continue;

                    Grid grid = Grid.Create(_doc, line);
                    if (grid != null)
                    {
                        NameGrid(grid, (j + 1).ToString());
                        SetGridExtents(grid, line);
                    }
                }
                catch
                {
                }
            }
        }

        private void CreateFloorForLevel(int tang, Level level)
        {
            var boundaries = GetFloorBoundariesForLevel(tang);
            if (boundaries == null || boundaries.Count == 0)
                return;

            foreach (List<XYZ> boundary in boundaries)
            {
                if (boundary == null || boundary.Count < 3)
                    continue;

                string key = tang + "_" + ComputeAreaKey(boundary);
                if (_createdFloorKeys.Contains(key))
                    continue;

                if (TryCreateFloor(boundary, level.Elevation, level.Id, false, _phanTich.LuoiTruc?.LoThung))
                {
                    _createdFloorKeys.Add(key);
                }
            }
        }

        private List<List<XYZ>> GetFloorBoundariesForLevel(int tang)
        {
            if (_phanTich.DanhSachVungSan != null &&
                _phanTich.DanhSachVungSan.Count > 0)
            {
                return _phanTich.DanhSachVungSan
                    .Where(v => v.DuongVien != null && v.DuongVien.Count >= 3)
                    .Select(v => v.DuongVien)
                    .ToList();
            }

            if (_phanTich.LuoiTruc != null)
            {
                List<XYZ> luoiBoundary = StructuralGridSystem.LayDuongVienChoTang(
                    _phanTich.LuoiTruc,
                    tang,
                    _levels.Count);

                if (luoiBoundary != null && luoiBoundary.Count >= 3)
                    return new List<List<XYZ>> { luoiBoundary };
            }

            return new List<List<XYZ>>();
        }

        private void CreateRoofFloor()
        {
            if (_levels == null || _levels.Count == 0)
                return;

            if (_roofLevel == null)
                return;

            var boundaries = GetFloorBoundariesForLevel(_levels.Count - 1);
            if (boundaries == null || boundaries.Count == 0)
            {
                List<XYZ> fallback = GetLargestFloorArea();
                if (fallback == null || fallback.Count < 3)
                    return;

                boundaries = new List<List<XYZ>> { fallback };
            }

            foreach (List<XYZ> boundary in boundaries)
            {
                if (boundary == null || boundary.Count < 3)
                    continue;

                string key = "MAI_" + ComputeAreaKey(boundary);
                if (_createdFloorKeys.Contains(key))
                    continue;

                if (TryCreateFloor(boundary, _roofLevel.Elevation, _roofLevel.Id, true, null))
                {
                    _createdFloorKeys.Add(key);
                }
            }
        }

        private List<XYZ> GetFloorBoundaryForLevel(int tang)
        {
            if (_phanTich.LuoiTruc != null)
            {
                List<XYZ> luoiBoundary = StructuralGridSystem.LayDuongVienChoTang(
                    _phanTich.LuoiTruc,
                    tang,
                    _levels.Count);

                if (luoiBoundary != null && luoiBoundary.Count >= 3)
                    return luoiBoundary;
            }

            return GetLargestFloorArea();
        }

        private List<XYZ> GetLargestFloorArea()
        {
            if (_phanTich.LuoiTruc?.DuongVienSan != null &&
                _phanTich.LuoiTruc.DuongVienSan.Count >= 3)
            {
                return _phanTich.LuoiTruc.DuongVienSan;
            }

            if (_phanTich.DanhSachVungSan == null || _phanTich.DanhSachVungSan.Count == 0)
                return null;

            return _phanTich.DanhSachVungSan
                .OrderByDescending(v => ComputePolygonArea(v.DuongVien))
                .First().DuongVien;
        }

        private bool TryCreateFloor(
            List<XYZ> boundary,
            double elevationFeet,
            ElementId levelId,
            bool isRoof,
            List<XYZ> opening)
        {
            try
            {
                if (boundary == null || boundary.Count < 3)
                    return false;

                FloorType floorType = new RevitTcvnHelper(_doc).TimFloorTypeTcvn(
                    _settings.BeDaySanMm,
                    isRoof);
                if (floorType == null)
                    return false;

                CurveLoop outerLoop = CreateCurveLoop(boundary, elevationFeet);
                if (outerLoop == null)
                    return false;

                var loops = new List<CurveLoop> { outerLoop };
                if (opening != null && opening.Count >= 3)
                {
                    CurveLoop innerLoop = CreateCurveLoop(opening, elevationFeet);
                    if (innerLoop != null)
                        loops.Add(innerLoop);
                }

                Floor.Create(_doc, loops, floorType.Id, levelId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static CurveLoop CreateCurveLoop(
            List<XYZ> points,
            double elevationFeet)
        {
            var curves = new List<Curve>();
            for (int i = 0; i < points.Count; i++)
            {
                XYZ a = Elevate(points[i], elevationFeet);
                XYZ b = Elevate(points[(i + 1) % points.Count], elevationFeet);

                if (a.DistanceTo(b) < UnitHelper.MmSangFeet(50))
                    continue;

                curves.Add(Line.CreateBound(a, b));
            }

            if (curves.Count < 3)
                return null;

            return CurveLoop.Create(curves);
        }

        private static XYZ Elevate(XYZ point, double elevationFeet)
        {
            return new XYZ(point.X, point.Y, elevationFeet);
        }

        private static string GetXAxisLabel(int index)
        {
            if (index < 26)
                return ((char)('A' + index)).ToString();

            return "A" + ((char)('A' + index - 26)).ToString();
        }

        private bool HasDuplicateGrid(Line newLine)
        {
            double tol = UnitHelper.MmSangFeet(80);
            XYZ p0 = newLine.GetEndPoint(0);
            XYZ p1 = newLine.GetEndPoint(1);

            foreach (Grid g in new FilteredElementCollector(_doc)
                .OfClass(typeof(Grid))
                .Cast<Grid>())
            {
                if (g.Name == null ||
                    g.Name.IndexOf("CAD2Revit", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                Curve c = g.Curve;
                if (c == null)
                    continue;

                XYZ a = c.GetEndPoint(0);
                XYZ b = c.GetEndPoint(1);
                if (a.DistanceTo(p0) < tol && b.DistanceTo(p1) < tol)
                    return true;
            }

            return false;
        }

        private void NameGrid(Grid grid, string name)
        {
            if (grid == null)
                return;

            try
            {
                grid.Name = name;
            }
            catch
            {
                grid.Name = name + "-" + grid.Id.Value;
            }
        }

        private void SetGridExtents(Grid grid, Line line)
        {
            if (grid == null || line == null || _levels == null || _levels.Count == 0)
                return;

            try
            {
                double zDuoi = _levels[0].Elevation;
                double zTren = _roofLevel != null
                    ? _roofLevel.Elevation
                    : _levels[_levels.Count - 1].Elevation;

                grid.SetVerticalExtents(zDuoi, zTren);

                var views = new FilteredElementCollector(_doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .Where(v => !v.IsTemplate && !v.IsAssemblyView)
                    .ToList();

                foreach (View view in views)
                {
                    try
                    {
                        grid.SetDatumExtentType(DatumEnds.End0, view, DatumExtentType.Model);
                        grid.SetCurveInView(DatumExtentType.Model, view, line);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        private static string ComputeAreaKey(List<XYZ> points)
        {
            if (points == null || points.Count == 0)
                return string.Empty;

            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);
            return Math.Round(minX) + "_" + Math.Round(minY) + "_" + Math.Round(maxX) + "_" + Math.Round(maxY);
        }

        private static double ComputePolygonArea(List<XYZ> points)
        {
            if (points == null || points.Count < 3)
                return 0;

            double area = 0;
            int n = points.Count;
            for (int i = 0; i < n; i++)
            {
                XYZ p = points[i];
                XYZ q = points[(i + 1) % n];
                area += p.X * q.Y - q.X * p.Y;
            }

            return Math.Abs(area) * 0.5;
        }
    }
}
