using CommunityToolkit.Mvvm.ComponentModel;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public partial class EditGroupsAircraft : ObservableObject
{
    [ObservableProperty]
    private string aircraftName;
    public ObservableCollection<GameBoundAircraft> Bindings { get; set; } = new();
}

public class EditGroupsTableData
{
    public List<string> BindingHeadings { get; set; } = new();
    public List<EditGroupsAircraft> Aircraft { get; set; } = new();
}

public partial class EditGroupsViewModel : ObservableObject
{
    [ObservableProperty]
    public List<GameBindingGroup> bindingGroups;
    [ObservableProperty]
    private GameBindingGroup currentBindingGroup;
    [ObservableProperty]
    private EditGroupsTableData groupData;

    public EditGroupsViewModel(List<GameBindingGroup> groups, DCSData data)
    {
        bindingGroups = groups.ToList();
        bindingGroups.Sort((x, y) => {
            return x.Name.CompareTo(y.Name);
        });
    }

    public void CurrentBindingGroupChanged()
    {
        GroupData = null;
        EditGroupsTableData newGroupData = new();

        Dictionary<string,int> bindingHeadingIndex = new();

        CurrentBindingGroup.Bindings.Sort((x, y) => {
            return x.Id.CompareTo(y.Id);
        });

        for (int i = 0; i < CurrentBindingGroup.Bindings.Count; i++)
        {
            bindingHeadingIndex[CurrentBindingGroup.Bindings[i].Id] = i;
            newGroupData.BindingHeadings.Add(CurrentBindingGroup.Bindings[i].Id);
        }

        foreach(GameBoundAircraft boundAircraft in CurrentBindingGroup.BoundAircraft)
        {
            EditGroupsAircraft aircraft = new ();
            aircraft = new EditGroupsAircraft()
            {
                AircraftName = boundAircraft.AircraftName,
                Bindings = new()
            };
            for(int j = 0; j < CurrentBindingGroup.Bindings.Count; j++)
            {
                aircraft.Bindings.Add(null);
            }
            aircraft.Bindings[bindingHeadingIndex[boundAircraft.BindingId]] = boundAircraft;
            newGroupData.Aircraft.Add(aircraft);
        }

        GroupData = newGroupData;
    }
}
