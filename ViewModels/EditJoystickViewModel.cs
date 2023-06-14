using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
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

    public ObservableCollection<string> Scales { get; set; } = new() { "400%", "200%", "100%", "75%", "50%", "25%" };

    [ObservableProperty]
    private int currentScale;

    public EditJoystickViewModel(GameJoystick joystick, List<string> fonts)
    {
        Stick = joystick;
        CurrentButton = null;
        foreach(string font in fonts)
        {
            FontNames.Add(font);
        }

        CurrentScale = 2;
    }

    partial void OnCurrentButtonChanged(GameJoystickButton oldValue, GameJoystickButton newValue)
    {
        if(oldValue != null)
        {
            oldValue.IsSelected = false;
        }
        newValue.IsSelected = true;
    }

    public void UpdateImage(string path)
    {
        Stick.Image = Ioc.Default.GetRequiredService<IFileService>().ReadImageFile(path);
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

    public void UpdateSettings(int defaultHeight, int defaultWidth)
    {
        Stick.DefaultLabelHeight = defaultHeight;
        Stick.DefaultLabelWidth = defaultWidth;
    }
}
