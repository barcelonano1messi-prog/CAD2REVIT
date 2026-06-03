using Cad2Revit.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cad2Revit.Converter
{
    public class LayerMapper
    {
        // Mapping do user tự thêm
        private static Dictionary<string, LoaiCauKien>
            _layerTuyChinh =
            new Dictionary<string, LoaiCauKien>();

        // Từ khóa mặc định
        private static readonly string[] _tuKhoaTuong =
        {
            "TUONG",
            "WALL",
            "PARTITION",
            "ARCH_WALL"
        };

        private static readonly string[] _tuKhoaCot =
        {
            "COT",
            "COLUMN",
            "COL",
            "PILLAR",
            "600X500",
            "60X50",
            "450X600",
            "45X60",
            "500X600",
            "50X60",
            "400X500",
            "40X50",
            "500X400",
            "50X40",
            "002"
        };

        private static readonly string[] _tuKhoaDam =
        {
            "DAM",
            "BEAM",
            "GIRT",
            "MAIN BEAM",
            "SECONDARY"
        };

        private static readonly string[] _tuKhoaSan =
        {
            "SAN",
            "SLAB",
            "FLOOR",
            "SAB",
            "PLATE",
            "A FLOR",
            "AFLOR"
        };

        private static readonly string[] _tuKhoaBoQua =
        {
            "OPEN",
            "OPENING",
            "OPENNING",
            "HOLE",
            "VOID"
        };

        private static readonly string[] _tuKhoaBoQua =
        {
            "OPEN",
            "OPENING",
            "OPENNING",
            "HOLE",
            "VOID"
        };

        /// <summary>
        /// User thêm mapping custom
        /// </summary>
        public static void ThemLayerTuyChinh(
            string tenLayer,
            LoaiCauKien loai)
        {
            if (string.IsNullOrWhiteSpace(tenLayer))
                return;

            string key = ChuanHoaTenLayer(tenLayer);

            if (_layerTuyChinh.ContainsKey(key))
                _layerTuyChinh[key] = loai;
            else
                _layerTuyChinh.Add(key, loai);
        }

        /// <summary>
        /// Reset custom mapping
        /// </summary>
        public static void XoaHetLayerTuyChinh()
        {
            _layerTuyChinh.Clear();
        }

        /// <summary>
        /// Hàm chính
        /// </summary>
        public static LoaiCauKien XacDinhLoai(
            string tenLayer)
        {
            if (string.IsNullOrWhiteSpace(tenLayer))
                return LoaiCauKien.KhongXacDinh;

            string ten =
                ChuanHoaTenLayer(tenLayer);

            // ====================================================
            // 1. Exact custom match
            // ====================================================

            if (_layerTuyChinh.ContainsKey(ten))
                return _layerTuyChinh[ten];

            // ====================================================
            // 2. Partial custom match
            // ====================================================

            foreach (var item in _layerTuyChinh)
            {
                if (ten.Contains(item.Key))
                    return item.Value;
            }

            // ====================================================
            // 3. Default keyword
            // ====================================================

            if (CoTuKhoa(ten, _tuKhoaTuong))
                return LoaiCauKien.Tuong;

            if (CoTuKhoa(ten, _tuKhoaCot))
                return LoaiCauKien.Cot;

            if (CoTuKhoa(ten, _tuKhoaBoQua))
                return LoaiCauKien.KhongXacDinh;

            if (CoTuKhoa(ten, _tuKhoaBoQua))
                return LoaiCauKien.KhongXacDinh;

            if (CoTuKhoa(ten, _tuKhoaDam))
                return LoaiCauKien.Dam;

            if (CoTuKhoa(ten, _tuKhoaSan))
                return LoaiCauKien.San;

            return LoaiCauKien.KhongXacDinh;
        }

        /// <summary>
        /// Chuẩn hóa tên layer
        /// </summary>
        private static string ChuanHoaTenLayer(
            string tenLayer)
        {
            if (tenLayer == null)
                return "";

            string ten = tenLayer.ToUpper().Trim();

            // đổi ký tự phân cách thành space
            ten = ten.Replace("-", " ");
            ten = ten.Replace("_", " ");
            ten = ten.Replace(".", " ");

            // xóa space dư
            ten = Regex.Replace(ten, @"\s+", " ");

            return ten;
        }

        /// <summary>
        /// Kiểm tra keyword an toàn
        /// </summary>
        private static bool CoTuKhoa(
            string tenLayer,
            string[] dsTuKhoa)
        {
            foreach (string tuKhoa in dsTuKhoa)
            {
                if (tenLayer.Contains(tuKhoa))
                    return true;
            }

            return false;
        }

        public static bool LaLayerBoQua(string tenLayer)
        {
            if (string.IsNullOrWhiteSpace(tenLayer))
                return false;

            return CoTuKhoa(ChuanHoaTenLayer(tenLayer), _tuKhoaBoQua);
        }
    }
}