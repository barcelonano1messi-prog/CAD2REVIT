using System;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Cad2Revit.Helpers;
using Cad2Revit.ViewModels;
using RDB = Autodesk.Revit.DB;

namespace Cad2Revit.Views
{
    public partial class MainForm : Form
    {
        private readonly MainViewModel _viewModel;
        private readonly BindingSource _layerBindingSource;

        public MainForm(UIApplication uiApp, RDB.Document doc)
        {
            InitializeComponent();

            _viewModel = new MainViewModel(uiApp, doc);
            _layerBindingSource = new BindingSource();

            InitializeBindings();
            InitializeLayerGrid();

            _viewModel.Initialize();
        }

        private void InitializeBindings()
        {
            lblCadStatus.DataBindings.Add("Text", _viewModel, nameof(_viewModel.CadStatusText));
            lblCadStatus.DataBindings.Add("ForeColor", _viewModel, nameof(_viewModel.CadStatusColor));

            btnReadCad.DataBindings.Add("Enabled", _viewModel, nameof(_viewModel.CanReadCad));
            btnConvert.DataBindings.Add("Enabled", _viewModel, nameof(_viewModel.CanConvert));
            btnApDungLayer.DataBindings.Add("Enabled", _viewModel, nameof(_viewModel.CanApplyLayer));

            txtChieuCaoTang1.DataBindings.Add("Text", _viewModel, nameof(_viewModel.ChieuCaoTang1), false, DataSourceUpdateMode.OnPropertyChanged);
            txtChieuCaoTangDienHinh.DataBindings.Add("Text", _viewModel, nameof(_viewModel.ChieuCaoTangDienHinh), false, DataSourceUpdateMode.OnPropertyChanged);
            txtBeDaySan.DataBindings.Add("Text", _viewModel, nameof(_viewModel.BeDaySan), false, DataSourceUpdateMode.OnPropertyChanged);
            txtDamRong.DataBindings.Add("Text", _viewModel, nameof(_viewModel.DamRong), false, DataSourceUpdateMode.OnPropertyChanged);
            txtDamCao.DataBindings.Add("Text", _viewModel, nameof(_viewModel.DamCao), false, DataSourceUpdateMode.OnPropertyChanged);
            txtSoTang.DataBindings.Add("Text", _viewModel, nameof(_viewModel.SoTang), false, DataSourceUpdateMode.OnPropertyChanged);

            chkWalls.DataBindings.Add("Checked", _viewModel, nameof(_viewModel.ConvertWalls), false, DataSourceUpdateMode.OnPropertyChanged);
            chkColumns.DataBindings.Add("Checked", _viewModel, nameof(_viewModel.ConvertColumns), false, DataSourceUpdateMode.OnPropertyChanged);
            chkBeams.DataBindings.Add("Checked", _viewModel, nameof(_viewModel.ConvertBeams), false, DataSourceUpdateMode.OnPropertyChanged);
            chkFloors.DataBindings.Add("Checked", _viewModel, nameof(_viewModel.ConvertFloors), false, DataSourceUpdateMode.OnPropertyChanged);


            lblStatWall.DataBindings.Add("Text", _viewModel, nameof(_viewModel.StatWall));
            lblStatCol.DataBindings.Add("Text", _viewModel, nameof(_viewModel.StatCol));
            lblStatBeam.DataBindings.Add("Text", _viewModel, nameof(_viewModel.StatBeam));
            lblStatFloor.DataBindings.Add("Text", _viewModel, nameof(_viewModel.StatFloor));
        }

        private void InitializeLayerGrid()
        {
            gridLayer.AutoGenerateColumns = false;
            colTenLayer.DataPropertyName = "LayerName";
            colLoaiCauKien.DataPropertyName = "CategoryName";
            colLoaiCauKien.Items.Clear();
            colLoaiCauKien.Items.AddRange(new object[] { "Tường", "Cột", "Dầm", "Sàn", "Bỏ qua" });

            _layerBindingSource.DataSource = _viewModel.LayerMapItems;
            gridLayer.DataSource = _layerBindingSource;
        }

        private void btnReadCad_Click(object sender, EventArgs e)
        {
            try
            {
                _viewModel.ReadCad();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnApDungLayer_Click(object sender, EventArgs e)
        {
            if (!_viewModel.CanApplyLayer)
            {
                MessageBox.Show(
                    "Hãy đọc CAD trước!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _viewModel.ApplyLayer();
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (!_viewModel.CanConvert)
            {
                MessageBox.Show(
                    "Hãy đọc CAD trước!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!_viewModel.HasValidSettings)
            {
                MessageBox.Show(
                    "Nhập đủ: H tầng 1, H tầng ĐH, độ dày sàn, kích thước dầm, số tầng.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult xacNhan = MessageBox.Show(
                "Tạo mô hình 3D?\n\n" + _viewModel.ThongSoTuDong,
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (xacNhan != DialogResult.Yes)
                return;

            btnConvert.Enabled = false;

            try
            {
                string ketQua = _viewModel.Convert();
                if (!string.IsNullOrEmpty(ketQua))
                {
                    MessageBox.Show(
                        ketQua,
                        "Hoàn thành",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConvert.Enabled = _viewModel.CanConvert;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
