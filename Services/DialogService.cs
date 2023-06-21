using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using RinceDCS.ServiceModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace RinceDCS.Services;

public class DialogService : IDialogService
{
    private readonly Window AppWindow;
    private readonly FrameworkElement Parent;

    public DialogService(Window appWindow)
    {
        AppWindow = appWindow;
        Parent =appWindow.Content as FrameworkElement;
    }

    public async Task OpenInfoDialog(string title, string message)
    {
        await OpenDialog(Parent, title, message);
    }

    public async Task<bool?> OpenConfirmationDialog(string title, string message)
    {
        ContentDialogResult result = await OpenDialog(Parent, title, message, "Yes", null, "", "No");

        if (result == ContentDialogResult.None)
        {
            return null;
        }

        return (result == ContentDialogResult.Primary);
    }

    public async Task<ContentDialogResult> OpenResponseDialog(string title, string message, string primaryButtonText, string secondaryButtonText, string cancelButtonText)
    {
        return await OpenDialog(Parent, title, message, primaryButtonText, null, secondaryButtonText, cancelButtonText);
    }

    public async Task OpenInfoPageDialog(string title, Page page)
    {
        await OpenDialog(Parent, title, page);
    }

    public async Task<ContentDialogResult> OpenResponsePageDialog(string title, Page page, string primaryButtonText, Binding primaryButtonEnabledBinding, string secondaryButtonText, string cancelButtonText)
    {
        return await OpenDialog(Parent, title, page, primaryButtonText, primaryButtonEnabledBinding, secondaryButtonText, cancelButtonText);
    }

    public async Task<string> OpenPickFile(string fileTypeFilter)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker
        {
            ViewMode = PickerViewMode.List
        };
        picker.FileTypeFilter.Add(fileTypeFilter);

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(AppWindow);
        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);

        // Open the picker for the user to pick a file
        StorageFile file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            return file.Path;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Allows user o select new save file.
    /// 
    /// Note: As a side affect the file is actually created!
    /// </summary>
    /// <param name="suggestedFileName"></param>
    /// <param name="fileTypeLabel"></param>
    /// <param name="fileTypeFilter"></param>
    /// <returns></returns>
    public async Task<string> OpenPickSaveFile(string suggestedFileName, string fileTypeLabel, string fileTypeFilter)
    {
        var picker = new Windows.Storage.Pickers.FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = suggestedFileName
        };
        picker.FileTypeChoices.Add(fileTypeLabel, new List<string>() { fileTypeFilter });

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(AppWindow);
        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);

        // Open the picker for the user to pick a file
        StorageFile file = await picker.PickSaveFileAsync();
        if (file != null)
        {
            return file.Path;
        }
        else
        {
            return null;
        }
    }

    public async Task<string> OpenPickFolder()
    {
        var picker = new Windows.Storage.Pickers.FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            ViewMode = PickerViewMode.List
        };
        picker.FileTypeFilter.Add("*");

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(AppWindow);

        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);

        // Open the picker for the user to pick a file
        StorageFolder folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            return folder.Path;
        }
        else
        {
            return null;
        }
    }

    private async Task<ContentDialogResult> OpenDialog
        (
        FrameworkElement parent,
        string title,
        Object content,
        string primaryButtonText = null,
        Binding primaryButtonEnabledBinding = null,
        string secondaryButtonText = null,
        string cancelButtonText = null
        )
    {
        ContentDialog dialog = new()
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = parent.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = title,
            PrimaryButtonText = primaryButtonText,
            SecondaryButtonText = secondaryButtonText,
            CloseButtonText = string.IsNullOrWhiteSpace(cancelButtonText) ? "Close" : cancelButtonText,
            DefaultButton = ContentDialogButton.Close,
            Content = content,
            MinWidth = 600,
            MinHeight = 400
        };

        if (primaryButtonEnabledBinding != null)
        {
            dialog.SetBinding(ContentDialog.IsPrimaryButtonEnabledProperty, primaryButtonEnabledBinding);
        }

        return await dialog.ShowAsync();
    }
}
