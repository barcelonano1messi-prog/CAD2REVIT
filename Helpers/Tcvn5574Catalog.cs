namespace Cad2Revit.Helpers
{
    public static class Tcvn5574Catalog
    {
        public const string beTongCot = "B25";
        public const string beTongDam = "B25";
        public const string beTongSan = "B25";
        public const string beTongTuong = "B20";

        public static readonly string[] VatLieu = new[]
        {
            "B25", "B30", "B20",
            "BTCT", "BE TONG", "BÊ TÔNG", "BETONG",
            "CONCRETE", "C25", "C30", "FC25", "FC30",
            "TCVN", "5574"
        };

        public static readonly string[] Family = new[]
        {
            "STEEL", "THEP", "THÉP", "TIMBER", "WOOD", "GO",
            "GỖ", "HSS", "W-", "HP", "ANGLE"
        };

        public static readonly string[] FamilyBeTong = new[]
        {
            "CONCRETE", "RECTANGULAR", "BTCT", "BE TONG", "BÊ TÔNG",
            "BETONG", "CỐT THÉP", "COT THEP", "RC "
        };

        public const string floorTypeMacDinh = "GENERIC";

        public static readonly string[] FloorType = new[]
        {
            "GENERIC",
            "CONCRETE", "BTCT", "BE TONG", "BÊ TÔNG", "BETONG",
            "SAN", "SLAB", "SÀN", "TCVN", "B25"
        };
    }
}
