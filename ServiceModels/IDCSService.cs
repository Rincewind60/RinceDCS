using RinceDCS.Models;
using System.Collections.Generic;
using System.Data;

namespace RinceDCS.ServiceModels;

public interface IDCSService
{
    public DCSData GetBindingData(string gameName, string gameExePath, string savedGameFolderPath, List<AttachedJoystick> sticks);

    public string GetSavedGamesPath(string gameFolderPath, string currentSavedGamesFolder);

    public void UpdateGameBindingData(string savedGameFolderPath, GameBindingGroups bindingGroups, DCSData data);
}
