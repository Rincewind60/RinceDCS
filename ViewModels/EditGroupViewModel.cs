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
    public List<string> ActionHeadings { get; set; } = new();
    public List<dynamic> Aircraft { get; set; } = new();
}

public partial class EditGroupViewModel : ObservableObject
{
    [ObservableProperty]
    public List<RinceDCSGroup> groups;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGroupSelected))]
    private RinceDCSGroup currentActionGroup;
    public bool IsGroupSelected { get { return CurrentActionGroup != null; } }
    [ObservableProperty]
    private EditGroupTableData groupData;

    public EditGroupViewModel(List<RinceDCSGroup> groups)
    {
        Groups = groups;
        Groups.Sort((x, y) => {
            return x.Name.CompareTo(y.Name);
        });
    }

    public void CurrentActionGroupChanged()
    {
        GroupData = null;

        if (!IsGroupSelected) return;

        EditGroupTableData newGroupData = new();

        Dictionary<string, int> actionHeadingIndex = new();

        CurrentActionGroup.Actions.Sort((x, y) => {
            return x.Id.CompareTo(y.Id);
        });

        for (int i = 0; i < CurrentActionGroup.Actions.Count; i++)
        {
            actionHeadingIndex[CurrentActionGroup.Actions[i].Id] = i;
            newGroupData.ActionHeadings.Add(CurrentActionGroup.Actions[i].Id);
        }

        foreach (RinceDCSGroupAircraft actionAircraft in CurrentActionGroup.Aircraft)
        {
            dynamic dynAircraft = new ExpandoObject();
            dynAircraft.AircraftName = actionAircraft.AircraftName;
            IDictionary<String, Object> dynAircraftMembers = (IDictionary<String, Object>)dynAircraft;
            for (int j = 0; j < CurrentActionGroup.Actions.Count; j++)
            {
                string bindingName = "Action" + j.ToString();
                if (actionHeadingIndex[actionAircraft.ActionId] == j)
                {
                    dynAircraftMembers.TryAdd(bindingName, actionAircraft);
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

        var query = from stickActiong in CurrentActionGroup.Joysticks
                    from button in stickActiong.Buttons
                    select Tuple.Create(stickActiong.Joystick, button);
    }
}
