using Autodesk.Revit.DB;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cad2Revit.Helpers
{
    public static class RevitLevelHelper
    {
        private const double TolCaoDoMm = 80;

        public static List<Level> LayDanhSachLevel(
            Document doc,
            ConversionSettings caiDat)
        {
            if (doc == null || caiDat == null)
                return new List<Level>();

            int soTang = caiDat.SoTang < 1 ? 1 : caiDat.SoTang;
            double tolFeet = UnitHelper.MmSangFeet(TolCaoDoMm);

            var levelsRevit = LayLevelRevitCoSan(doc);
            var ketQua = new List<Level>();
            var daDungId = new HashSet<long>();

            Level tang1 = TimLevelTheoTenTang(levelsRevit, 1);
            double elevationBase = tang1?.Elevation ?? 0;

            for (int i = 0; i < soTang; i++)
            {
                double caoDoMucTieu = TinhCaoDoMucTieu(
                    caiDat,
                    i,
                    levelsRevit,
                    elevationBase);

                Level level = TimLevelTheoTenTang(levelsRevit, i + 1);

                if (level == null)
                {
                    level = TimLevelTheoCaoDo(
                        levelsRevit,
                        caoDoMucTieu,
                        tolFeet);
                }

                if (level != null && daDungId.Contains(level.Id.Value))
                {
                    level = null;
                }

                if (level != null &&
                    Math.Abs(level.Elevation - caoDoMucTieu) > tolFeet)
                {
                    level = null;
                }

                if (level == null)
                {
                    level = TimLevelTheoCaoDo(
                        levelsRevit,
                        caoDoMucTieu,
                        tolFeet);

                    if (level == null || daDungId.Contains(level.Id.Value))
                    {
                        level = Level.Create(doc, caoDoMucTieu);
                        string ten = "Tầng " + (i + 1);
                        DatTenLevel(level, ten);
                        levelsRevit.Add(level);
                        levelsRevit.Sort((a, b) =>
                            a.Elevation.CompareTo(b.Elevation));
                    }
                }

                if (level == null)
                {
                    continue;
                }

                daDungId.Add(level.Id.Value);
                ketQua.Add(level);
            }

            return ketQua;
        }

        public static Level LayLevelMai(
            Document doc,
            IList<Level> daChon,
            ConversionSettings caiDat)
        {
            if (doc == null)
                return null;

            var levelsRevit = LayLevelRevitCoSan(doc);

            Level mai = levelsRevit.FirstOrDefault(l =>
                TenLaMai(l.Name));

            if (mai != null)
            {
                if (daChon != null &&
                    daChon.Any(l => l != null && l.Id == mai.Id))
                {
                    mai = null;
                }
            }

            if (mai != null)
            {
                return mai;
            }

            if (daChon != null && daChon.Count > 0)
            {
                Level tren = daChon[daChon.Count - 1];
                double caoMai = tren.Elevation +
                    UnitHelper.MmSangFeet(
                        caiDat.LayChieuCaoTang(caiDat.SoTang - 1));

                mai = TimLevelTheoCaoDo(
                    levelsRevit,
                    caoMai,
                    UnitHelper.MmSangFeet(TolCaoDoMm));

                if (mai != null && !daChon.Contains(mai))
                {
                    return mai;
                }

                mai = Level.Create(doc, caoMai);
                DatTenLevel(mai, "Tầng mái");
                return mai;
            }

            return null;
        }

        private static List<Level> LayLevelRevitCoSan(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .Where(l => !LaLevelCad2Revit(l) && !LaLevelKhongPhaiTang(l))
                .OrderBy(l => l.Elevation)
                .ToList();
        }

        private static bool LaLevelKhongPhaiTang(Level level)
        {
            if (level?.Name == null)
                return true;

            string ten = ChuanHoaTenLevel(level.Name);

            if (TenLaMai(ten))
                return true;

            string[] boQua =
            {
                "móng",
                "mong",
                "foundation",
                "basement",
                "tầng hầm",
                "tang ham",
                "hầm",
                "ham",
                "roof",
                "site",
                "existing",
                "demolished"
            };

            foreach (string k in boQua)
            {
                if (ten.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static Level TimLevelTheoTenTang(
            List<Level> levels,
            int soTang)
        {
            if (levels == null || levels.Count == 0)
                return null;

            foreach (string ten in CacTenTang(soTang))
            {
                Level lv = levels.FirstOrDefault(l =>
                    TenLevelKhopChinhXac(l.Name, ten, soTang));

                if (lv != null)
                    return lv;
            }

            return null;
        }

        private static bool TenLevelKhopChinhXac(
            string tenLevel,
            string mauTen,
            int soTang)
        {
            if (string.IsNullOrWhiteSpace(tenLevel))
                return false;

            string a = ChuanHoaTenLevel(tenLevel);
            string b = ChuanHoaTenLevel(mauTen);

            if (a.Equals(b, StringComparison.OrdinalIgnoreCase))
                return true;

            if (a.Equals("tang" + soTang, StringComparison.OrdinalIgnoreCase))
                return true;

            if (a.Equals("level" + soTang, StringComparison.OrdinalIgnoreCase))
                return true;

            var rx = new Regex(
                @"^(?:tầng|tang|level|tầng\s*)\s*0*" + soTang + @"\s*$",
                RegexOptions.IgnoreCase);

            return rx.IsMatch(a);
        }

        private static string ChuanHoaTenLevel(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
                return "";

            ten = ten.Trim();
            ten = Regex.Replace(ten, @"tầng", "Tầng", RegexOptions.IgnoreCase);
            ten = Regex.Replace(ten, @"tang", "Tầng", RegexOptions.IgnoreCase);
            return ten;
        }

        private static string[] CacTenTang(int soTang)
        {
            return new[]
            {
                "Tầng " + soTang,
                "Tang " + soTang,
                "Level " + soTang,
                "TANG " + soTang,
                "TẦNG " + soTang,
                "TẦNG " + soTang.ToString("00"),
                "Tầng " + soTang.ToString("00"),
                "L" + soTang,
                "L-" + soTang,
                soTang.ToString()
            };
        }

        private static bool TenLaMai(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
                return false;

            ten = ChuanHoaTenLevel(ten);
            return ten.Equals("Tầng mái", StringComparison.OrdinalIgnoreCase) ||
                ten.Equals("Tang mai", StringComparison.OrdinalIgnoreCase) ||
                ten.Equals("Level mái", StringComparison.OrdinalIgnoreCase) ||
                ten.IndexOf("mái", StringComparison.OrdinalIgnoreCase) >= 0 ||
                ten.IndexOf("mai", StringComparison.OrdinalIgnoreCase) >= 0 ||
                ten.IndexOf("roof", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool LaLevelCad2Revit(Level level)
        {
            return level?.Name != null &&
                level.Name.StartsWith(
                    "CAD2Revit",
                    StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Cao độ mục tiêu: Level Revit có sẵn → ưu tiên; không thì bước từ Tầng1–2; cuối cùng từ H nhập UI.
        /// </summary>
        private static double TinhCaoDoMucTieu(
            ConversionSettings caiDat,
            int chiSoTang,
            List<Level> levels,
            double elevationBase)
        {
            Level coSan = TimLevelTheoTenTang(levels, chiSoTang + 1);
            if (coSan != null)
                return coSan.Elevation;

            double caoDo = elevationBase;

            for (int j = 0; j < chiSoTang; j++)
            {
                if (j > 0 || chiSoTang > 0)
                {
                    caoDo += UnitHelper.MmSangFeet(
                        caiDat.LayChieuCaoTang(j));
                }
            }

            return caoDo;
        }

        private static Level TimLevelTheoCaoDo(
            List<Level> levels,
            double caoDo,
            double tolFeet)
        {
            if (levels == null || levels.Count == 0)
                return null;

            return levels
                .Where(l => !LaLevelKhongPhaiTang(l))
                .OrderBy(l => Math.Abs(l.Elevation - caoDo))
                .FirstOrDefault(l =>
                    Math.Abs(l.Elevation - caoDo) < tolFeet);
        }

        private static void DatTenLevel(Level level, string ten)
        {
            if (level == null)
                return;

            try
            {
                level.Name = ten;
            }
            catch
            {
                level.Name = ten + " (" + level.Id.Value + ")";
            }
        }
    }
}
