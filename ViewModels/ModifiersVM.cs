using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public partial class ModifierData : ObservableObject
{
    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private string device;
    [ObservableProperty]
    private string key;
    [ObservableProperty]
    private bool isSwitch;
    [ObservableProperty]
    private bool isCoreGame;
    [ObservableProperty]
    private bool isDefault;

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
        foreach(RinceDCSGroupModifier modifier in modifiers)
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
        foreach(ModifierData modifier in Modifiers)
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
