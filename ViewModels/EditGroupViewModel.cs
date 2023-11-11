using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public class EditGroupTableData
{
    public List<string> BindingHeadings { get; set; } = new();
    public List<dynamic> Aircraft { get; set; } = new();
}

public partial class EditGroupViewModel : ObservableObject
{
    [ObservableProperty]
    public List<RinceDCSGroup> groups;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGroupSelected))]
    private RinceDCSGroup currentBindingGroup;
    public bool IsGroupSelected { get { return CurrentBindingGroup != null; } }
    [ObservableProperty]
    private EditGroupTableData groupData;

    public EditGroupViewModel(List<RinceDCSGroup> groups)
    {
        Groups = groups;
        Groups.Sort((x, y) => {
            return x.Name.CompareTo(y.Name);
        });

    }

    public void CurrentBindingGroupChanged()
    {
        GroupData = null;

        if (!IsGroupSelected) return;

        EditGroupTableData newGroupData = new();

        Dictionary<string, int> bindingHeadingIndex = new();

        CurrentBindingGroup.Bindings.Sort((x, y) => {
            return x.Id.CompareTo(y.Id);
        });

        for (int i = 0; i < CurrentBindingGroup.Bindings.Count; i++)
        {
            bindingHeadingIndex[CurrentBindingGroup.Bindings[i].Id] = i;
            newGroupData.BindingHeadings.Add(CurrentBindingGroup.Bindings[i].Id);
        }

        foreach (RinceDCSGroupAircraft boundAircraft in CurrentBindingGroup.AircraftBindings)
        {
            dynamic dynAircraft = new ExpandoObject();
            dynAircraft.AircraftName = boundAircraft.AircraftName;
            IDictionary<String, Object> dynAircraftMembers = (IDictionary<String, Object>)dynAircraft;
            for (int j = 0; j < CurrentBindingGroup.Bindings.Count; j++)
            {
                string bindingName = "Binding" + j.ToString();
                if (bindingHeadingIndex[boundAircraft.BindingId] == j)
                {
                    dynAircraftMembers.TryAdd(bindingName, boundAircraft);
                    dynAircraftMembers.TryAdd(bindingName + "Visible", Visibility.Visible);
                }
                else
                {
                    dynAircraftMembers.TryAdd(bindingName, null);
                    dynAircraftMembers.TryAdd(bindingName + "Visible", Visibility.Collapsed);
                }
            }
            newGroupData.Aircraft.Add(dynAircraft);
        }

        GroupData = newGroupData;

        var query = from stickBinding in currentBindingGroup.JoystickBindings
                    from button in stickBinding.Buttons
                    select Tuple.Create(stickBinding.Joystick, button);
    }
}
