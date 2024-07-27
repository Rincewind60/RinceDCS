// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using RinceDCS.Models;
using RinceDCS.ViewModels;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views;

public sealed partial class EditGroupsControl : UserControl
{
    public EditGroupsControl(RinceDCSGroups groups, List<AttachedJoystick> sticks)
    {
        this.InitializeComponent();

        EditGroupsVM vm = new(groups, sticks);

        DataContext = vm;

        UpdateDataGrid();
    }

    public EditGroupsVM ViewModel => (EditGroupsVM)DataContext;

    private void dataGrid_Sorting(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridColumnEventArgs e)
    {
    }

    private void UpdateDataGrid()
    {
        ViewModel.ReBuildData();

        groupsDataGrid.Columns.Clear();
        groupsDataGrid.Columns.Add(new CommunityToolkit.WinUI.UI.Controls.DataGridTextColumn()
        {
            Header = "Group",
            Binding = new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath("Name"), Mode = BindingMode.OneWay }
        });

        int bindingIndex = 0;
        foreach (string stickName in ViewModel.GroupsData.Headings)
        {
            DataGridTemplateColumn column = new() { Header = stickName };

            string bindingName = "Stick" + bindingIndex.ToString();
            string Xaml = "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                    "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                    "<StackPanel " +
                        "Orientation=\"Horizontal\" " +
                        "Visibility=\"{Binding " + bindingName + "Visible}\">" +
                        "<TextBlock " +
                            "Margin=\"12,0,12,0\" " +
                            "Text=\"{Binding " + bindingName + "Buttons}\">" +
                        "</TextBlock>" +
                    "</StackPanel>" +
                "</DataTemplate>";

            DataTemplate cellTemplate = XamlReader.Load(Xaml) as DataTemplate;
            column.CellTemplate = cellTemplate;

            groupsDataGrid.Columns.Add(column);
            bindingIndex++;
        }
    }
}
