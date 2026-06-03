using Autodesk.Revit.DB;
using Cad2Revit.Helpers;
using System;
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

    public class DiemCot
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double RongMm { get; set; }
        public double SauMm { get; set; }
        public string TenLayer { get; set; }

        public XYZ Tam => new XYZ(X, Y, 0);
    }

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

        public List<DiemCot> DanhSachDiemCot { get; set; } = new List<DiemCot>();
        public List<DuongDam> DanhSachDuongDam { get; set; } = new List<DuongDam>();
        public List<VungSan> DanhSachVungSan { get; set; } = new List<VungSan>();
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
            ThongBaoLoi = string.Empty;
        }
    }

    public class ConversionSettings
    {
        public double ChieuCaoTang1Mm { get; set; } = 3300;
        public double ChieuCaoTangDienHinhMm { get; set; } = 3300;
        public double BeDaySanMm { get; set; } = 150;
        public int SoTang { get; set; } = 4;
        public double DamRongMm { get; set; } = 200;
        public double DamCaoMm { get; set; } = 500;
        public bool UuTienKichThuocDamTuUi { get; set; } = true;
        public bool BoQuaSanTangTret { get; set; } = true;
        public bool TaoSanMai { get; set; } = true;
        public bool ConvertWalls { get; set; } = true;
        public bool ConvertColumns { get; set; } = true;
        public bool ConvertBeams { get; set; } = true;
        public bool ConvertFloors { get; set; } = true;
        public bool SuDungLevelRevitCoSan { get; set; } = false;
        public bool SuDungGridRevitCoSan { get; set; } = false;
        public bool TaoLevelMoiKhiThieu { get; set; } = true;

        public int SoTamSanDuKien
        {
            get
            {
                if (!BoQuaSanTangTret)
                    return SoTang + (TaoSanMai ? 1 : 0);

                int tam = Math.Max(0, SoTang - 1);
                if (TaoSanMai && SoTang > 1)
                    tam += 1;

                return tam;
            }
        }

        public double LayChieuCaoTang(int chiSoTang)
        {
            if (chiSoTang <= 0)
                return ChieuCaoTang1Mm;

            return ChieuCaoTangDienHinhMm;
        }
    }

    public class LayerMapItem
    {
        public string LayerName { get; set; }
        public string CategoryName { get; set; }
    }

    public class LayerMapping
    {
        public string LayerName { get; set; }
        public LoaiCauKien Loai { get; set; }
    }
}
