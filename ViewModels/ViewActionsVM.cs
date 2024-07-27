// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.UI.Xaml;
using RinceDCS.Models;
using RinceDCS.ViewModels.Messages;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace RinceDCS.ViewModels;
public class ActionTableData
{
    public List<string> JoystickHeadings { get; set; } = new();
    public List<dynamic> Actions { get; set; } = new();
}

public partial class ActionCategory : ObservableObject, IComparable<ActionCategory>
{
    [ObservableProperty]
    private string categoryName;

    public int CompareTo(ActionCategory other)
    {
        return CategoryName.CompareTo(other.CategoryName);
    }
}

public partial class ViewActionsVM : ObservableRecipient,
                                     IRecipient<PropertyChangedMessage<string>>,
                                     IRecipient<PropertyChangedMessage<bool>>
{
    [ObservableProperty]
    private ActionTableData filteredActionTableData;
    [ObservableProperty]
    private ActionTableData actionTableData;
    private DCSData DCSData { get; set; }
    string SortColumn { get; set; }
    bool IsSortedAscending { get; set; }

    public ViewActionsVM()
    {
        SortColumn = null;
    }

    public void Initialize(DCSData data)
    {
        DCSData = data;
        ReBuildActions();
    }

    public void Receive(PropertyChangedMessage<string> message)
    {
        if (message.Sender is FilterToolbarVM)
        {
            if (message.PropertyName == "SelectedAircraft")
            {
                ReBuildActions();
            }
            else if (message.PropertyName == "SelectedCategory")
            {
                FilterAndSortActions();
            }
        }
    }

    public void Receive(PropertyChangedMessage<bool> message)
    {
        if (message.Sender is FilterToolbarVM)
        {
            if (message.PropertyName == "WithButtons")
            {
                FilterAndSortActions();
            }
        }
    }

    public void UpdateSortColumn(string column, bool isAscending)
    {
        SortColumn = column;
        IsSortedAscending = isAscending;
        FilterAndSortActions();
    }

    private void ReBuildActions()
    {
        ActionTableData = null;
        FilteredActionTableData = null;

        ActionTableData newActionData = new();

        Dictionary<string, int> joystickHeadingIndex = new();

        DCSJoystick[] sticks = new DCSJoystick[DCSData.Joysticks.Count]; ;
        DCSData.Joysticks.Values.CopyTo(sticks, 0);
        sticks = sticks.OrderBy(x => x.Joystick.Name).ToArray();

        for (int i = 0; i < sticks.Count(); i++)
        {
            joystickHeadingIndex[sticks[i].Joystick.Name] = i;
            newActionData.JoystickHeadings.Add(sticks[i].Joystick.Name);
        }

        if (FilterToolbarVM.Default.SelectedAircraft == null || FilterToolbarVM.Default.SelectedAircraft == "All")
        {
            ActionTableData = newActionData;
            FilterAndSortActions();
            WeakReferenceMessenger.Default.Send(new BindingsDataUpdatedMessage());
            return;
        }

        List<ActionCategory> newCategories = new();

        DCSAircraftKey key = new(FilterToolbarVM.Default.SelectedAircraft);
        foreach (DCSAction action in DCSData.Aircraft[key].Actions.Values)
        {
            DCSAircraftAction dcsAircraftAction = action.Aircraft[key];
            ActionCategory category = AddCategory(newCategories, dcsAircraftAction);

            dynamic dynAction = new ExpandoObject();
            dynAction.CategoryName = category.CategoryName;
            dynAction.ActionName = dcsAircraftAction.Action;

            IDictionary<String, Object> dynActionMembers = (IDictionary<String, Object>)dynAction;
            for (int j = 0; j < joystickHeadingIndex.Count; j++)
            {
                string bindingName = "Joystick" + j.ToString();
                string actionLabel = BuildJoystickButtonLabel(action, sticks[j].Key);

                if (!String.IsNullOrWhiteSpace(actionLabel))
                {
                    dynActionMembers.TryAdd(bindingName + "Buttons", actionLabel);
                    dynActionMembers.TryAdd(bindingName + "Visible", Visibility.Visible);
                }
                else
                {
                    dynActionMembers.TryAdd(bindingName + "Buttons", null);
                    dynActionMembers.TryAdd(bindingName + "Visible", Visibility.Collapsed);
                }
            }

            newActionData.Actions.Add(dynAction);
        }
        ActionTableData = newActionData;
        FilterAndSortActions();
        WeakReferenceMessenger.Default.Send(new BindingsDataUpdatedMessage());
    }

    private ActionCategory AddCategory(List<ActionCategory> newCategories, DCSAircraftAction dcsAircraftAction)
    {
        ActionCategory category;

        string categoryName = string.IsNullOrWhiteSpace(dcsAircraftAction.Category) ? "Uknown" : dcsAircraftAction.Category;

        category = newCategories.Find(cat => cat.CategoryName == categoryName);
        if (category == null)
        {
            category = new() { CategoryName = categoryName };
            newCategories.Add(category);
        }

        return category;
    }

    private string BuildJoystickButtonLabel(DCSAction action, DCSJoystickKey joystickKey)
    {
        string buttons = "";
        string modifiers = "";
        DCSAircraftJoystickKey key = new(FilterToolbarVM.Default.SelectedAircraft, joystickKey.Id);

        if (action.AircraftJoysticks.ContainsKey(key))
        {
            foreach (DCSButton button in action.AircraftJoysticks[key].Buttons.Values)
            {
                if (button.IsModifier)
                {
                    modifiers += "[";
                    for (int i = 0; i < button.Modifiers.Count; i++)
                    {
                        modifiers += button.Modifiers[i] + (button.Modifiers.Count > 1 && i < (button.Modifiers.Count - 1) ? "," : "");
                    }
                    modifiers += "] ";
                }

                buttons = buttons + (buttons.Length > 0 ? "; " : "") + button.Name;
            }
        }

        return modifiers + buttons;
    }

    private void FilterAndSortActions()
    {
        FilteredActionTableData = new();
        FilteredActionTableData.JoystickHeadings.AddRange(ActionTableData.JoystickHeadings);

        if (FilterToolbarVM.Default.SelectedCategory == null || FilterToolbarVM.Default.SelectedAircraft == null || FilterToolbarVM.Default.SelectedAircraft == "All") return;

        foreach (dynamic dynAction in ActionTableData.Actions)
        {
            if (FilterToolbarVM.Default.SelectedCategory == "All" || dynAction.CategoryName == FilterToolbarVM.Default.SelectedCategory)
            {
                if (FilterToolbarVM.Default.WithButtons)
                {
                    IDictionary<String, Object> dynActionMembers = (IDictionary<String, Object>)dynAction;
                    for (int j = 0; j < ActionTableData.JoystickHeadings.Count; j++)
                    {
                        string bindingName = "Joystick" + j.ToString() + "Buttons";
                        if (dynActionMembers[bindingName] != null)
                        {
                            FilteredActionTableData.Actions.Add(dynAction);
                            break;
                        }
                    }
                }
                else
                {
                    FilteredActionTableData.Actions.Add(dynAction);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(SortColumn))
        {
            if (IsSortedAscending)
            {
                FilteredActionTableData.Actions.Sort((x, y) => String.Compare((string)((IDictionary<string, object>)x)[SortColumn], (string)((IDictionary<string, object>)y)[SortColumn]));
            }
            else
            {
                FilteredActionTableData.Actions.Sort((x, y) => String.Compare((string)((IDictionary<string, object>)y)[SortColumn], (string)((IDictionary<string, object>)x)[SortColumn]));
            }
        }
    }

}
