// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;

namespace RinceDCS.ViewModels;

public partial class JoystickSettingsVM : ObservableObject
{
    [ObservableProperty]
    private int defaultHeight;
    [ObservableProperty]
    private int defaultWidth;

    public JoystickSettingsVM(int defaultHeight, int defaultWidth)
    {
        DefaultHeight = defaultHeight;
        DefaultWidth = defaultWidth;
    }
}
