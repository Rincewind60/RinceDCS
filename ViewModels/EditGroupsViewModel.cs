using CommunityToolkit.Mvvm.ComponentModel;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public class EditBinding
{

}

public class EditAircraftBindings
{
    public string Aircraft { get; set; }
    public string[] Bindings { get; set; }
}

public partial class EditGroupsViewModel : ObservableObject
{
    [ObservableProperty]
    public GameBindingGroups bindingGroups;
    [ObservableProperty]
    private GameBindingGroup currentBindingGroup;

    public ObservableCollection<EditAircraftBindings> Bindings { get; set; } = new();

    public ObservableCollection<string> Aircraft { get; set; }

    [ObservableProperty]
    private string bindingHeading0;

    public EditGroupsViewModel(GameBindingGroups groups, DCSData data)
    {
        bindingGroups = groups;

        Bindings.Add(new EditAircraftBindings() { Aircraft = "Craft 1", Bindings = new string[] { "121212", "212121", "32343" } });
        Bindings.Add(new EditAircraftBindings() { Aircraft = "Craft 2", Bindings = new string[] { "121212", "", "32343" } });
        Bindings.Add(new EditAircraftBindings() { Aircraft = "Craft 3", Bindings = new string[] { "121212", "212121", "" } });
    }

    public void CurrentBindingGroupChanged()
    {
    }
}
