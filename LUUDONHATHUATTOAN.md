# Lưu Đồ Thuật Toán CAD2Revit

## 1. THUẬT TOÁN TẠO LƯỚI TRỤC (Grid System)

```mermaid
graph TD
    A["<b>ApDungChuanRevit</b><br/>Bắt đầu"] --> B{"Kiểm tra dữ liệu<br/>DanhSachDiemCot<br/>có ≥2 phần tử?"}
    B -->|Không| Z1["❌ Trả về null"]
    B -->|Có| C["TaoLuoiTuCot<br/>Tạo lưới từ vị trí cột"]
    
    C --> C1["Gom Trục X<br/>Gom Trục Y<br/>Loại bỏ trục trùng<br/>Sai số ≤150mm"]
    C1 --> C2["Xác định MinX, MaxX<br/>MinY, MaxY"]
    C2 --> D["Tính tâm lưới<br/>tamX = MinX + MaxX / 2<br/>tamY = MinY + MaxY / 2"]
    
    D --> E["Dịch chuyển tọa độ<br/>về gốc CAD2Revit"]
    E --> E1["DichPhanTich"]
    E1 --> E2["DichDuongCad"]
    E2 --> F["SnapCotVeLuoi<br/>Căn cột vào trục lưới"]
    
    F --> G["TaoDuongVienSan<br/>Tạo đường viền sàn<br/>với margin"]
    G --> H["XacDinhMaiThuNho<br/>Kiểm tra mái thu hẹp?"]
    
    H --> H1{"Số tầng ≥ 6<br/>Đủ cột góc<br/>& trong?"}
    H1 -->|Có| H2["Đặt SoNhipBoMeTangTren = 1<br/>Tầng trên bỏ mép ngoài"]
    H1 -->|Không| H3["SoNhipBoMeTangTren = 0"]
    
    H2 --> I["TaoDamTheoLuoi<br/>Tạo dầm theo lưới"]
    H3 --> I
    I --> J["Kiểm tra Grid Revit<br/>có sẵn?"]
    
    J -->|Có| J1["RevitGridHelper<br/>Căn mô hình vào<br/>Grid Revit template"]
    J -->|Không| J2["Dùng lưới<br/>CAD2Revit mới"]
    
    J1 --> K["Lưu LuoiTruc<br/>vào phanTich"]
    J2 --> K
    K --> Z2["✅ Trả về LuoiTruc"]
```

---

## 2. THUẬT TOÁN TẠO CỘT (Column Creation)

```mermaid
graph TD
    A["<b>TaoCotTheoTang</b><br/>Tạo cột tại tầng"] --> B["Xác định key<br/>key = tang_viTri_kichThuoc"]
    
    B --> C{"Cột này<br/>đã tạo?"}
    C -->|Có| D["❌ Trả về false<br/>Bỏ qua"]
    C -->|Không| E["Lấy DiemCot<br/>từ lưới trục"]
    
    E --> F["TimFamilyCot<br/>Tìm loại cột<br/>theo kích thước"]
    F --> F1{"Loại cột<br/>có trong<br/>Catalog?"}
    F1 -->|Không| D
    F1 -->|Có| G["Kiểm tra Active<br/>Nếu chưa → Activate"]
    
    G --> H["Tạo FamilyInstance<br/>NewFamilyInstance<br/>- Vị trí: X, Y<br/>- Level: levelDuoi<br/>- Type: Column"]
    
    H --> I{"Cột tạo<br/>thành công?"}
    I -->|Không| D
    I -->|Có| J{"Có Level<br/>trên?"}
    
    J -->|Có| J1["DatChieuCaoCot<br/>Từ Level_duoi<br/>đến Level_tren"]
    J -->|Không| J2["DatChieuCaoCotBangOffset<br/>Dùng giá trị chiều cao<br/>của tầng"]
    
    J1 --> K["Đặt kích thước<br/>Family Parameter"]
    J2 --> K
    K --> L["GanVatLieuTcvn<br/>Gán vật liệu<br/>Bê tông cột"]
    
    L --> M["Thêm key vào<br/>cotDaTaoTheoTang"]
    M --> N["✅ Trả về true<br/>Cột đã tạo"]
    
    style A fill:#e1f5ff
    style D fill:#ffebee
    style N fill:#e8f5e9
```

---

## 3. THUẬT TOÁN TẠO DẦM (Beam Creation)

```mermaid
graph TD
    A["<b>TaoDamTheoTang</b><br/>Tạo dầm tại tầng"] --> B["Xác định key<br/>key = tang_duongDam"]
    
    B --> C{"Dầm này<br/>đã tạo?"}
    C -->|Có| D["❌ Trả về false"]
    C -->|Không| E{"Chiều dài nhịp<br/>≥ 400mm?"}
    
    E -->|Không| D
    E -->|Có| F["TimFamilyDam<br/>Tìm loại dầm<br/>theo W × H"]
    
    F --> F1{"Loại dầm<br/>có trong<br/>Catalog?"}
    F1 -->|Không| D
    F1 -->|Có| G["Activate<br/>nếu cần"]
    
    G --> H["Xác định Level dầm<br/>chiSoLevelDam<br/>= tang + 1 hoặc tang"]
    H --> I["Lấy Elevation<br/>zLevel = Level.Elevation"]
    
    I --> J["Tạo 2 điểm:<br/>p1 = DiemDau<br/>p2 = DiemCuoi<br/>ở độ cao zLevel"]
    
    J --> K["Làm thẳng đường dầm<br/>LamThangDuong<br/>Loại bỏ kink"]
    K --> L{"p1 ≈ p2?"}
    L -->|Có| D
    L -->|Không| M["Tạo Line<br/>từ p1 tới p2"]
    
    M --> N["Tạo FamilyInstance<br/>NewFamilyInstance<br/>- Curve: line<br/>- Level: levelDam<br/>- Type: Beam"]
    
    N --> O{"Dầm tạo<br/>thành công?"}
    O -->|Không| D
    O -->|Có| P["Tính offsetLen<br/>= caoTrinhFeet<br/>  - zLevel"]
    
    P --> Q{"offsetLen<br/>> 5mm?"}
    Q -->|Có| R["DatOffsetDam<br/>Đặt offset<br/>Z parameter"]
    Q -->|Không| S["offsetLen = 0<br/>Không offset"]
    
    R --> T["Đặt kích thước<br/>Family Parameter"]
    S --> T
    T --> U["GanVatLieuTcvn<br/>Gán bê tông dầm"]
    
    U --> V["Thêm key vào<br/>damDaTaoTheoTang"]
    V --> W["✅ Trả về true"]
    
    style A fill:#e1f5ff
    style D fill:#ffebee
    style W fill:#e8f5e9
```

---

## 4. THUẬT TOÁN TẠO SÀN (Floor Creation)

```mermaid
graph TD
    A["<b>TaoLuoiVaSanTruoc</b><br/>Tạo lưới và sàn"] --> A1["Kiểm tra:<br/>SuDungGridRevitCoSan?"]
    
    A1 -->|Không| A2["TaoLuoiRevit<br/>Tạo Grid mới"]
    A1 -->|Có| A3["Dùng Grid<br/>Revit có sẵn"]
    
    A2 --> B["VÒNG LẶP: Mỗi tầng"]
    A3 --> B
    
    B --> B1["Lấy level hiện tại"]
    B1 --> B2{"NenTaoSanChoTang?<br/>- Không phải tầng trệt<br/>- ConvertFloors = true"}
    
    B2 -->|Không| C0["Bỏ qua tầng"]
    B2 -->|Có| C["TaoSanChoTang"]
    
    C0 --> D
    C --> C1["LayDuongVienSanChoTang<br/>Lấy đường viền"]
    
    C1 --> C2{"DuongVien<br/>≥ 3 điểm?"}
    C2 -->|Không| C3["❌ Không tạo sàn"]
    C2 -->|Có| C4["Xác định key<br/>key = tang_maDuongVien"]
    
    C3 --> C5{"Đã tạo<br/>sàn này?"}
    C4 --> C5
    C5 -->|Có| C6["Bỏ qua<br/>trùng lặp"]
    C5 -->|Không| C7["Lấy LoThung<br/>từ LuoiTruc<br/>nếu tầng > 0"]
    
    C6 --> D
    C7 --> C8["TaoSanTuDuongVien<br/>Tạo Floor object"]
    
    C8 --> C9["Thêm key vào<br/>sanDaTaoTheoTang"]
    C9 --> D["Tầng tiếp theo<br/>tang++"]
    
    D --> D1{"tang < số lượng<br/>Level?"}
    D1 -->|Có| B1
    D1 -->|Không| E
    
    E{"TaoSanMai<br/>= true?"}
    E -->|Không| F["Kết thúc"]
    E -->|Có| F1["TaoSanMai"]
    
    F1 --> F2["Lấy Level Mái<br/>ChuanBiLevelMai"]
    F2 --> F3["LayVungSanLonNhat<br/>Sàn lớn nhất"]
    
    F3 --> F4{"Vùng sàn<br/>có?"}
    F4 -->|Không| F5["Kết thúc"]
    F4 -->|Có| F6["TaoSanTuDuongVien<br/>Tạo sàn mái"]
    
    F6 --> F5
    
    style A fill:#e1f5ff
    style C3 fill:#ffebee
    style F5 fill:#e8f5e9
```

---

## 5. LUỒNG CHÍNH: TaoTatCaCauKien (Main Process)

```mermaid
graph TD
    START["🚀 <b>TaoTatCaCauKien</b><br/>Bắt đầu chuyển đổi"] --> STEP0["<b>BƯỚC 0:</b><br/>Xóa nội dung cũ"]
    
    STEP0 --> S0A["XoaTatCaGrid<br/>nếu không dùng<br/>Grid Revit có sẵn"]
    S0A --> S0B["XoaTatCaCauKien<br/>Xóa cột, dầm, tường"]
    S0B --> S0C["XoaLevelMacDinh<br/>Xóa Level 1, 2..."]
    
    S0C --> STEP1["<b>BƯỚC 1:</b><br/>Chuẩn hóa tọa độ"]
    STEP1 --> S1A["ChuanHoaVeGocKienTruc<br/>Căn góc kiến trúc"]
    S1A --> S1B["ApDungChuanRevit<br/>Tạo lưới trục"]
    S1B --> S1C["XoaKetQuaLanTruoc<br/>Xóa CAD2Revit cũ"]
    
    S1C --> STEP2["<b>BƯỚC 2:</b><br/>Chuẩn bị Level"]
    STEP2 --> S2A{"SuDungLevelRevit<br/>CoSan?"}
    
    S2A -->|Có| S2B["LayDanhSachLevel<br/>từ Revit"]
    S2A -->|Không| S2C["TaoHoacLayDanhSachLevel<br/>Tạo Level mới"]
    
    S2B --> S2D["phanTich.SoTang<br/>= số Level"]
    S2C --> S2D
    
    S2D --> S2E{"TaoSanMai?"}
    S2E -->|Có| S2F["ChuanBiLevelMai<br/>Tạo Level Mái"]
    S2E -->|Không| S2G["Bỏ qua Mái"]
    
    S2F --> STEP3["<b>BƯỚC 2+3:</b><br/>Lưới & Sàn"]
    S2G --> STEP3
    
    STEP3 --> S3A["TaoLuoiVaSanTruoc<br/>- Tạo lưới trục<br/>- Tạo sàn trước"]
    
    S3A --> STEP4["<b>BƯỚC 4:</b><br/>Tạo cấu kiện"]
    STEP4 --> S4A["VÒNG TẦN: tang = 0 → số Level"]
    
    S4A --> S4B["Lấy level hiện tại<br/>& level trên"]
    S4B --> S4C{"ConvertColumns?"}
    
    S4C -->|Có| S4D["Lấy danh sách<br/>DiemCot của tầng"]
    S4D --> S4E["VÒNG LẶP: Mỗi cột"]
    S4E --> S4E1["TaoCotTheoTang"]
    
    S4E1 --> S4F{"Còn cột?"}
    S4F -->|Có| S4E1
    S4F -->|Không| S4C2
    
    S4C -->|Không| S4C2{"ConvertBeams?"}
    
    S4C2 -->|Có| S4G["Lấy danh sách<br/>DuongDam"]
    S4G --> S4H["VÒNG LẶP: Mỗi dầm"]
    S4H --> S4H1["DamThuocPhamViTang?"]
    S4H1 -->|Có| S4H2["TaoDamTheoTang"]
    S4H1 -->|Không| S4H3["Bỏ qua"]
    
    S4H2 --> S4I{"Còn dầm?"}
    S4H3 --> S4I
    S4I -->|Có| S4H1
    S4I -->|Không| S4C2B
    
    S4C2 -->|Không| S4C2B{"ConvertWalls?"}
    
    S4C2B -->|Có| S4J["Lấy danh sách<br/>DuongTuong"]
    S4J --> S4K["VÒNG LẶP: Mỗi tường"]
    S4K --> S4K1["TaoTuong"]
    S4K1 --> S4L{"Còn tường?"}
    S4L -->|Có| S4K1
    S4L -->|Không| S4LB
    
    S4C2B -->|Không| S4LB["Tầng tiếp theo<br/>tang++"]
    
    S4LB --> S4LB1{"Còn tầng?"}
    S4LB1 -->|Có| S4B
    S4LB1 -->|Không| STEP5
    
    STEP5["<b>BƯỚC 5:</b><br/>Cập nhật tọa độ CAD"]
    STEP5 --> S5A["DichCadImportVeGoc<br/>Dịch CAD về gốc"]
    
    S5A --> STEP6["<b>BƯỚC 6:</b><br/>Tối ưu hóa"]
    STEP6 --> S6A{"SuDungGridRevitCoSan?"}
    
    S6A -->|Không| S6B["ThuPhamViGridVaLevel<br/>Thu gọn phạm vi"]
    S6A -->|Có| S6C["Dùng phạm vi<br/>Revit có sẵn"]
    
    S6B --> END["✅ HOÀN THÀNH<br/>Trả về 'done'"]
    S6C --> END
    
    style START fill:#b3e5fc
    style STEP0 fill:#fff9c4
    style STEP1 fill:#fff9c4
    style STEP2 fill:#fff9c4
    style STEP3 fill:#fff9c4
    style STEP4 fill:#fff9c4
    style STEP5 fill:#fff9c4
    style STEP6 fill:#fff9c4
    style END fill:#c8e6c9
```

---

## 6. CHI TIẾT: LayDuongVienChoTang (Floor Perimeter by Floor)

```mermaid
graph TD
    A["<b>LayDuongVienChoTang</b><br/>Lấy đường viền sàn<br/>cho tầng cụ thể"] --> B{"Tầng = Tầng trên cùng<br/>& Có mái thu hẹp?"}
    
    B -->|Không| B1["bo = 0<br/>Dùng toàn bộ lưới"]
    B -->|Có| B2["bo = SoNhipBoMeTangTren<br/>Bỏ mép ngoài"]
    
    B1 --> C["i0 = 0<br/>i1 = TrucX.Count - 1<br/>j0 = 0<br/>j1 = TrucY.Count - 1"]
    B2 --> C2["i0 = bo<br/>i1 = TrucX.Count - 1 - bo<br/>j0 = bo<br/>j1 = TrucY.Count - 1 - bo"]
    
    C --> D{"i1 ≤ i0<br/>hoặc<br/>j1 ≤ j0?"}
    C2 --> D
    
    D -->|Có| D1["Đặt lại:<br/>i0=0, j0=0<br/>i1=Max, j1=Max<br/>để tránh lỗi"]
    D -->|Không| E["m = Margin<br/>MarginFloorMm"]
    
    D1 --> E
    E --> F["minX = TrucX[i0] - m<br/>maxX = TrucX[i1] + m<br/>minY = TrucY[j0] - m<br/>maxY = TrucY[j1] + m"]
    
    F --> G["Tạo 4 đỉnh hình chữ nhật:<br/>- (minX, minY, 0)<br/>- (maxX, minY, 0)<br/>- (maxX, maxY, 0)<br/>- (minX, maxY, 0)"]
    
    G --> H["✅ Trả về danh sách<br/>4 điểm XYZ"]
    
    style A fill:#e1f5ff
    style H fill:#e8f5e9
```

---

## 7. QUY TRÌNH PHÂN TÍCH: TaoLuoiTuCot (Grid from Columns)

```mermaid
graph TD
    A["<b>TaoLuoiTuCot</b><br/>Tạo lưới từ cột"] --> B["Lấy tất cả điểm cột<br/>DanhSachDiemCot"]
    
    B --> C["GomTruc(X)<br/>Gom các tọa độ X"]
    C --> C1["- Sắp xếp X<br/>- So sánh lần lượt<br/>- Nếu hiệu < 150mm<br/>  → gọp lại (lấy trung bình)<br/>- Nếu hiệu ≥ 150mm<br/>  → tạo trục mới"]
    C1 --> D["TrucX = danh sách<br/>tọa độ X sau gom"]
    
    D --> E["GomTruc(Y)<br/>Tương tự cho Y"]
    E --> E1["TrucY = danh sách<br/>tọa độ Y sau gom"]
    
    E1 --> F["MinX = TrucX[0]<br/>MaxX = TrucX[cuoi]<br/>MinY = TrucY[0]<br/>MaxY = TrucY[cuoi]"]
    
    F --> G["✅ Trả về LuoiTrucKetCau<br/>- TrucX, TrucY<br/>- MinX, MaxX, MinY, MaxY"]
    
    style A fill:#e1f5ff
    style G fill:#e8f5e9
```

---

## 📊 BIỂU ĐỒ TUẦN TỰ: Thứ tự Thực Thi

```mermaid
sequenceDiagram
    participant Main as Main Process
    participant Grid as Grid System
    participant Col as Column Creator
    participant Beam as Beam Creator
    participant Floor as Floor Creator
    
    Main->>Main: 1. Xóa nội dung cũ
    Main->>Grid: 2. ApDungChuanRevit - Tạo lưới
    Grid-->>Main: LuoiTruc
    
    Main->>Main: 3. Chuẩn bị Level
    Main->>Floor: 4. TaoLuoiVaSanTruoc
    Floor-->>Main: Sàn đã tạo
    
    Main->>Col: 5. TaoCotTheoTang (vòng lặp)
    Col-->>Main: Cột đã tạo
    
    Main->>Beam: 6. TaoDamTheoTang (vòng lặp)
    Beam-->>Main: Dầm đã tạo
    
    Main->>Main: 7. DichCadImportVeGoc
    Main->>Main: 8. Tối ưu hóa phạm vi
    Main->>Main: HOÀN THÀNH ✅
```

---

## 📋 TÓMLƯU TRỮ THAM CHIẾU

### Các Hằng Số Quan Trọng

| Hằng Số | Giá Trị | Mô Tả |
|---------|--------|-------|
| `TolGomTrucMm` | 150 mm | Sai số gom trục (khoảng cách tối thiểu để gộp 2 trục) |
| `MarginGridMm` | 4500 mm | Khoảng lề khi tạo grid (kéo dài grid vượt ra ngoài) |
| `MarginFloorMm` | 0 mm | Khoảng lề sàn (để sàn chỉ tới cột biên) |
| `TolTrungMm` | 80 mm | Sai số để xác định 2 grid trùng nhau |
| `TolMm` (Floor) | 100 mm | Sai số để ghép đường thành vùng kín |
| `DienTichToiThieuMm²` | 2,000,000 | Diện tích tối thiểu vùng sàn |

### Các Lớp Dữ Liệu Chính

```csharp
// Lưới trục kết cấu
class LuoiTrucKetCau {
    List<double> TrucX;        // Các trục ngang
    List<double> TrucY;        // Các trục dọc
    double MinX, MaxX, MinY, MaxY;
    List<XYZ> DuongVienSan;    // Đường viền sàn
    List<XYZ> LoThung;         // Lỗ thông (nếu có)
    int SoNhipBoMeTangTren;     // Số nhịp bỏ mép (mái)
}

// Điểm cột
class DiemCot {
    double X, Y;               // Tọa độ
    double RongMm, SauMm;      // Kích thước
}

// Đường dầm
class DuongDam {
    XYZ DiemDau, DiemCuoi;     // 2 đầu
    double RongMm, CaoMm;      // Kích thước tiết diện
    double ChieuDaiNhipMm;     // Chiều dài nhịp
}

// Vùng sàn
class VungSan {
    List<XYZ> DuongVien;       // Đường viền kín
    double BeDayMm;            // Bề dày sàn
    string TenLayer;           // Tên lớp
}
```

---

## 🔑 Điểm Chính Của Mỗi Thuật Toán

### **Lưới Trục (Grid)**
- ✅ Gom các trục trùng nhau (sai số 150mm)
- ✅ Căn tâm mô hình về gốc kiến trúc
- ✅ Xác định mái thu hẹp (tầng trên bỏ mép)
- ✅ Tạo Grid Revit hoặc dùng Grid có sẵn

### **Cột (Column)**
- ✅ Tìm loại cột từ Catalog theo kích thước
- ✅ Tránh tạo cột trùng lặp (theo key)
- ✅ Căn cột vào lưới trục
- ✅ Đặt chiều cao từ Level hoặc Offset

### **Dầm (Beam)**
- ✅ Kiểm tra chiều dài ≥ 400mm
- ✅ Làm thẳng đường dầm (loại bỏ kink)
- ✅ Tránh tạo dầm trùng lặp
- ✅ Tính offset Z nếu cần

### **Sàn (Floor)**
- ✅ Lấy đường viền từ lưới hoặc vùng CAD
- ✅ Xử lý lỗ thông (nếu có)
- ✅ Tránh tạo sàn trùng lặp
- ✅ Bỏ qua tầng trệt (tuỳ cài đặt)
- ✅ Tạo sàn mái nếu cần

---

## 🎯 Luồng Chính Tóm Tắt

1. **Xóa**: Xóa nội dung cũ
2. **Chuẩn hóa**: Chuẩn hóa tọa độ, tạo lưới
3. **Chuẩn bị**: Tạo Level, Level Mái (nếu cần)
4. **Sàn & Lưới**: Tạo Grid Revit, Sàn
5. **Cột, Dầm, Tường**: Lặp qua từng tầng tạo cấu kiện
6. **Hoàn thiện**: Cập nhật tọa độ, tối ưu hóa

---

*Tài liệu lưu đồ thuật toán CAD2Revit - 2026*
