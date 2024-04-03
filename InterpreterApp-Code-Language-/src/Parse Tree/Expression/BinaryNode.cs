using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Expression
{
    public class BinaryNode : ExpressionNode
    {
        public BinaryNode(ExpressionNode left, Token token_operator, ExpressionNode right)
        {
            Left = left;
            Token_Operator = token_operator;
            Right = right;
        }

        public ExpressionNode Left { get; }
        public Token Token_Operator { get; }
        public ExpressionNode Right { get; }
    }
}
