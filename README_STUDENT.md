# 🏗️ CAD 2D → Revit 3D Converter (Cad2Revit)

**Đồ án tốt nghiệp | Revit API 2024 | C# .NET Framework 4.8 | MVVM Pattern**

---

## 📌 Mục Đích Project

Tự động chuyên đổi dữ liệu từ file vẽ kỹ thuật CAD (2D) sang mô hình Revit 3D, giúp kiến trúc sư tiết kiệm 80% thời gian chuyên đồi bản vẽ.

### Vấn Đề
- Kiến trúc sư nhận bản vẽ CAD từ khách hàng
- Phải vẽ lại tất cả các lớp (sàn, cột, dầm) trong Revit
- Mất 5-10 ngày cho mỗi dự án

### Giải Pháp
Add-in này cho phép user:
1. Chọn file CAD
2. Chỉ định lớp CAD nào → Loại phần tử nào (Sàn/Cột/Dầm)
3. Nhập thông số (độ dày, chiều cao, v.v.)
4. **Click 1 nút → tất cả được tạo tự động**

---

## 🏗️ Kiến Trúc Project

```
Cad2Revit/
│
├─ Core/
│  ├─ App.cs          → Khởi tạo add-in khi Revit mở
│  └─ Command.cs      → Xử lý lệnh khi user bấm nút
│
├─ Views/ (UI)
│  ├─ MainForm.cs            → Giao diện người dùng (WinForms)
│  └─ MainForm.Designer.cs   → Thiết kế giao diện
│
├─ ViewModels/
│  ├─ MainViewModel.cs  → Logic xử lý chính (MVVM)
│  ├─ LayerMapItem.cs   → Model dữ liệu 1 lớp CAD
│  └─ ViewModelBase.cs  → Base class (INotifyPropertyChanged)
│
├─ Converter/
│  ├─ CadReader.cs              → Đọc file CAD
│  ├─ CadGeometryAnalyzer.cs    → Phân tích hình học
│  ├─ ElementCreator.cs         → Tạo phần tử Revit
│  ├─ RevitTcvnHelper.cs        → Áp dụng tiêu chuẩn TCVN
│  ├─ FloorContourBuilder.cs    → Xây dựng đường bao sàn
│  └─ LayerMapper.cs            → Ánh xạ lớp CAD
│
├─ Helpers/
│  ├─ UnitHelper.cs             → Chuyên đổi đơn vị (mm ↔ feet)
│  ├─ Logger.cs                 → Ghi log
│  ├─ CadCleanupHelper.cs       → Làm sạch dữ liệu CAD
│  ├─ CadCoordinateHelper.cs    → Chuyên đổi tọa độ
│  ├─ RevitGridHelper.cs        → Tạo Grid
│  ├─ RevitLevelHelper.cs       → Tạo Level
│  └─ Tcvn5574Catalog.cs        → Tiêu chuẩn TCVN 5574
│
├─ Models/
│  ├─ CadElement.cs             → Thông tin phần tử CAD
│  └─ ConversionSettings.cs     → Cài đặt chuyên đổi
│
└─ Cad2Revit.addin              → Manifest đăng ký add-in
```

---

## 🔄 Luồng Hoạt Động

```
1️⃣  User click nút "Cad2Revit" trong Revit
    ↓
2️⃣  Command.cs → Mở MainForm (giao diện)
    ↓
3️⃣  User nhập: file CAD, lớp, thông số
    ↓
4️⃣  MainViewModel.Convert() kích hoạt
    ├─ CadReader: Đọc file CAD
    ├─ CadGeometryAnalyzer: Phân tích
    ├─ FOR EACH layer:
    │   ├─ ElementCreator: Tạo phần tử
    │   ├─ RevitTcvnHelper: Chọn loại
    │   └─ Revit API: Floor.Create()
    ↓
5️⃣  Revit: Hiển thị các phần tử mới
    ↓
6️⃣  Log: Ghi chi tiết hoàn thành
```

---

## 🛠️ Công Nghệ Sử Dụng

| Thành Phần | Công Nghệ |
|-----------|----------|
| Ngôn ngữ | C# |
| Framework | .NET Framework 4.8 |
| IDE | Visual Studio 2022 |
| API | Autodesk Revit API 2024 |
| Giao diện | Windows Forms |
| Mô hình | MVVM |

---

## 💡 Tính Năng Chính

✅ Đọc file CAD (.dwg, .dxf)  
✅ Tạo sàn, cột, dầm tự động  
✅ Phân tích hình học CAD  
✅ Tạo Grid & Level tự động  
✅ Áp dụng tiêu chuẩn TCVN 5574  
✅ Điều chỉnh độ dày sàn  
✅ Ghi log chi tiết  

---

## 🚀 Cách Chạy Project

### 1️⃣ Chuẩn Bị
```
✓ Visual Studio 2022
✓ .NET Framework 4.8
✓ Autodesk Revit 2024
```

### 2️⃣ Mở & Build
```bash
1. Mở Cad2Revit.sln
2. Ctrl + Shift + B (Build)
3. Chờ "Build succeeded"
```

### 3️⃣ Chạy
```bash
1. Nhấn F5 (Start Debugging)
2. Visual Studio tự động:
   - Build
   - Khởi động Revit
   - Load add-in
3. Tìm nút "Cad2Revit" trên Ribbon
```

### 4️⃣ Sử Dụng
```
1. Click nút "Cad2Revit"
2. Chọn file CAD
3. Chỉ định lớp → Loại phần tử
4. Nhập thông số
5. Click "Chuyên Đổi"
6. Kiểm tra Revit
```

---

## 📚 Tài Liệu Hướng Dẫn

| File | Mục Đích |
|------|---------|
| **[HUONG_DAN_PROJECT.md](HUONG_DAN_PROJECT.md)** | Tổng quan + cấu trúc |
| **[CHEAT_SHEET.md](CHEAT_SHEET.md)** | Tóm tắt + Q&A phỏng vấn |
| **[HUONG_DAN_CHAY_DEBUG.md](HUONG_DAN_CHAY_DEBUG.md)** | Chạy, debug, xử lý lỗi |
| **[HUONG_DAN_PHONG_VAN.md](HUONG_DAN_PHONG_VAN.md)** | Chuẩn bị phỏng vấn |

---

## 📖 Lộ Trình Học Tập

### Tuần 1-2: Hiểu Cấu Trúc
- Đọc `HUONG_DAN_PROJECT.md`
- Mở từng file .cs, đọc comments
- Vẽ sơ đồ cấu trúc

### Tuần 3-4: Hiểu Luồng
- Theo dõi flow: Command → MainForm → MainViewModel → Converter
- Vẽ flowchart
- Giải thích bằng lời nói

### Tuần 5-6: Hiểu Chi Tiết
- Đọc các hàm chính (xem comments trong code):
  - `TaoSanTuContour()` → Tạo sàn
  - `TimFloorTypeTcvn()` → Chọn loại sàn
  - `ReadCad()` → Đọc CAD

### Tuần 7+: Customize
- Thêm tính năng mới
- Sửa lỗi
- Tối ưu performance

---

## 🐛 Xử Lý Lỗi Thường Gặp

| Lỗi | Giải Pháp |
|-----|----------|
| "Cannot find Revit API" | Thêm Reference: RevitAPI.dll, RevitAPIUI.dll |
| "Add-in không load" | Xóa `C:\ProgramData\Autodesk\Revit\Addins\2024\`, build lại |
| "FloorType null" | Kiểm tra file CAD có layer hợp lệ |
| "Unit mismatch" | Kiểm tra chuyên đổi mm ↔ feet |

👉 **Chi tiết:** [HUONG_DAN_CHAY_DEBUG.md](HUONG_DAN_CHAY_DEBUG.md)

---

## 🎓 Chuẩn Bị Phỏng Vấn

Đọc **[HUONG_DAN_PHONG_VAN.md](HUONG_DAN_PHONG_VAN.md)** để chuẩn bị trả lời:
- "Project này làm gì?"
- "Cấu trúc code như nào?"
- "Gặp khó khăn gì?"
- "Giải pháp thế nào?"
- "MVVM là gì?"
- "Revit API khó như thế nào?"

---

## 📌 Các Khái Niệm Quan Trọng

| Khái Niệm | Ý Nghĩa |
|----------|--------|
| **Revit API** | Bộ lệnh để lập trình Revit |
| **Element** | Phần tử kiến trúc (Floor, Column, Beam) |
| **FloorType** | Loại sàn có sẵn trong Revit |
| **CAD Layer** | Lớp trong file CAD |
| **Level** | Tầng trong Revit (mực độ cao) |
| **MVVM** | Mô hình kiến trúc (Model-View-ViewModel) |
| **Unit Conversion** | Chuyên đổi đơn vị (mm ↔ feet) |

---

## 🔗 Tham Khảo

- **Revit API Docs**: https://www.revitapidocs.com/
- **Autodesk Developer**: https://developer.autodesk.com/
- **Revit Forum**: https://forums.autodesk.com/t5/revit-api/

---

**Chúc bạn thành công! 🎓**
