using RinceDCS.Properties;
using RinceDCS.ServiceModels;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Storage;

namespace RinceDCS.Services;

public class SettingsService : ISettingsService
{
    public string GetSetting(RinceDCSSettings key)
    {
        switch (key)
        {
            case RinceDCSSettings.SavedGamesPath:
                return GetKnownFolderPath(key);
            case RinceDCSSettings.LastSavePath:
                return Settings.Default.LastSavePath;
            case RinceDCSSettings.JoysticScaleIndex:
                return Settings.Default.JoysticScaleIndex;
        }
        return null;
    }

    public void SetSetting(RinceDCSSettings key, string value)
    {
        switch (key)
        {
            case RinceDCSSettings.SavedGamesPath:
                break;
            case RinceDCSSettings.LastSavePath:
                Settings.Default.LastSavePath = value;
                Settings.Default.Save();
                break;
            case RinceDCSSettings.JoysticScaleIndex:
                Settings.Default.JoysticScaleIndex = value;
                Settings.Default.Save();
                break;
        }
    }

    private static string GetKnownFolderPath(RinceDCSSettings folderType)
    {
        IntPtr pathPtr = IntPtr.Zero;
        Guid guid = KnownFolders[folderType];
        try
        {
            SHGetKnownFolderPath(ref guid, 0, IntPtr.Zero, out pathPtr);
            return Marshal.PtrToStringUni(pathPtr);
        }
        finally
        {
            Marshal.FreeCoTaskMem(pathPtr);
        }
    }

    private static readonly Dictionary<RinceDCSSettings, Guid> KnownFolders = new()
    {
        [RinceDCSSettings.SavedGamesPath] = new("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4")
    };

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);
}
