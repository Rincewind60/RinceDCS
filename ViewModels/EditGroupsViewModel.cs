using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public partial class EditGroupsViewModel : ObservableObject
{
    public ObservableCollection<string> BindingGroups { get; set; }
    [ObservableProperty]
    private string currentBindingGroup;

    public ObservableCollection<string> Aircraft { get; set; }

    [ObservableProperty]
    private string bindingHeading0;

    public void CurrentBindingGroupChanged()
    {
    }
}
