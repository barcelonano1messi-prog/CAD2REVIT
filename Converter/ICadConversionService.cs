using Cad2Revit.Models;
using System.Collections.Generic;

namespace Cad2Revit.Services
{
    public interface ICadConversionService
    {
        IReadOnlyList<string> LayerNames { get; }
        KetQuaDocCAD CadResult { get; }
        KetQuaPhanTich AnalysisResult { get; }

        bool ReadCad(out string error);
        void ApplyLayerMappings(IEnumerable<LayerMapping> mappings);
        void Analyse(ConversionSettings settings);
        string ConvertModel(ConversionSettings settings);
    }
}
