using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Table;
using InterpreterApp.Analysis.Tree;
using InterpreterApp.Analysis.Tree.Expression;
using InterpreterApp.Analysis.Tree.Statement;
using InterpreterApp.Analysis.Type;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using DataType = InterpreterApp.Analysis.Type.DataType;

namespace InterpreterApp.src.SyntaxAnalyzer
{
    public class Semantic
    {
        private VariableTable variable_table;

        public Semantic()
        {
            variable_table = new VariableTable();
        }

        public void Analyze(ProgramNode program)
        {
            foreach (StatementNode statement in program.Statements)
            {
                switch (statement)
                {
                    case VariableDeclarationNode var_stmt:
                        CheckVariableDeclaration(var_stmt);
                        break;
                    case AssignmentNode assign_stmt:
                        CheckAssignment(assign_stmt);
                        break;
                    case DisplayNode display_stmt:
                        CheckDisplay(display_stmt);
                        break;
                    case ScanNode scan_stmt:
                        CheckScan(scan_stmt);
                        break;
                    case ConditionalNode cond_stmt:
                        CheckCondition(cond_stmt);
                        break;
                    case LoopNode loop_stmt:
                        CheckLoop(loop_stmt);
                        break;
                }
            }
        }

        private void CheckVariableDeclaration(VariableDeclarationNode statement)
        {
            DataType data_type = Grammar.dataType(statement.Data_Type_Token.Token_Type);

            foreach (var variable in statement.Variables)
            {
                string identifier = variable.Key;

                if (!variable_table.Exist(identifier))
                {
                    if (variable.Value != null)
                    {
                        DataType data_expression_type = CheckExpression(variable.Value);

                        if (!Grammar.MatchDataType(data_type, data_expression_type))
                            throw new Exception($"({statement.Data_Type_Token.Row},{statement.Data_Type_Token.Column}): Unable to assign {data_expression_type} on \"{variable.Key}\".");
                    }
                    variable_table.AddIdentifier(identifier, data_type);
                }
                else
                    throw new Exception($"({statement.Data_Type_Token.Row},{statement.Data_Type_Token.Column}): Variable \"{variable}\" already exists.");
                
            }
        }

        private void CheckAssignment(AssignmentNode statement)
        {
            int index = 0;
            foreach (string identifier in statement.Identifiers)
            {
                if (variable_table.Exist(identifier))
                {
                    DataType data_type = variable_table.GetType(identifier);
                    DataType data_expression_type = CheckExpression(statement.Expression);

                    if (!Grammar.MatchDataType(data_type, data_expression_type))
                        throw new Exception($"({statement.Equals_Token[index].Row},{statement.Equals_Token[index].Column}): Unable to assign {data_expression_type} on \"{identifier}\".");
                }
                else
                    throw new Exception($"({statement.Equals_Token[index].Row},{statement.Equals_Token[index].Column}): Variable \"{identifier}\" does not exists.");

                index++;
            }
        }

        private void CheckDisplay(DisplayNode statement)
        {
            foreach (var expression in statement.Expressions)
            {
                switch (expression)
                {
                    case IdentifierNode iden_expr:
                        if (!variable_table.Exist(iden_expr.Name))
                            throw new Exception($"({iden_expr.Identifier_Token.Row},{iden_expr.Identifier_Token.Column}): Variable \"{iden_expr.Name}\" does not exists.");
                        break;
                }
            }
        }

        private void CheckScan(ScanNode statement)
        {
            foreach (var identifier in statement.Identifiers)
            {
                if (!variable_table.Exist(identifier))
                    throw new Exception($"({statement.Scan_Token.Row},{statement.Scan_Token.Column}): Variable \"{identifier}\" does not exists.");
            }
        }

        private void CheckCondition(ConditionalNode statement)
        {
            int index = 0;
            foreach (ExpressionNode expression in statement.Expressions)
            {
                if (expression != null)
                {
                    if (CheckExpression(expression) != DataType.Bool)
                        throw new Exception($"({statement.Tokens[index].Row},{statement.Tokens[index].Column}): Expression is not {DataType.Bool}");
                }

                Analyze(statement.Statements[index]);

                index++;
            }
        }

        private void CheckLoop(LoopNode statement)
        {
            if (CheckExpression(statement.Expression) != DataType.Bool)
                throw new Exception($"({statement.While_Token.Row},{statement.While_Token.Column}): Expression is not {DataType.Bool}");

            Analyze(statement.Statement);
        }

        private DataType CheckExpression(ExpressionNode expression)
        {
            switch (expression)
            {
                case BinaryNode bin_expr:
                    return CheckBinaryExpression(bin_expr);

                case UnaryNode unary_expr:
                    return CheckUnaryExpression(unary_expr);

                case ParenthesisNode paren_expr:
                    return CheckExpression(paren_expr.Expression);

                case IdentifierNode iden_expr:
                    return CheckIdentifierExpression(iden_expr);

                case LiteralNode literal_expr:
                    return CheckLiteralExpression(literal_expr);

                default:
                    throw new Exception($"Unknown expression.");
            }
        }
        private DataType CheckLiteralExpression(LiteralNode expression)
        {
            object val = expression.Literal;

            if (val.GetType() == typeof(int))
                return DataType.Int;
            else if (val.GetType() == typeof(float) || val.GetType() == typeof(double))
                return DataType.Float;
            else if (val.GetType() == typeof(char))
                return DataType.Char;
            else if (val.GetType() == typeof(bool))
                return DataType.Bool;
            else if (val.GetType() == typeof(string))
                return DataType.String;
            else
                throw new Exception($"({expression.Literal_Token.Row},{expression.Literal_Token.Column}): Unknown data type {expression.Literal}");
        }
        
        private DataType CheckUnaryExpression(UnaryNode expression)
        {
            Token operator_token = expression.Token_Operator;
            DataType expression_data_type = CheckExpression(expression.Expression);

            if (operator_token.Token_Type == TokenType.NOT)
            {
                if (expression_data_type != DataType.Bool)
                    throw new Exception($"({operator_token.Row},{operator_token.Column}): Operator '{operator_token.Code}' cannot be applied to {expression_data_type}");
                return DataType.Bool;
            }

            return expression_data_type;
        }
        private DataType CheckBinaryExpression(BinaryNode expression)
        {
            Token operator_token = expression.Token_Operator;
            DataType left_dt = CheckExpression(expression.Left);
            DataType right_dt = CheckExpression(expression.Right);

            if (!MatchExpressionDataType(left_dt, right_dt))
                throw new Exception($"({operator_token.Row},{operator_token.Column}): Operator '{operator_token.Code}' cannot be applied to operands of type {left_dt} and {right_dt}");

            if (Grammar.ArithmeticOperator(operator_token.Token_Type)
                && ((left_dt == DataType.Char || left_dt == DataType.String || left_dt == DataType.Bool)
                && (right_dt == DataType.Char || right_dt == DataType.String || right_dt == DataType.Bool)))
                throw new Exception($"({operator_token.Row},{operator_token.Column}): Operator '{operator_token.Code}' cannot be applied to operands of type {left_dt} and {right_dt}");
            else if (Grammar.ComparisonOperator(operator_token.Token_Type)
                && !MatchExpressionDataType(left_dt, right_dt))
                throw new Exception($"({operator_token.Row},{operator_token.Column}): Operator '{operator_token.Code}' cannot be applied to operands of type {left_dt} and {right_dt}");

            if (Grammar.ComparisonOperator(operator_token.Token_Type))
                return DataType.Bool;
            else
                return left_dt;
        }

        private DataType CheckIdentifierExpression(IdentifierNode expression)
        {
            if (!variable_table.Exist(expression.Name))
                throw new Exception($"({expression.Identifier_Token.Row},{expression.Identifier_Token.Column}): Variable \"{expression.Name}\" does not exists.");

            return variable_table.GetType(expression.Name);
        }

        private bool MatchExpressionDataType(DataType ldt, DataType rdt)
        {
            if ((ldt == DataType.Int && rdt == DataType.Float) || (ldt == DataType.Float && rdt == DataType.Int))
                return true;
            return ldt == rdt;
        }
    }
}
