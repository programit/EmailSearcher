using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSearcher.Lucene
{
    public static class LuceneConstants
    {
        public const string LuceneStoreDirectory = @"D:\EmailSearcher\LuceneIndex";

        #region FieldNames
        public const string IsPriorityField = "IsPriority";
        public const string HasAttachmentsField = "HasAttachments";
        public const string AttachmentCountField = "AttachmentCount";
        public const string DateField = "Date";
        public const string BodyField = "Body";
        public const string ToField = "To";
        public const string CCField = "CC";
        public const string BCCField = "BCC";
        public const string FromField = "From";
        public const string SubjectField = "Subject";
        public const string UIDField = "UID";
        #endregion

    }
}