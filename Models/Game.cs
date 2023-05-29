using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace RinceDCS.Models;

public partial class Game : ObservableObject
{
    public ObservableCollection<GameJoystick> Joysticks { get; set; } = new();

    public ObservableCollection<GameInstance> Instances { get; set; } = new();

    public string CurrentInstanceName { get; set; }
}
