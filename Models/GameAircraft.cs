﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RinceDCS.Models;

public partial class GameAircraft : ObservableObject
{
    [ObservableProperty]
    private string name;
}
