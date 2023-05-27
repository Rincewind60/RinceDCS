using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RinceDCS.Models;

public partial class GameInstance : ObservableObject
{
    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string gameExePath;

    [ObservableProperty]
    private string savedGameFolderPath;

    public ObservableCollection<GameAircraft> Aircraft { get; set; } = new ObservableCollection<GameAircraft>();

    public string CurrentAircraftName { get; set; }

    [JsonIgnore]
    public DCSData BindingsData;
}