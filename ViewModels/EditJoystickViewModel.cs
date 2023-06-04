using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.UI.Xaml.Media;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using System;
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
    public ObservableCollection<int> FontSizes { get; set; } = new() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28 };

    public ObservableCollection<string> Scales { get; set; } = new() { "400%", "200%", "100%", "75%", "50%", "25%" };

    [ObservableProperty]
    private string currentScale;

    public EditJoystickViewModel(GameJoystick joystick, List<string> fonts)
    {
        Stick = joystick;
        CurrentButton = null;
        foreach(string font in fonts)
        {
            FontNames.Add(font);
        }

        CurrentScale = Scales[2];
    }

    public void UpdateImage(string path)
    {
        Stick.Image = Ioc.Default.GetRequiredService<IFileService>().ReadImageFile(path);
    }
}
