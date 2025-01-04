// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using RinceDCS.Models;
using RinceDCS.Services;
using RinceDCS.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RinceDCS.ViewModels;

public partial class EditLayoutVM : ObservableObject
{
    [ObservableProperty]
    private RinceDCSJoystick stick;

    [ObservableProperty]
    private RinceDCSJoystickButton currentButton;

    public ObservableCollection<string> FontNames { get; set; } = new();
    public ObservableCollection<int> FontSizes { get; set; } = new() { 20, 22, 24, 26, 28, 32, 36, 40 };

    public ScaleVMHelper ScaleHelper { get; }

    public EditLayoutVM(RinceDCSJoystick joystick, List<string> fonts)
    {
        Stick = joystick;
        CurrentButton = null;
        foreach (string font in fonts)
        {
            FontNames.Add(font);
        }
        ScaleHelper = ScaleVMHelper.Default;
    }

    [RelayCommand]
    private void AlignButtonLeft()
    {
        if (CurrentButton == null) return;

        CurrentButton.Alignment = "Left";
    }

    [RelayCommand]
    private void AlignButtonCenter()
    {
        if (CurrentButton == null) return;

        CurrentButton.Alignment = "Center";
    }

    [RelayCommand]
    private void AlignButtonRight()
    {
        if (CurrentButton == null) return;

        CurrentButton.Alignment = "Right";
    }

    public void UpdateSettings(int height, int width)
    {
        Stick.ButtonHeight = height;
        Stick.ButtonWidth = width;
    }

    public void UpdateImage(string path)
    {
        Stick.Image = FileService.Default.ReadImageFile(path);
    }

    partial void OnCurrentButtonChanged(RinceDCSJoystickButton oldValue, RinceDCSJoystickButton newValue)
    {
        if (oldValue != null) oldValue.IsSelected = false;
        if (newValue != null) newValue.IsSelected = true;
    }

    public void PlaceButtonOnJoystick(RinceDCSJoystickButton button, int x, int y)
    {
        button.TopX = x;
        button.TopY = y;
        button.OnLayout = true;
    }

    public void DrawLineFromButtonToLocation(RinceDCSJoystickButton button, int x, int y)
    {
        button.LineEndX = x;
        button.LineEndY = y;
        button.DrawLine = true;
    }

    internal void HideButtonLine(RinceDCSJoystickButton button)
    {
        button.DrawLine = false;
    }
}
