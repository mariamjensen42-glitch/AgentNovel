using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;

namespace AgentNovel.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();
        TitleBar.ExtendsContentIntoTitleBar = true;
    }
}
