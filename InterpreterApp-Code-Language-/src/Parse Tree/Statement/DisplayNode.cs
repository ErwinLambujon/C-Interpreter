using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Tree.Expression;
using InterpreterApp.Analysis.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Statement
{
    public class DisplayNode : StatementNode
    {
        public DisplayNode(Token display_token, List<ExpressionNode> expressions)
        {
            Display_Token = display_token;
            Expressions = expressions;
        }

        public Token Display_Token { get; }
        public List<ExpressionNode> Expressions { get; }
    }
}
