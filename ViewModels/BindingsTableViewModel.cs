using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.WinUI.UI;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RinceDCS.ViewModels;

public class AircraftBinding : IComparable<AircraftBinding>
{
    public string CategoryName { get; set; }
    public string CommandName { get; set; }
    public string JoystickButton0 { get; set; }
    public string JoystickButton1 { get; set; }
    public string JoystickButton2 { get; set; }
    public string JoystickButton3 { get; set; }
    public string JoystickButton4 { get; set; }
    public string JoystickButton5 { get; set; }
    public string JoystickButton6 { get; set; }
    public string JoystickButton7 { get; set; }

    public int CompareTo(AircraftBinding other)
    {
        return CommandName.CompareTo(other.CommandName);
    }
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
    [ObservableProperty]
    public ObservableCollection<AircraftBinding> filteredBindings;
    public ObservableCollection<CommandCategory> Categories { get; set; }

    [ObservableProperty]
    private CommandCategory currentCategory;
    private DCSData BindingsData { get; set; }
    private DCSAircraftKey CurrentAircraftKey { get; set; }

    [ObservableProperty]
    private string joystickHeading0;
    [ObservableProperty]
    private string joystickHeading1;
    [ObservableProperty]
    private string joystickHeading2;
    [ObservableProperty]
    private string joystickHeading3;
    [ObservableProperty]
    private string joystickHeading4;
    [ObservableProperty]
    private string joystickHeading5;
    [ObservableProperty]
    private string joystickHeading6;
    [ObservableProperty]
    private string joystickHeading7;

    [ObservableProperty]
    private bool showCommandsWithButtons;

    private List<AircraftBinding> BindingsList { get; set; }

    string SortColumn { get; set; }
    bool IsSortedAscending { get; set; }

    public BindingsTableViewModel(DCSData data, RinceDCSAircraft currentAircraft)
    {
        FilteredBindings = new();
        Categories = new();
        BindingsList = new();
        ShowCommandsWithButtons = false;
        SortColumn = null;

        BindingsData = data;
        CurrentAircraftKey = currentAircraft == null ? null : new(currentAircraft.Name);

        ReBuildBindings();
    }

    public void Receive(PropertyChangedMessage<RinceDCSAircraft> message)
    {
        CurrentAircraftKey = message.NewValue == null ? null : new(message.NewValue.Name);
        ReBuildBindings();
    }

    public void CurrentCategoryChanged()
    {
        FilterSortBindings();
    }

    [RelayCommand]
    private void CommandsWithButtonsChanged()
    {
        FilterSortBindings();
    }

    public void UpdateSortColumn(string column, bool isAscending)
    {
        SortColumn = column;
        IsSortedAscending = isAscending;
        FilterSortBindings();
    }

    private void FilterSortBindings()
    {
        if (CurrentCategory == null)
        {
            return;
        }

        List<AircraftBinding> newBindings = new();
        foreach (AircraftBinding binding in BindingsList)
        {
            if (CurrentCategory.CategoryName == "All" || binding.CategoryName == CurrentCategory.CategoryName)
            {
                if (ShowCommandsWithButtons)
                {
                    if(!string.IsNullOrWhiteSpace(binding.JoystickButton0) ||
                       !string.IsNullOrWhiteSpace(binding.JoystickButton1) ||
                       !string.IsNullOrWhiteSpace(binding.JoystickButton2) ||
                       !string.IsNullOrWhiteSpace(binding.JoystickButton3) ||
                       !string.IsNullOrWhiteSpace(binding.JoystickButton4) ||
                       !string.IsNullOrWhiteSpace(binding.JoystickButton5) ||
                       !string.IsNullOrWhiteSpace(binding.JoystickButton6) ||
                       !string.IsNullOrWhiteSpace(binding.JoystickButton7))
                    {
                        newBindings.Add(binding);
                    }
                }
                else
                {
                    newBindings.Add(binding);
                }
            }
        }

        if(string.IsNullOrWhiteSpace(SortColumn))
        {
            FilteredBindings = new(newBindings);
        }
        else
        {
            if(IsSortedAscending)
            {
                FilteredBindings = new(newBindings.OrderBy(bind => typeof(AircraftBinding).GetProperty(SortColumn).GetValue(bind)));
            }
            else
            {
                FilteredBindings = new(newBindings.OrderByDescending(bind => typeof(AircraftBinding).GetProperty(SortColumn).GetValue(bind)));
            }
        }

    }

    private void ReBuildBindings()
    {
        FilteredBindings.Clear();
        Categories.Clear();
        CurrentCategory = null;
        BindingsList.Clear();

        if (CurrentAircraftKey != null)
        {
            SetJoystickColumnHeadings();

            List<CommandCategory> newCategories = new();

            foreach(DCSBinding binding in BindingsData.Aircraft[CurrentAircraftKey].Bindings.Values)
            {
                BuildAircraftBindings(newCategories, binding);
            }

            BindingsList.Sort();

            newCategories.Sort();
            Categories.Add(new CommandCategory() { CategoryName = "All" });
            foreach (CommandCategory category in newCategories)
            {
                Categories.Add(category);
            }
            CurrentCategory = Categories[0];

            FilterSortBindings();
        }
    }

    private void SetJoystickColumnHeadings()
    {
        if(BindingsData == null) return;

        int joystickCount = BindingsData.Joysticks.Count;
        for (int i = 0; i < 8; i++)
        {
            switch (i)
            {
                case 0:
                    JoystickHeading0 = i < joystickCount ? BindingsData.Joysticks.ElementAt(i).Value.Joystick.Name : "";
                    break;
                case 1:
                    JoystickHeading1 = i < joystickCount ? BindingsData.Joysticks.ElementAt(i).Value.Joystick.Name : "";
                    break;
                case 2:
                    JoystickHeading2 = i < joystickCount ? BindingsData.Joysticks.ElementAt(i).Value.Joystick.Name : "";
                    break;
                case 3:
                    JoystickHeading3 = i < joystickCount ? BindingsData.Joysticks.ElementAt(i).Value.Joystick.Name : "";
                    break;
                case 4:
                    JoystickHeading4 = i < joystickCount ? BindingsData.Joysticks.ElementAt(i).Value.Joystick.Name : "";
                    break;
                case 5:
                    JoystickHeading5 = i < joystickCount ? BindingsData.Joysticks.ElementAt(i).Value.Joystick.Name : "";
                    break;
                case 6:
                    JoystickHeading6 = i < joystickCount ? BindingsData.Joysticks.ElementAt(i).Value.Joystick.Name : "";
                    break;
                case 7:
                    JoystickHeading7 = i < joystickCount ? BindingsData.Joysticks.ElementAt(i).Value.Joystick.Name : "";
                    break;
            }
        }
    }

    private void BuildAircraftBindings(List<CommandCategory> newCategories, DCSBinding binding)
    {
        DCSAircraftBinding dcsAircraftBinding = binding.AircraftWithBinding[CurrentAircraftKey];
        CommandCategory category = new() { CategoryName = dcsAircraftBinding.CategoryName };

        if (string.IsNullOrWhiteSpace(category.CategoryName))
        {
            category.CategoryName = "Uknown";
        }

        if(!newCategories.Any(cat => cat.CategoryName == category.CategoryName))
        {
            newCategories.Add(category); 
        }

        AircraftBinding aircraftBinding = new()
        {
            CategoryName = category.CategoryName,
            CommandName = dcsAircraftBinding.CommandName
        };
        BuildJoystickButtons(binding, aircraftBinding);

        BindingsList.Add(aircraftBinding);
    }

    private void BuildJoystickButtons(DCSBinding binding, AircraftBinding aircraftBinding)
    {
        int joystickCount = 0;
        foreach (DCSJoystick stick in binding.JoysticksWithBinding.Values)
        {
            string button = BuildJoystickButtonLabel(binding, stick.Key);
            if (!string.IsNullOrWhiteSpace(button))
            {
                switch (joystickCount)
                {
                    case 0:
                        aircraftBinding.JoystickButton0 = button;
                        break;
                    case 1:
                        aircraftBinding.JoystickButton1 = button;
                        break;
                    case 2:
                        aircraftBinding.JoystickButton2 = button;
                        break;
                    case 3:
                        aircraftBinding.JoystickButton3 = button;
                        break;
                    case 4:
                        aircraftBinding.JoystickButton4 = button;
                        break;
                    case 5:
                        aircraftBinding.JoystickButton5 = button;
                        break;
                    case 6:
                        aircraftBinding.JoystickButton6 = button;
                        break;
                    case 7:
                        aircraftBinding.JoystickButton7 = button;
                        break;
                }
            }

            joystickCount += 1;
        }
    }

    private string BuildJoystickButtonLabel(DCSBinding binding, DCSJoystickKey joystickKey)
    {
        string buttons = "";
        string modifiers = "";
        DCSAircraftJoystickKey key = new(CurrentAircraftKey.Name, joystickKey.Id);

        if (binding.AircraftJoystickBindings.ContainsKey(key))
        {
            foreach(IDCSButton button in binding.AircraftJoystickBindings[key].AssignedButtons.Values)
            {
                DCSKeyButton keyButton = button as DCSKeyButton;
                if (keyButton != null)
                {
                    foreach (string modifier in keyButton.Modifiers)
                    {
                        modifiers += modifier + " - ";
                    }
                }

                buttons = buttons + (buttons.Length > 0 ? "; " : "") + button.Key.Name;
            }
        }

        return modifiers + buttons;
    }
}
