using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace RinceDCS.Models;

public partial class RinceDCSInstance : ObservableObject
{
    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string gameExePath;

    [ObservableProperty]
    private string savedGamesPath;

    [ObservableProperty]
    private RinceDCSGroups groups;

    public ObservableCollection<RinceDCSAircraft> Aircraft { get; set; } = new();

    public string CurrentAircraftName { get; set; }

    [JsonIgnore]
    public DCSData BindingsData;
}