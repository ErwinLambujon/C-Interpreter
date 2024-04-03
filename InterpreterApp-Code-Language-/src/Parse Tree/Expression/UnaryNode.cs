using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Expression
{
    public class UnaryNode : ExpressionNode
    {
        public UnaryNode(Token token_operator, ExpressionNode expression)
        {
            Token_Operator = token_operator;
            Expression = expression;
        }

        public Token Token_Operator { get; }
        public ExpressionNode Expression { get; }
    }
}
