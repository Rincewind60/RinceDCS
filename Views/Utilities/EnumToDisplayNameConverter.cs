using Microsoft.UI.Xaml.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace RinceDCS.Views.Utilities;

/// <summary>
/// Use in xaml to replace enum value with its display name.
/// 
/// Example
/// 
/// <Page
///    x:Class="EnumDisplayNameExample.MainPage"
///    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
///    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
///    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
///    xmlns:local="using:EnumDisplayNameExample"
///    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
///    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}"
///    mc:Ignorable="d">
///    <Page.Resources>
///        <local:ViewModel x:Name="ViewModel" />
///        <local:EnumToDisplayNameConverter x:Key="EnumToDisplayNameConverter" />
///    </Page.Resources>
///
///    <StackPanel>
///        <ComboBox
///            Width = "160"
///            ItemsSource="{x:Bind ViewModel.ViewsMode}"
///            SelectedItem="{x:Bind ViewModel.ViewMode, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}">
///            <ComboBox.ItemTemplate>
///                <DataTemplate>
///                    <TextBlock Text = "{Binding Converter={StaticResource EnumToDisplayNameConverter}}" />
///                </ DataTemplate >
///            </ ComboBox.ItemTemplate >
///        </ ComboBox >
///        < TextBlock Text="{x:Bind ViewModel.ViewMode, Mode=OneWay, Converter={StaticResource EnumToDisplayNameConverter}}" />
///    </StackPanel>
///
/// </Page>
/// 
/// </summary>
public class EnumToDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is Enum enumValue &&
            enumValue.GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()
                    ?.GetCustomAttribute<DisplayAttribute>()
                    ?.GetName() is string displayName
                        ? displayName
                        : $"Unknow value: {value}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}