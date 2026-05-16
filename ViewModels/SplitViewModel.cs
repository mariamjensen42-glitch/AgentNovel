using System;
using System.IO;
using System.Threading.Tasks;
using AgentNovel.Messages;
using AgentNovel.Models;
using AgentNovel.Services;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AgentNovel.ViewModels;

public partial class SplitViewModel : ViewModelBase
{
    private readonly PdfService _pdfService = new();
    private readonly SettingsService _settingsService = new();

    [ObservableProperty]
    private PdfFile? _currentFile;

    [ObservableProperty]
    private string _pageRange = string.Empty;

    [ObservableProperty]
    private bool _extractMode = true;

    [ObservableProperty]
    private int _progress;

    [ObservableProperty]
    private bool _isProcessing;

    [RelayCommand]
    private async Task SelectFile()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        var storage = desktop.MainWindow?.StorageProvider;
        if (storage == null) return;

        var settings = await _settingsService.LoadAsync();
        var options = new FilePickerOpenOptions
        {
            Title = "选择PDF文件",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } } }
        };
        if (!string.IsNullOrEmpty(settings.LastProjectPath))
        {
            options.SuggestedStartLocation = await StorageProviderHelper.TryGetFolderFromPathAsync(
                storage, settings.LastProjectPath);
        }

        var files = await storage.OpenFilePickerAsync(options);

        if (files.Count > 0)
        {
            try
            {
                var filePath = files[0].Path.LocalPath;
                CurrentFile = await _pdfService.LoadPdfAsync(filePath);
                PageRange = $"1-{CurrentFile.PageCount}";

                settings.LastProjectPath = Path.GetDirectoryName(filePath);
                await _settingsService.SaveAsync(settings);
            }
            catch (Exception)
            {
                WeakReferenceMessenger.Default.Send(
                    new NotificationMessage(new NotificationInfo { Message = "文件加载失败", Type = NotificationType.Error }));
            }
        }
    }

    [RelayCommand]
    private async Task Split()
    {
        if (CurrentFile == null || string.IsNullOrWhiteSpace(PageRange)) return;

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        var storage = desktop.MainWindow?.StorageProvider;
        if (storage == null) return;

        var folder = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "选择保存位置",
            AllowMultiple = false
        });

        if (folder.Count == 0) return;

        IsProcessing = true;
        Progress = 0;

        try
        {
            var pageNumbers = Helpers.PageRangeParser.Parse(PageRange, CurrentFile.PageCount);
            if (pageNumbers.Count == 0)
            {
                WeakReferenceMessenger.Default.Send(
                    new NotificationMessage(new NotificationInfo { Message = "页面范围格式无效", Type = NotificationType.Warning }));
                return;
            }

            var outputPath = Path.Combine(folder[0].Path.LocalPath, $"{Path.GetFileNameWithoutExtension(CurrentFile.FileName)}_split.pdf");
            await _pdfService.SplitPdfAsync(CurrentFile.FilePath, pageNumbers, outputPath);

            WeakReferenceMessenger.Default.Send(
                new NotificationMessage(new NotificationInfo { Message = "PDF拆分成功", Type = NotificationType.Success }));
        }
        catch (Exception)
        {
            WeakReferenceMessenger.Default.Send(
                new NotificationMessage(new NotificationInfo { Message = "PDF拆分失败", Type = NotificationType.Error }));
        }
        finally
        {
            IsProcessing = false;
            Progress = 0;
        }
    }
}
