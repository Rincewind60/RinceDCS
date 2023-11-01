// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using RinceDCS.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PrintPage : Page
    {
        public PrintPage(PrintDocument pd)
        {
            this.InitializeComponent();

            var printers = PrinterSettings.InstalledPrinters;
            string printer = pd.PrinterSettings.PrinterName;

            string[] printerNames = new string[printers.Count];
            printers.CopyTo(printerNames, 0);

            this.DataContext = new PrinterViewModel(printerNames.ToList(), printer);
        }

        public PrinterViewModel ViewModel => (PrinterViewModel)DataContext;
    }
}
