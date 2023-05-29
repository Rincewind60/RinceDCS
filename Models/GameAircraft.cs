using CommunityToolkit.Mvvm.ComponentModel;

namespace RinceDCS.Models;

public partial class GameAircraft : ObservableObject
{
    [ObservableProperty]
    private string name;
}
