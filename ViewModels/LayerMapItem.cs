namespace Cad2Revit.ViewModels
{
    public class LayerMapItem : ViewModelBase
    {
        private string _layerName;
        private string _categoryName;

        public string LayerName
        {
            get => _layerName;
            set => SetProperty(ref _layerName, value);
        }

        public string CategoryName
        {
            get => _categoryName;
            set => SetProperty(ref _categoryName, value);
        }
    }
}
