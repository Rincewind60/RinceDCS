using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ServiceModels;

public interface IFileService
{
    public Task<Game> OpenGame();

    public Task SaveGame(Game game);

    public Task SaveAsGame(Game game);

    public Image OpenImageFile(string path);
}
