using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammer;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using LuceneVersion = Lucene.Net.Util.Version;
namespace EmailSearcher.Lucene
{
    public class LuceneSearcher
    {
        private const int MaxSearchResults = 100;

        private readonly Analyzer analyzer;
        private readonly TextParser textParser;
        private readonly IndexSearcher searcher;

        private readonly QueryParser bodyParser;
        private readonly QueryParser toParser;
        private readonly QueryParser ccParser;
        private readonly QueryParser bccParser;
        private readonly QueryParser fromParser;
        private readonly QueryParser subjectParser;

        private static readonly string[] EmptyStringList = new string[0];
        private static readonly List<DocObject> EmptyResults = new List<DocObject>(0);
        private static readonly char[] SplitCharacters = new char[] { ',' };

        public LuceneSearcher()
        {
            this.analyzer = new WhitespaceAnalyzer();
            this.textParser = new TextParser();
            this.searcher = new IndexSearcher(FSDirectory.Open(LuceneConstants.LuceneStoreDirectory));

            this.bodyParser = new QueryParser(LuceneVersion.LUCENE_30, LuceneConstants.BodyField, this.analyzer);
            this.toParser = new QueryParser(LuceneVersion.LUCENE_30, LuceneConstants.ToField, this.analyzer);
            this.ccParser = new QueryParser(LuceneVersion.LUCENE_30, LuceneConstants.ToField, this.analyzer);
            this.bccParser = new QueryParser(LuceneVersion.LUCENE_30, LuceneConstants.ToField, this.analyzer);
            this.fromParser = new QueryParser(LuceneVersion.LUCENE_30, LuceneConstants.FromField, this.analyzer);
            this.subjectParser = new QueryParser(LuceneVersion.LUCENE_30, LuceneConstants.SubjectField, this.analyzer);
        }

        public List<DocObject> Search(ICollection<Tuple<FieldType, string>> expressions)
        {            
            List<HashSet<int>> docList = new List<HashSet<int>>();
            foreach(Tuple<FieldType, string> match in expressions)
            {
                switch(match.Item1)
                {
                    case FieldType.To:
                        docList.Add(new HashSet<int>(this.QueryString(this.toParser, match.Item2).Select(v => v.Doc)));
                        break;
                    case FieldType.From:
                        docList.Add(new HashSet<int>(this.QueryString(this.fromParser, match.Item2).Select(v => v.Doc)));
                        break;
                    case FieldType.Subject:
                        docList.Add(new HashSet<int>(this.QueryString(this.subjectParser, match.Item2).Select(v => v.Doc)));
                        break;
                }
            }

            if(docList.Count == 0)
            {
                return LuceneSearcher.EmptyResults;
            }

            List<int> commonDocs = this.UnionDocs(docList);
            List<DocObject> results = this.ConvertDocIdsToObjects(commonDocs);
            return results;
        } 

        private List<DocObject> ConvertDocIdsToObjects(List<int> docs)
        {
            List<DocObject> objects = new List<DocObject>(docs.Count);
            foreach(int docId in docs)
            {
                Document doc = this.searcher.Doc(docId);
                if(doc == null)
                {
                    continue;
                }

                IEnumerable<string> bcc = string.IsNullOrEmpty(doc.GetField(LuceneConstants.BCCField).StringValue)
                        ? LuceneSearcher.EmptyStringList
                        : doc.GetField(LuceneConstants.BCCField).StringValue.Split(LuceneSearcher.SplitCharacters, StringSplitOptions.RemoveEmptyEntries);

                IEnumerable<string> to = string.IsNullOrEmpty(doc.GetField(LuceneConstants.ToField).StringValue)
                        ? LuceneSearcher.EmptyStringList
                        : doc.GetField(LuceneConstants.ToField).StringValue.Split(LuceneSearcher.SplitCharacters, StringSplitOptions.RemoveEmptyEntries);

                IEnumerable<string> cc = string.IsNullOrEmpty(doc.GetField(LuceneConstants.CCField).StringValue)
                        ? LuceneSearcher.EmptyStringList
                        : doc.GetField(LuceneConstants.CCField).StringValue.Split(LuceneSearcher.SplitCharacters, StringSplitOptions.RemoveEmptyEntries);

                string from = doc.GetField(LuceneConstants.FromField).StringValue;
                string subject = doc.GetField(LuceneConstants.SubjectField).StringValue;
                string body = doc.GetField(LuceneConstants.BodyField).StringValue;
                DateTime date = doc.GetField(LuceneConstants.DateField) == null
                    ? DateTime.MinValue
                    : DateTime.FromFileTime(long.Parse(doc.GetField(LuceneConstants.DateField).StringValue));

                DocObject obj = new DocObject()
                {
                    Bcc = bcc,
                    To = to,
                    Cc = cc,
                    From = from,
                    Subject = subject,
                    Body = body,
                    Date = date,
                };

                objects.Add(obj);
            }

            return objects;
        }

        private List<int> UnionDocs(List<HashSet<int>> docList)
        {
            List<int> commonDocs = new List<int>();
            HashSet<int> firstDoc = docList[0];
            foreach (int doc in firstDoc)
            {
                bool notFound = false;
                foreach (HashSet<int> docs in docList)
                {
                    if (docs == firstDoc)
                    {
                        continue;
                    }

                    if (!docs.Contains(doc))
                    {
                        notFound = true;
                        break;
                    }
                }

                if (notFound)
                {
                    continue;
                }

                commonDocs.Add(doc);
            }

            return commonDocs;
        }
        
        private void QueryNumericRange(Tuple<FieldType, string> match)
        {

        }

        private ScoreDoc[] QueryString(QueryParser parser, string queryText)
        {
            Query query = parser.Parse(queryText);
            TopDocs hits = this.searcher.Search(query, LuceneSearcher.MaxSearchResults);
            return hits.ScoreDocs;
        }
    }
}