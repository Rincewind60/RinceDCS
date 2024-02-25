using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using RinceDCS.Models;
using RinceDCS.ViewModels.Messages;
using SharpDX.DirectInput;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

public partial class ViewActionsViewModel : ObservableRecipient,
                                            IRecipient<PropertyChangedMessage<RinceDCSAircraft>>
{
    public ObservableCollection<ActionCategory> Categories { get; set; }
    [ObservableProperty]
    private ActionCategory currentCategory;
    [ObservableProperty]
    private bool showActionsWithButtons;
    [ObservableProperty]
    private ActionTableData filteredActionData;
    [ObservableProperty]
    private ActionTableData actionData;

    private DCSAircraftKey CurrentAircraftKey { get; set; }
    private DCSData ActionsData { get; set; }

    string SortColumn { get; set; }
    bool IsSortedAscending { get; set; }

    public ViewActionsViewModel()
    {
        Categories = new();
        ShowActionsWithButtons = false;
        SortColumn = null;
    }

    public void Initialize(DCSData data, RinceDCSAircraft currentAircraft)
    {
        ActionsData = data;
        CurrentAircraftKey = currentAircraft == null ? null : new(currentAircraft.Name);
        ReBuildActions();
    }

    public void Receive(PropertyChangedMessage<RinceDCSAircraft> message)
    {
        CurrentAircraftKey = message.NewValue == null ? null : new(message.NewValue.Name);
        ReBuildActions();
    }

    public void CurrentCategoryChanged()
    {
        FilterSortActions();
    }

    [RelayCommand]
    private void ActionsWithButtonsChanged()
    {
        FilterSortActions();
    }

    public void UpdateSortColumn(string column, bool isAscending)
    {
        SortColumn = column;
        IsSortedAscending = isAscending;
        FilterSortActions();
    }

    private void FilterSortActions()
    {
        if (CurrentCategory == null) return;

        FilteredActionData = new();

        FilteredActionData.JoystickHeadings.AddRange(ActionData.JoystickHeadings);

        foreach(dynamic dynAction in ActionData.Actions)
        {
            if (CurrentCategory.CategoryName == "All" || dynAction.CategoryName == CurrentCategory.CategoryName)
            {
                if (ShowActionsWithButtons)
                {
                    IDictionary<String, Object> dynActionMembers = (IDictionary<String, Object>)dynAction;
                    for (int j = 0; j < ActionData.JoystickHeadings.Count; j++)
                    {
                        string bindingName = "Joystick" + j.ToString() + "Buttons";
                        if (dynActionMembers[bindingName] != null)
                        {
                            FilteredActionData.Actions.Add(dynAction);
                            break;
                        }
                    }
                }
                else
                {
                    FilteredActionData.Actions.Add(dynAction);
                }
            }
        }

        if(!string.IsNullOrWhiteSpace(SortColumn))
        {
            if(IsSortedAscending)
            {
                FilteredActionData.Actions.Sort((x, y) => String.Compare((string)((IDictionary<string, object>)x)[SortColumn], (string)((IDictionary<string, object>)y)[SortColumn]));
            }
            else
            {
                FilteredActionData.Actions.Sort((x, y) => String.Compare((string)((IDictionary<string, object>)y)[SortColumn], (string)((IDictionary<string, object>)x)[SortColumn]));
            }
        }
    }

    private void ReBuildActions()
    {
        ActionData = null;
        FilteredActionData = null;
        Categories.Clear();
        CurrentCategory = null;

        if (CurrentAircraftKey == null) return;

        ActionTableData newActionData = new();

        Dictionary<string, int> joystickHeadingIndex = new();

        DCSJoystick[] sticks = new DCSJoystick[ActionsData.Joysticks.Count]; ;
        ActionsData.Joysticks.Values.CopyTo(sticks,0);
        sticks = sticks.OrderBy(x => x.Joystick.Name).ToArray();

        for (int i = 0; i < sticks.Count(); i++)
        {
            joystickHeadingIndex[sticks[i].Joystick.Name] = i;
            newActionData.JoystickHeadings.Add(sticks[i].Joystick.Name);
        }

        List<ActionCategory> newCategories = new();

        foreach(DCSAction action in ActionsData.Aircraft[CurrentAircraftKey].Actions.Values)
        {
            DCSAircraftAction dcsAircraftAction = action.Aircraft[CurrentAircraftKey];
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
        ActionData = newActionData;

        newCategories.Sort();
        Categories.Add(new ActionCategory() { CategoryName = "All" });
        foreach (ActionCategory category in newCategories)
        {
            Categories.Add(category);
        }
        CurrentCategory = Categories[0];

        WeakReferenceMessenger.Default.Send(new BindingsDataUpdatedMessage());

        FilterSortActions();
    }

    private ActionCategory AddCategory(List<ActionCategory> newCategories, DCSAircraftAction dcsAircraftAction)
    {
        ActionCategory category;

        string categoryName = string.IsNullOrWhiteSpace(dcsAircraftAction.Category) ? "Uknown" : dcsAircraftAction.Category;

        category = newCategories.Find(cat => cat.CategoryName == categoryName);
        if(category == null)
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
        DCSAircraftJoystickKey key = new(CurrentAircraftKey.Name, joystickKey.Id);

        if (action.AircraftJoysticks.ContainsKey(key))
        {
            foreach(DCSButton button in action.AircraftJoysticks[key].Buttons.Values)
            {
                if(button.IsModifier)
                {
                    modifiers += "[";
                    for(int i = 0; i < button.Modifiers.Count; i++)
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
}
