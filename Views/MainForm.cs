using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Cad2Revit.Converter;
using Cad2Revit.Helpers;
using Cad2Revit.Models;
using Cad2Revit.Services;
using RDB = Autodesk.Revit.DB;

namespace Cad2Revit.Views
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private readonly UIApplication _uiApp;
        private readonly RDB.Document _doc;
        private readonly CadConversionService _conversionService;
        private readonly BindingSource _layerBindingSource;
        private readonly BindingList<LayerMapItem> _layerMapItems;
        private KetQuaDocCAD _ketQuaCAD;
        private KetQuaPhanTich _phanTich;
        private bool _canReadCad;
        private bool _canConvert;
        private bool _canApplyLayer;

        public MainForm(UIApplication uiApp, RDB.Document doc)
        {
            InitializeComponent();

            _uiApp = uiApp;
            _doc = doc;
            _conversionService = new CadConversionService(doc);
            _layerMapItems = new BindingList<LayerMapItem>();
            _layerBindingSource = new BindingSource();

            InitializeLayerGrid();
            SetDefaults();
            UpdateCadImportStatus();
        }

        private void InitializeLayerGrid()
        {
            gridLayer.AutoGenerateColumns = false;
            colTenLayer.DataPropertyName = "LayerName";
            colLoaiCauKien.DataPropertyName = "CategoryName";
            colLoaiCauKien.Items.Clear();
            colLoaiCauKien.Items.AddRange(new object[] { "Tường", "Cột", "Dầm", "Sàn", "Bỏ qua" });

            _layerBindingSource.DataSource = _layerMapItems;
            gridLayer.DataSource = _layerBindingSource;
        }

        private void SetDefaults()
        {
            txtChieuCaoTang1.Text = "4200";
            txtChieuCaoTangDienHinh.Text = "3300";
            txtBeDaySan.Text = "150";
            txtDamRong.Text = "200";
            txtDamCao.Text = "500";
            txtSoTang.Text = "4";

            chkWalls.Checked = true;
            chkColumns.Checked = true;
            chkBeams.Checked = true;
            chkFloors.Checked = true;

            lblStatWall.Text = "0";
            lblStatCol.Text = "0";
            lblStatBeam.Text = "0";
            lblStatFloor.Text = "0";
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
                lblCadStatus.Text = "Đã có file CAD import — nhấn Đọc CAD rồi Chuyển đổi.";
                lblCadStatus.ForeColor = System.Drawing.Color.Green;
                _canReadCad = true;
            }
            else
            {
                lblCadStatus.Text = "Chưa import CAD. Dùng Insert → Import CAD trước.";
                lblCadStatus.ForeColor = System.Drawing.Color.Red;
                _canReadCad = false;
            }

            _canApplyLayer = false;
            _canConvert = false;
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            btnReadCad.Enabled = _canReadCad;
            btnApDungLayer.Enabled = _canApplyLayer;
            btnConvert.Enabled = _canConvert;
        }

        private void btnReadCad_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_conversionService.ReadCad(out string error))
                {
                    lblCadStatus.Text = error ?? "Không đọc được CAD.";
                    lblCadStatus.ForeColor = System.Drawing.Color.Red;
                    _canConvert = false;
                    _canApplyLayer = false;
                    UpdateStatistics();
                    UpdateButtonStates();
                    return;
                }

                _ketQuaCAD = _conversionService.CadResult;
                _phanTich = null;

                UpdateLayerMap(_conversionService.LayerNames);
                ApplyDefaultLayerMapping();
                RefreshAnalysis();

                _canConvert = _ketQuaCAD?.DanhSachDuong?.Count > 0;
                _canApplyLayer = true;
                _canReadCad = true;
                UpdateButtonStates();

                lblCadStatus.Text = "Đã đọc CAD thành công. Có thể áp dụng layer hoặc chuyển đổi.";
                lblCadStatus.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnApDungLayer_Click(object sender, EventArgs e)
        {
            if (!_canApplyLayer)
            {
                MessageBox.Show(
                    "Hãy đọc CAD trước!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var mappings = _layerMapItems
                .Where(item => !string.IsNullOrWhiteSpace(item.LayerName))
                .Select(item => new LayerMapping
                {
                    LayerName = item.LayerName,
                    Loai = ConvertNameToType(item.CategoryName)
                })
                .Where(mapping => mapping.Loai != LoaiCauKien.KhongXacDinh)
                .ToList();

            _conversionService.ApplyLayerMappings(mappings);
            _ketQuaCAD = _conversionService.CadResult;
            RefreshAnalysis();
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (!_canConvert)
            {
                MessageBox.Show(
                    "Hãy đọc CAD trước!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ConversionSettings settings = CreateConversionSettings();
            if (settings == null)
            {
                MessageBox.Show(
                    "Nhập đủ: H tầng 1, H tầng ĐH, độ dày sàn, kích thước dầm, số tầng.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string thongSoTuDong = GetThongSoTuDong(settings);
            DialogResult xacNhan = MessageBox.Show(
                "Tạo mô hình 3D?\n\n" + thongSoTuDong,
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (xacNhan != DialogResult.Yes)
                return;

            btnConvert.Enabled = false;

            try
            {
                using (Transaction transaction = new Transaction(_doc, "CAD to 3D"))
                {
                    transaction.Start();
                    string ketQua = _conversionService.ConvertModel(settings);
                    transaction.Commit();

                    MessageBox.Show(
                        ketQua,
                        "Hoàn thành",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    ZoomToFit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConvert.Enabled = _canConvert;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void UpdateLayerMap(System.Collections.Generic.IEnumerable<string> layerNames)
        {
            _layerMapItems.Clear();
            if (layerNames == null)
                return;

            foreach (string layerName in layerNames)
            {
                _layerMapItems.Add(new LayerMapItem
                {
                    LayerName = layerName,
                    CategoryName = ConvertTypeToName(LayerMapper.XacDinhLoai(layerName))
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

        private void RefreshAnalysis()
        {
            ConversionSettings settings = CreateConversionSettings();
            if (settings == null)
                return;

            _conversionService.Analyse(settings);
            _phanTich = _conversionService.AnalysisResult;
            _ketQuaCAD = _conversionService.CadResult;
            UpdateStatistics();
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

            lblStatWall.Text = soTuong.ToString();
            lblStatCol.Text = soCot.ToString();
            lblStatBeam.Text = soDam.ToString();
            lblStatFloor.Text = soSan.ToString();
        }

        private string GetThongSoTuDong(ConversionSettings settings)
        {
            if (_ketQuaCAD == null)
                return string.Empty;

            int soVung = _phanTich.DanhSachVungSan?.Count ?? 0;
            int soCot = _phanTich.DanhSachDiemCot?.Count ?? 0;
            int soDam = _phanTich.DanhSachDuongDam?.Count ?? 0;

            string toaDo = string.Format(
                "Gốc CAD: X={0:F1}m Y={1:F1}m | ",
                UnitHelper.FeetSangMm(_ketQuaCAD.GocX) / 1000.0,
                UnitHelper.FeetSangMm(_ketQuaCAD.GocY) / 1000.0);

            return toaDo + "TCVN B25 | Cột: " + soCot +
                " | Dầm: " + soDam + " nhịp (" +
                settings.DamRongMm + "×" + settings.DamCaoMm +
                " mm nhập tay)\r\nSàn " + Math.Round(settings.BeDaySanMm) +
                " mm | Vùng: " + soVung;
        }

        private ConversionSettings CreateConversionSettings()
        {
            if (!double.TryParse(txtChieuCaoTang1.Text, out double h1) || h1 <= 0)
                return null;
            if (!double.TryParse(txtChieuCaoTangDienHinh.Text, out double hDh) || hDh <= 0)
                return null;
            if (!double.TryParse(txtBeDaySan.Text, out double beDaySan) || beDaySan <= 0)
                return null;
            if (!double.TryParse(txtDamRong.Text, out double damRong) || damRong <= 0)
                return null;
            if (!double.TryParse(txtDamCao.Text, out double damCao) || damCao <= 0)
                return null;
            if (!int.TryParse(txtSoTang.Text, out int soTang) || soTang < 1)
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
                SuDungLevelRevitCoSan = true,
                SuDungGridRevitCoSan = false,
                TaoLevelMoiKhiThieu = true,
                TaoSanMai = true,
                ConvertWalls = chkWalls.Checked,
                ConvertColumns = chkColumns.Checked,
                ConvertBeams = chkBeams.Checked,
                ConvertFloors = chkFloors.Checked
            };
        }

        private LoaiCauKien ConvertNameToType(string ten)
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

        private string ConvertTypeToName(LoaiCauKien loai)
        {
            if (loai == LoaiCauKien.Tuong) return "Tường";
            if (loai == LoaiCauKien.Cot) return "Cột";
            if (loai == LoaiCauKien.Dam) return "Dầm";
            if (loai == LoaiCauKien.San) return "Sàn";
            return "Bỏ qua";
        }

        private void ZoomToFit()
        {
            try
            {
                UIDocument uiDoc = _uiApp.ActiveUIDocument;
                if (uiDoc == null)
                    return;

                var views = uiDoc.GetOpenUIViews();
                if (views != null && views.Count > 0)
                    views[0].ZoomToFit();

                uiDoc.RefreshActiveView();
            }
            catch
            {
                // bỏ qua nếu view không hỗ trợ zoom
            }
        }
    }
}
