using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.Models;
using RinceDCS.ViewModels;
using RinceDCS.Views.Utilities;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static System.Net.Mime.MediaTypeNames;

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

        Tuple<GameBindingGroups, DCSData> data = e.Parameter as Tuple<GameBindingGroups, DCSData>;

        EditGroupsViewModel vm = new(data.Item1.Groups, data.Item2);
        this.DataContext = vm;
        //vm.IsActive = true;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        //ViewModel.IsActive = false;
    }

    public EditGroupsViewModel ViewModel => (EditGroupsViewModel)DataContext;

    public static Visibility IsBindingColumnVisible(string heading)
    {
        return string.IsNullOrEmpty(heading) ? Visibility.Collapsed : Visibility.Visible;
    }

    private void dataGrid_Sorting(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridColumnEventArgs e)
    {
        /*
        string sortColumn = e.Column.Tag.ToString();
        if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
        {
            ViewModel.UpdateSortColumn(sortColumn, true);
            e.Column.SortDirection = DataGridSortDirection.Ascending;
        }
        else
        {
            ViewModel.UpdateSortColumn(sortColumn, false);
            e.Column.SortDirection = DataGridSortDirection.Descending;
        }

        foreach (var col in dataGrid.Columns)
        {
            if (col.Tag.ToString() != sortColumn)
            {
                col.SortDirection = null;
            }
        }
        */
    }

    private void BindingGroupsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.CurrentBindingGroupChanged();

        dataGrid.Columns.Clear();

        if (!ViewModel.IsGroupSelected) return;

        dataGrid.Columns.Add(new CommunityToolkit.WinUI.UI.Controls.DataGridTextColumn()
        {
            Header = "Aircraft",
            Binding = new Binding { Path = new PropertyPath("AircraftName"), Mode = BindingMode.OneWay }
        });

        int bindingIndex = 0;
        foreach (GameBinding binding in ViewModel.CurrentBindingGroup.Bindings)
        {
            DataGridTemplateColumn column = new() { Header = binding.Id };

            string bindingName = "Binding" + bindingIndex.ToString();
            string Xaml = "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                    "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                    "<StackPanel " +
                        "Orientation=\"Horizontal\" " +
                        "Visibility=\"{Binding " + bindingName + "Visible}\">" +
                        "<CheckBox " +
                            "Margin=\"12,0,12,0\" " +
                            "Content=\"{Binding " + bindingName + ".CommandName}\" " +
                            "IsChecked=\"{Binding " + bindingName + ".IsActive, Mode=TwoWay}\">" +
                        "</CheckBox>" +
                    "</StackPanel>" +
                "</DataTemplate>";

            DataTemplate cellTemplate = XamlReader.Load(Xaml) as DataTemplate;
            column.CellTemplate = cellTemplate;

            dataGrid.Columns.Add(column);
            bindingIndex++;
        }
    }

    private void UpdateGroup_Click(object sender, RoutedEventArgs e)
    {

    }

    private void AddGroup_Click(object sender, RoutedEventArgs e)
    {

    }

    private void DeleteGroup_Click(object sender, RoutedEventArgs e)
    {

    }

    private void MergeGroup_Click(object sender, RoutedEventArgs e)
    {

    }
}
