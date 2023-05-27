using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.AppBroadcasting;

namespace RinceDCS.ServiceModels;

public enum RinceDCSSettings
{
    LastSavePath,
    SavedGamesPath
}

/// <summary>
/// Simple wrapper of the windows local app setting.
/// 
/// Note: Currently only supports settings that are strings.
/// </summary>
public interface ISettingsService
{
    public string GetSetting(RinceDCSSettings key);

    public void SetSetting(RinceDCSSettings key, string value);
}
