// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace RinceDCS.ViewModels;

public partial class JoystickSettingsVM : ObservableObject
{
    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private int height;

    [ObservableProperty]
    private int width;

    [ObservableProperty]
    public bool isValid;

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
