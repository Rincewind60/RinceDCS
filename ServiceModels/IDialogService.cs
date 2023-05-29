using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System.Threading.Tasks;

namespace RinceDCS.ServiceModels;

public interface IDialogService
{
    public Task OpenInfoDialog(string title, string message);

    public Task<bool?> OpenConfirmationDialog(string title, string message);

    public Task<ContentDialogResult> OpenResponseDialog(string title, string message, string primaryButtonText, string secondaryButtonText, string cancelButtonText);

    public Task OpenInfoPageDialog(string title, Page page);

    public Task<ContentDialogResult> OpenResponsePageDialog(string title, Page page, string primaryButtonText, Binding primaryButtonEnabledBinding, string secondaryButtonText, string cancelButtonText);

    public Task<string> OpenPickFile(string fileTypeFilter);

    public Task<string> OpenPickSaveFile(string suggestedFileName, string fileTypeLabel, string fileTypeFilter);

    public Task<string> OpenPickFolder();
}
