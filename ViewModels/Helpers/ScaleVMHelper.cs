﻿using CommunityToolkit.Mvvm.ComponentModel;
using RinceDCS.Properties;
using System.Collections.Generic;

namespace RinceDCS.ViewModels.Helpers;

/// <summary>
/// Helper class to help caling of Joystick views.
/// 
/// Created as an Ioc singleton so the one occurance is shared by all Joystick views.
/// 
/// Dependencies:
/// 
/// - The Initialize() method must be called after all the Ioc services are created as it depeneds on the Settings Service.
/// 
/// </summary>
public partial class ScaleVMHelper : ObservableObject
{
    private static ScaleVMHelper defaultInstance = new ScaleVMHelper();

    public static ScaleVMHelper Default
    {
        get { return defaultInstance; }
    }

    public List<string> Scales { get; set; } = new() { "400%", "200%", "150%", "100%", "80%", "60%", "50%", "40%", "30%" };
    public float[] ZoomFactors = { 4F, 2F, 1.5F, 1F, 0.8F, 0.6F, 0.5F, 0.4F, 0.30F };

    [ObservableProperty]
    private int currentScale;

    public ScaleVMHelper()
    {
        string scaleIndex = Settings.Default.JoysticScaleIndex;
        CurrentScale = string.IsNullOrWhiteSpace(scaleIndex) ? 3 : int.Parse(scaleIndex);
    }

    partial void OnCurrentScaleChanged(int value)
    {
        Settings.Default.JoysticScaleIndex = value.ToString();
        Settings.Default.Save();
    }
}
