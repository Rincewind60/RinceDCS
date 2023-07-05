using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using RinceDCS.ServiceModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public List<string> Scales { get; set; } = new() { "400%", "200%", "100%", "75%", "50%", "25%" };

    [ObservableProperty]
    private int currentScale;

    public void Initialize()
    {
        string scaleIndex = Ioc.Default.GetRequiredService<ISettingsService>().GetSetting(RinceDCSSettings.JoysticScaleIndex);
        CurrentScale = string.IsNullOrWhiteSpace(scaleIndex) ? 2 : int.Parse(scaleIndex);
    }

    partial void OnCurrentScaleChanged(int value)
    {
        Ioc.Default.GetRequiredService<ISettingsService>().SetSetting(RinceDCSSettings.JoysticScaleIndex, value.ToString());
    }

}
