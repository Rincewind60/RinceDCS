// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace RinceDCS.ViewModels;

public partial class JoystickSettingsVM : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial int Height { get; set; }

    [ObservableProperty]
    public partial int Width { get; set; }

    [ObservableProperty]
    public partial bool IsValid { get; set; }

    public JoystickSettingsVM(string title, int height, int width)
    {
        Title = title;
        Height = height;
        Width = width;
        IsValid = false;
    }

    internal void TitleChanged(string text)
    {
        IsValid = !String.IsNullOrWhiteSpace(text);
    }
}
