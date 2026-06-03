using Autodesk.Revit.DB;
using Cad2Revit.Converter;
using Cad2Revit.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cad2Revit.Services
{
    public class CadConversionService : ICadConversionService
    {
        private readonly Document _doc;
        private readonly CadReader _cadReader;

        public IReadOnlyList<string> LayerNames { get; private set; } = new List<string>();
        public KetQuaDocCAD CadResult { get; private set; }
        public KetQuaPhanTich AnalysisResult { get; private set; }

        public CadConversionService(Document doc)
        {
            _doc = doc ?? throw new ArgumentNullException(nameof(doc));
            _cadReader = new CadReader(doc);
        }

        public bool ReadCad(out string error)
        {
            error = null;
            CadResult = _cadReader.DocCadTuImport();

            if (CadResult == null || !CadResult.ThanhCong)
            {
                if (CadResult == null)
                    error = "Không đọc được file CAD.";
                else
                    error = CadResult.ThongBaoLoi;

                LayerNames = new List<string>();
                AnalysisResult = null;
                return false;
            }

            LayerNames = _cadReader.DanhSachLayerTimThay?
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList() ?? new List<string>();
            return true;
        }

        public void ApplyLayerMappings(IEnumerable<LayerMapping> mappings)
        {
            if (CadResult == null)
                return;

            LayerMapper.XoaHetLayerTuyChinh();

            if (mappings != null)
            {
                foreach (var mapping in mappings)
                {
                    if (string.IsNullOrWhiteSpace(mapping.LayerName))
                        continue;

                    LayerMapper.ThemLayerTuyChinh(mapping.LayerName, mapping.Loai);
                }
            }

            foreach (CadLine duong in CadResult.DanhSachDuong)
            {
                duong.Loai = LayerMapper.XacDinhLoai(duong.TenLayer);
            }
        }

        public void Analyse(ConversionSettings settings)
        {
            if (CadResult == null)
                return;

            if (settings == null)
                settings = new ConversionSettings();

            var analyzer = new CadGeometryAnalyzer();
            AnalysisResult = analyzer.PhanTich(CadResult, settings);
            CadResult.PhanTich = AnalysisResult;
        }

        public string ConvertModel(ConversionSettings settings)
        {
            if (CadResult == null)
                return "Lỗi: chưa đọc CAD.";

            if (AnalysisResult == null)
            {
                Analyse(settings);
            }

            var creator = new ElementCreator(_doc, settings, AnalysisResult, CadResult);
            return creator.TaoTatCaCauKien(CadResult);
        }
    }
}
