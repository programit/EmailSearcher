using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmailSearcher
{
    public class TextParser
    {        
        //private const string CommonRegex = "\\s?=\\s?((\".*?\")|(.*?\\s))";

        //private Regex ToRegex = new Regex($"To{TextParser.CommonRegex}", RegexOptions.IgnoreCase);
        //private Regex FromRegex = new Regex($"From{TextParser.CommonRegex}", RegexOptions.IgnoreCase);
        //private Regex SubjectRegex = new Regex($"Subject{TextParser.CommonRegex}", RegexOptions.IgnoreCase);

        //public ICollection<Tuple<FieldType, string>> Parse(string text)
        //{
        //    List<Tuple<FieldType, string>> matches = new List<Tuple<FieldType, string>>();

        //    this.Match(text, this.ToRegex, FieldType.To, matches);
        //    this.Match(text, this.FromRegex, FieldType.From, matches);
        //    this.Match(text, this.SubjectRegex, FieldType.Subject, matches);

        //    return matches;
        //}

        //private void Match(string text, Regex regex, FieldType field, List<Tuple<FieldType, string>> matches)
        //{
        //    string matchResult;
        //    if (this.TryMatch(regex, text, out matchResult))
        //    {
        //        matches.Add(new Tuple<FieldType, string>(field, matchResult));
        //    }
        //}

        //private bool TryMatch(Regex regex, string text, out string matchResult)
        //{
        //    Match match = regex.Match(text);
        //    if(match.Success)
        //    {
        //        matchResult = match.Groups[1].Value.Replace("\"", "");
        //        return true;
        //    }

        //    matchResult = string.Empty;
        //    return false;
        //} 
    }
}