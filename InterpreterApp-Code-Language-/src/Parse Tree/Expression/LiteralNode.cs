using InterpreterApp.Analysis.Syntax;

namespace InterpreterApp.Analysis.Tree.Expression
{
    public class LiteralNode : ExpressionNode
    {
        public LiteralNode(Token literal_token, object literal)
        {
            Literal_Token = literal_token;
            Literal = literal;
        }

        public Token Literal_Token { get; }
        public object Literal { get; }
    }
}
