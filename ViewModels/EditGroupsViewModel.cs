using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public class EditGroupsTableData
{
    public List<string> Headings { get; set; } = new();
    public List<dynamic> Groups { get; set; } = new();
}


public partial class EditGroupsViewModel : ObservableObject
{
    public ObservableCollection<string> Categories { get; set; } = new();
    [ObservableProperty]
    private string currentCategory;
    [ObservableProperty]
    private EditGroupsTableData groupsData;

    private RinceDCSGroups Groups;
    private List<AttachedJoystick> Sticks;

    public EditGroupsViewModel(RinceDCSGroups groups, List<AttachedJoystick> sticks)
    {
        Groups = groups;
        Sticks = sticks;

        BuildCategories();
    }

    private void BuildCategories()
    {
        Categories.Add("All");
        foreach (string category in (from grp in Groups.Groups where !string.IsNullOrEmpty(grp.Category) select grp.Category).Distinct().Order())
        {
            Categories.Add(category);
        }
        CurrentCategory = Categories[0];
    }

    public void CurrentCategoryChanged()
    {
        GroupsData = null;

        EditGroupsTableData newGroupsData = new();

        //Dictionary<AttachedJoystick, int> joystickHeadingIndex = new();

        AttachedJoystick[] sticks = new AttachedJoystick[Sticks.Count]; ;
        Sticks.CopyTo(sticks, 0);
        sticks = sticks.OrderBy(x => x.Name).ToArray();

        for (int i = 0; i < sticks.Count(); i++)
        {
            //joystickHeadingIndex[sticks[i]] = i;
            newGroupsData.Headings.Add(sticks[i].Name);
        }

        List<RinceDCSGroup> groupsToDisplay = new();
        if(CurrentCategory == "All")
        {
            groupsToDisplay.AddRange(Groups.Groups);
        }
        else
        {
            groupsToDisplay = (from grp in Groups.Groups where grp.Category == CurrentCategory select grp).OrderBy(row => row.Name).ToList();
        }

        foreach (RinceDCSGroup grp in groupsToDisplay)
        {
            dynamic dynGroup = new ExpandoObject();
            dynGroup.Name = grp.Name;
            IDictionary<String, Object> dynGroupMembers = (IDictionary<String, Object>)dynGroup;
            for (int j = 0; j < sticks.Count(); j++)
            {
                string bindingName = "Stick" + j.ToString();

                RinceDCSGroupJoystick grpStick = grp.Joysticks.Find(row => row.Joystick == sticks[j]);
                if(grpStick.Buttons.Count > 0)
                {
                    dynGroupMembers.TryAdd(bindingName + "Buttons", grpStick.GetButtonsLabel());
                    dynGroupMembers.TryAdd(bindingName + "Visible", Visibility.Visible);
                }
                else
                {
                    dynGroupMembers.TryAdd(bindingName + "Buttons", null);
                    dynGroupMembers.TryAdd(bindingName + "Visible", Visibility.Collapsed);
                }
            }
            newGroupsData.Groups.Add(dynGroup);
        }

        GroupsData = newGroupsData;
    }
}
