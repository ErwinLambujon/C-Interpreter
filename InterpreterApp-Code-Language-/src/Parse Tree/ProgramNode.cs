using InterpreterApp.Analysis.Tree.Statement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree
{
    public class ProgramNode : ASTNode
    {
        public ProgramNode(List<StatementNode> statements)
        {
            Statements = statements;
        }

        public List<StatementNode> Statements { get; }
    }
}
