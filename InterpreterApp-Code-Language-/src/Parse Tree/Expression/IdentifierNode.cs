using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Type;

namespace InterpreterApp.Analysis.Tree.Expression
{
    public class IdentifierNode : ExpressionNode
    {
        public IdentifierNode(Token identifier_token, string name)
        {
            Identifier_Token = identifier_token;
            Name = name;
        }

        public Token Identifier_Token { get; }
        public string Name { get; }
    }
}
