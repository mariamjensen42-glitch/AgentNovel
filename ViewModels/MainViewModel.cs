using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentNovel.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage = new MergeViewModel();

    [ObservableProperty]
    private bool _isMergeView = true;

    [ObservableProperty]
    private bool _isSplitView;

    [ObservableProperty]
    private bool _isPageManagerView;

    [ObservableProperty]
    private bool _isSettingsView;

    partial void OnIsMergeViewChanged(bool value)
    {
        if (value)
        {
            CurrentPage = new MergeViewModel();
            IsSplitView = false;
            IsPageManagerView = false;
            IsSettingsView = false;
        }
    }

    partial void OnIsSplitViewChanged(bool value)
    {
        if (value)
        {
            CurrentPage = new SplitViewModel();
            IsMergeView = false;
            IsPageManagerView = false;
            IsSettingsView = false;
        }
    }

    partial void OnIsPageManagerViewChanged(bool value)
    {
        if (value)
        {
            CurrentPage = new PageManagerViewModel();
            IsMergeView = false;
            IsSplitView = false;
            IsSettingsView = false;
        }
    }

    partial void OnIsSettingsViewChanged(bool value)
    {
        if (value)
        {
            CurrentPage = new SettingsViewModel();
            IsMergeView = false;
            IsSplitView = false;
            IsPageManagerView = false;
        }
    }
}
