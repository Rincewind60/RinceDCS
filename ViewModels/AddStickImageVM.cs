// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public partial class AddStickImageVM : ObservableObject
{
    [ObservableProperty]
    private RinceDCSJoystick stick;

    [ObservableProperty]
    private string newImageTabTitle;

    [ObservableProperty]
    public bool isValid;

    public AddStickImageVM(RinceDCSJoystick stick)
    {
        Stick = stick;
        IsValid = false;
    }

    public void TextChanged(string newText)
    {
        IsValid = !String.IsNullOrWhiteSpace(newText);
    }
}
