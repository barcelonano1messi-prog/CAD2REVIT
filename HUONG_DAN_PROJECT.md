# Hướng Dẫn Project Cad2Revit - Dành Cho Sinh Viên

## 📋 Mục Đích Project

Project **Cad2Revit** là một add-in cho phần mềm **Autodesk Revit 2024**. Add-in này giúp:
- Đọc file CAD (.dwg, .dxf)
- Phân tích các hình học từ CAD
- Chuyển đổi thành các thành phần kiến trúc trong Revit (sàn, cột, dầm)
- Áp dụng tiêu chuẩn TCVN 5574 (vật liệu bê tông Việt Nam)

---

## 🏗️ Cấu Trúc Project

```
Cad2Revit/
├── Core/                          # Lõi của add-in
│   ├── App.cs                     # Khởi tạo add-in khi Revit mở
│   └── Command.cs                 # Lệnh kích hoạt khi user click nút
│
├── UI/                            # Giao diện người dùng (màn hình)
│   ├── MainForm.cs                # Cửa sổ chính, xử lý sự kiện
│   └── MainForm.Designer.cs       # Thiết kế giao diện (tự động sinh)
│
├── ViewModels/                    # Xử lý logic (MVVM Pattern)
│   ├── MainViewModel.cs           # Logic chính: đọc CAD, chuyển đổi
│   ├── LayerMapItem.cs            # Thông tin 1 lớp CAD
│   └── ViewModelBase.cs           # Base class cho MVVM
│
├── Converter/                     # Chuyển đổi CAD → Revit
│   ├── CadReader.cs               # Đọc file CAD
│   ├── CadGeometryAnalyzer.cs     # Phân tích hình học CAD
│   ├── ElementCreator.cs          # Tạo phần tử Revit
│   ├── RevitTcvnHelper.cs         # Áp dụng tiêu chuẩn TCVN
│   ├── FloorContourBuilder.cs     # Xây dựng đường bao sàn
│   └── LayerMapper.cs             # Ánh xạ lớp CAD
│
├── Helpers/                       # Các hàm hỗ trợ
│   ├── UnitHelper.cs              # Chuyển đổi đơn vị (mm ↔ feet)
│   ├── Logger.cs                  # Ghi log thông tin
│   ├── CadCleanupHelper.cs        # Làm sạch dữ liệu CAD
│   └── ... (các helper khác)
│
└── Models/                        # Mô hình dữ liệu
    ├── ConversionSettings.cs      # Cài đặt chuyển đổi
    └── CadElement.cs              # Thông tin phần tử CAD
```

---

## 🔄 Luồng Hoạt Động Chính

### Bước 1: User Click Nút "Đọc CAD"
```
User mở Revit → Click "Cad2Revit" → Click "Đọc CAD"
              ↓
        Command.cs kích hoạt
              ↓
        MainForm.cs hiện lên (giao diện)
```

### Bước 2: User Chọn File CAD và Cài Đặt
```
MainForm.cs hiển thị:
- Chọn file CAD
- Chọn lớp CAD cần chuyển (bảng grid)
- Nhập độ dày sàn (ví dụ: 200mm)
- Chọn level Revit

Toàn bộ cài đặt được lưu vào: ViewModels/MainViewModel.cs
```

### Bước 3: Click "Chuyển Đổi"
```
MainViewModel.cs xử lý:
1. Đọc file CAD
   └─ CadReader.cs đọc file
   └─ CadGeometryAnalyzer.cs phân tích hình học

2. Chuyển đổi từng lớp CAD thành thành phần Revit
   └─ ElementCreator.cs tạo phần tử
   └─ RevitTcvnHelper.cs áp dụng tiêu chuẩn TCVN

3. Ghi vào Revit
   └─ Các phần tử được thêm vào mô hình Revit
```

---

## 💡 Các Khái Niệm Quan Trọng

### 1. **CAD Layer (Lớp CAD)**
- Trong file CAD, các đối tượng được nhóm vào các lớp
- Ví dụ: lớp "Floor", lớp "Column", lớp "Beam"
- Tôi cần ánh xạ lớp CAD → thành phần Revit

### 2. **Revit Element (Phần Tử Revit)**
- Là các đối tượng kiến trúc: Floor (sàn), Column (cột), Beam (dầm)
- Mỗi phần tử có kiểu (Type) và thuộc tính (Material, Level, v.v.)

### 3. **Unit Conversion (Chuyển Đổi Đơn Vị)**
- CAD sử dụng: **mm (milimetre)**
- Revit sử dụng: **feet**
- Công thức: `1mm = 0.00328084 feet`
- → File `Helpers/UnitHelper.cs` giúp chuyển đổi

### 4. **MVVM Pattern**
- **Model**: `Models/` - dữ liệu (ConversionSettings, CadElement)
- **View**: `UI/MainForm.cs` - giao diện, không xử lý logic
- **ViewModel**: `ViewModels/MainViewModel.cs` - xử lý logic, kết nối View và Model

---

## 📝 Các File Quan Trọng - Bạn Cần Hiểu

### 1. **Core/Command.cs**
```csharp
// Khi user click nút "Cad2Revit" trong Revit
public class Command : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // 1. Lấy tài liệu Revit hiện tại
        // 2. Hiển thị giao diện MainForm
        // 3. Chế độ modal (chờ user hoàn thành)
    }
}
```

### 2. **UI/MainForm.cs**
```csharp
// Giao diện người dùng
// Mục đích: Nhận input từ user (file, cài đặt)
// Không xử lý logic chuyển đổi (chỉ gọi ViewModel)

private MainViewModel _viewModel;

// Khi click "Đọc CAD":
private void BtnDocCad_Click(...)
{
    _viewModel.ReadCad(filePath);
}

// Khi click "Chuyển Đổi":
private void BtnChuyenDoi_Click(...)
{
    _viewModel.Convert();
}
```

### 3. **ViewModels/MainViewModel.cs**
```csharp
// Xử lý logic chính
public class MainViewModel : ViewModelBase
{
    // Bước 1: Đọc file CAD
    public void ReadCad(string filePath)
    {
        _cadReader.Read(filePath);  // Đọc file
        // Hiển thị danh sách lớp CAD lên grid
    }

    // Bước 2: Chuyển đổi thành Revit
    public void Convert()
    {
        foreach (var layer in SelectedLayers)
        {
            if (layer.Type == "Floor")
                _creator.CreateFloor(layer);
            else if (layer.Type == "Column")
                _creator.CreateColumn(layer);
            // ... v.v.
        }
    }
}
```

### 4. **Converter/ElementCreator.cs**
```csharp
// Tạo các phần tử Revit từ dữ liệu CAD

// Tạo sàn
public bool CreateFloor(double thickness, int level)
{
    // 1. Chọn loại sàn phù hợp (FloorType)
    FloorType floorType = _tcvn.SelectFloorType(thickness);
    
    // 2. Tạo đường bao sàn từ dữ liệu CAD
    CurveLoop outline = BuildFloorOutline();
    
    // 3. Tạo sàn trong Revit
    Floor floor = Floor.Create(_doc, outline, floorType.Id, levelId);
    
    return floor != null;
}
```

### 5. **Converter/RevitTcvnHelper.cs**
```csharp
// Áp dụng tiêu chuẩn TCVN (vật liệu bê tông Việt Nam)

public FloorType SelectFloorType(double thicknessMm)
{
    // 1. Tìm FloorType "Generic" (mặc định)
    FloorType generic = FindGenericFloor();
    
    // 2. Nếu độ dày không khớp, tạo FloorType mới
    if (generic.Thickness != thicknessMm)
    {
        FloorType newType = generic.Duplicate($"Generic - {thicknessMm}mm");
        // Điều chỉnh độ dày
        AdjustThickness(newType, thicknessMm);
        return newType;
    }
    
    return generic;
}
```

---

## 🎓 Cách Học Từng Phần

### Tuần 1: Hiểu Cấu Trúc
1. Đọc file `HUONG_DAN_PROJECT.md` này
2. Mở từng file trong project
3. Đọc comments và hiểu mục đích
4. Không cần hiểu chi tiết, chỉ cần biết chương trình làm gì

### Tuần 2: Hiểu Luồng Chính
1. Theo dõi từ `Command.cs` → `MainForm.cs` → `MainViewModel.cs`
2. Vẽ sơ đồ flow (flowchart) trên giấy
3. Viết lại bằng lời nói của bạn

### Tuần 3: Hiểu Chi Tiết
1. Đọc từng hàm (function)
2. Hiểu input, output, và logic
3. Thử viết test case (kiểm tra hàm)

### Tuần 4: Customize Project
1. Thêm tính năng mới
2. Sửa lỗi
3. Tối ưu hóa

---

## 🔑 Các Từ Khóa Quan Trọng - Bạn Phải Biết

| Từ khóa | Ý nghĩa | Ví dụ |
|---------|---------|-------|
| **Element** | Phần tử | Floor, Column, Beam |
| **FloorType** | Kiểu sàn | Generic, Composite Floor 200mm |
| **Level** | Mức độ cao | Level 0 (0m), Level 1 (3m) |
| **Layer** | Lớp CAD | Lớp Floor, lớp Column |
| **Material** | Vật liệu | Bê tông, thép, gạch |
| **CompoundStructure** | Cấu trúc lớp (sàn) | Lớp bê tông + xốp + lót |
| **Curve** | Đường cong | Đường bao sàn |
| **CurveLoop** | Vòng đường cong | Đường viền sàn khép kín |
| **Document** | Tài liệu Revit | File `.rvt` đang mở |
| **Transaction** | Giao dịch | Nhóm các thay đổi lại với nhau |

---

## 💻 Cách Giải Thích Project Khi Phỏng Vấn

### **Người phỏng vấn hỏi: "Cái project này làm gì?"**

**Trả lời:**
> "Em xây dựng một add-in (plugin) cho phần mềm Revit. Mục đích là giúp kiến trúc sư chuyển dữ liệu từ file CAD sang Revit tự động. Thay vì vẽ lại tất cả, người dùng chỉ cần chọn file CAD, chọn lớp nào cần chuyển, và chương trình sẽ tự động tạo các phần tử kiến trúc (sàn, cột, dầm) trong Revit theo tiêu chuẩn TCVN 5574."

---

### **Người phỏng vấn hỏi: "Cấu trúc code của em như nào?"**

**Trả lời:**
> "Em tổ chức project theo mô hình MVVM:
> - **Core/**: Khởi tạo add-in và lệnh
> - **UI/**: Giao diện người dùng (MainForm)
> - **ViewModels/**: Logic xử lý chính
> - **Converter/**: Chuyển đổi CAD sang Revit
> - **Helpers/**: Các hàm hỗ trợ (chuyển đổi đơn vị, ghi log)
> - **Models/**: Mô hình dữ liệu
> 
> Khi user click nút, Command.cs kích hoạt → hiển thị MainForm → MainViewModel xử lý logic → gọi ElementCreator để tạo phần tử Revit."

---

### **Người phỏng vấn hỏi: "Em gặp vấn đề gì khi làm?"**

**Trả lời:**
> "Em gặp vấn đề:
> 1. **Chuyển đổi đơn vị**: CAD dùng mm, Revit dùng feet → cần chuyển đổi chính xác
> 2. **Ánh xạ lớp CAD**: CAD có nhiều lớp, phải biết lớp nào là sàn, lớp nào là cột
> 3. **Revit API phức tạp**: Revit API có rất nhiều class và method, phải tìm cách gọi đúng
> 4. **Tạo FloorType động**: Nếu độ dày sàn không khớp với template có sẵn, phải tạo mới
> 
> Em giải quyết bằng cách:
> - Dùng UnitHelper để chuyển đổi đơn vị
> - Dùng Layer name hoặc color để nhận diện loại phần tử
> - Đọc API documentation của Revit
> - Dùng Duplicate() để tạo FloorType mới"

---

## 📚 Tài Liệu Tham Khảo

- **Revit API Documentation**: https://www.revitapidocs.com/
- **Autodesk Developer Network**: https://developer.autodesk.com/
- **TCVN 5574**: Tiêu chuẩn kỹ thuật quốc gia về bê tông cốt thép

---

## ✅ Checklist - Bạn Nên Làm

- [ ] Đọc file này hoàn toàn
- [ ] Vẽ sơ đồ cấu trúc project trên giấy
- [ ] Vẽ flowchart luồng hoạt động
- [ ] Mở từng file `.cs`, đọc code và comments
- [ ] Viết lại bằng lời nói của bạn (không copy-paste)
- [ ] Chuẩn bị câu trả lời cho các câu hỏi trên
- [ ] Thực hành: chạy project, sửa lỗi nhỏ, thêm tính năng nhỏ

---

**Chúc bạn học tốt!** 🎓

