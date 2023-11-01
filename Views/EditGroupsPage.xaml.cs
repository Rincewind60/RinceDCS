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
    public EditGroupsPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Tuple<RinceDCSGroups, DCSData> data = e.Parameter as Tuple<RinceDCSGroups, DCSData>;

        EditGroupsViewModel vm = new(data.Item1.Groups, data.Item2);
        this.DataContext = vm;

        TabViewItem newByGroupItem = new TabViewItem();
        newByGroupItem.Header = "By Group";
        newByGroupItem.IsClosable = false;

        EditGroupControl groupCtrl = new(data.Item1.Groups);
        //ctrl.ViewModel.IsActive = true;

        newByGroupItem.Content = groupCtrl;
        EditGroupTabs.TabItems.Add(newByGroupItem);

        TabViewItem newByAircraftItem = new TabViewItem();
        newByAircraftItem.Header = "By Aircraft";
        newByAircraftItem.IsClosable = false;

        EditGroupAircraftControl aircraftCtrl = new(data.Item1.AllAircraftNames.Values.ToList(), data.Item1.AllGroups);
        //ctrl.ViewModel.IsActive = true;

        newByAircraftItem.Content = aircraftCtrl;
        EditGroupTabs.TabItems.Add(newByAircraftItem);

        //vm.IsActive = true;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        //ViewModel.IsActive = false;
    }

    public EditGroupsViewModel ViewModel => (EditGroupsViewModel)DataContext;
}
