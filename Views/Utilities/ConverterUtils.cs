﻿using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using RinceDCS.ViewModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Windows.UI.Text;

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

public class TrueToVisibleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class FalseToVisibleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class TrueToCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class FalseToCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class NullToFalseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class IsValidBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value == true ? null : new SolidColorBrush(Colors.LightPink);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ButtonOnLayoutConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value == true ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ButtonOnLayoutFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value == true ? FontWeights.ExtraBold : FontWeights.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class SelectedButtonBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || (bool)value == false)
        {
            return null;
        }
        else
        {
            return new SolidColorBrush(Colors.AliceBlue);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class SelectedButtonBorderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || (bool)value == false)
        {
            return new SolidColorBrush(Colors.Black);
        }
        else
        {
            return new SolidColorBrush(Colors.Blue);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class AlignmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        bool isChecked = (bool)value;

        if (isChecked)
        {
            return parameter.ToString();
        }
        else
        {
            return null;
        }
    }
}

public class ModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        bool isChecked = (bool)value;

        if (isChecked)
        {
            DetailsDisplayMode mode;
            Enum.TryParse(parameter.ToString(), true, out mode);
            return mode;
        }
        else
        {
            return null;
        }
    }
}

public class ModeVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value.ToString() == parameter.ToString() ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class AircraftVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value.ToString() == DetailsDisplayMode.Edit.ToString() ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
