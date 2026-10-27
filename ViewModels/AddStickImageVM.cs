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
    public partial RinceDCSJoystick Stick { get; set; }

    [ObservableProperty]
    public partial string NewImageTabTitle { get; set; }

    [ObservableProperty]
    public partial bool IsValid { get; set; }

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
