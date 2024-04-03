using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Expression
{
    public class ParenthesisNode : ExpressionNode
    {
        public ParenthesisNode(Token open, ExpressionNode expression, Token close)
        {
            Open = open;
            Expression = expression;
            Close = close;
        }

        public Token Open { get; }
        public ExpressionNode Expression { get; }
        public Token Close { get; }
    }
}
