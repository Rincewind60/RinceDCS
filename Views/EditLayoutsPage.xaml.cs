// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.Models;
using RinceDCS.ViewModels.Messages;
using RinceDCS.Views.Utilities;
using SharpDX.DirectInput;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditLayoutsPage : Page
    {
        public EditLayoutsPage()
        {
            this.InitializeComponent();

            WeakReferenceMessenger.Default.Register<AddNewJoystickImageMessage>(this, (r, m) =>
            {
                AddNewStickImageTab(m.Stick, m.StickImage);
            });
            WeakReferenceMessenger.Default.Register<DeleteJoystickImageMessage>(this, (r, m) =>
            {
                DeleteStickImageTab(m.Stick, m.StickImage);
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            RinceDCSFile rinceDCSFile = (RinceDCSFile)e.Parameter;

            foreach (RinceDCSJoystick stick in rinceDCSFile.Joysticks)
            {
                foreach (RinceDCSJoystickImage stickImage in stick.Images)
                {
                    AddNewStickImageTab(stick, stickImage);
                }
            }
        }

        private void AddNewStickImageTab(RinceDCSJoystick stick, RinceDCSJoystickImage stickImage)
        {
            TabViewItem newItem = new TabViewItem();
            Binding titleBinding = new Binding
            {
                Source = stickImage,
                Path = new PropertyPath("Title"),
                Mode = BindingMode.OneWay
            };
            newItem.SetBinding(TabViewItem.HeaderProperty, titleBinding);
            newItem.IsClosable = false;
            EditLayoutsControl ctrl = new(stick, stickImage);
            newItem.Content = ctrl;
            EditJoystickLayouts.TabItems.Add(newItem);
        }

        private void DeleteStickImageTab(RinceDCSJoystick stick, RinceDCSJoystickImage stickImage)
        {
            foreach (TabViewItem tab in EditJoystickLayouts.TabItems)
            {
                EditLayoutsControl ctrl = tab.Content as EditLayoutsControl;
                if (ctrl.ViewModel.StickImage == stickImage)
                {
                    EditJoystickLayouts.TabItems.Remove(tab);
                    return;
                }
            }
        }
    }
}
