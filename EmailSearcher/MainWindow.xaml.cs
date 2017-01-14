using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EmailSearcher.Lucene;
using Grammer;
using InputGrammar;
using Irony.Parsing;

namespace EmailSearcher
{
    public partial class MainWindow : Window, IDisposable
    {
        private readonly BackgroundService backgroundService = new BackgroundService();
        private readonly LuceneSearcher searcher = new LuceneSearcher();
        private readonly CancellationTokenSource token = new CancellationTokenSource();
        private readonly InputGrammer grammerParser;
        private readonly UseGrammar grammerExpressionGenerator;
        private readonly Parser parser;

        private bool disposed = false;

        public MainWindow()
        {
            this.grammerParser = new InputGrammer();
            this.grammerExpressionGenerator = new UseGrammar();
            this.parser = new Parser(this.grammerParser);

            this.InitializeComponent();
            Task.Factory.StartNew(() => this.backgroundService.Start(this.token.Token));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.token.Cancel();
            this.backgroundService.Dispose();
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel model = this.MainGrid.DataContext as ViewModel;
                model.Results.Clear();

                string text = this.SearchBox.Text;
                this.SearchBox.Text = string.Empty;
                
                ParseTree parseTree = this.parser.Parse(text);
                List<Tuple<FieldType, string>> parsedExpressions = this.grammerExpressionGenerator.Parse(parseTree.Root);                
                foreach (DocObject obj in this.searcher.Search(parsedExpressions))
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

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.backgroundService.Dispose();
                    this.token.Dispose();
                    this.searcher.Dispose();
                }

                this.disposed = true;
            }
        }
    }
}