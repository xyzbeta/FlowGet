using CommunityToolkit.Mvvm.ComponentModel;

namespace FlowGet.ViewModels
{
    public  abstract partial class ViewModelBase : ObservableObject
    {
        [ObservableProperty]
        public partial string Title { get; set; } = string.Empty;
    }
}
