using CommunityToolkit.Mvvm.DependencyInjection;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RinceDCS.Services;

public class FileService : IFileService
{
    public async Task<RinceDCSFile> OpenGame(string path)
    {
        RinceDCSFile rinceDCSFile = null;

        if(File.Exists(path))
        {
            FileStream stream = File.OpenRead(path);
            rinceDCSFile = await JsonSerializer.DeserializeAsync<RinceDCSFile>(stream);

            foreach(RinceDCSJoystick stick in rinceDCSFile.Joysticks)
            {
                foreach(RinceDCSJoystickButton button in stick.Buttons)
                {
                    button.Font = stick.Font;
                    button.FontSize = stick.FontSize;
                }
            }

            stream.Dispose();

            Ioc.Default.GetRequiredService<ISettingsService>().SetSetting(RinceDCSSettings.LastSavePath, path);
        }

        return rinceDCSFile;
    }

    public async Task SaveGame(RinceDCSFile rinceDCSFile)
    {
        string savePath = Ioc.Default.GetRequiredService<ISettingsService>().GetSetting(RinceDCSSettings.LastSavePath);
        if(savePath == null)
        {
            await SaveAsGame(rinceDCSFile);
        }
        else
        {
            await SaveGameToPath(rinceDCSFile, savePath);
        }
    }

    public async Task SaveAsGame(RinceDCSFile rinceDCSFile)
    {
        string savePath = await Ioc.Default.GetRequiredService<IDialogService>().OpenPickSaveFile("RinceDCS.json","DCS Tool",".json");
        if (savePath == null)
        {
            await Ioc.Default.GetRequiredService<IDialogService>().OpenInfoDialog("Save Error", "No save file selected.");
        }
        else
        {
            await SaveGameToPath(rinceDCSFile, savePath);
        }

    }

    public Image OpenImageFile(string path)
    {
        Image image;
        using (FileStream fs = new(path, FileMode.Open, FileAccess.Read))
        {
            image = Image.FromStream(fs, false, false);
        }
        return image;
    }

    public byte[] ReadImageFile(string path)
    {
        FileInfo fileInfo = new(path);

        // The byte[] to save the data in
        byte[] data = new byte[fileInfo.Length];

        // Load a filestream and put its content into the byte[]
        using (FileStream fs = fileInfo.OpenRead())
        {
            fs.Read(data, 0, data.Length);
        }

        return data;
    }

    private async Task SaveGameToPath(RinceDCSFile rinceDCSFile, string savePath)
    {
        //  Update Save Path setting so we remember where to save to/open from
        Ioc.Default.GetRequiredService<ISettingsService>().SetSetting(RinceDCSSettings.LastSavePath, savePath);

        using (FileStream stream = File.Create(savePath))
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };
            await JsonSerializer.SerializeAsync(stream, rinceDCSFile, options);
        }
    }
}
