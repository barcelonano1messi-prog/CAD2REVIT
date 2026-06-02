using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Cad2Revit.Converter;
using Cad2Revit.Helpers;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DrawingColor = System.Drawing.Color;
using System.Linq;

namespace Cad2Revit.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly UIApplication _uiApp;
        private readonly Document _doc;
        private CadReader _cadReader;
        private KetQuaDocCAD _ketQuaCAD;
        private KetQuaPhanTich _phanTich;

        private string _cadStatusText;
        private DrawingColor _cadStatusColor;
        private bool _canReadCad;
        private bool _canConvert;
        private bool _canApplyLayer;
        private string _chieuCaoTang1;
        private string _chieuCaoTangDienHinh;
        private string _beDaySan;
        private string _damRong;
        private string _damCao;
        private string _soTang;
        private bool _convertWalls;
        private bool _convertColumns;
        private bool _convertBeams;
        private bool _convertFloors;
        private string _thongSoTuDong;
        private string _statWall;
        private string _statCol;
        private string _statBeam;
        private string _statFloor;
        private string _statUnknown;
        private string _logText;

        public BindingList<LayerMapItem> LayerMapItems { get; } = new BindingList<LayerMapItem>();

        public string CadStatusText
        {
            get => _cadStatusText;
            private set => SetProperty(ref _cadStatusText, value);
        }

        public DrawingColor CadStatusColor
        {
            get => _cadStatusColor;
            private set => SetProperty(ref _cadStatusColor, value);
        }

        public bool CanReadCad
        {
            get => _canReadCad;
            private set => SetProperty(ref _canReadCad, value);
        }

        public bool CanConvert
        {
            get => _canConvert;
            private set => SetProperty(ref _canConvert, value);
        }

        public bool CanApplyLayer
        {
            get => _canApplyLayer;
            private set => SetProperty(ref _canApplyLayer, value);
        }

        public string ChieuCaoTang1
        {
            get => _chieuCaoTang1;
            set => SetProperty(ref _chieuCaoTang1, value);
        }

        public string ChieuCaoTangDienHinh
        {
            get => _chieuCaoTangDienHinh;
            set => SetProperty(ref _chieuCaoTangDienHinh, value);
        }

        public string BeDaySan
        {
            get => _beDaySan;
            set => SetProperty(ref _beDaySan, value);
        }

        public string DamRong
        {
            get => _damRong;
            set => SetProperty(ref _damRong, value);
        }

        public string DamCao
        {
            get => _damCao;
            set => SetProperty(ref _damCao, value);
        }

        public string SoTang
        {
            get => _soTang;
            set => SetProperty(ref _soTang, value);
        }

        public bool ConvertWalls
        {
            get => _convertWalls;
            set => SetProperty(ref _convertWalls, value);
        }

        public bool ConvertColumns
        {
            get => _convertColumns;
            set => SetProperty(ref _convertColumns, value);
        }

        public bool ConvertBeams
        {
            get => _convertBeams;
            set => SetProperty(ref _convertBeams, value);
        }

        public bool ConvertFloors
        {
            get => _convertFloors;
            set => SetProperty(ref _convertFloors, value);
        }

        public string ThongSoTuDong
        {
            get => _thongSoTuDong;
            private set => SetProperty(ref _thongSoTuDong, value);
        }

        public string StatWall
        {
            get => _statWall;
            private set => SetProperty(ref _statWall, value);
        }

        public string StatCol
        {
            get => _statCol;
            private set => SetProperty(ref _statCol, value);
        }

        public string StatBeam
        {
            get => _statBeam;
            private set => SetProperty(ref _statBeam, value);
        }

        public string StatFloor
        {
            get => _statFloor;
            private set => SetProperty(ref _statFloor, value);
        }

        public string StatUnknown
        {
            get => _statUnknown;
            private set => SetProperty(ref _statUnknown, value);
        }

        public string LogText
        {
            get => _logText;
            private set => SetProperty(ref _logText, value);
        }

        public bool HasValidSettings => CreateConversionSettings() != null;

        public MainViewModel(UIApplication uiApp, Document doc)
        {
            _uiApp = uiApp ?? throw new ArgumentNullException(nameof(uiApp));
            _doc = doc ?? throw new ArgumentNullException(nameof(doc));
        }

        public void Initialize()
        {
            SetDefaults();
            UpdateCadImportStatus();
        }

        public void ReadCad()
        {
            _cadReader = new CadReader(_doc);
            _ketQuaCAD = _cadReader.DocCadTuImport();

            if (_ketQuaCAD == null || !_ketQuaCAD.ThanhCong)
            {
                return;
            }

            UpdateLayerMap(_cadReader.DanhSachLayerTimThay);
            ApplyDefaultLayerMapping();

            ConversionSettings settings = CreateConversionSettings();
            if (settings != null)
                RunAnalysis(settings);

            UpdateStatistics();
            UpdateCanConvert(_ketQuaCAD.DanhSachDuong.Count > 0);
            CanApplyLayer = true;
        }

        public void ApplyLayer()
        {
            if (_ketQuaCAD == null)
                return;

            LayerMapper.XoaHetLayerTuyChinh();

            foreach (LayerMapItem item in LayerMapItems)
            {
                if (string.IsNullOrWhiteSpace(item.LayerName))
                    continue;

                LoaiCauKien loai = ChuyenTenSangLoai(item.CategoryName);
                if (loai != LoaiCauKien.KhongXacDinh)
                    LayerMapper.ThemLayerTuyChinh(item.LayerName, loai);
            }

            foreach (CadLine duong in _ketQuaCAD.DanhSachDuong)
            {
                duong.Loai = LayerMapper.XacDinhLoai(duong.TenLayer);
            }

            ConversionSettings settings = CreateConversionSettings();
            if (settings != null)
                RunAnalysis(settings);

            UpdateStatistics();
        }

        public string Convert()
        {
            if (_ketQuaCAD == null || !_ketQuaCAD.ThanhCong)
                return null;

            ConversionSettings settings = CreateConversionSettings();
            if (settings == null)
                return null;

            RunAnalysis(settings);

            try
            {
                using (Transaction transaction = new Transaction(_doc, "CAD to 3D"))
                {
                    transaction.Start();

                    ElementCreator creator = new ElementCreator(
                        _doc,
                        settings,
                        _phanTich,
                        _ketQuaCAD);

                    string result = creator.TaoTatCaCauKien(_ketQuaCAD);
                    transaction.Commit();
                    ZoomToFit();
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void SetDefaults()
        {
            ChieuCaoTang1 = "4200";
            ChieuCaoTangDienHinh = "3300";
            BeDaySan = "150";
            DamRong = "200";
            DamCao = "500";
            SoTang = "4";

            ConvertWalls = true;
            ConvertColumns = true;
            ConvertBeams = true;
            ConvertFloors = true;

            ThongSoTuDong = "Đọc CAD → tự nhận cột/nhịp dầm.";
            StatWall = "0";
            StatCol = "0";
            StatBeam = "0";
            StatFloor = "0";
            StatUnknown = "0";
            LogText = string.Empty;
            UpdateCanConvert(false);
        }

        private void UpdateCadImportStatus()
        {
            ImportInstance cadImport = new FilteredElementCollector(_doc)
                .OfClass(typeof(ImportInstance))
                .WhereElementIsNotElementType()
                .Cast<ImportInstance>()
                .OrderByDescending(i => i.Id.IntegerValue)
                .FirstOrDefault();

            if (cadImport != null)
            {
                SetCadStatus(
                    "Đã có file CAD import — nhấn Đọc CAD rồi Chuyển đổi.",
                    DrawingColor.Green);
                CanReadCad = true;
            }
            else
            {
                SetCadStatus(
                    "Chưa import CAD. Dùng Insert → Import CAD trước.",
                    DrawingColor.Red);
                CanReadCad = false;
            }

            CanApplyLayer = false;
        }

        private void SetCadStatus(string text, DrawingColor color)
        {
            CadStatusText = text;
            CadStatusColor = color;
        }

        private void UpdateLayerMap(List<string> layerNames)
        {
            LayerMapItems.Clear();
            if (layerNames == null)
                return;

            foreach (string layerName in layerNames)
            {
                LayerMapItems.Add(new LayerMapItem
                {
                    LayerName = layerName,
                    CategoryName = ChuyenLoaiSangTen(
                        LayerMapper.XacDinhLoai(layerName))
                });
            }
        }

        private void ApplyDefaultLayerMapping()
        {
            if (_ketQuaCAD == null)
                return;

            foreach (CadLine duong in _ketQuaCAD.DanhSachDuong)
            {
                duong.Loai = LayerMapper.XacDinhLoai(duong.TenLayer);
            }
        }

        private void RunAnalysis(ConversionSettings settings)
        {
            var analyzer = new CadGeometryAnalyzer();
            _phanTich = analyzer.PhanTich(_ketQuaCAD, settings);
            _ketQuaCAD.PhanTich = _phanTich;

            int soVung = _phanTich.DanhSachVungSan?.Count ?? 0;
            int soCot = _phanTich.DanhSachDiemCot?.Count ?? 0;
            int soDam = _phanTich.DanhSachDuongDam?.Count ?? 0;

            string toaDo = string.Empty;
            if (_ketQuaCAD != null)
            {
                toaDo = string.Format(
                    "Gốc CAD: X={0:F1}m Y={1:F1}m | ",
                    UnitHelper.FeetSangMm(_ketQuaCAD.GocX) / 1000.0,
                    UnitHelper.FeetSangMm(_ketQuaCAD.GocY) / 1000.0);
            }

            ThongSoTuDong = toaDo + "TCVN B25 | Cột: " + soCot +
                " | Dầm: " + soDam + " nhịp (" +
                settings.DamRongMm + "×" + settings.DamCaoMm +
                " mm nhập tay)\r\nSàn " + Math.Round(settings.BeDaySanMm) +
                " mm | Vùng: " + soVung;
        }

        private void UpdateStatistics()
        {
            int soTuong = 0;
            int soCot = 0;
            int soDam = 0;
            int soSan = 0;
            int soKhac = 0;

            if (_ketQuaCAD != null)
            {
                foreach (CadLine duong in _ketQuaCAD.DanhSachDuong)
                {
                    switch (duong.Loai)
                    {
                        case LoaiCauKien.Tuong:
                            soTuong++;
                            break;
                        case LoaiCauKien.Cot:
                            soCot++;
                            break;
                        case LoaiCauKien.Dam:
                            soDam++;
                            break;
                        case LoaiCauKien.San:
                            soSan++;
                            break;
                        default:
                            soKhac++;
                            break;
                    }
                }
            }

            StatWall = soTuong.ToString();
            StatCol = soCot.ToString();
            StatBeam = soDam.ToString();
            StatFloor = soSan.ToString();
            StatUnknown = soKhac.ToString();
        }

        private ConversionSettings CreateConversionSettings()
        {
            if (!double.TryParse(ChieuCaoTang1, out double h1) || h1 <= 0)
                return null;
            if (!double.TryParse(ChieuCaoTangDienHinh, out double hDh) || hDh <= 0)
                return null;
            if (!double.TryParse(BeDaySan, out double beDaySan) || beDaySan <= 0)
                return null;
            if (!double.TryParse(DamRong, out double damRong) || damRong <= 0)
                return null;
            if (!double.TryParse(DamCao, out double damCao) || damCao <= 0)
                return null;
            if (!int.TryParse(SoTang, out int soTang) || soTang < 1)
                return null;

            return new ConversionSettings
            {
                ChieuCaoTang1Mm = h1,
                ChieuCaoTangDienHinhMm = hDh,
                BeDaySanMm = beDaySan,
                DamRongMm = damRong,
                DamCaoMm = damCao,
                UuTienKichThuocDamTuUi = true,
                SoTang = soTang,
                BoQuaSanTangTret = true,
                SuDungLevelRevitCoSan = false,
                SuDungGridRevitCoSan = false,
                TaoLevelMoiKhiThieu = true,
                TaoSanMai = true,
                ConvertWalls = ConvertWalls,
                ConvertColumns = ConvertColumns,
                ConvertBeams = ConvertBeams,
                ConvertFloors = ConvertFloors
            };
        }

        private void UpdateCanConvert(bool value)
        {
            CanConvert = value;
        }

        private void ZoomToFit()
        {
            try
            {
                UIDocument uiDoc = _uiApp.ActiveUIDocument;
                if (uiDoc == null)
                    return;

                IList<UIView> views = uiDoc.GetOpenUIViews();
                if (views != null && views.Count > 0)
                    views[0].ZoomToFit();

                uiDoc.RefreshActiveView();
            }
            catch
            {
                // bỏ qua nếu view không hỗ trợ zoom
            }
        }

        private string ChuyenLoaiSangTen(LoaiCauKien loai)
        {
            if (loai == LoaiCauKien.Tuong) return "Tường";
            if (loai == LoaiCauKien.Cot) return "Cột";
            if (loai == LoaiCauKien.Dam) return "Dầm";
            if (loai == LoaiCauKien.San) return "Sàn";
            return "Bỏ qua";
        }

        private LoaiCauKien ChuyenTenSangLoai(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
                return LoaiCauKien.KhongXacDinh;

            string value = ten.Trim().ToUpper();
            if (value == "TƯỜNG" || value == "TUONG") return LoaiCauKien.Tuong;
            if (value == "CỘT" || value == "COT") return LoaiCauKien.Cot;
            if (value == "DẦM" || value == "DAM") return LoaiCauKien.Dam;
            if (value == "SÀN" || value == "SAN") return LoaiCauKien.San;
            return LoaiCauKien.KhongXacDinh;
        }
    }
}
