// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace RinceDCS.Models;

public partial class RinceDCSInstance : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string GameExePath { get; set; }

    [ObservableProperty]
    public partial string SavedGamesPath { get; set; }

    [ObservableProperty]
    public partial RinceDCSGroups Groups { get; set; }

    public ObservableCollection<RinceDCSAircraft> Aircraft { get; set; } = [];

    [JsonIgnore]
    public DCSData ControlsData;
}