using CommunityToolkit.Mvvm.ComponentModel;
using RinceDCS.Models;
using RinceDCS.Properties;
using RinceDCS.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public partial class FilterToolbarVM : ObservableRecipient
{
    private static readonly FilterToolbarVM defaultInstance = new();

    public static FilterToolbarVM Default
    {
        get { return defaultInstance; }
    }

    public ObservableCollection<string> Aircraft = [];
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private string selectedAircraft;

    public ObservableCollection<string> Categories = [];
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private string selectedCategory;

    public ObservableCollection<string> Groups = [];
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private string selectedGroup;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private bool showActionsWithButtons;

    public FilterToolbarVM()
    {
        IsActive = true;
    }

    public void ResetFilters(RinceDCSInstance currentInstance)
    {
        Aircraft = ["All", .. (from ac in currentInstance.Aircraft select ac.Name).Order()];
        Categories = ["All", .. (from grp in currentInstance.Groups.Groups select grp.Category).Distinct().Select(x => String.IsNullOrWhiteSpace(x) ? "None" : x).Order()];
        Groups = ["All", .. (from grp in currentInstance.Groups.Groups select grp.Name).Order()];

        SelectedAircraft = Aircraft.Where(a => a == Settings.Default.SelectedAircraft).FirstOrDefault("All");
        SelectedCategory = Categories.Where(a => a == Settings.Default.SelectedCategory).FirstOrDefault("All");
        SelectedGroup = Groups.Where(a => a == Settings.Default.SelectedGroup).FirstOrDefault("All");
        ShowActionsWithButtons = Settings.Default.ShowActionsWithButtons;
    }

    partial void OnSelectedAircraftChanged(string value)
    {
        Settings.Default.SelectedAircraft = value;
        Settings.Default.Save();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        Settings.Default.SelectedCategory = value;
        Settings.Default.Save();
    }

    partial void OnSelectedGroupChanged(string value)
    {
        Settings.Default.SelectedGroup = value;
        Settings.Default.Save();
    }

    partial void OnShowActionsWithButtonsChanged(bool value)
    {
        Settings.Default.ShowActionsWithButtons = value;
        Settings.Default.Save();
    }
}
