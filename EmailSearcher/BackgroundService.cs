using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using EmailSearcher.Imap;
using EmailSearcher.Lucene;

namespace EmailSearcher
{
    public class BackgroundService : IDisposable
    {
        private static readonly TimeSpan WaitBetweenNewMailChecks = TimeSpan.FromMinutes(5);
        private static readonly IMapUidStore ImapUidStore = new IMapUidStore();

        private readonly LuceneWriter lucentHandler;

        private bool disposed = false;

        public BackgroundService()
        {
            this.lucentHandler = new LuceneWriter(BackgroundService.ImapUidStore);
        }

        public async Task Start(CancellationToken token)
        {
            do
            {
                using (IMapHandler imapHandler = new IMapHandler(BackgroundService.ImapUidStore))
                {
                    try
                    {
                        IEnumerable<Tuple<uint, MailMessage>> newMessages = imapHandler.GetNewMessages(token);
                        this.ProcessMessages(newMessages, token);
                    }
                    catch (TaskCanceledException)
                    {
                        Trace.TraceInformation("Task cancelled");
                        return;
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError($"Error: {e}");
                    }
                }

                await Task.Delay(BackgroundService.WaitBetweenNewMailChecks, token);
            } while (!token.IsCancellationRequested);
        }

        private void ProcessMessages(IEnumerable<Tuple<uint, MailMessage>> messages, CancellationToken token)
        {
            foreach (Tuple<uint, MailMessage> message in messages)
            {
                if (token.IsCancellationRequested)
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

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.lucentHandler.Dispose();
                }

                this.disposed = true;
            }
        }
    }
}
