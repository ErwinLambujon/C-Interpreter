using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Tree.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Statement
{
    public class LoopNode : StatementNode
    {
        public LoopNode(Token while_token, ExpressionNode expression, ProgramNode statement)
        {
            While_Token = while_token;
            Expression = expression;
            Statement = statement;
        }

        public Token While_Token { get; }
        public ExpressionNode Expression { get; }
        public ProgramNode Statement { get; }
    }
}
