using InterpreterApp.Analysis.Syntax;

namespace InterpreterApp.Analysis.Tree.Statement
{
    public class ScanNode : StatementNode
    {
        public ScanNode(Token scan_token, List<string> identifiers) 
        {
            Scan_Token = scan_token;
            Identifiers = identifiers;
        }

        public Token Scan_Token { get; }
        public List<string> Identifiers { get; }
    }
}
