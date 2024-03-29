﻿using Microsoft.UI.Xaml;
using RinceDCS.Models;
using System.Drawing;
using System.Threading.Tasks;

namespace RinceDCS.ServiceModels;

public interface IFileService
{
    public Task<Game> OpenGame(string path);

    public Task SaveGame(Game game);

    public Task SaveAsGame(Game game);

    public Image OpenImageFile(string path);
    
    public byte[] ReadImageFile(string path);
}
