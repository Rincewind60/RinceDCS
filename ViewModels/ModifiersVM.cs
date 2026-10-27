// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RinceDCS.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RinceDCS.ViewModels;

public partial class ModifierData : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }
    [ObservableProperty]
    public partial string Device { get; set; }
    [ObservableProperty]
    public partial string Key { get; set; } 
    [ObservableProperty]
    public partial bool IsSwitch { get; set; }
    [ObservableProperty]
    public partial bool IsCoreGame { get; set; }
    [ObservableProperty]
    public partial bool IsDefault { get; set; }

    [RelayCommand]
    public void BindToButton()
    {

    }

    [RelayCommand]
    public void Delete()
    {

    }
}

public partial class ModifiersVM : ObservableObject
{
    public ObservableCollection<ModifierData> Modifiers = new();

    public ModifiersVM(List<RinceDCSGroupModifier> modifiers, string defaultModifierName)
    {
        foreach (RinceDCSGroupModifier modifier in modifiers)
        {
            Modifiers.Add(new()
            {
                Name = modifier.Name,
                Device = modifier.Device,
                Key = modifier.Key,
                IsSwitch = modifier.Switch,
                IsCoreGame = modifier.IsCoreGame,
                IsDefault = modifier.Name == defaultModifierName
            });
        }
    }

    public void AddModifier()
    {
        Modifiers.Add(new ModifierData());
    }

    public List<RinceDCSGroupModifier> GetUpdatedModifiers()
    {
        List<RinceDCSGroupModifier> newModifiers = new();
        foreach (ModifierData modifier in Modifiers)
        {
            newModifiers.Add(new()
            {
                Name = modifier.Name,
                Device = modifier.Device,
                Key = modifier.Key,
                Switch = modifier.IsSwitch,
                IsCoreGame = modifier.IsCoreGame
            });
        }
        return newModifiers;
    }

    public string GetUpdatedDefaultModifierName()
    {
        return Modifiers.Where(m => m.IsDefault).FirstOrDefault(Modifiers.Last()).Name;
    }
}
