using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EmailSearcher.Imap;
using EmailSearcher.Lucene;

namespace EmailSearcher
{
    public class BackgroundService : IDisposable
    {
        private static readonly TimeSpan WaitBetweenNewMailChecks = TimeSpan.FromMinutes(5);
        private static readonly IMapUidStore imapUidStore = new IMapUidStore();

        private readonly IMapHandler imapHandler;
        private readonly LuceneWriter lucentHandler;

        private bool disposed = false;


        public BackgroundService()
        {
            this.imapHandler = new IMapHandler(BackgroundService.imapUidStore);
            this.lucentHandler = new LuceneWriter(BackgroundService.imapUidStore);
        }

        public async Task Start(CancellationToken token)
        {
            do
            {
                try
                {
                    IEnumerable<Tuple<uint, MailMessage>> newMessages = this.imapHandler.GetNewMessages(token);
                    this.ProcessMessages(newMessages, token);
                }
                catch (TaskCanceledException)
                {
                    Trace.TraceInformation("Task cancelled");
                    return;
                }
                catch (Exception e)
                {
                    //TODO: On forced connection closed we should reopen. Or only open when talking
                    Trace.TraceError($"Error: {e}");                    
                }

                await Task.Delay(BackgroundService.WaitBetweenNewMailChecks, token);
            } while (!token.IsCancellationRequested);
        }

        private void ProcessMessages(IEnumerable<Tuple<uint, MailMessage>> messages, CancellationToken token)
        {
            foreach(Tuple<uint, MailMessage> message in messages)
            {
                if(token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                this.lucentHandler.IndexMessage(message);
            }

            this.lucentHandler.OptimizeIndex();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.lucentHandler.Dispose();
                    this.imapHandler.Dispose();
                }

                this.disposed = true;
            }
        }
    }
}
