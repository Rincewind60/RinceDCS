using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
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

    public EditGroupsViewModel(RinceDCSGroups groups)
    {
        Groups = groups;

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

        Dictionary<string, int> bindingHeadingIndex = new();

        //CurrentBindingGroup.Bindings.Sort((x, y) => {
        //    return x.Id.CompareTo(y.Id);
        //});

        //for (int i = 0; i < CurrentBindingGroup.Bindings.Count; i++)
        //{
        //    bindingHeadingIndex[CurrentBindingGroup.Bindings[i].Id] = i;
        //    newGroupData.BindingHeadings.Add(CurrentBindingGroup.Bindings[i].Id);
        //}

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
            //IDictionary<String, Object> dynGroupMembers = (IDictionary<String, Object>)dynGroup;
            //for (int j = 0; j < grp.JoystickBindings.Count; j++)
            //{
            //    string bindingName = "Binding" + j.ToString();
            //    if (bindingHeadingIndex[boundAircraft.BindingId] == j)
            //    {
            //        dynAircraftMembers.TryAdd(bindingName, boundAircraft);
            //        dynAircraftMembers.TryAdd(bindingName + "Visible", Visibility.Visible);
            //    }
            //    else
            //    {
            //        dynAircraftMembers.TryAdd(bindingName, null);
            //        dynAircraftMembers.TryAdd(bindingName + "Visible", Visibility.Collapsed);
            //    }
            //}
            newGroupsData.Groups.Add(dynGroup);
        }

        GroupsData = newGroupsData;
    }
}
