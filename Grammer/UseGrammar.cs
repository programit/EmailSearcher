using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;

namespace Grammer
{
    public class UseGrammar
    {
        string currentBuildValue;
        string currentBuildField;        
        public List<Tuple<FieldType, string>> Parse(ParseTreeNode node)
        {
            List<Tuple<FieldType, string>> expressions = new List<Tuple<FieldType, string>>();
            this.Switch(node, expressions);
            return expressions;
        }

        private void Switch(ParseTreeNode node, List<Tuple<FieldType, string>> expressions)
        {
            if(node == null)
            {
                return;
            }

            switch (node.Term.Name)
            {
                case "expr":
                    foreach (ParseTreeNode child in node.ChildNodes)
                    {
                        this.Switch(child, expressions);
                    }

                    break;
                case "singleExpr":
                    foreach (ParseTreeNode child in node.ChildNodes)
                    {
                        this.Switch(child, expressions);
                    }

                    expressions.Add(new Tuple<FieldType, string>((FieldType) Enum.Parse(typeof(FieldType), currentBuildField, true), currentBuildValue));
                    currentBuildField = string.Empty;
                    currentBuildValue = string.Empty;
                    break;
                case "multipleExpr":
                    foreach (ParseTreeNode child in node.ChildNodes)
                    {
                        this.Switch(child, expressions);
                    }

                    break;
                case "knownFields":
                    currentBuildField = node.ChildNodes[0].Token.Text;
                    break;
                case "rightSide":
                    foreach (ParseTreeNode child in node.ChildNodes)
                    {
                        this.Switch(child, expressions);
                    }

                    break;
                case "rightSideWithQuotes":
                    foreach (ParseTreeNode child in node.ChildNodes)
                    {
                        this.Switch(child, expressions);
                    }
                    break;
                case "rightSideWithoutQuotes":
                    foreach (ParseTreeNode child in node.ChildNodes)
                    {
                        this.Switch(child, expressions);
                    }
                    break;
                case "multipleWords":
                    foreach (ParseTreeNode child in node.ChildNodes)
                    {
                        this.Switch(child, expressions);
                    }
                    break;
                case "literalChars":
                    this.currentBuildValue += node.ChildNodes[0].Token.Text;
                    break;
            }
        }
    }
}