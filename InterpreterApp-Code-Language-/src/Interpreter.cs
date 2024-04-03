using InterpreterApp.Analysis.Tree.Statement;
using InterpreterApp.Analysis.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterApp.Analysis.Tree.Expression;
using System.Diagnostics;
using InterpreterApp.Analysis.Type;
using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Table;

using System.Reflection.Emit;
using InterpreterApp.src.SyntaxAnalyzer;

namespace InterpreterApp.src
{
    public class Interpreter
    {
        private VariableTable variable_table;
        private ProgramNode program;

        public Interpreter(string code)
        {
            Lexer lex = new Lexer(code);
            Parser parser = new Parser(lex);
            Semantic semantic = new Semantic();

            program = parser.ParseProgram();
            semantic.Analyze(program);
            variable_table = new VariableTable();
        }

        public void Execute(ProgramNode statement_block = null)
        {
            ProgramNode prog = statement_block == null ? program : statement_block;

            foreach (StatementNode statement in prog.Statements)
            {
                switch (statement)
                {
                    case VariableDeclarationNode var_stmt:
                        VariableDeclaration(var_stmt);
                        break;
                    case AssignmentNode assign_stmt:
                        ExecuteAssignment(assign_stmt);
                        break;
                    case DisplayNode display_stmt:
                        ExecuteDisplay(display_stmt);
                        break;
                    case ScanNode scan_stmt:
                        ExecuteScan(scan_stmt);
                        break;
                    case ConditionalNode cond_stmt:
                        ExecuteCondition(cond_stmt);
                        break;
                    case LoopNode loop_stmt:
                        ExecuteLoop(loop_stmt);
                        break;
                }
            }
        }
        private void VariableDeclaration(VariableDeclarationNode statement)
        {
            foreach (var variable in statement.Variables)
            {
                string identifier = variable.Key;
                
                object value = null;

                if (variable.Value != null)
                    value = EvaluateExpression(variable.Value);

                variable_table.AddVariable(identifier, Grammar.dataType(statement.Data_Type_Token.Token_Type), value);
            }
        }

        private void ExecuteAssignment(AssignmentNode statement)
        {
            object value = null;

            foreach (string identifier in statement.Identifiers)
            {
                value = EvaluateExpression(statement.Expression);

                variable_table.AddValue(identifier, value);
            }
        }

        private void ExecuteDisplay(DisplayNode statement)
        {
            string result = "";

            foreach (var expression in statement.Expressions)
                result += EvaluateExpression(expression);

            Console.Write(result);
        }

        private void ExecuteScan(ScanNode statement)
        {
            List<string> values = new List<string>();
            List<string> identifiers = statement.Identifiers;
            string inputted = "";

            Console.Write("");
            inputted = Console.ReadLine();

            values = inputted.Replace(" ", "").Split(',').ToList();
            
            if (values.Count != identifiers.Count)
                throw new Exception($"Runtime Error: Missing input/s.");

            object value = null;

            int index = 0;
            foreach (var val in values)
            {
                value = Grammar.Conversion(val);

                if (!Grammar.MatchDataType(variable_table.GetType(identifiers[index]), Grammar.dataType(value)))
                    throw new Exception($"Runtime Error: Unable to assign {Grammar.dataType(value)} on \"{identifiers[index]}\".");

                variable_table.AddValue(identifiers[index], value);

                index++;
            }
        }

        private void ExecuteCondition(ConditionalNode statement)
        {
            bool displayed = false;
            int index = 0;

            foreach (var expression in statement.Expressions)
            {
                if (expression != null)
                {

                    if ((bool)EvaluateExpression(expression))
                    {
                        displayed = true;
                        Execute(statement.Statements[index]);
                        break;
                    }
                }
                else
                    break;

                index++;
            }

            if (statement.Expressions[index] == null)
                if (!displayed)
                    Execute(statement.Statements[index]);
        }

        private void ExecuteLoop(LoopNode statement)
        {
            while ((bool)EvaluateExpression(statement.Expression))
                Execute(statement.Statement);
        }
        private object EvaluateExpression(ExpressionNode expression)
        {
            switch(expression)
            {
                case BinaryNode binary_expr:
                    return EvaluateBinaryExpression(binary_expr);

                case UnaryNode unary_expr:
                    return EvaluateUnaryExpression(unary_expr);

                case ParenthesisNode parenthesis_expr:
                    return EvaluateExpression(parenthesis_expr.Expression);

                case IdentifierNode identifier_expr:
                    return EvaluateIdentifierExpression(identifier_expr);

                case LiteralNode literal_expr:
                    return literal_expr.Literal;

                default:
                    throw new Exception("Unknown expression.");
            }
        }

        private object EvaluateBinaryExpression(BinaryNode expression)
        {
            dynamic left = EvaluateExpression(expression.Left);
            dynamic right = EvaluateExpression(expression.Right);
            dynamic bin_result;

            switch (expression.Token_Operator.Token_Type)
            {
                case TokenType.PLUS:
                    bin_result = left + right;
                    return bin_result;
                case TokenType.MINUS:
                    bin_result = left - right;
                    return bin_result;
                case TokenType.STAR:
                    bin_result = left * right;
                    return bin_result;
                case TokenType.SLASH:
                    bin_result = left / right;
                    return bin_result;
                case TokenType.PERCENT:
                    bin_result = left % right;
                    return bin_result;
                case TokenType.LESSTHAN:
                    bin_result = left < right;
                    return bin_result;
                case TokenType.GREATERTHAN:
                    bin_result = left > right;
                    return bin_result;
                case TokenType.LESSEQUAL:
                    bin_result = left <= right;
                    return bin_result;
                case TokenType.GREATEREQUAL:
                    bin_result = left >= right;
                    return bin_result;
                case TokenType.EQUALTO:
                    bin_result = left == right;
                    return bin_result;
                case TokenType.NOTEQUAL:
                    bin_result = left != right;
                    return bin_result;
                case TokenType.AND:
                    bin_result = left && right;
                    return bin_result;
                case TokenType.OR:
                    bin_result = left || right;
                    return bin_result;
                default:
                    throw new Exception($"Unknown operator.");
            }
        }
    
        private object EvaluateUnaryExpression(UnaryNode expression)
        {
            dynamic unary_value = EvaluateExpression(expression.Expression);
            if (expression.Token_Operator.Token_Type == TokenType.MINUS)
                return -unary_value;
            else if (expression.Token_Operator.Token_Type == TokenType.NOT)
                return !(unary_value.Contains("TRUE") ? true : false);
            else
                return unary_value;
        }
    
        private object EvaluateIdentifierExpression(IdentifierNode expression)
        {
            if (variable_table.GetValue(expression.Name) == null)
                throw new Exception($"({expression.Identifier_Token.Row},{expression.Identifier_Token.Column}): Variable '{expression.Name}' is null.");

            object result = variable_table.GetValue(expression.Name);

            if (result.GetType() == typeof(bool))
                return (bool)result ? "TRUE" : "FALSE";
            return result;
        }
    }
}
