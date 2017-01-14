using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EmailSearcher.Imap;
using S22.Imap;

namespace EmailSearcher.Imap
{
    public class IMapHandler : IDisposable
    {
        private const int IMapPort = 993;
        private const string IMapHost = "imap-mail.outlook.com";        

        private readonly ImapClient imapClient;
        private readonly IMapUidStore imapUidStore;

        private bool disposed = false;

        public IMapHandler(IMapUidStore imapUidStore)
        {
            if(imapUidStore == null)
            {
                throw new ArgumentNullException(nameof(imapUidStore));
            }

            this.imapUidStore = imapUidStore;

            this.imapClient = this.CreateIMapClient();
        }        

        public IEnumerable<Tuple<uint, MailMessage>> GetNewMessages(CancellationToken token)
        {
            ICollection<uint> newUids = this.GetNewMessageIds();
            foreach(uint uid in newUids)
            {
                if(token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                MailMessage message = imapClient.GetMessage(uid);
                yield return new Tuple<uint, MailMessage>(uid, message);
            }
        } 

        private ICollection<uint> GetNewMessageIds()
        {
            IEnumerable<uint> uids = imapClient.Search(SearchCondition.All());
            List<uint> newIds = new List<uint>();
            foreach(uint uid in uids)
            {
                if(!imapUidStore.IsUidStored(uid))
                {
                    newIds.Add(uid);
                }
            }

            return newIds;
        }

        private ImapClient CreateIMapClient()
        {
            return new ImapClient(IMapHandler.IMapHost, IMapHandler.IMapPort, ImapCredentials.Username, ImapCredentials.Password, AuthMethod.Auto, true);
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
                    this.imapClient.Dispose();    
                }

                this.disposed = true;
            }
        }
    }
}