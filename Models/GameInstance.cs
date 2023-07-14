using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace RinceDCS.Models;

public partial class GameInstance : ObservableObject
{
    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string gameExePath;

    [ObservableProperty]
    private string savedGameFolderPath;

    [ObservableProperty]
    private GameBindingGroups bindingGroups;

    public ObservableCollection<GameAircraft> Aircraft { get; set; } = new();

    public string CurrentAircraftName { get; set; }

    [JsonIgnore]
    public DCSData BindingsData;
}