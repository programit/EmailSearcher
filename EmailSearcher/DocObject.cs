using System;
using System.Collections.Generic;

namespace EmailSearcher
{
    public sealed class DocObject
    {
        public IEnumerable<string> To { get; set; }

        public IEnumerable<string> Cc { get; set; }

        public IEnumerable<string> Bcc { get; set; }

        public string From { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public DateTime Date { get; set; }

        public string FormattedDate
        {
            get
            {
                return this.Date.Date == DateTime.Now.Date ? this.Date.ToString("t") : this.Date.ToString("d");
            }
        }

        public string FormattedBody
        {
            get
            {
                return this.Body.Replace("\n", string.Empty);
            }
        }
    }
}