namespace RinceDCS.ServiceModels;

public enum RinceDCSSettings
{
    LastSavePath,
    SavedGamesPath,
    JoysticScaleIndex
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
