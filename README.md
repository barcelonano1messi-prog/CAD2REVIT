# CAD 2D → Revit 3D Converter
### Đồ án tốt nghiệp | Revit API 2024 | C# .NET 4.8 | Visual Studio 2019

---

## CẤU TRÚC PROJECT

```
Cad2Revit/
├── Cad2Revit.csproj          ← File project Visual Studio
├── Cad2Revit.addin           ← Manifest đăng ký Add-in với Revit
├── Properties/
│   └── AssemblyInfo.cs
├── Core/
│   ├── App.cs                ← IExternalApplication (tạo nút Ribbon)
│   └── Command.cs            ← IExternalCommand (xử lý khi bấm nút)
├── Models/
│   ├── CadElement.cs         ← Data model cho geometry CAD
│   └── ConversionSettings.cs ← Tham số cài đặt chuyển đổi
├── Converter/
│   ├── LayerMapper.cs
│   ├── CadReader.cs
│   ├── CadGeometryAnalyzer.cs
│   ├── StructuralPointExtractor.cs
│   ├── FloorContourBuilder.cs
│   ├── RevitTcvnHelper.cs
│   └── ElementCreator.cs
├── UI/
│   ├── MainForm.cs
│   ├── MainForm.Designer.cs
│   └── MainForm.resx
└── Helpers/
    ├── CadDimensionHelper.cs   ← Kích thước cột từ tên layer + DIM
    ├── CadCoordinateHelper.cs
    ├── StructuralGridSystem.cs
    ├── RevitLevelHelper.cs
    ├── RevitGridHelper.cs
    ├── Logger.cs
    └── UnitHelper.cs
```

---

## BƯỚC 1 — SETUP VISUAL STUDIO 2019

### 1.1 Tạo project
1. Mở Visual Studio 2019
2. **File → Open → Project/Solution** → chọn `Cad2Revit.csproj`
   - Hoặc: **File → New → Project** → Class Library (.NET Framework) → .NET 4.8

### 1.2 Kiểm tra References
Trong **Solution Explorer → References**, đảm bảo có:
- `RevitAPI` → trỏ đến `C:\Program Files\Autodesk\Revit 2024\RevitAPI.dll`
- `RevitAPIUI` → trỏ đến `C:\Program Files\Autodesk\Revit 2024\RevitAPIUI.dll`

> ⚠️ Nếu đường dẫn khác, click phải vào Reference → Properties → sửa HintPath

### 1.3 Kiểm tra Build Platform
- **Build → Configuration Manager**
- Đặt Platform = **x64** (Revit chỉ chạy 64-bit)

---

## BƯỚC 2 — CẤU HÌNH OUTPUT PATH

Mở `Cad2Revit.csproj`, sửa `<OutputPath>` trong Debug:

```xml
<OutputPath>C:\ProgramData\Autodesk\Revit\Addins\2024\</OutputPath>
```

> Đây là thư mục Revit tự động load Add-in khi khởi động.
> Cần **quyền Admin** hoặc dùng thư mục user:
> `C:\Users\{TênUser}\AppData\Roaming\Autodesk\Revit\Addins\2024\`

---

## BƯỚC 3 — FILE .ADDIN

File `Cad2Revit.addin` phải nằm CÙNG THƯ MỤC với `Cad2Revit.dll`.
Khi OutputPath = thư mục Addins, file .addin được copy tự động (đã cấu hình trong .csproj).

Nếu copy thủ công, đảm bảo file .addin có đúng:
```xml
<Assembly>Cad2Revit.dll</Assembly>
<FullClassName>Cad2Revit.Core.App</FullClassName>
```

---

## BƯỚC 4 — BUILD VÀ CHẠY

1. **Build → Build Solution** (Ctrl+Shift+B)
2. Đảm bảo không có lỗi build
3. **Mở Revit 2024**
4. Kiểm tra tab **"CAD→3D Converter"** xuất hiện trên Ribbon

---

## BƯỚC 5 — SỬ DỤNG TRONG REVIT

### Chuẩn bị bản vẽ:
1. Mở một Revit Project mới (Architectural hoặc Structural template)
2. Tạo ít nhất **1 Level** (thường đã có sẵn: Level 1, Level 2)
3. Load Family cột và dầm:
   - **Insert → Load Family**
   - Tìm: `Concrete-Rectangular-Column.rfa` (trong thư viện Revit)
   - Tìm: `Concrete-Rectangular Beam.rfa`

### Import CAD:
4. **Insert → Import CAD**
   - Chọn file `.dwg` hoặc `.dxf`
   - Import Unit: **Millimeters**
   - Positioning: **Auto - Origin to Origin**
   - Bấm **Open**

### Chạy Add-in:
5. Vào tab **"CAD→3D Converter"** → bấm **"Chuyển CAD sang 3D"**
6. Form hiện ra — **chỉ nhập 2 thông số**:
   - **Chiều cao công trình** (mm) — ví dụ 13200
   - **Số tầng** — ví dụ 4 → chiều cao mỗi tầng tự tính
7. Bước ①: **Đọc CAD** → add-in tự nhận layer, tự tính cột/dầm/sàn từ geometry
8. Bước ②: **Chuyển đổi sang 3D** → tạo Level, tường, cột, dầm, sàn theo số tầng

---

## VẬT LIỆU TCVN 5574:2018

Add-in tự chọn family **bê tông cốt thép** (loại trừ thép/gỗ) và gán mác:

| Cấu kiện | Mác bê tông (TCVN) |
|----------|-------------------|
| Cột | **B25** |
| Dầm | **B25** |
| Sàn | **B25** |

Nếu project chưa có vật liệu phù hợp, add-in tạo `BTCT B25 - TCVN 5574:2018`.

**Sàn:** chỉ tạo từ **vùng khép kín** (không tạo từng đoạn line) → tránh lỗi *floors overlap*.  
**FloorType:** mặc định chọn **Generic** (bề dày gần giá trị nhập trên form).

---

## TỰ ĐỘNG TÍNH KÍCH THƯỚC (CadGeometryAnalyzer)

Người dùng **không cần nhập** bề dày tường, tiết diện cột/dầm, độ dày sàn. Add-in suy ra từ CAD:

| Cấu kiện | Cách nhận diện |
|----------|----------------|
| Tường | Khoảng cách giữa 2 đường song song layer tường |
| Cột | Hình chữ nhật trên layer cột, cạnh ngắn, hoặc `300x400` trong tên layer |
| Dầm | Đường song song trên layer dầm, hoặc `200x500` trong tên layer |
| Sàn | Độ dày từ cặp đường song song; vùng khép kín → tạo sàn theo contour |

**Chiều cao tầng** = Chiều cao công trình ÷ Số tầng (do bạn nhập).

---

## QUY ƯỚC ĐẶT TÊN LAYER (LayerMapper.cs)

Add-in nhận diện cấu kiện qua tên layer (không phân biệt hoa/thường):

| Cấu kiện | Từ khóa layer nhận diện                                |
|----------|--------------------------------------------------------|
| Tường    | TUONG, WALL, W-, TG, PARTITION, A-WALL, S-WALL         |
| Cột      | COT, COLUMN, COL, C-, PILLAR, S-COLS                   |
| Dầm      | DAM, BEAM, DM, B-, S-BEAM                              |
| Sàn      | SAN, FLOOR, SLAB, SF, S-SLAB, A-FLOR                  |

> **Lưu ý:** Nếu layer trong bản vẽ của bạn khác, thêm vào `LayerMapper.cs`.

---

## XỬ LÝ LỖI THƯỜNG GẶP

| Lỗi | Nguyên nhân | Giải pháp |
|-----|-------------|-----------|
| "Không tìm thấy CAD Import" | Chưa import CAD | Dùng Insert > Import CAD trước |
| "Không tìm thấy WallType" | Project rỗng chưa có type | Mở template mặc định của Revit |
| "Không tìm thấy Column Family" | Chưa load family cột | Load từ thư viện Revit |
| Build error: RevitAPI not found | Sai đường dẫn DLL | Sửa HintPath trong .csproj |
| Add-in không hiện trên Ribbon | .addin không đúng vị trí | Copy cả .dll và .addin vào thư mục Addins |

---

## GHI CHÚ KỸ THUẬT

- **Đơn vị nội bộ Revit:** Feet (1 foot = 304.8 mm) — xem `UnitHelper.cs`
- **Layer name trong Revit:** Lấy qua `GraphicsStyleId` → `GraphicsStyle.Name`
- **Transaction:** Tất cả tạo phần tử phải nằm trong `using (Transaction tx = ...)`
- **Geometry đệ quy:** CAD Import lồng nhiều `GeometryInstance` — phải duyệt đệ quy

---

*Đồ án tốt nghiệp — Hệ thống tự động chuyển đổi bản vẽ CAD 2D sang mô hình 3D trong Revit*
