using System.Collections.ObjectModel;

namespace EmailSearcher
{
    public class ViewModel
    {
        public ObservableCollection<DocObject> Results { get; set; }

        public ViewModel()
        {
            this.Results = new ObservableCollection<DocObject>();
        }
    }
}
