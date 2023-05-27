using CommunityToolkit.Mvvm.DependencyInjection;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Drawing;
using System.Text.Json.Serialization;

namespace RinceDCS.Services;

public class FileService : IFileService
{
    public async Task<Game> OpenGame()
    {
        Game game = null;

        string savePath = Ioc.Default.GetRequiredService<ISettingsService>().GetSetting(RinceDCSSettings.LastSavePath);
        if(File.Exists(savePath))
        {
            FileStream stream = File.OpenRead(savePath);
            game = await JsonSerializer.DeserializeAsync<Game>(stream);
            stream.Dispose();
        }

        return game;
    }

    public async Task SaveGame(Game game)
    {
        string savePath = Ioc.Default.GetRequiredService<ISettingsService>().GetSetting(RinceDCSSettings.LastSavePath);
        if(savePath == null)
        {
            await SaveAsGame(game);
        }
        else
        {
            await SaveGameToPath(game, savePath);
        }
    }

    public async Task SaveAsGame(Game game)
    {
        string savePath = await Ioc.Default.GetRequiredService<IDialogService>().OpenPickSaveFile("RinceDCS.json","DCS Tool",".json");
        if (savePath == null)
        {
            await Ioc.Default.GetRequiredService<IDialogService>().OpenInfoDialog("Save Error", "No save file selected.");
        }
        else
        {
            await SaveGameToPath(game, savePath);
        }

    }

    public Image OpenImageFile(string path)
    {
        using (var fileSrtream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            Image image = Image.FromStream(fileSrtream, false, false);
            return image;
        }
    }

    private async Task SaveGameToPath(Game game, string savePath)
    {
        //  Update Save Path setting so we remember where to save to/open from
        Ioc.Default.GetRequiredService<ISettingsService>().SetSetting(RinceDCSSettings.LastSavePath, savePath);

        FileStream stream = File.Create(savePath);
        JsonSerializerOptions options = new JsonSerializerOptions()
        {
            //ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true
        };
        await JsonSerializer.SerializeAsync(stream, game, options);
        stream.Dispose();
    }
}
