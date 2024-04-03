using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Tree.Expression;

namespace InterpreterApp.Analysis.Tree.Statement
{
    public class AssignmentNode : StatementNode
    {
        public AssignmentNode(List<string> identifiers, List<Token> equals_token, ExpressionNode expression)
        {
            Identifiers = identifiers;
            Equals_Token = equals_token;
            Expression = expression;
        }

        public List<string> Identifiers { get; }
        public List<Token> Equals_Token { get; }
        public ExpressionNode Expression { get; }
    }
}
