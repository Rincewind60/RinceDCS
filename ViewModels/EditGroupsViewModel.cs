using CommunityToolkit.Mvvm.ComponentModel;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public class EditGroupsAircraft
{
    public string AircraftName { get; set; }
    public GameBoundAircraft[] Bindings { get; set; }
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
        UpdateAircraftBindings();
    }

    private void UpdateAircraftBindings()
    {
        /*                 <controls:DataGridTextColumn Header="{x:Bind ViewModel.BindingHeading0, Mode=OneWay}" Binding="{Binding Binding0}" Tag="Binding0" Visibility="{x:Bind local:EditGroupsPage.IsBindingColumnVisible(ViewModel.BindingHeading0), Mode=OneWay}"/>
 */
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
                Bindings = new GameBoundAircraft[CurrentBindingGroup.Bindings.Count]
            };
            aircraft.Bindings[bindingHeadingIndex[boundAircraft.BindingId]] = boundAircraft;
            newGroupData.Aircraft.Add(aircraft);
        }

        GroupData = newGroupData;
    }
}
