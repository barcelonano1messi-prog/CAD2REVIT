# Hướng Dẫn Chạy & Debug Project Cad2Revit

## 🚀 Cách Chạy Project

### Bước 1: Chuẩn Bị Môi Trường
```
✓ Cài Visual Studio 2022
✓ Cài .NET Framework 4.8
✓ Cài Autodesk Revit 2024
✓ Mở project Cad2Revit.sln
```

### Bước 2: Build Project
1. Chuột phải vào project → **Build**
2. Chờ đến khi thấy dòng: `Build succeeded`
3. Nếu có lỗi màu đỏ → xem phần **Xử Lý Lỗi** dưới đây

### Bước 3: Chạy Add-in
1. **Bắt đầu Revit** (bấm F5 hoặc Ctrl+F5)
   - Visual Studio sẽ tự động:
     - Build project
     - Copy file add-in vào thư mục Revit
     - Khởi động Revit
     - Load add-in

2. **Trong Revit**:
   - Tạo project mới: `New → Project`
   - Tìm nút **"Cad2Revit"** trên thanh Ribbon (dải menu)
   - Click nút để mở giao diện chính

### Bước 4: Sử Dụng Add-in
1. **Click "Đọc CAD"** → chọn file `.dwg` hoặc `.dxf`
2. **Chỉnh sửa bảng lớp CAD**:
   - Chọn loại từ cột thứ 2 (Tường, Cột, Dầm, Sàn)
   - Nhập thông số (độ dày, chiều cao, v.v.)
3. **Click "Chuyển Đổi"** → chờ hoàn thành
4. **Kiểm tra Revit** → các phần tử được tạo

---

## 🐛 Xử Lý Lỗi Khi Build

### Lỗi 1: "Cannot find Revit API"
```
Lỗi: 
  The type or namespace name 'Autodesk' could not be found
  
Giải pháp:
  1. Chuột phải Project → Add Reference
  2. Browse → C:\Program Files\Autodesk\Revit 2024\
  3. Chọn: RevitAPI.dll, RevitAPIUI.dll
  4. Click Add → OK
```

### Lỗi 2: "Cannot find CAD reader library"
```
Lỗi:
  DLL reference missing hoặc file .addin không tìm thấy
  
Giải pháp:
  1. Xóa thư mục: C:\ProgramData\Autodesk\Revit\Addins\2024\Cad2Revit
  2. Build lại project
  3. Visual Studio sẽ tự tạo lại
```

### Lỗi 3: "Color ambiguous"
```
Lỗi:
  'Color' is an ambiguous reference between 'System.Drawing.Color' 
  and 'Autodesk.Revit.DB.Color'
  
Giải pháp (đã sửa trong code):
  Thêm dòng này ở đầu file:
  using DrawingColor = System.Drawing.Color;
  
  Sau đó dùng: DrawingColor.Red thay vì Color.Red
```

---

## 🔍 Cách Debug (Tìm Lỗi)

### Phương Pháp 1: Sử Dụng Breakpoint

```csharp
// Trong Visual Studio, click vào số dòng trái để đặt breakpoint (chấm đỏ)

private void ReadCadFile()
{
    // Breakpoint ở dòng này
    _cadReader.Read(filePath);  // ← Click vào đây
    
    // Nhấn F5 (Start Debugging)
    // Khi chạy đến dòng này, nó sẽ dừng
    // Bạn có thể xem giá trị của biến trong cửa sổ "Locals"
}
```

### Phương Pháp 2: Sử Dụng Ghi Log

```csharp
// Trong file MainViewModel.cs hoặc ElementCreator.cs:

_logger.GhiThongTin("Bắt đầu đọc file CAD");  // Ghi thông tin (xanh)
_logger.GhiCanhBao("Không tìm thấy layer");  // Ghi cảnh báo (vàng)
_logger.GhiLoi("Lỗi: file không đúng format");  // Ghi lỗi (đỏ)

// Các dòng này sẽ hiển thị trong ô "Log" của giao diện
// Giúp bạn theo dõi chương trình đang làm gì
```

### Phương Pháp 3: Output Window (Cửa Sổ Output)

```
Khi debug (F5):
  - Mở Debug → Windows → Output
  - Hoặc nhấn Ctrl + Alt + O
  
Các thông báo từ Console.WriteLine sẽ hiển thị ở đây
```

---

## 💡 Mẹo Debug Hiệu Quả

### 1. Sửa và Chạy Lại (Edit and Continue)
```
Không cần restart Revit khi code nhỏ thay đổi:
  - Sửa code
  - Nhấn Ctrl+Shift+B (Build)
  - Chạy test lại
```

### 2. Ghi Log Chi Tiết
```csharp
public bool CreateFloor(...)
{
    _logger.GhiThongTin($"Bắt đầu tạo sàn, độ dày: {thickness}mm");
    
    FloorType floorType = _tcvn.SelectFloorType(thickness);
    if (floorType == null)
    {
        _logger.GhiLoi("Không tìm được FloorType phù hợp!");
        return false;
    }
    
    _logger.GhiThongTin($"Chọn FloorType: {floorType.Name}");
    
    // ... tiếp tục code ...
    
    _logger.GhiThongTin("Tạo sàn thành công!");
    return true;
}
```

### 3. Kiểm Tra Null (Null Check)
```csharp
// Trước khi dùng một object, luôn kiểm tra null:

FloorType floorType = _tcvn.SelectFloorType(thickness);

if (floorType == null)
{
    _logger.GhiLoi("FloorType là null!");
    return false;  // Thoát sớm
}

// Nếu không null, tiếp tục sử dụng
Floor floor = Floor.Create(_doc, curves, floorType.Id, levelId);
```

---

## 📋 Checklist Trước Khi Chạy

- [ ] Cài Revit 2024 đúng phiên bản
- [ ] Visual Studio có thể build project (nhấn Ctrl+B)
- [ ] Revit không chạy (nếu có, tắt trước)
- [ ] File CAD test (`.dwg` hoặc `.dxf`) đã có sẵn
- [ ] Có project Revit (`.rvt`) để import vào

---

## 🎯 Quy Trình Test Toàn Bộ

### Test 1: Build Thành Công
```
1. Ctrl + Shift + B (Build Solution)
2. Xem Output window → "Build succeeded"
3. Không có lỗi màu đỏ
```

### Test 2: Add-in Load Thành Công
```
1. F5 (Start Debugging)
2. Revit mở lên
3. Tìm nút "Cad2Revit" trên Ribbon
4. Nút tồn tại, không bị disable
```

### Test 3: Đọc File CAD
```
1. Click "Đọc CAD"
2. Chọn file `.dwg` test
3. Chờ 2-3 giây
4. Bảng lớp CAD hiển thị dữ liệu
5. Log có thông báo "Đọc CAD thành công"
```

### Test 4: Chuyển Đổi Thành Công
```
1. Chọn lớp CAD muốn convert
2. Nhập thông số (độ dày, chiều cao)
3. Click "Chuyển Đổi"
4. Chờ 5-10 giây (tùy số lượng phần tử)
5. Log có thông báo "Tạo sàn thành công"
6. Kiểm tra Revit → có sàn mới được tạo
```

---

## 🚨 Lỗi Thường Gặp & Cách Sửa

| Lỗi | Dấu Hiệu | Giải Pháp |
|-----|---------|----------|
| **Add-in không load** | Nút không xuất hiện | Xóa thư mục Addins, build lại |
| **FloorType null** | Không tạo được sàn | Kiểm tra file CAD có layer "Floor" không |
| **Unit mismatch** | Sàn to/bé quá | Kiểm tra chuyển đổi mm ↔ feet |
| **Memory leak** | Revit chậm sau chạy many times | Đóng form, restart Revit |
| **File CAD không đọc được** | Lỗi hoặc file rỗng | Kiểm tra file CAD có hợp lệ không |

---

## 📞 Khi Không Hiểu Revit API

1. **Google**: "Revit API how to [cái bạn muốn]"
2. **RevitAPI Docs**: https://www.revitapidocs.com/
3. **Revit Forum**: https://forums.autodesk.com/t5/revit-api/ct-p/area-p127
4. **Sử dụng Reflection** (PowerShell):
```powershell
# Kiểm tra method của class
$path = 'C:\Program Files\Autodesk\Revit 2024\RevitAPI.dll'
Add-Type -Path $path
$type = [Autodesk.Revit.DB.Floor]
$type.GetMethods() | Where-Object { $_.Name -eq 'Create' }
```

---

**Chúc bạn debug vui vẻ! 🎉**

