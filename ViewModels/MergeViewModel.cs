using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

public partial class MergeViewModel : ViewModelBase
{
    private readonly PdfService _pdfService = new();
    private readonly SettingsService _settingsService = new();

    [ObservableProperty]
    private ObservableCollection<PdfFile> _pdfFiles = new();

    [ObservableProperty]
    private PdfFile? _selectedFile;

    [ObservableProperty]
    private int _progress;

    [ObservableProperty]
    private bool _isProcessing;

    [RelayCommand]
    private async Task AddFiles()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        var storage = desktop.MainWindow?.StorageProvider;
        if (storage == null) return;

        var settings = await _settingsService.LoadAsync();
        var options = new FilePickerOpenOptions
        {
            Title = "选择PDF文件",
            AllowMultiple = true,
            FileTypeFilter = new[] { new FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } } }
        };

        var files = await storage.OpenFilePickerAsync(options);

        foreach (var file in files)
        {
            try
            {
                var pdfFile = await _pdfService.LoadPdfAsync(file.Path.LocalPath);
                PdfFiles.Add(pdfFile);

                settings.LastProjectPath = Path.GetDirectoryName(file.Path.LocalPath);
                await _settingsService.SaveAsync(settings);
            }
            catch (Exception)
            {
                WeakReferenceMessenger.Default.Send(
                    new NotificationMessage(new NotificationInfo { Message = $"无法加载文件: {file.Name}", Type = NotificationType.Error }));
            }
        }
    }

    [RelayCommand]
    private void RemoveFile()
    {
        if (SelectedFile != null)
        {
            PdfFiles.Remove(SelectedFile);
        }
    }

    [RelayCommand]
    private void ClearFiles()
    {
        PdfFiles.Clear();
    }

    [RelayCommand]
    private async Task Merge()
    {
        if (PdfFiles.Count < 2) return;

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        var storage = desktop.MainWindow?.StorageProvider;
        if (storage == null) return;

        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "保存合并后的PDF",
            SuggestedFileName = "merged.pdf",
            FileTypeChoices = new[] { new FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } } }
        });

        if (file == null) return;

        IsProcessing = true;
        Progress = 0;

        try
        {
            var filePaths = PdfFiles.Select(f => f.FilePath).ToList();
            var progress = new Progress<int>(p => Progress = p);
            await _pdfService.MergePdfsAsync(filePaths, file.Path.LocalPath, progress);

            WeakReferenceMessenger.Default.Send(
                new NotificationMessage(new NotificationInfo { Message = $"成功合并 {filePaths.Count} 个PDF文件", Type = NotificationType.Success }));
        }
        catch (Exception)
        {
            WeakReferenceMessenger.Default.Send(
                new NotificationMessage(new NotificationInfo { Message = "PDF合并失败", Type = NotificationType.Error }));
        }
        finally
        {
            IsProcessing = false;
            Progress = 0;
        }
    }
}
