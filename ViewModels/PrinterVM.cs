// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RinceDCS.ViewModels;

public partial class PrinterVM : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<string> Printers { get; set; }

    [ObservableProperty]
    public partial string Printer { get; set; }

    public PrinterVM(List<string> printers, string printer)
    {
        Printers = new(printers);
        Printer = (from pr in Printers
                   where pr == printer
                   select pr).First();
    }
}
