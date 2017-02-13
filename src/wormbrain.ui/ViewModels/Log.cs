using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace wormbrain.ui.ViewModels
{
    public class Log : BaseViewModel
    {

        public ObservableCollection<string> LogEntries { get; private set; }

        public Log(ObservableCollection<string> logs)
        {
            LogEntries = logs;
        }

        public Log(IEnumerable<string> logs) : this(new ObservableCollection<string>(logs)) { }

    }
}
