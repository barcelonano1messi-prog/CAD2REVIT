# Cheat Sheet - Tóm Tắt Nhanh Cad2Revit

## 🎯 Mục Đích Project (30 giây)

**Tôi xây dựng một phần mềm phụ trợ (add-in) cho Revit 2024. Nó giúp:**
- Đọc dữ liệu từ file vẽ kỹ thuật CAD (.dwg, .dxf)
- Tự động chuyển những dữ liệu này thành các phần tử kiến trúc trong Revit (sàn, cột, dầm)
- Áp dụng tiêu chuẩn kỹ thuật Việt Nam (TCVN 5574) cho vật liệu bê tông

**Lợi ích:** Kiến trúc sư không cần vẽ lại, tiết kiệm thời gian 80%

---

## 🏗️ Cấu Trúc Code (1 phút)

```
Core/          → Khởi động add-in, lắng nghe lệnh từ Revit
│
├─ UI/         → Giao diện (form nhập liệu)
│
├─ ViewModels/ → Xử lý logic chính (MVVM pattern)
│
├─ Converter/  → Chuyển đổi CAD → Revit
│
└─ Helpers/    → Các hàm tiện ích (chuyển đơn vị, ghi log)
```

---

## 🔄 Luồng Hoạt Động (2 phút)

```
1️⃣  USER CLICK NÚT
    ↓
2️⃣  Command.cs → Mở MainForm (giao diện)
    ↓
3️⃣  USER NHẬP LỰA CHỌN
    ├─ Chọn file CAD
    ├─ Chọn lớp CAD nào → Loại phần tử gì (Sàn/Cột/Dầm)
    ├─ Nhập thông số (độ dày sàn, chiều cao cột, v.v.)
    ↓
4️⃣  MainViewModel → Xử lý
    ├─ CadReader: Đọc file CAD
    ├─ ElementCreator: Tạo phần tử Revit
    ├─ RevitTcvnHelper: Áp dụng tiêu chuẩn TCVN
    ↓
5️⃣  KẾT QUẢ
    └─ Các phần tử được thêm vào Revit
```

---

## 📝 5 File Quan Trọng Nhất

### 1. **Core/Command.cs** (Điểm khởi động)
```csharp
// Khi user bấm nút Cad2Revit:

public Result Execute(...)
{
    // Lấy document Revit
    // Mở MainForm
    // Chờ user hoàn thành
}
```
**Cách giải thích:** 
> "Đây là lệnh được gọi khi user bấm nút trên Revit. Nó kiểm tra có project nào đang mở không, rồi hiển thị giao diện chính."

---

### 2. **Views/MainForm.cs** (Giao diện)
```csharp
// Màn hình nhập liệu

private MainViewModel _viewModel;  // Kết nối logic

// Khi user bấm nút:
btnReadCad_Click()    → Gọi ReadCad()
btnConvert_Click()    → Gọi Convert()
btnApplyLayer_Click() → Gọi ApplyLayer()

// Hiển thị dữ liệu:
gridLayer         → Bảng lớp CAD
txtBeDaySan       → Nhập độ dày
lblCadStatus      → Hiển thị trạng thái
txtLog            → Hiển thị chi tiết (debug)
```
**Cách giải thích:**
> "View là giao diện người dùng thấy. Nó không xử lý logic, chỉ hiển thị dữ liệu từ ViewModel và gọi các hàm xử lý logic khi user bấm nút."

---

### 3. **ViewModels/MainViewModel.cs** (Logic chính)
```csharp
public class MainViewModel : ViewModelBase
{
    // Đọc file CAD
    public void ReadCad()
    {
        _cadReader.Read(filePath);
        // Load lớp CAD vào LayerMapItems
    }

    // Chuyển đổi
    public void Convert()
    {
        foreach (var layer in LayerMapItems)
        {
            if (layer.Type == "Floor")
                CreateFloor(layer);
            else if (layer.Type == "Column")
                CreateColumn(layer);
            // ...
        }
    }

    // Tính toán tự động
    public string ThongSoTuDong
    {
        get
        {
            // Dựa trên số tầng, tính chiều cao
            int soTang = int.Parse(SoTang);
            return $"Tổng chiều cao: {soTang * 3}m";
        }
    }
}
```
**Cách giải thích:**
> "ViewModel là nơi xử lý toàn bộ logic. Khi MainForm gọi một hàm, ViewModel sẽ gọi Converter để chuyển đổi CAD sang Revit. ViewModel cũng tính toán các thông số tự động (ví dụ: tổng chiều cao = số tầng × 3m)."

---

### 4. **Converter/ElementCreator.cs** (Tạo phần tử Revit)
```csharp
public class ElementCreator
{
    // Tạo sàn
    public bool CreateFloor(double thickness, List<XYZ> contour, int levelId)
    {
        // 1. Chọn loại sàn (FloorType)
        FloorType floorType = _tcvn.SelectFloorType(thickness);
        
        // 2. Xây dựng đường bao sàn từ dữ liệu CAD
        CurveLoop outline = BuildOutline(contour);
        
        // 3. Tạo sàn trong Revit
        Floor floor = Floor.Create(_doc, new List<CurveLoop> { outline }, 
                                   floorType.Id, levelId);
        
        return floor != null;
    }

    // Tương tự cho cột, dầm
    public bool CreateColumn(...) { }
    public bool CreateBeam(...) { }
}
```
**Cách giải thích:**
> "ElementCreator là nơi thực tế tạo các phần tử Revit. Nó nhận dữ liệu từ CAD (tọa độ, loại phần tử) và dùng Revit API để tạo phần tử đó trong tài liệu Revit."

---

### 5. **Converter/RevitTcvnHelper.cs** (Áp dụng tiêu chuẩn)
```csharp
public class RevitTcvnHelper
{
    // Chọn loại sàn phù hợp
    public FloorType SelectFloorType(double thickness)
    {
        // 1. Tìm FloorType "Generic" (mặc định trong Revit)
        FloorType generic = FindGenericFloor();
        
        // 2. Nếu độ dày không khớp, tạo FloorType mới
        if (Math.Abs(GetThickness(generic) - thickness) > 1mm)
        {
            FloorType newType = generic.Duplicate($"Generic - {thickness}mm");
            AdjustThickness(newType, thickness);
            return newType;
        }
        
        return generic;
    }

    // Điều chỉnh độ dày sàn
    private void AdjustThickness(FloorType floorType, double thickness)
    {
        CompoundStructure cs = floorType.GetCompoundStructure();
        // Thay đổi chiều rộng của từng lớp (bê tông, xốp, lót)
        // ...
    }

    // Gán vật liệu TCVN
    public void ApplyMaterial(Floor floor, string materialName)
    {
        // Gán vật liệu bê tông theo tiêu chuẩn TCVN 5574
    }
}
```
**Cách giải thích:**
> "RevitTcvnHelper chứa logic về lựa chọn/tạo các loại sàn (FloorType) phù hợp với độ dày user nhập. Nếu không có sẵn, nó tạo mới bằng cách sao chép (Duplicate) một FloorType có sẵn và điều chỉnh độ dày."

---

## 💻 Các Thuật Ngữ Bạn Phải Biết

| Thuật Ngữ | Ý Nghĩa | Ví Dụ |
|-----------|---------|-------|
| **Add-in** | Plugin cho Revit | Cad2Revit, Dynamo |
| **Element** | Phần tử kiến trúc | Floor, Column, Beam, Wall |
| **FloorType** | Loại/kiểu sàn | Generic, Composite Floor |
| **Layer (CAD)** | Lớp trong file CAD | Layer "Floor", "Column" |
| **Level** | Tầng trong Revit | Level 0, Level 1, Level 2 |
| **Revit API** | Bộ lệnh để lập trình Revit | Floor.Create(), Column.Create() |
| **MVVM** | Mô hình kiến trúc | Model-View-ViewModel |
| **DataBinding** | Kết nối dữ liệu tự động | Khi ViewModel thay đổi, View tự cập nhật |
| **Transaction** | Nhóm các thay đổi lại | Tất cả hoặc không tất cả |

---

## 🎤 Câu Trả Lời Phỏng Vấn

### Q1: "Project này làm gì?"
**A:** 
> Tôi xây dựng một add-in cho Revit 2024. Nó tự động chuyển dữ liệu từ file CAD (bản vẽ kỹ thuật) sang Revit (phần mềm kiến trúc). Thay vì vẽ lại tất cả, người dùng chỉ cần chọn file CAD, chọn lớp nào cần chuyển (sàn, cột, dầm), nhập thông số (độ dày), và chương trình sẽ tự tạo các phần tử Revit theo tiêu chuẩn TCVN 5574.

### Q2: "Bạn dùng công nghệ gì?"
**A:** 
> C# .NET Framework 4.8, Revit API 2024, WinForms giao diện, MVVM pattern. Kiến trúc dự án tách riêng: Core (khởi động), UI (giao diện), ViewModels (logic), Converter (chuyển đổi CAD → Revit), Helpers (hàm hỗ trợ).

### Q3: "Khó khăn lớn nhất là gì?"
**A:** 
> - **Revit API phức tạp**: Cần học cách dùng các class như Floor, FloorType, CompoundStructure, Level
> - **Chuyển đổi đơn vị**: CAD dùng mm, Revit dùng feet → cần chuyển chính xác
> - **Ánh xạ lớp CAD**: Phải nhận dạng lớp CAD nào là sàn, lớp nào là cột
> - **Điều chỉnh độ dày sàn**: Khi độ dày không khớp với template, phải tạo loại sàn mới

### Q4: "Giải quyết thế nào?"
**A:** 
> - Đọc API documentation, tham khảo forum Revit
> - Tạo UnitHelper để chuyển đổi đơn vị tập trung
> - Dùng tên/màu lớp CAD để nhận dạng loại phần tử
> - Dùng Duplicate() để tạo FloorType mới với độ dày mong muốn

### Q5: "Tính năng chính?"
**A:** 
> 1. Đọc file CAD → hiển thị danh sách lớp
> 2. Chọn lớp CAD + kiểu phần tử (Sàn/Cột/Dầm)
> 3. Nhập thông số (độ dày, chiều cao, chiều rộng)
> 4. Click chuyển đổi → tạo phần tử Revit
> 5. Ghi log chi tiết để user biết chuyều xảy ra

---

## ✅ Kiểm Tra Lần Cuối Trước Phỏng Vấn

- [ ] Có thể giải thích luồng chương trình bằng sơ đồ
- [ ] Biết vai trò của mỗi folder (Core, UI, ViewModels, Converter, Helpers)
- [ ] Hiểu 5 file chính trên
- [ ] Có thể trả lời các câu Q1-Q5
- [ ] Biết cách build, chạy, debug project
- [ ] Biết các thuật ngữ quan trọng
- [ ] Có thể viết 2-3 dòng giải thích cho từng file chính

---

## 🎯 Mẹo Phỏng Vấn

1. **Nói chậm, rõ ràng** - Giáo sư không cần nghe bạn nói nhanh
2. **Vẽ sơ đồ** - Hình vẽ sơ đồ luồng giúp giáo sư hiểu rõ hơn
3. **Đặt vấn đề trước** - "Project này khó vì Revit API phức tạp, tôi giải quyết bằng..."
4. **Có ví dụ cụ thể** - Chỉ một class, một method cụ thể thay vì nói chung chung
5. **Thành thật khi không biết** - "Cái này tôi vẫn chưa hiểu rõ, nhưng tôi biết nó làm..."

---

**Chúc bạn thành công! 🎓**

