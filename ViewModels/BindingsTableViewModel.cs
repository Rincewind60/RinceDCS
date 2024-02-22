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
public class CommandButtonsTableData
{
    public List<string> JoystickHeadings { get; set; } = new();
    public List<dynamic> Commands { get; set; } = new();
}

public partial class CommandCategory : ObservableObject, IComparable<CommandCategory>
{
    [ObservableProperty]
    private string categoryName;

    public int CompareTo(CommandCategory other)
    {
        return CategoryName.CompareTo(other.CategoryName);
    }
}

public partial class BindingsTableViewModel : ObservableRecipient,
                                              IRecipient<PropertyChangedMessage<RinceDCSAircraft>>
{
    public ObservableCollection<CommandCategory> Categories { get; set; }
    [ObservableProperty]
    private CommandCategory currentCategory;
    [ObservableProperty]
    private bool showCommandsWithButtons;
    [ObservableProperty]
    private CommandButtonsTableData filteredCommandData;
    [ObservableProperty]
    private CommandButtonsTableData commandData;

    private DCSAircraftKey CurrentAircraftKey { get; set; }
    private DCSData BindingsData { get; set; }

    string SortColumn { get; set; }
    bool IsSortedAscending { get; set; }

    public BindingsTableViewModel()
    {
        Categories = new();
        ShowCommandsWithButtons = false;
        SortColumn = null;
    }

    public void Initialize(DCSData data, RinceDCSAircraft currentAircraft)
    {
        BindingsData = data;
        CurrentAircraftKey = currentAircraft == null ? null : new(currentAircraft.Name);
        ReBuildCommands();
    }

    public void Receive(PropertyChangedMessage<RinceDCSAircraft> message)
    {
        CurrentAircraftKey = message.NewValue == null ? null : new(message.NewValue.Name);
        ReBuildCommands();
    }

    public void CurrentCategoryChanged()
    {
        FilterSortCommands();
    }

    [RelayCommand]
    private void CommandsWithButtonsChanged()
    {
        FilterSortCommands();
    }

    public void UpdateSortColumn(string column, bool isAscending)
    {
        SortColumn = column;
        IsSortedAscending = isAscending;
        FilterSortCommands();
    }

    private void FilterSortCommands()
    {
        if (CurrentCategory == null) return;

        FilteredCommandData = new();

        FilteredCommandData.JoystickHeadings.AddRange(CommandData.JoystickHeadings);

        foreach(dynamic dynCommand in CommandData.Commands)
        {
            if (CurrentCategory.CategoryName == "All" || dynCommand.CategoryName == CurrentCategory.CategoryName)
            {
                if (ShowCommandsWithButtons)
                {
                    IDictionary<String, Object> dynCommandMembers = (IDictionary<String, Object>)dynCommand;
                    for (int j = 0; j < CommandData.JoystickHeadings.Count; j++)
                    {
                        string bindingName = "Joystick" + j.ToString() + "Buttons";
                        if (dynCommandMembers[bindingName] != null)
                        {
                            FilteredCommandData.Commands.Add(dynCommand);
                            break;
                        }
                    }
                }
                else
                {
                    FilteredCommandData.Commands.Add(dynCommand);
                }
            }
        }

        if(!string.IsNullOrWhiteSpace(SortColumn))
        {
            if(IsSortedAscending)
            {
                FilteredCommandData.Commands.Sort((x, y) => String.Compare((string)((IDictionary<string, object>)x)[SortColumn], (string)((IDictionary<string, object>)y)[SortColumn]));
            }
            else
            {
                FilteredCommandData.Commands.Sort((x, y) => String.Compare((string)((IDictionary<string, object>)y)[SortColumn], (string)((IDictionary<string, object>)x)[SortColumn]));
            }
        }
    }

    private void ReBuildCommands()
    {
        CommandData = null;
        FilteredCommandData = null;
        Categories.Clear();
        CurrentCategory = null;

        if (CurrentAircraftKey == null) return;

        CommandButtonsTableData newCommandData = new();

        Dictionary<string, int> joystickHeadingIndex = new();

        DCSJoystick[] sticks = new DCSJoystick[BindingsData.Joysticks.Count]; ;
        BindingsData.Joysticks.Values.CopyTo(sticks,0);
        sticks = sticks.OrderBy(x => x.Joystick.Name).ToArray();

        for (int i = 0; i < sticks.Count(); i++)
        {
            joystickHeadingIndex[sticks[i].Joystick.Name] = i;
            newCommandData.JoystickHeadings.Add(sticks[i].Joystick.Name);
        }

        List<CommandCategory> newCategories = new();

        foreach(DCSBinding binding in BindingsData.Aircraft[CurrentAircraftKey].Bindings.Values)
        {
            DCSAircraftBinding dcsAircraftBinding = binding.Aircraft[CurrentAircraftKey];
            CommandCategory category = AddCategory(newCategories, dcsAircraftBinding);

            dynamic dynCommand = new ExpandoObject();
            dynCommand.CategoryName = category.CategoryName;
            dynCommand.CommandName = dcsAircraftBinding.Command;

            IDictionary<String, Object> dynCommandMembers = (IDictionary<String, Object>)dynCommand;
            for (int j = 0; j < joystickHeadingIndex.Count; j++)
            {
                string bindingName = "Joystick" + j.ToString();
                string buttonLabel = BuildJoystickButtonLabel(binding, sticks[j].Key);

                if (!String.IsNullOrWhiteSpace(buttonLabel))
                {
                    dynCommandMembers.TryAdd(bindingName + "Buttons", buttonLabel);
                    dynCommandMembers.TryAdd(bindingName + "Visible", Visibility.Visible);
                }
                else
                {
                    dynCommandMembers.TryAdd(bindingName + "Buttons", null);
                    dynCommandMembers.TryAdd(bindingName + "Visible", Visibility.Collapsed);
                }
            }

            newCommandData.Commands.Add(dynCommand);
        }
        CommandData = newCommandData;

        newCategories.Sort();
        Categories.Add(new CommandCategory() { CategoryName = "All" });
        foreach (CommandCategory category in newCategories)
        {
            Categories.Add(category);
        }
        CurrentCategory = Categories[0];

        WeakReferenceMessenger.Default.Send(new BindingsDataUpdatedMessage());

        FilterSortCommands();
    }

    private CommandCategory AddCategory(List<CommandCategory> newCategories, DCSAircraftBinding dcsAircraftBinding)
    {
        CommandCategory category;

        string categoryName = string.IsNullOrWhiteSpace(dcsAircraftBinding.Category) ? "Uknown" : dcsAircraftBinding.Category;

        category = newCategories.Find(cat => cat.CategoryName == categoryName);
        if(category == null)
        {
            category = new() { CategoryName = categoryName };
            newCategories.Add(category);
        }

        return category;
    }

    private string BuildJoystickButtonLabel(DCSBinding binding, DCSJoystickKey joystickKey)
    {
        string buttons = "";
        string modifiers = "";
        DCSAircraftJoystickKey key = new(CurrentAircraftKey.Name, joystickKey.Id);

        if (binding.AircraftJoysticks.ContainsKey(key))
        {
            foreach(DCSButton button in binding.AircraftJoysticks[key].Buttons.Values)
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
