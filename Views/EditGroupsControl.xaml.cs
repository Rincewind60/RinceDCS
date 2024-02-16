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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views;

public sealed partial class EditGroupsControl : UserControl
{
    public EditGroupsControl(RinceDCSGroups groups)
    {
        this.InitializeComponent();

        EditGroupsViewModel vm = new(groups);

        DataContext = vm;
    }

    public EditGroupsViewModel ViewModel => (EditGroupsViewModel)DataContext;

    private void dataGrid_Sorting(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridColumnEventArgs e)
    {
    }

    private void CategoriesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.CurrentCategoryChanged();

        groupsDataGrid.Columns.Clear();

        groupsDataGrid.Columns.Add(new CommunityToolkit.WinUI.UI.Controls.DataGridTextColumn()
        {
            Header = "Group",
            Binding = new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath("Name"), Mode = BindingMode.OneWay }
        });

        //int bindingIndex = 0;
        //foreach (RinceDCSGroupBinding binding in ViewModel.CurrentBindingGroup.Bindings)
        //{
        //    DataGridTemplateColumn column = new() { Header = binding.Id };

        //    string bindingName = "Binding" + bindingIndex.ToString();
        //    string Xaml = "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
        //            "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
        //            "<StackPanel " +
        //                "Orientation=\"Horizontal\" " +
        //                "Visibility=\"{Binding " + bindingName + "Visible}\">" +
        //                "<CheckBox " +
        //                    "Margin=\"12,0,12,0\" " +
        //                    "Content=\"{Binding " + bindingName + ".CommandName}\" " +
        //                    "IsChecked=\"{Binding " + bindingName + ".IsActive, Mode=TwoWay}\">" +
        //                "</CheckBox>" +
        //            "</StackPanel>" +
        //        "</DataTemplate>";

        //    DataTemplate cellTemplate = XamlReader.Load(Xaml) as DataTemplate;
        //    column.CellTemplate = cellTemplate;

        //    aircraftDataGrid.Columns.Add(column);
        //    bindingIndex++;
        //}
    }
}