using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using RinceDCS.ViewModels.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace RinceDCS.ViewModels;

public partial class EditJoystickViewModel : ObservableObject
{
    [ObservableProperty]
    private GameJoystick stick;

    [ObservableProperty]
    private GameJoystickButton currentButton;

    public ObservableCollection<string> FontNames { get; set; } = new();
    public ObservableCollection<int> FontSizes { get; set; } = new() { 20, 22, 24, 26, 28, 32, 36, 40 };

    public ScaleVMHelper ScaleHelper { get; }

    public EditJoystickViewModel(GameJoystick joystick, List<string> fonts)
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
        Stick.Image = Ioc.Default.GetRequiredService<IFileService>().ReadImageFile(path);
    }

    partial void OnCurrentButtonChanged(GameJoystickButton oldValue, GameJoystickButton newValue)
    {
        if(oldValue != null) oldValue.IsSelected = false;
        if(newValue != null) newValue.IsSelected = true;
    }

    public void PlaceButtonOnJoystick(GameJoystickButton button, int x, int y)
    {
        button.TopX = x;
        button.TopY = y;
        button.Width = Stick.DefaultLabelWidth;
        button.Height = Stick.DefaultLabelHeight;
        button.OnLayout = true;
    }

    public void UpdateButtonDimensions(GameJoystickButton button, int newRight, int newBottom)
    {
        button.Width = Math.Max(0, (int)(newRight - button.TopX));
        button.Height = Math.Max(0, (int)(newBottom - button.TopY));
    }
}
