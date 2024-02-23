using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.Models;
using RinceDCS.ViewModels;
using System;
using System.Linq;
using System.Xml.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class EditGroupsPage : Page
{
    public bool IsGroupSelected { get; set; } = false;

    public EditGroupsPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Tuple<RinceDCSGroups, DCSData> data = e.Parameter as Tuple<RinceDCSGroups, DCSData>;

        TabViewItem newByGroupsItem = new TabViewItem();
        newByGroupsItem.Header = "All Groups";
        newByGroupsItem.IsClosable = false;
        EditGroupsControl groupsCtrl = new(data.Item1);
        newByGroupsItem.Content = groupsCtrl;
        EditGroupTabs.TabItems.Add(newByGroupsItem);

        TabViewItem newByGroupItem = new TabViewItem();
        newByGroupItem.Header = "By Group";
        newByGroupItem.IsClosable = false;
        EditGroupControl groupCtrl = new(data.Item1);
        newByGroupItem.Content = groupCtrl;
        EditGroupTabs.TabItems.Add(newByGroupItem);

        TabViewItem newByAircraftItem = new TabViewItem();
        newByAircraftItem.Header = "By Aircraft";
        newByAircraftItem.IsClosable = false;
        EditGroupAircraftControl aircraftCtrl = new(data.Item1.AllAircraftNames.ToList(), data.Item1.AllGroups);
        newByAircraftItem.Content = aircraftCtrl;
        EditGroupTabs.TabItems.Add(newByAircraftItem);
    }

    private void UpdateGroup_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void MergeGroup_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void DeleteGroup_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void AddGroup_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }
}
