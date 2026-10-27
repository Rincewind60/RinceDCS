// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RinceDCS.ViewModels;

public partial class SelectAircraft : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }
    [ObservableProperty]
    public partial bool Selected { get; set; }
}

public partial class SelectAircraftVM : ObservableObject
{
    public ObservableCollection<SelectAircraft> Aircraft = new();

    [ObservableProperty]
    public partial bool SelectAll { get; set; }

    public SelectAircraftVM(List<string> aircraftNames)
    {
        foreach (string name in aircraftNames)
        {
            Aircraft.Add(new SelectAircraft() { Name = name, Selected = true });
        }
        SelectAll = true;
    }

    [RelayCommand]
    public void SelectAllChecked()
    {
        foreach (SelectAircraft aircraft in Aircraft)
        {
            aircraft.Selected = true;
        }
    }

    public void SelectAllUnchecked()
    {
        foreach (SelectAircraft aircraft in Aircraft)
        {
            aircraft.Selected = false;
        }
    }

    public List<string> GetSelectedAircraft()
    {
        return (from craft in Aircraft
                where craft.Selected == true
                select craft.Name).ToList();
    }
}
