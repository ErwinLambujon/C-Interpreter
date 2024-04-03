using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Tree.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Statement
{
    public class ConditionalNode : StatementNode
    {
        public ConditionalNode(List<Token> tokens, List<ExpressionNode> expressions, List<ProgramNode> statements)
        {
            Tokens = tokens;
            Expressions = expressions;
            Statements = statements;
        }

        public List<Token> Tokens { get; }
        public List<ExpressionNode> Expressions { get; }
        public List<ProgramNode> Statements { get; }
    }
}
