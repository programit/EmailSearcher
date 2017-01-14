using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;

namespace InputGrammar
{
    public class InputGrammer : Grammar
    {
        public InputGrammer() : base(false)
        {
            NonTerminal expr = new NonTerminal("expr");
            NonTerminal singleExpr = new NonTerminal("singleExpr");
            NonTerminal multipleExpr = new NonTerminal("multipleExpr");
            NonTerminal andOrWithExpr = new NonTerminal("andOrWithExpr");
            NonTerminal combo = new NonTerminal("combo");            
            NonTerminal andOr = new NonTerminal("andOr");
            NonTerminal rightSide = new NonTerminal("rightSide");
            NonTerminal rightSideWithQuotes = new NonTerminal("rightSideWithQuotes");
            NonTerminal rightSideWithoutQuotes = new NonTerminal("rightSideWithoutQuotes");
            NonTerminal multipleWords = new NonTerminal("multipleWords");
            NonTerminal literalChars = new NonTerminal("literalChars");
            NonTerminal knownFields = new NonTerminal("knownFields");

            IdentifierTerminal field = new IdentifierTerminal("field");
            IdentifierTerminal literal = new IdentifierTerminal("literal");
            IdentifierTerminal andOrLiteral = new IdentifierTerminal("andOrLiteral");

            expr.Rule = singleExpr | singleExpr + multipleExpr;
            singleExpr.Rule = knownFields + "=" + rightSide;
            multipleExpr.Rule = andOr + singleExpr | andOr + singleExpr + multipleExpr;

            knownFields.Rule = ToTerm("from") | ToTerm("to") | ToTerm("subject") | ToTerm("date") | ToTerm("body") | ToTerm("cc") | ToTerm("bcc");

            rightSide.Rule = rightSideWithoutQuotes | rightSideWithQuotes;
            rightSideWithQuotes.Rule = "\"" + multipleWords + "\"";
            rightSideWithoutQuotes.Rule = multipleWords;

            multipleWords.Rule = literalChars | multipleWords + literalChars;
            literalChars.Rule = literal | "@" | "." | " ";
            
            andOr.Rule = ToTerm("|") | ToTerm("&") | ToTerm("||") | ToTerm("&&");
            
            this.Root = expr;
        }
    }
}
