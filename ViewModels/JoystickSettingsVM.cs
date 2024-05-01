using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public partial class JoystickSettingsVM : ObservableObject
{
    [ObservableProperty]
    private int defaultHeight;
    [ObservableProperty]
    private int defaultWidth;

    public JoystickSettingsVM(int defaultHeight, int defaultWidth)
    {
        DefaultHeight = defaultHeight;
        DefaultWidth = defaultWidth;
    }
}
