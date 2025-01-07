// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using RinceDCS.Models;
using RinceDCS.Services;
using RinceDCS.ViewModels.Helpers;
using RinceDCS.ViewModels.Messages;
using RinceDCS.Views;
using RinceDCS.Views.Utilities;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace RinceDCS.ViewModels;

public partial class EditLayoutVM : ObservableObject
{
    [ObservableProperty]
    private RinceDCSJoystick stick;

    [ObservableProperty]
    private RinceDCSJoystickImage stickImage;

    [ObservableProperty]
    private RinceDCSJoystickButton currentButton;
     
    public bool IsNotLastImage {  get {  return Stick.Images.Count > 1; } }

    public ObservableCollection<string> FontNames { get; set; } = new();
    public ObservableCollection<int> FontSizes { get; set; } = new() { 20, 22, 24, 26, 28, 32, 36, 40 };

    public ScaleVMHelper ScaleHelper { get; }

    public EditLayoutVM(RinceDCSJoystick joystick, RinceDCSJoystickImage stickImage, List<string> fonts)
    {
        Stick = joystick;
        StickImage = stickImage;
        CurrentButton = null;
        foreach (string font in fonts)
        {
            FontNames.Add(font);
        }
        ScaleHelper = ScaleVMHelper.Default;

        WeakReferenceMessenger.Default.Register<DeleteJoystickImageMessage>(this, (r, m) =>
        {
            StickImageTabDeleted(m.Stick, m.StickImage);
        });
    }

    private void StickImageTabDeleted(RinceDCSJoystick stick, RinceDCSJoystickImage stickImage)
    {
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<bool>(this, "IsNotLastImage", false, IsNotLastImage));
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

    [RelayCommand]
    private async void AddStickImage()
    {
        AddStickImageDialog page = new AddStickImageDialog(Stick);
        Binding addBinding = new Binding
        {
            Source = page.ViewModel,
            Path = new PropertyPath("IsValid"),
            Mode = BindingMode.OneWay
        };

        ContentDialogResult result = await DialogService.Default.OpenResponsePageDialog("Add Joystick Image Tab", page, "Create", addBinding, null, "Cancel");
        if (result == ContentDialogResult.Primary)
        {
            RinceDCSJoystickImage newImage = new RinceDCSJoystickImage() { Image = null, Title = page.ViewModel.NewImageTabTitle, Buttons = [] };
            foreach(RinceDCSJoystickButton button in stickImage.Buttons)
            {
                RinceDCSJoystickButton newButton = new RinceDCSJoystickButton(stick)
                {
                    ButtonName = button.ButtonName,
                    IsKeyButton = button.IsKeyButton,
                    IsModifier = button.IsModifier
                };
                newImage.Buttons.Add(newButton);
            }
            Stick.Images.Add(newImage);

            WeakReferenceMessenger.Default.Send(new 
                AddNewJoystickImageMessage(Stick, newImage));
        }
    }

    [RelayCommand]
    private async void DeleteStickImage()
    {
        bool? result = await DialogService.Default.OpenConfirmationDialog("Delete Joystick Image Tab", "Do you want to delete this image from the " + Stick.AttachedJoystick.Name + " Joystick");
        if(result.HasValue && result.Value)
        {
            Stick.Images.Remove(StickImage);
            WeakReferenceMessenger.Default.Send(new DeleteJoystickImageMessage(Stick, StickImage));
        }
    }

    public void UpdateSettings(string title, int height, int width)
    {
        StickImage.Title = title;
        Stick.ButtonHeight = height;
        Stick.ButtonWidth = width;
    }

    public void UpdateImage(string path)
    {
        StickImage.Image = FileService.Default.ReadImageFile(path);
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
