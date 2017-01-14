using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSearcher
{
    public class ViewModel
    {
        public ObservableCollection<DocObject> Results { get; set; }

        public ViewModel()
        {
            Results = new ObservableCollection<DocObject>();
        }
    }
}
