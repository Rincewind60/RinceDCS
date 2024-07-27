// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace RinceDCS.Models;

public partial class RinceDCSFile : ObservableObject
{
    public ObservableCollection<RinceDCSJoystick> Joysticks { get; set; } = new();

    public ObservableCollection<RinceDCSInstance> Instances { get; set; } = new();

    public string CurrentInstanceName { get; set; }
}
