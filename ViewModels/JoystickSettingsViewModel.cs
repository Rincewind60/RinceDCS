using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public partial class JoystickSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private int defaultHeight;
    [ObservableProperty]
    private int defaultWidth;

    public JoystickSettingsViewModel(int defaultHeight, int defaultWidth)
    {
        DefaultHeight = defaultHeight;
        DefaultWidth = defaultWidth;
    }
}
