namespace Cad2Revit.Views
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblCadStatus = new System.Windows.Forms.Label();
            this.grpSettings = new System.Windows.Forms.GroupBox();
            this.lblMm6 = new System.Windows.Forms.Label();
            this.txtDamCao = new System.Windows.Forms.TextBox();
            this.lblXDam = new System.Windows.Forms.Label();
            this.txtDamRong = new System.Windows.Forms.TextBox();
            this.lblDamSize = new System.Windows.Forms.Label();
            this.lblMm3 = new System.Windows.Forms.Label();
            this.txtBeDaySan = new System.Windows.Forms.TextBox();
            this.lblBeDaySan = new System.Windows.Forms.Label();
            this.lblMm2b = new System.Windows.Forms.Label();
            this.txtSoTang = new System.Windows.Forms.TextBox();
            this.chkUseExistingLevels = new System.Windows.Forms.CheckBox();
            this.lblSoTang = new System.Windows.Forms.Label();
            this.lblMm2 = new System.Windows.Forms.Label();
            this.txtChieuCaoTangDienHinh = new System.Windows.Forms.TextBox();
            this.lblChieuCaoTangDienHinh = new System.Windows.Forms.Label();
            this.lblMm1 = new System.Windows.Forms.Label();
            this.txtChieuCaoTang1 = new System.Windows.Forms.TextBox();
            this.lblChieuCaoTang1 = new System.Windows.Forms.Label();
            this.grpElements = new System.Windows.Forms.GroupBox();
            this.chkFloors = new System.Windows.Forms.CheckBox();
            this.chkBeams = new System.Windows.Forms.CheckBox();
            this.chkColumns = new System.Windows.Forms.CheckBox();
            this.chkWalls = new System.Windows.Forms.CheckBox();
            this.grpLayerMap = new System.Windows.Forms.GroupBox();
            this.gridLayer = new System.Windows.Forms.DataGridView();
            this.colTenLayer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLoaiCauKien = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.btnApDungLayer = new System.Windows.Forms.Button();
            this.grpStats = new System.Windows.Forms.GroupBox();
            this.lblStatFloor = new System.Windows.Forms.Label();
            this.lblStatLblFloor = new System.Windows.Forms.Label();
            this.lblStatBeam = new System.Windows.Forms.Label();
            this.lblStatLblBeam = new System.Windows.Forms.Label();
            this.lblStatCol = new System.Windows.Forms.Label();
            this.lblStatLblCol = new System.Windows.Forms.Label();
            this.lblStatWall = new System.Windows.Forms.Label();
            this.lblStatLblWall = new System.Windows.Forms.Label();
            this.btnReadCad = new System.Windows.Forms.Button();
            this.btnConvert = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.grpSettings.SuspendLayout();
            this.grpElements.SuspendLayout();
            this.grpLayerMap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLayer)).BeginInit();
            this.grpStats.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCadStatus
            // 
            this.lblCadStatus.Location = new System.Drawing.Point(12, 9);
            this.lblCadStatus.Name = "lblCadStatus";
            this.lblCadStatus.Size = new System.Drawing.Size(756, 20);
            this.lblCadStatus.TabIndex = 0;
            this.lblCadStatus.Text = "Đang kiểm tra...";
            // 
            // grpSettings
            // 
            this.grpSettings.Controls.Add(this.lblMm6);
            this.grpSettings.Controls.Add(this.txtDamCao);
            this.grpSettings.Controls.Add(this.lblXDam);
            this.grpSettings.Controls.Add(this.txtDamRong);
            this.grpSettings.Controls.Add(this.lblDamSize);
            this.grpSettings.Controls.Add(this.lblMm3);
            this.grpSettings.Controls.Add(this.txtBeDaySan);
            this.grpSettings.Controls.Add(this.lblBeDaySan);
            this.grpSettings.Controls.Add(this.lblMm2b);
            this.grpSettings.Controls.Add(this.txtSoTang);
            this.grpSettings.Controls.Add(this.chkUseExistingLevels);
            this.grpSettings.Controls.Add(this.lblSoTang);
            this.grpSettings.Controls.Add(this.lblMm2);
            this.grpSettings.Controls.Add(this.txtChieuCaoTangDienHinh);
            this.grpSettings.Controls.Add(this.lblChieuCaoTangDienHinh);
            this.grpSettings.Controls.Add(this.lblMm1);
            this.grpSettings.Controls.Add(this.txtChieuCaoTang1);
            this.grpSettings.Controls.Add(this.lblChieuCaoTang1);
            this.grpSettings.Location = new System.Drawing.Point(12, 35);
            this.grpSettings.Name = "grpSettings";
            this.grpSettings.Size = new System.Drawing.Size(370, 216);
            this.grpSettings.TabIndex = 1;
            this.grpSettings.TabStop = false;
            this.grpSettings.Text = "Thông số nhập tay";
            // 
            // lblMm6
            // 
            this.lblMm6.AutoSize = true;
            this.lblMm6.Location = new System.Drawing.Point(241, 136);
            this.lblMm6.Name = "lblMm6";
            this.lblMm6.Size = new System.Drawing.Size(25, 13);
            this.lblMm6.TabIndex = 16;
            this.lblMm6.Text = "mm";
            // 
            // txtDamCao
            // 
            this.txtDamCao.Location = new System.Drawing.Point(193, 133);
            this.txtDamCao.Name = "txtDamCao";
            this.txtDamCao.Size = new System.Drawing.Size(45, 22);
            this.txtDamCao.TabIndex = 15;
            // 
            // lblXDam
            // 
            this.lblXDam.AutoSize = true;
            this.lblXDam.Location = new System.Drawing.Point(178, 136);
            this.lblXDam.Name = "lblXDam";
            this.lblXDam.Size = new System.Drawing.Size(15, 13);
            this.lblXDam.TabIndex = 14;
            this.lblXDam.Text = "×";
            // 
            // txtDamRong
            // 
            this.txtDamRong.Location = new System.Drawing.Point(130, 133);
            this.txtDamRong.Name = "txtDamRong";
            this.txtDamRong.Size = new System.Drawing.Size(45, 22);
            this.txtDamRong.TabIndex = 13;
            // 
            // lblDamSize
            // 
            this.lblDamSize.AutoSize = true;
            this.lblDamSize.Location = new System.Drawing.Point(8, 136);
            this.lblDamSize.Name = "lblDamSize";
            this.lblDamSize.Size = new System.Drawing.Size(99, 13);
            this.lblDamSize.TabIndex = 12;
            this.lblDamSize.Text = "Dầm (rộng × cao):";
            // 
            // lblMm3
            // 
            this.lblMm3.AutoSize = true;
            this.lblMm3.Location = new System.Drawing.Point(183, 106);
            this.lblMm3.Name = "lblMm3";
            this.lblMm3.Size = new System.Drawing.Size(25, 13);
            this.lblMm3.TabIndex = 11;
            this.lblMm3.Text = "mm";
            // 
            // txtBeDaySan
            // 
            this.txtBeDaySan.Location = new System.Drawing.Point(130, 103);
            this.txtBeDaySan.Name = "txtBeDaySan";
            this.txtBeDaySan.Size = new System.Drawing.Size(50, 22);
            this.txtBeDaySan.TabIndex = 10;
            // 
            // lblBeDaySan
            // 
            this.lblBeDaySan.AutoSize = true;
            this.lblBeDaySan.Location = new System.Drawing.Point(8, 106);
            this.lblBeDaySan.Name = "lblBeDaySan";
            this.lblBeDaySan.Size = new System.Drawing.Size(67, 13);
            this.lblBeDaySan.TabIndex = 9;
            this.lblBeDaySan.Text = "Độ dày sàn:";
            // 
            // lblMm2b
            // 
            this.lblMm2b.AutoSize = true;
            this.lblMm2b.Location = new System.Drawing.Point(183, 50);
            this.lblMm2b.Name = "lblMm2b";
            this.lblMm2b.Size = new System.Drawing.Size(31, 13);
            this.lblMm2b.TabIndex = 8;
            this.lblMm2b.Text = "tầng";
            // 
            // txtSoTang
            // 
            this.txtSoTang.Location = new System.Drawing.Point(130, 47);
            this.txtSoTang.Name = "txtSoTang";
            this.txtSoTang.Size = new System.Drawing.Size(50, 22);
            this.txtSoTang.TabIndex = 7;
            // 
            // chkUseExistingLevels
            // 
            this.chkUseExistingLevels.AutoSize = true;
            this.chkUseExistingLevels.Location = new System.Drawing.Point(130, 76);
            this.chkUseExistingLevels.Name = "chkUseExistingLevels";
            this.chkUseExistingLevels.Size = new System.Drawing.Size(170, 17);
            this.chkUseExistingLevels.TabIndex = 8;
            this.chkUseExistingLevels.Text = "Dùng Level Revit có sẵn";
            this.chkUseExistingLevels.UseVisualStyleBackColor = true;
            this.chkUseExistingLevels.Visible = false;
            // 
            // lblSoTang
            // 
            this.lblSoTang.AutoSize = true;
            this.lblSoTang.Location = new System.Drawing.Point(8, 50);
            this.lblSoTang.Name = "lblSoTang";
            this.lblSoTang.Size = new System.Drawing.Size(50, 13);
            this.lblSoTang.TabIndex = 6;
            this.lblSoTang.Text = "Số tầng:";
            // 
            // lblMm2
            // 
            this.lblMm2.AutoSize = true;
            this.lblMm2.Location = new System.Drawing.Point(328, 22);
            this.lblMm2.Name = "lblMm2";
            this.lblMm2.Size = new System.Drawing.Size(25, 13);
            this.lblMm2.TabIndex = 5;
            this.lblMm2.Text = "mm";
            // 
            // txtChieuCaoTangDienHinh
            // 
            this.txtChieuCaoTangDienHinh.Location = new System.Drawing.Point(275, 19);
            this.txtChieuCaoTangDienHinh.Name = "txtChieuCaoTangDienHinh";
            this.txtChieuCaoTangDienHinh.Size = new System.Drawing.Size(50, 22);
            this.txtChieuCaoTangDienHinh.TabIndex = 4;
            // 
            // lblChieuCaoTangDienHinh
            // 
            this.lblChieuCaoTangDienHinh.AutoSize = true;
            this.lblChieuCaoTangDienHinh.Location = new System.Drawing.Point(210, 22);
            this.lblChieuCaoTangDienHinh.Name = "lblChieuCaoTangDienHinh";
            this.lblChieuCaoTangDienHinh.Size = new System.Drawing.Size(64, 13);
            this.lblChieuCaoTangDienHinh.TabIndex = 3;
            this.lblChieuCaoTangDienHinh.Text = "H tầng ĐH:";
            // 
            // lblMm1
            // 
            this.lblMm1.AutoSize = true;
            this.lblMm1.Location = new System.Drawing.Point(183, 22);
            this.lblMm1.Name = "lblMm1";
            this.lblMm1.Size = new System.Drawing.Size(25, 13);
            this.lblMm1.TabIndex = 2;
            this.lblMm1.Text = "mm";
            // 
            // txtChieuCaoTang1
            // 
            this.txtChieuCaoTang1.Location = new System.Drawing.Point(130, 19);
            this.txtChieuCaoTang1.Name = "txtChieuCaoTang1";
            this.txtChieuCaoTang1.Size = new System.Drawing.Size(50, 22);
            this.txtChieuCaoTang1.TabIndex = 1;
            // 
            // lblChieuCaoTang1
            // 
            this.lblChieuCaoTang1.AutoSize = true;
            this.lblChieuCaoTang1.Location = new System.Drawing.Point(8, 22);
            this.lblChieuCaoTang1.Name = "lblChieuCaoTang1";
            this.lblChieuCaoTang1.Size = new System.Drawing.Size(97, 13);
            this.lblChieuCaoTang1.TabIndex = 0;
            this.lblChieuCaoTang1.Text = "Chiều cao tầng 1:";
            // 
            // grpElements
            // 
            this.grpElements.Controls.Add(this.chkFloors);
            this.grpElements.Controls.Add(this.chkBeams);
            this.grpElements.Controls.Add(this.chkColumns);
            this.grpElements.Controls.Add(this.chkWalls);
            this.grpElements.Location = new System.Drawing.Point(394, 35);
            this.grpElements.Name = "grpElements";
            this.grpElements.Size = new System.Drawing.Size(374, 178);
            this.grpElements.TabIndex = 2;
            this.grpElements.TabStop = false;
            this.grpElements.Text = "Tạo cấu kiện";
            // 
            // chkFloors
            // 
            this.chkFloors.AutoSize = true;
            this.chkFloors.Checked = true;
            this.chkFloors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFloors.Location = new System.Drawing.Point(15, 115);
            this.chkFloors.Name = "chkFloors";
            this.chkFloors.Size = new System.Drawing.Size(102, 17);
            this.chkFloors.TabIndex = 3;
            this.chkFloors.Text = "Tạo Sàn (Floor)";
            this.chkFloors.UseVisualStyleBackColor = true;
            // 
            // chkBeams
            // 
            this.chkBeams.AutoSize = true;
            this.chkBeams.Checked = true;
            this.chkBeams.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBeams.Location = new System.Drawing.Point(15, 85);
            this.chkBeams.Name = "chkBeams";
            this.chkBeams.Size = new System.Drawing.Size(107, 17);
            this.chkBeams.TabIndex = 2;
            this.chkBeams.Text = "Tạo Dầm (Beam)";
            this.chkBeams.UseVisualStyleBackColor = true;
            // 
            // chkColumns
            // 
            this.chkColumns.AutoSize = true;
            this.chkColumns.Checked = true;
            this.chkColumns.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkColumns.Location = new System.Drawing.Point(15, 55);
            this.chkColumns.Name = "chkColumns";
            this.chkColumns.Size = new System.Drawing.Size(114, 17);
            this.chkColumns.TabIndex = 1;
            this.chkColumns.Text = "Tạo Cột (Column)";
            this.chkColumns.UseVisualStyleBackColor = true;
            // 
            // chkWalls
            // 
            this.chkWalls.AutoSize = true;
            this.chkWalls.Checked = true;
            this.chkWalls.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWalls.Location = new System.Drawing.Point(15, 25);
            this.chkWalls.Name = "chkWalls";
            this.chkWalls.Size = new System.Drawing.Size(112, 17);
            this.chkWalls.TabIndex = 0;
            this.chkWalls.Text = "Tạo Tường (Wall)";
            this.chkWalls.UseVisualStyleBackColor = true;
            // 
            // grpLayerMap
            // 
            this.grpLayerMap.Controls.Add(this.gridLayer);
            this.grpLayerMap.Controls.Add(this.btnApDungLayer);
            this.grpLayerMap.Location = new System.Drawing.Point(12, 245);
            this.grpLayerMap.Name = "grpLayerMap";
            this.grpLayerMap.Size = new System.Drawing.Size(756, 188);
            this.grpLayerMap.TabIndex = 3;
            this.grpLayerMap.TabStop = false;
            this.grpLayerMap.Text = "Layer CAD";
            // 
            // gridLayer
            // 
            this.gridLayer.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridLayer.BackgroundColor = System.Drawing.Color.White;
            this.gridLayer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridLayer.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTenLayer,
            this.colLoaiCauKien});
            this.gridLayer.Location = new System.Drawing.Point(8, 58);
            this.gridLayer.Name = "gridLayer";
            this.gridLayer.RowHeadersVisible = false;
            this.gridLayer.Size = new System.Drawing.Size(738, 118);
            this.gridLayer.TabIndex = 2;
            // 
            // colTenLayer
            // 
            this.colTenLayer.HeaderText = "Tên Layer trong file CAD";
            this.colTenLayer.Name = "colTenLayer";
            // 
            // colLoaiCauKien
            // 
            this.colLoaiCauKien.HeaderText = "Loại cấu kiện";
            this.colLoaiCauKien.Items.AddRange(new object[] {
            "Tường",
            "Cột",
            "Dầm",
            "Sàn",
            "Bỏ qua"});
            this.colLoaiCauKien.Name = "colLoaiCauKien";
            // 
            // btnApDungLayer
            // 
            this.btnApDungLayer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.btnApDungLayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApDungLayer.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnApDungLayer.ForeColor = System.Drawing.Color.White;
            this.btnApDungLayer.Location = new System.Drawing.Point(626, 16);
            this.btnApDungLayer.Name = "btnApDungLayer";
            this.btnApDungLayer.Size = new System.Drawing.Size(120, 36);
            this.btnApDungLayer.TabIndex = 1;
            this.btnApDungLayer.Text = "Áp dụng Gán Layer";
            this.btnApDungLayer.UseVisualStyleBackColor = false;
            this.btnApDungLayer.Click += new System.EventHandler(this.btnApDungLayer_Click);
            // 
            // grpStats
            // 
            this.grpStats.Controls.Add(this.lblStatFloor);
            this.grpStats.Controls.Add(this.lblStatLblFloor);
            this.grpStats.Controls.Add(this.lblStatBeam);
            this.grpStats.Controls.Add(this.lblStatLblBeam);
            this.grpStats.Controls.Add(this.lblStatCol);
            this.grpStats.Controls.Add(this.lblStatLblCol);
            this.grpStats.Controls.Add(this.lblStatWall);
            this.grpStats.Controls.Add(this.lblStatLblWall);
            this.grpStats.Location = new System.Drawing.Point(12, 445);
            this.grpStats.Name = "grpStats";
            this.grpStats.Size = new System.Drawing.Size(756, 46);
            this.grpStats.TabIndex = 4;
            this.grpStats.TabStop = false;
            this.grpStats.Text = "Thống kê";
            // 
            // lblStatFloor
            // 
            this.lblStatFloor.AutoSize = true;
            this.lblStatFloor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatFloor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(84)))), ((int)(((byte)(166)))));
            this.lblStatFloor.Location = new System.Drawing.Point(567, 18);
            this.lblStatFloor.Name = "lblStatFloor";
            this.lblStatFloor.Size = new System.Drawing.Size(19, 15);
            this.lblStatFloor.TabIndex = 7;
            this.lblStatFloor.Text = "—";
            // 
            // lblStatLblFloor
            // 
            this.lblStatLblFloor.AutoSize = true;
            this.lblStatLblFloor.Location = new System.Drawing.Point(537, 18);
            this.lblStatLblFloor.Name = "lblStatLblFloor";
            this.lblStatLblFloor.Size = new System.Drawing.Size(29, 13);
            this.lblStatLblFloor.TabIndex = 6;
            this.lblStatLblFloor.Text = "Sàn:";
            // 
            // lblStatBeam
            // 
            this.lblStatBeam.AutoSize = true;
            this.lblStatBeam.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatBeam.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(84)))), ((int)(((byte)(166)))));
            this.lblStatBeam.Location = new System.Drawing.Point(422, 18);
            this.lblStatBeam.Name = "lblStatBeam";
            this.lblStatBeam.Size = new System.Drawing.Size(19, 15);
            this.lblStatBeam.TabIndex = 5;
            this.lblStatBeam.Text = "—";
            // 
            // lblStatLblBeam
            // 
            this.lblStatLblBeam.AutoSize = true;
            this.lblStatLblBeam.Location = new System.Drawing.Point(387, 18);
            this.lblStatLblBeam.Name = "lblStatLblBeam";
            this.lblStatLblBeam.Size = new System.Drawing.Size(33, 13);
            this.lblStatLblBeam.TabIndex = 4;
            this.lblStatLblBeam.Text = "Dầm:";
            // 
            // lblStatCol
            // 
            this.lblStatCol.AutoSize = true;
            this.lblStatCol.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatCol.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(84)))), ((int)(((byte)(166)))));
            this.lblStatCol.Location = new System.Drawing.Point(272, 18);
            this.lblStatCol.Name = "lblStatCol";
            this.lblStatCol.Size = new System.Drawing.Size(19, 15);
            this.lblStatCol.TabIndex = 3;
            this.lblStatCol.Text = "—";
            // 
            // lblStatLblCol
            // 
            this.lblStatLblCol.AutoSize = true;
            this.lblStatLblCol.Location = new System.Drawing.Point(242, 18);
            this.lblStatLblCol.Name = "lblStatLblCol";
            this.lblStatLblCol.Size = new System.Drawing.Size(28, 13);
            this.lblStatLblCol.TabIndex = 2;
            this.lblStatLblCol.Text = "Cột:";
            // 
            // lblStatWall
            // 
            this.lblStatWall.AutoSize = true;
            this.lblStatWall.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatWall.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(84)))), ((int)(((byte)(166)))));
            this.lblStatWall.Location = new System.Drawing.Point(142, 18);
            this.lblStatWall.Name = "lblStatWall";
            this.lblStatWall.Size = new System.Drawing.Size(19, 15);
            this.lblStatWall.TabIndex = 1;
            this.lblStatWall.Text = "—";
            // 
            // lblStatLblWall
            // 
            this.lblStatLblWall.AutoSize = true;
            this.lblStatLblWall.Location = new System.Drawing.Point(95, 18);
            this.lblStatLblWall.Name = "lblStatLblWall";
            this.lblStatLblWall.Size = new System.Drawing.Size(43, 13);
            this.lblStatLblWall.TabIndex = 0;
            this.lblStatLblWall.Text = "Tường:";
            // 
            // btnReadCad
            // 
            this.btnReadCad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.btnReadCad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReadCad.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnReadCad.ForeColor = System.Drawing.Color.White;
            this.btnReadCad.Location = new System.Drawing.Point(12, 502);
            this.btnReadCad.Name = "btnReadCad";
            this.btnReadCad.Size = new System.Drawing.Size(110, 34);
            this.btnReadCad.TabIndex = 5;
            this.btnReadCad.Text = "Đọc CAD";
            this.btnReadCad.UseVisualStyleBackColor = false;
            this.btnReadCad.Click += new System.EventHandler(this.btnReadCad_Click);
            // 
            // btnConvert
            // 
            this.btnConvert.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(124)))), ((int)(((byte)(16)))));
            this.btnConvert.Enabled = false;
            this.btnConvert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConvert.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnConvert.ForeColor = System.Drawing.Color.White;
            this.btnConvert.Location = new System.Drawing.Point(134, 502);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(190, 34);
            this.btnConvert.TabIndex = 6;
            this.btnConvert.Text = "Chuyển đổi sang 3D";
            this.btnConvert.UseVisualStyleBackColor = false;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // btnClose
            // 
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Location = new System.Drawing.Point(704, 502);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(64, 34);
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "Đóng";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 552);
            this.Controls.Add(this.lblCadStatus);
            this.Controls.Add(this.grpSettings);
            this.Controls.Add(this.grpElements);
            this.Controls.Add(this.grpLayerMap);
            this.Controls.Add(this.grpStats);
            this.Controls.Add(this.btnReadCad);
            this.Controls.Add(this.btnConvert);
            this.Controls.Add(this.btnClose);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CAD 2D → Revit 3D Converter";
            this.grpSettings.ResumeLayout(false);
            this.grpSettings.PerformLayout();
            this.grpElements.ResumeLayout(false);
            this.grpElements.PerformLayout();
            this.grpLayerMap.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridLayer)).EndInit();
            this.grpStats.ResumeLayout(false);
            this.grpStats.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblCadStatus;
        private System.Windows.Forms.GroupBox grpSettings;
        private System.Windows.Forms.Label lblChieuCaoTang1;
        private System.Windows.Forms.TextBox txtChieuCaoTang1;
        private System.Windows.Forms.Label lblMm1;
        private System.Windows.Forms.Label lblChieuCaoTangDienHinh;
        private System.Windows.Forms.TextBox txtChieuCaoTangDienHinh;
        private System.Windows.Forms.Label lblMm2;
        private System.Windows.Forms.Label lblSoTang;
        private System.Windows.Forms.TextBox txtSoTang;
        private System.Windows.Forms.CheckBox chkUseExistingLevels;
        private System.Windows.Forms.Label lblMm2b;
        private System.Windows.Forms.Label lblBeDaySan;
        private System.Windows.Forms.TextBox txtBeDaySan;
        private System.Windows.Forms.Label lblMm3;
        private System.Windows.Forms.Label lblDamSize;
        private System.Windows.Forms.TextBox txtDamRong;
        private System.Windows.Forms.Label lblXDam;
        private System.Windows.Forms.TextBox txtDamCao;
        private System.Windows.Forms.Label lblMm6;
        private System.Windows.Forms.GroupBox grpElements;
        private System.Windows.Forms.CheckBox chkWalls;
        private System.Windows.Forms.CheckBox chkColumns;
        private System.Windows.Forms.CheckBox chkBeams;
        private System.Windows.Forms.CheckBox chkFloors;
        private System.Windows.Forms.GroupBox grpLayerMap;
        private System.Windows.Forms.Button btnApDungLayer;
        private System.Windows.Forms.DataGridView gridLayer;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTenLayer;
        private System.Windows.Forms.DataGridViewComboBoxColumn colLoaiCauKien;
        private System.Windows.Forms.GroupBox grpStats;
        private System.Windows.Forms.Label lblStatLblWall;
        private System.Windows.Forms.Label lblStatWall;
        private System.Windows.Forms.Label lblStatLblCol;
        private System.Windows.Forms.Label lblStatCol;
        private System.Windows.Forms.Label lblStatLblBeam;
        private System.Windows.Forms.Label lblStatBeam;
        private System.Windows.Forms.Label lblStatLblFloor;
        private System.Windows.Forms.Label lblStatFloor;
        private System.Windows.Forms.Button btnReadCad;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.Button btnClose;
    }
}
