﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using RinceDCS.Models;
using RinceDCS.Services;
using RinceDCS.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RinceDCS.ViewModels;

public partial class EditJoystickViewModel : ObservableObject
{
    [ObservableProperty]
    private RinceDCSJoystick stick;

    [ObservableProperty]
    private RinceDCSJoystickButton currentButton;

    public ObservableCollection<string> FontNames { get; set; } = new();
    public ObservableCollection<int> FontSizes { get; set; } = new() { 20, 22, 24, 26, 28, 32, 36, 40 };

    public ScaleVMHelper ScaleHelper { get; }

    public EditJoystickViewModel(RinceDCSJoystick joystick, List<string> fonts)
    {
        Stick = joystick;
        CurrentButton = null;
        foreach(string font in fonts)
        {
            FontNames.Add(font);
        }
        ScaleHelper = Ioc.Default.GetRequiredService<ScaleVMHelper>();
    }

    [RelayCommand]
    private void AlignButtonLeft()
    {
        if(CurrentButton == null) return;

        CurrentButton.Alignment = "Left";
    }

    [RelayCommand]
    private void AlignButtonCenter()
    {
        if(CurrentButton == null) return;

        CurrentButton.Alignment = "Center";
    }

    [RelayCommand]
    private void AlignButtonRight()
    {
        if(CurrentButton == null) return;

        CurrentButton.Alignment = "Right";
    }

    public void UpdateSettings(int defaultHeight, int defaultWidth)
    {
        Stick.DefaultLabelHeight = defaultHeight;
        Stick.DefaultLabelWidth = defaultWidth;
    }

    public void UpdateImage(string path)
    {
        Stick.Image = FileService.Default.ReadImageFile(path);
    }

    partial void OnCurrentButtonChanged(RinceDCSJoystickButton oldValue, RinceDCSJoystickButton newValue)
    {
        if(oldValue != null) oldValue.IsSelected = false;
        if(newValue != null) newValue.IsSelected = true;
    }

    public void PlaceButtonOnJoystick(RinceDCSJoystickButton button, int x, int y)
    {
        button.TopX = x;
        button.TopY = y;
        button.Width = Stick.DefaultLabelWidth;
        button.Height = Stick.DefaultLabelHeight;
        button.OnLayout = true;
    }

    public void UpdateButtonDimensions(RinceDCSJoystickButton button, int newRight, int newBottom)
    {
        button.Width = Math.Max(0, (int)(newRight - button.TopX));
        button.Height = Math.Max(0, (int)(newBottom - button.TopY));
    }
}
