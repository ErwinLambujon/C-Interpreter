using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Tree.Expression;

namespace InterpreterApp.Analysis.Tree.Statement
{
    public class VariableDeclarationNode : StatementNode
    {
        public VariableDeclarationNode(Token data_type_token, Dictionary<string, ExpressionNode> variables)
        {
            Data_Type_Token = data_type_token;
            Variables = variables;
        }

        public Token Data_Type_Token { get; }
        public Dictionary<string, ExpressionNode> Variables { get; }
    }
}
