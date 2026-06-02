using System;

namespace Cad2Revit.Models
{
    public class ConversionSettings
    {
        /// <summary>Chiều cao tầng 1 / tầng trệt lên tầng 2 (mm).</summary>
        public double ChieuCaoTang1Mm { get; set; } = 3300;

        /// <summary>Chiều cao các tầng điển hình phía trên (mm).</summary>
        public double ChieuCaoTangDienHinhMm { get; set; } = 3300;

        public double BeDaySanMm { get; set; } = 150;

        public int SoTang { get; set; } = 4;

        /// <summary>Dầm rộng × cao (mm) — nhập tay, ưu tiên hơn CAD.</summary>
        public double DamRongMm { get; set; } = 200;

        public double DamCaoMm { get; set; } = 500;

        /// <summary>true = dầm theo ô nhập UI; false = đọc từ CAD/DIM/layer.</summary>
        public bool UuTienKichThuocDamTuUi { get; set; } = true;

        /// <summary>true = đáy (Level thấp nhất) không sàn; các tầng phía trên đều có sàn.</summary>
        public bool BoQuaSanTangTret { get; set; } = true;

        /// <summary>Tạo thêm sàn mái tại tầng trên cùng.</summary>
        public bool TaoSanMai { get; set; } = true;

        public bool ConvertWalls { get; set; } = true;
        public bool ConvertColumns { get; set; } = true;
        public bool ConvertBeams { get; set; } = true;
        public bool ConvertFloors { get; set; } = true;

        /// <summary>Dùng Level Revit có sẵn (Tầng 1, Tầng 2, …) — không tạo CAD2Revit Tầng n.</summary>
        public bool SuDungLevelRevitCoSan { get; set; } = false;

        /// <summary>Không tạo Grid CAD2Revit — dùng lưới trục template Revit.</summary>
        public bool SuDungGridRevitCoSan { get; set; } = false;

        /// <summary>Tự tạo Level khi thiếu để đủ SoTang (luôn bật trong RevitLevelHelper).</summary>
        public bool TaoLevelMoiKhiThieu { get; set; } = true;

        /// <summary>
        /// Số tấm sàn: mỗi tầng trên đáy + mái (đáy không tính).
        /// 4 tầng → sàn t2, t3, t4 + mái (đáy/tầng 1 không sàn).
        /// </summary>
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
}
