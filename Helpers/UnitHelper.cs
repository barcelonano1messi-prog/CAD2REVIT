namespace Cad2Revit.Helpers
{
    public class UnitHelper
    {
        private const double doiFeetSangMm = 304.8;

        public static double MmSangFeet(double mm)
        {
            return mm / doiFeetSangMm;
        }

        public static double FeetSangMm(double feet)
        {
            return feet * doiFeetSangMm;
        }
    }
}
