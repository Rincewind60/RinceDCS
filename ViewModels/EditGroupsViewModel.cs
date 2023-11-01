using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public partial class EditGroupsViewModel : ObservableObject
{
    public EditGroupsViewModel(List<RinceDCSGroup> groups, DCSData data)
    {
    }
}
