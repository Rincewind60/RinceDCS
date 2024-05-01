using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RinceDCS.ViewModels;

public partial class PrinterVM : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string> printers;

    [ObservableProperty]
    private string printer;

    public PrinterVM(List<string> printers, string printer)
    {
        Printers = new(printers);
        Printer = (from pr in Printers
                  where pr == printer
                  select pr).First();
    }
}
