using Autodesk.Revit.DB;
using Cad2Revit.Helpers;
using System.Collections.Generic;

namespace Cad2Revit.Models
{
    public enum LoaiCauKien
    {
        KhongXacDinh,
        Tuong,
        Cot,
        Dam,
        San
    }

    public class CadLine
    {
        public XYZ DiemDau { get; set; }
        public XYZ DiemCuoi { get; set; }
        public string TenLayer { get; set; }
        public LoaiCauKien Loai { get; set; }

        public double? ChieuRongMm { get; set; }
        public double? ChieuSauMm { get; set; }
        public double? BeDayMm { get; set; }

        public double ChieuDaiFeet()
        {
            if (DiemDau == null || DiemCuoi == null)
                return 0;
            return DiemDau.DistanceTo(DiemCuoi);
        }
    }

    /// <summary>Một vị trí cột đã gộp từ geometry CAD (tâm + tiết diện).</summary>
    public class DiemCot
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double RongMm { get; set; }
        public double SauMm { get; set; }
        public string TenLayer { get; set; }

        public XYZ Tam => new XYZ(X, Y, 0);
    }

    /// <summary>Dầm đã khử trùng (một đường trục).</summary>
    public class DuongDam
    {
        public XYZ DiemDau { get; set; }
        public XYZ DiemCuoi { get; set; }
        public double RongMm { get; set; }
        public double CaoMm { get; set; }
        public string TenLayer { get; set; }

        public double ChieuDaiNhipMm =>
            UnitHelper.FeetSangMm(
                DiemDau != null && DiemCuoi != null
                    ? DiemDau.DistanceTo(DiemCuoi)
                    : 0);
    }

    public class VungSan
    {
        public List<XYZ> DuongVien { get; set; } = new List<XYZ>();
        public double BeDayMm { get; set; }
        public string TenLayer { get; set; }
    }

    public class KetQuaPhanTich
    {
        public double ChieuCaoCaoNhaMm { get; set; }
        public int SoTang { get; set; }
        public double ChieuCaoTangMm { get; set; }

        public double BeDayTuongMm { get; set; }
        public double CotRongMm { get; set; }
        public double CotSauMm { get; set; }
        public double DamRongMm { get; set; }
        public double DamCaoMm { get; set; }
        public double BeDaySanMm { get; set; }

        public List<DiemCot> DanhSachDiemCot { get; set; } =
            new List<DiemCot>();

        public List<DuongDam> DanhSachDuongDam { get; set; } =
            new List<DuongDam>();

        public List<VungSan> DanhSachVungSan { get; set; } =
            new List<VungSan>();

        public LuoiTrucKetCau LuoiTruc { get; set; }

        public string TomTat { get; set; }
    }

    public class KetQuaDocCAD
    {
        public List<CadLine> DanhSachDuong { get; set; }
        public bool ThanhCong { get; set; }
        public string ThongBaoLoi { get; set; }
        public KetQuaPhanTich PhanTich { get; set; }
        public double GocX { get; set; }
        public double GocY { get; set; }
        public double DoiTatX { get; set; }
        public double DoiTatY { get; set; }

        public ElementId CadImportId { get; set; }

        public bool DaDichCadVeGoc { get; set; }

        public KetQuaDocCAD()
        {
            DanhSachDuong = new List<CadLine>();
            ThanhCong = false;
            ThongBaoLoi = "";
        }
    }
}
