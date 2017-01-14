using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EmailSearcher.Lucene;
using Grammer;
using InputGrammar;
using Irony.Parsing;

namespace EmailSearcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundService backgroundService = new BackgroundService();
        private readonly LuceneSearcher searcher = new LuceneSearcher();
        private readonly CancellationTokenSource token = new CancellationTokenSource();
        private readonly InputGrammer GrammerParser;
        private readonly UseGrammar GrammerExpressionGenerator;
        private readonly Parser parser;

        public MainWindow()
        {
            this.GrammerParser = new InputGrammer();
            this.GrammerExpressionGenerator = new UseGrammar();
            this.parser = new Parser(this.GrammerParser);

            InitializeComponent();
            Task.Factory.StartNew(() => backgroundService.Start(token.Token));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            token.Cancel();
            this.backgroundService.Dispose();
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                ViewModel model = this.MainGrid.DataContext as ViewModel;
                model.Results.Clear();

                string text = this.SearchBox.Text;
                this.SearchBox.Text = string.Empty;
                
                ParseTree parseTree = parser.Parse(text);
                List<Tuple<FieldType, string>> parsedExpressions = this.GrammerExpressionGenerator.Parse(parseTree.Root);                
                foreach(DocObject obj in searcher.Search(parsedExpressions))
                {
                    model.Results.Add(obj);
                }                
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DocObject doc = this.listView.SelectedItem as DocObject;
            this.browser.NavigateToString(doc.Body);
        }
    }
}
