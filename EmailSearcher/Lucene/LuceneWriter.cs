using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using EmailSearcher.Imap;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using S22.Imap;

namespace EmailSearcher.Lucene
{
    public class LuceneWriter : IDisposable
    {        
        private readonly IndexWriter writer;
        private readonly IMapUidStore imapUidStore;

        private bool disposed = false;

        public LuceneWriter(IMapUidStore imapUidStore)
        {
            if (imapUidStore == null)
            {
                throw new ArgumentNullException(nameof(imapUidStore));
            }

            this.imapUidStore = imapUidStore;

            this.writer = this.GetIndexWriter();
        }

        public void IndexMessage(Tuple<uint, MailMessage> message)
        {
            Document doc = this.CreateDocumentFromMailMessage(message);
            this.writer.AddDocument(doc);
            this.writer.Flush(true, true, true);            
            this.imapUidStore.AddUid(message.Item1);
        }

        public void OptimizeIndex()
        {
            this.writer.Optimize();
        }

        protected IndexWriter GetIndexWriter()
        {
            if (System.IO.Directory.Exists(LuceneConstants.LuceneStoreDirectory))
            {
                return this.OpenExistingWriter();
            }
            else
            {
                return this.CreateWriter();
            }
        }

        private Document CreateDocumentFromMailMessage(Tuple<uint, MailMessage> messageTuple)
        {
            MailMessage message = messageTuple.Item2;
            Document doc = new Document();

            if (message.Date().HasValue)
            {
                doc.Add(new NumericField(LuceneConstants.DateField, 1, Field.Store.YES, true).SetLongValue(message.Date().Value.ToFileTimeUtc()));
            }
            
            doc.Add(new Field(LuceneConstants.ToField, message.To.Count > 0 ? this.CommaSeparateIEnumerable(message.To.Select(t => t.Address)) : string.Empty, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(LuceneConstants.CCField, message.CC.Count > 0 ? this.CommaSeparateIEnumerable(message.CC.Select(t => t.Address)) : string.Empty, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(LuceneConstants.BCCField, message.Bcc.Count > 0 ? this.CommaSeparateIEnumerable(message.Bcc.Select(t => t.Address)) : string.Empty, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(LuceneConstants.FromField, message.From?.Address ?? string.Empty, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(LuceneConstants.SubjectField, message.Subject, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(LuceneConstants.BodyField, message.Body, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new NumericField(LuceneConstants.UIDField, 1, Field.Store.YES, true).SetLongValue(messageTuple.Item1));
            doc.Add(new NumericField(LuceneConstants.AttachmentCountField, 1, Field.Store.YES, true).SetIntValue(message?.Attachments.Count ?? 0));
            doc.Add(new NumericField(LuceneConstants.HasAttachmentsField, 1, Field.Store.YES, true).SetIntValue(message?.Attachments.Count ?? 0));
            doc.Add(new NumericField(LuceneConstants.IsPriorityField, 1, Field.Store.YES, true).SetIntValue((int)message.Priority));           
             
            return doc;
        }

        private string CommaSeparateIEnumerable<T>(IEnumerable<T> items)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool first = true;
            foreach (T t in items)
            {
                if (!first)
                {
                    stringBuilder.Append(",");
                }
                else
                {
                    first = false;
                }

                stringBuilder.Append(t.ToString());
            }

            return stringBuilder.ToString();
        }        

        private IndexWriter OpenExistingWriter()
        {
            Analyzer analyzer = new WhitespaceAnalyzer();
            Directory dir = FSDirectory.Open(LuceneConstants.LuceneStoreDirectory);
            return new IndexWriter(dir, analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
        }

        private IndexWriter CreateWriter()
        {
            Analyzer analyzer = new WhitespaceAnalyzer();
            Directory dir = FSDirectory.Open(LuceneConstants.LuceneStoreDirectory);
            return new IndexWriter(dir, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
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
                    this.writer.Dispose();
                }

                this.disposed = true;
            }
        }
    }
}