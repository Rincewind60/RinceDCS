// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;

namespace RinceDCS.ViewModels;

public partial class JoystickSettingsVM : ObservableObject
{
    [ObservableProperty]
    private int height;
    [ObservableProperty]
    private int width;

    public JoystickSettingsVM(int height, int width)
    {
        Height = height;
        Width = width;
    }
}
