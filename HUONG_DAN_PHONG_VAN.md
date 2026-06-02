# Hướng Dẫn Phỏng Vấn Project Cad2Revit

## 📌 Các Câu Hỏi Có Thể Được Hỏi (Theo Mức Độ)

---

## ⭐ CẤP 1: Câu Hỏi Cơ Bản

### Q1.1: "Cái project này làm gì? Nó giải quyết vấn đề gì?"

**Trả lời tốt:**
> "Tôi xây dựng một add-in cho Autodesk Revit 2024. Vấn đề: Kiến trúc sư thường có bản vẽ CAD từ khách hàng, nhưng phải vẽ lại tất cả trong Revit - mất nhiều thời gian (có thể 80% dự án). 
>
> Giải pháp: Add-in của tôi tự động chuyển dữ liệu từ file CAD sang Revit trong vài phút. User chỉ cần:
> 1. Chọn file CAD
> 2. Chọn lớp nào là sàn, lớp nào là cột, dầm
> 3. Nhập thông số (độ dày sàn, chiều cao cột)
> 4. Click nút → add-in tự tạo các phần tử Revit
>
> Lợi ích: Tiết kiệm 80% thời gian chuyên đồi, giảm lỗi nhân công."

---

### Q1.2: "Bạn sử dụng công nghệ gì?"

**Trả lời tốt:**
> "Ngôn ngữ: C# (.NET Framework 4.8)
> 
> Thư viện chính: Autodesk Revit API 2024
> 
> Giao diện: Windows Forms (WinForms)
> 
> Mô hình kiến trúc: MVVM (Model-View-ViewModel) - tách riêng giao diện, logic, dữ liệu
> 
> Thêm: Chuyên đổi đơn vị (CAD: mm, Revit: feet), chuyên đọc file CAD, tiêu chuẩn TCVN 5574."

---

### Q1.3: "Dự án này mất bao lâu để hoàn thành?"

**Trả lời tốt:**
> "Từ khái niệm đến hoàn thiện: khoảng [nói lựa số tháng thực tế].
> 
> Giai đoạn:
> - Tuần 1-2: Học Revit API, hiểu cách hoạt động
> - Tuần 3-4: Code phần cơ bản (đọc CAD, tạo sàn)
> - Tuần 5-6: Thêm cột, dầm, grid, level
> - Tuần 7-8: Sửa lỗi, tối ưu, thêm tiêu chuẩn TCVN
> - Tuần 9-10: Kiểm thử, viết tài liệu"

---

## ⭐⭐ CẤP 2: Câu Hỏi Về Kiến Trúc

### Q2.1: "Giải thích cấu trúc project của bạn"

**Trả lời tốt (vẽ sơ đồ):**
```
┌─ Cad2Revit Project
│
├─ Core/
│  ├─ App.cs         → Khởi tạo add-in khi Revit mở
│  └─ Command.cs     → Lệnh kích hoạt khi user bấm nút
│
├─ UI/
│  ├─ MainForm.cs            → Giao diện (View trong MVVM)
│  └─ MainForm.Designer.cs   → Thiết kế tự động sinh
│
├─ ViewModels/
│  ├─ MainViewModel.cs  → Logic chính (xử lý business logic)
│  ├─ LayerMapItem.cs   → Model dữ liệu 1 lớp
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
│  ├─ UnitHelper.cs         → Chuyển đổi đơn vị (mm ↔ feet)
│  ├─ Logger.cs             → Ghi log thông tin
│  ├─ CadCleanupHelper.cs   → Làm sạch dữ liệu
│  └─ ...
│
└─ Models/
   ├─ ConversionSettings.cs → Cài đặt chuyên đổi
   └─ CadElement.cs         → Thông tin phần tử CAD
```

**Giải thích từng phần:**
- **Core**: Nơi Revit gọi add-in của bạn
- **UI**: Giao diện người dùng (không xử lý logic)
- **ViewModels**: Xử lý logic tất cả (đọc CAD, chuyên đổi, tạo phần tử)
- **Converter**: Các class chuyên biệt cho chuyên đổi
- **Helpers**: Các hàm tiện ích (không liên quan lõi logic)

---

### Q2.2: "Bạn tại sao lại chọn MVVM pattern?"

**Trả lời tốt:**
> "MVVM có 3 lợi ích chính:
> 
> 1. **Tách riêng**: Model (dữ liệu), View (giao diện), ViewModel (logic)
>    - Dễ bảo trì, dễ test, dễ sửa
> 
> 2. **Binding**: Khi ViewModel thay đổi, View tự cập nhật
>    - Không cần viết code cập nhật giao diện, tự động
> 
> 3. **Tái sử dụng**: Nếu sau này muốn đổi từ WinForms sang WPF, chỉ cần viết View mới, ViewModel dùng lại
> 
> Ví dụ: Khi user nhập độ dày sàn, MainViewModel tự động tính tổng chiều cao, và giao diện tự cập nhật."

---

### Q2.3: "Luồng xử lý chính của bạn là gì?"

**Trả lời (vẽ flowchart):**
```
START
  ↓
User click nút "Cad2Revit" trong Revit
  ↓
Command.cs → Mở MainForm (giao diện)
  ↓
User nhập:
  - Chọn file CAD
  - Chọn lớp CAD → Loại phần tử (Sàn/Cột/Dầm)
  - Nhập thông số (độ dày, chiều cao)
  ↓
User click "Chuyên Đổi"
  ↓
MainViewModel.Convert() kích hoạt
  ↓
CadReader: Đọc file CAD
  ↓
CadGeometryAnalyzer: Phân tích hình học (tọa độ, lớp)
  ↓
FOR EACH layer in SelectedLayers
  ├─ ElementCreator.CreateFloor() / CreateColumn() / CreateBeam()
  ├─ RevitTcvnHelper: Chọn FloorType phù hợp
  └─ Revit API: Floor.Create() / Column.Create()
  ↓
Revit: Cập nhật 3D model, hiển thị các phần tử mới
  ↓
Log: Ghi thông tin đã tạo bao nhiêu sàn, cột, dầm
  ↓
END
```

---

## ⭐⭐⭐ CẤP 3: Câu Hỏi Kỹ Thuật

### Q3.1: "Bạn gặp khó khăn gì khi làm?"

**Trả lời (nêu 3-4 vấn đề):**
> "4 khó khăn lớn:
>
> **1. Revit API phức tạp:**
>    - Revit API có hàng nghìn class, method, khó tìm cách dùng đúng
>    - Ví dụ: Floor.Create() cần tham số CurveLoop (vòng đường cong), không phải danh sách tọa độ
>    - Giải pháp: Đọc RevitAPI docs, tham khảo forum, dùng Reflection để kiểm tra method
>
> **2. Chuyên đổi đơn vị:**
>    - CAD dùng mm, Revit dùng feet
>    - 1mm = 0.00328084 feet
>    - Nếu sai sẽ tạo sàn to/nhỏ sai tính
>    - Giải pháp: Tạo UnitHelper riêng, kiểm tra kỹ trong unit test
>
> **3. Ánh xạ lớp CAD:**
>    - File CAD có nhiều lớp, không biết lớp nào là sàn, nào là cột
>    - Giải pháp: Dùng tên lớp (nếu có tiêu chuẩn) hoặc color (nếu quy ước)
>
> **4. Điều chỉnh độ dày sàn:**
>    - Revit có các FloorType có sẵn, nhưng độ dày không khớp
>    - Revit không có method SetThickness(), phải dùng CompoundStructure
>    - Giải pháp: Duplicate() FloorType → lấy CompoundStructure → scale từng lớp"

---

### Q3.2: "Bạn dùng design pattern nào?"

**Trả lời:**
> "3 design pattern chính:
>
> **1. MVVM (Model-View-ViewModel):**
>    - Tách Model, View, ViewModel
>    - MainForm (View) bind dữ liệu từ MainViewModel
>    - Khi MainViewModel thay đổi, MainForm tự cập nhật
>
> **2. Singleton (log, database, v.v.):**
>    - Logger tạo 1 lần, dùng lại
>    - Document Revit cũng singleton (chỉ 1 project mở)
>
> **3. Strategy (đọc CAD, tạo phần tử):**
>    - CadReader có thể đọc .dwg, .dxf (2 strategy khác nhau)
>    - ElementCreator có thể tạo sàn, cột, dầm (3 strategy)"

---

### Q3.3: "Code snippets - Giải thích một đoạn code"

**Người hỏi:** "Giải thích hàm CreateFloor()"

**Trả lời:**
```csharp
private bool TaoSanTuContour(
    List<XYZ> contour,      // Danh sách tọa độ (X, Y) của đường bao sàn
    double beDayMm,         // Độ dày sàn (mm)
    double elevationFeet,   // Độ cao tầng (feet)
    ElementId levelId,      // ID của tầng Revit
    bool laSanMai,          // Sàn mái?
    List<XYZ> loThung)      // Lỗ bên trong (nếu có)
{
    // ============ BƯỚC 1: KIỂM TRA DỮ LIỆU ============
    if (contour == null || contour.Count < 3)
        return false;  // Cần ít nhất 3 điểm để tạo tam giác
    
    // ============ BƯỚC 2: CHỌN LOẠI SÀN ============
    // Gọi RevitTcvnHelper để chọn FloorType phù hợp
    // Nếu không có, nó tự tạo mới
    FloorType loaiSan = _tcvn.TimFloorTypeTcvn(beDayMm, laSanMai);
    if (loaiSan == null)
        return false;
    
    // ============ BƯỚC 3: TẠO ĐƯỜNG BAO ============
    // Chuyển danh sách tọa độ thành CurveLoop (vòng đường cong)
    CurveLoop loopNgoai = TaoCurveLoop(contour, elevationFeet);
    if (loopNgoai == null)
        return false;
    
    var loops = new List<CurveLoop> { loopNgoai };
    
    // BƯỚC 4: (Nếu có lỗ, thêm vào)
    if (loThung != null && loThung.Count >= 3)
    {
        CurveLoop loopTrong = TaoCurveLoop(loThung, elevationFeet);
        if (loopTrong != null)
            loops.Add(loopTrong);
    }
    
    // ============ BƯỚC 5: GỌI REVIT API ============
    // Tạo sàn: Floor.Create(doc, danh_sách_vòng, loại_sàn, tầng)
    Floor san = Floor.Create(_doc, loops, loaiSan.Id, levelId);
    
    // ============ BƯỚC 6: KIỂM TRA ============
    if (san == null)
        return false;
    
    return true;
}
```

**Giải thích:**
> "Hàm này tạo 1 sàn trong Revit từ dữ liệu CAD.
>
> Input: Danh sách tọa độ (đường bao sàn), độ dày, tầng
>
> Logic:
> 1. Kiểm tra dữ liệu hợp lệ (ít nhất 3 điểm)
> 2. Chọn FloorType (loại sàn) phù hợp - nếu không có, tạo mới
> 3. Chuyên đổi danh sách tọa độ thành CurveLoop (Revit API yêu cầu)
> 4. Nếu có lỗ bên trong, thêm vào danh sách vòng
> 5. Gọi Revit API: Floor.Create()
> 6. Kiểm tra kết quả
>
> Output: true (thành công) hoặc false (lỗi)"

---

### Q3.4: "Bạn xử lý lỗi thế nào?"

**Trả lời:**
> "3 cách xử lý lỗi:
>
> **1. Kiểm tra Null:**
> ```csharp
> if (floorType == null)
> {
>     logger.Error(\"FloorType không tìm thấy!\");
>     return false;
> }
> ```
>
> **2. Try-Catch:**
> ```csharp
> try
> {
>     Floor.Create(...);
> }
> catch (Exception ex)
> {
>     logger.Error($\"Lỗi tạo sàn: {ex.Message}\");
> }
> ```
>
> **3. Ghi Log:**
> - Mỗi bước quan trọng → ghi log
> - Giúp debug khi có lỗi
> - Người dùng thấy được chương trình đang làm gì"

---

## ⭐⭐⭐⭐ CẤP 4: Câu Hỏi Deepdive

### Q4.1: "Tại sao bạn chọn .NET Framework 4.8 thay vì .NET 6/7/8?"

**Trả lời:**
> "Vì Revit 2024 API chỉ hỗ trợ .NET Framework 4.8, không hỗ trợ .NET 6+.
>
> Nếu dùng .NET 6+, không thể import RevitAPI.dll → không thể làm add-in Revit.
>
> Để tương thích, phải dùng .NET Framework 4.8 (version cũ nhưng vẫn được hỗ trợ)."

---

### Q4.2: "Bạn cải thiện performance như thế nào?"

**Trả lời:**
> "2 cách tối ưu performance:
>
> **1. Caching:**
> - Revit API rất chậm (xem/sửa model = slow)
> - Lần đầu tìm FloorType, lưu vào cache (_symbolCotCache)
> - Lần sau dùng cache → nhanh hơn
>
> **2. Batch Operation:**
> - Thay vì tạo 1 sàn → log → tạo cột → log (lâu)
> - Tạo tất cả sàn, rồi tất cả cột, rồi log 1 lần (nhanh)
> - Transaction cũng merge các thay đổi → nhanh hơn"

---

### Q4.3: "Bạn kiểm thử (test) project thế nào?"

**Trả lời:**
> "2 cách test:
>
> **1. Unit Test:**
> - Test hàm chuyên đổi đơn vị (mm ↔ feet)
> - Test hàm tính toán tự động (số tầng → chiều cao)
>
> **2. Integration Test:**
> - Chuẩn bị file CAD test
> - Chạy add-in
> - Kiểm tra các phần tử được tạo đúng không
> - Kiểm tra độ dày, tọa độ, tầng
>
> **3. Manual Test:**
> - Chạy add-in trong Revit
> - Kiểm tra giao diện, các nút bấm
> - Kiểm tra log ghi thông tin đúng không"

---

## 📋 Bảng Kiểm Trước Phỏng Vấn

- [ ] Nắm vững cấu trúc project (vẽ được sơ đồ)
- [ ] Biết 3-4 khó khăn chính và cách giải quyết
- [ ] Hiểu 5 file chính (Command, MainForm, MainViewModel, ElementCreator, RevitTcvnHelper)
- [ ] Có thể giải thích hàm CreateFloor() bằng lời nói
- [ ] Biết MVVM là gì và tại sao dùng
- [ ] Biết Revit API là gì, tại sao khó dùng
- [ ] Chuẩn bị câu trả lời cho 10 câu hỏi phổ biến trên
- [ ] Có thể vẽ flowchart, sơ đồ cấu trúc
- [ ] Biết khi nào code dùng mm, khi nào dùng feet

---

## 🎯 Mẹo Phỏng Vấn

1. **Nghe kỹ câu hỏi** - Không nên trả lời lơ là
2. **Trả lời từ đơn giản đến phức tạp** - Bắt đầu overview, nếu người hỏi muốn chi tiết thì nói thêm
3. **Dùng ví dụ cụ thể** - "Ví dụ hàm X làm Y"
4. **Vẽ sơ đồ** - Hình vẽ giúp người hiểu rõ hơn
5. **Thành thật khi không biết** - "Em chưa hiểu rõ cái này, nhưng em biết nó dùng để..."
6. **Nói về vấn đề trước** - "Vấn đề em gặp là X, em giải quyết bằng Y"

---

**Chúc bạn phỏng vấn thành công! 🎓**

