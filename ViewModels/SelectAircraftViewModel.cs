using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public partial class SelectAircraft : ObservableObject
{
    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private bool selected;
}

public partial class SelectAircraftViewModel : ObservableObject
{
    public ObservableCollection<SelectAircraft> Aircraft = new();

    [ObservableProperty]
    private bool selectAll;

    public SelectAircraftViewModel(List<string> aircraftNames)
    {
        foreach(string name in aircraftNames)
        {
            Aircraft.Add(new SelectAircraft() { Name = name, Selected = true });
        }
        SelectAll = true;
    }

    public void SelectChecked()
    {
        foreach(SelectAircraft aircraft in Aircraft)
        {
            aircraft.Selected = true;
        }
    }

    public void SelectUnchecked()
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
