using Microsoft.UI.Xaml;
using RinceDCS.Models;
using System.Drawing;
using System.Threading.Tasks;

namespace RinceDCS.ServiceModels;

public interface IFileService
{
    public Task<RinceDCSFile> OpenGame(string path);

    public Task SaveGame(RinceDCSFile rinceDCSFile);

    public Task SaveAsGame(RinceDCSFile rinceDCSFile);

    public Image OpenImageFile(string path);
    
    public byte[] ReadImageFile(string path);
}
