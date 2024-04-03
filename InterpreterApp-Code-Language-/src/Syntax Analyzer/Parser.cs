using InterpreterApp.Analysis.Tree;
using InterpreterApp.Analysis.Tree.Statement;
using InterpreterApp.Analysis.Tree.Expression;
using InterpreterApp.Analysis.Type;
using System.Diagnostics;
using InterpreterApp.src.SyntaxAnalyzer;

namespace InterpreterApp.Analysis.Syntax
{
    public class Parser
    {
        private readonly Lexer _lexer;
        private Token _current_token;
        private List<string> _variable_names;
        private bool _can_declare;

        public Parser(Lexer lexer)
        {
            this._lexer = lexer;
            this._current_token = lexer.GetNextToken();
            this._variable_names = new List<string>();
            this._can_declare = true;
        }

        public ProgramNode ParseProgram(TokenType token_type = TokenType.CODE)
        {
            while (MatchToken(TokenType.NEWLINE))
                ConsumeToken(TokenType.NEWLINE);

            ConsumeToken(TokenType.BEGIN);
            ConsumeToken(token_type);

            while (MatchToken(TokenType.NEWLINE))
                ConsumeToken(TokenType.NEWLINE);

            List<StatementNode> statements = ParseStatements();

            while (MatchToken(TokenType.NEWLINE))
                ConsumeToken(TokenType.NEWLINE);

            ConsumeToken(TokenType.END);
            ConsumeToken(token_type);

            while (MatchToken(TokenType.NEWLINE))
                ConsumeToken(TokenType.NEWLINE);

            if (token_type == TokenType.CODE)
                ConsumeToken(TokenType.ENDOFFILE);

            return new ProgramNode(statements);
        }
        private List<StatementNode> ParseStatements()
        {
            List<StatementNode> statement_list = new List<StatementNode>();

            while (!MatchToken(TokenType.END))
            {
                if (MatchToken(TokenType.INT) || MatchToken(TokenType.FLOAT) ||
                    MatchToken(TokenType.CHAR) || MatchToken(TokenType.BOOL))
                {
                    if (_can_declare)
                        statement_list.Add(ParseVariableDeclarationStatement());
                    else
                        throw new Exception($"({_current_token.Row},{_current_token.Column}): Invalid syntax.");
                }
                else if (MatchToken(TokenType.IDENTIFIER))
                {
                    _can_declare = false;
                    statement_list.Add(ParseAssignmentStatement());
                }
                else if (MatchToken(TokenType.DISPLAY))
                {
                    _can_declare = false;
                    statement_list.Add(ParseDisplayStatement());
                }
                else if (MatchToken(TokenType.SCAN))
                {
                    _can_declare = false;
                    statement_list.Add(ParseScanStatement());
                }
                else if (MatchToken(TokenType.IF))
                {
                    _can_declare = false;
                    statement_list.Add(ParseIfStatement());
                }
                else if (MatchToken(TokenType.WHILE))
                {
                    _can_declare = false;
                    statement_list.Add(ParseWhileStatement());
                }

                else if (MatchToken(TokenType.ENDOFFILE))
                    throw new Exception($"({_current_token.Row},{_current_token.Column}): Missing End Statement.");

                else
                    throw new Exception($"({_current_token.Row},{_current_token.Column}): Invalid syntax \"{_current_token.Code}\".");

                while (MatchToken(TokenType.NEWLINE))
                    ConsumeToken(TokenType.NEWLINE);
            }

            return statement_list;
        }

        private StatementNode ParseVariableDeclarationStatement()
        {
            Token data_type_token = _current_token;
            ConsumeToken(data_type_token.Token_Type);

            Dictionary<string, ExpressionNode> variables = new Dictionary<string, ExpressionNode>();

            (string, ExpressionNode) variable = GetVariable();

            variables.Add(variable.Item1, variable.Item2);
            _variable_names.Add(variable.Item1);

            while (MatchToken(TokenType.COMMA))
            {
                ConsumeToken(TokenType.COMMA);
                variable = GetVariable();
                variables.Add(variable.Item1, variable.Item2);
                _variable_names.Add(variable.Item1);
            }
            return new VariableDeclarationNode(data_type_token, variables);
        }
        
        private StatementNode ParseAssignmentStatement()
        {
            List<string> identifiers = new List<string>();
            List<Token> equals = new List<Token>();

            Token identifier_token = _current_token;
            ConsumeToken(TokenType.IDENTIFIER);
            Token equal_token = _current_token;
            ConsumeToken(TokenType.EQUAL);

            identifiers.Add(identifier_token.Code);
            equals.Add(equal_token);

            ExpressionNode expression_value = ParseExpression();

            while (MatchToken(TokenType.EQUAL))
            {
                IdentifierNode iden_expr = (IdentifierNode)expression_value;
                equal_token = _current_token;
                ConsumeToken(TokenType.EQUAL);
                
                identifiers.Add(iden_expr.Name);
                equals.Add(equal_token);

                expression_value = ParseExpression();
            }

            return new AssignmentNode(identifiers, equals, expression_value);
        }
        
        private StatementNode ParseDisplayStatement()
        {
            Token display_token = _current_token;
            ConsumeToken(TokenType.DISPLAY);
            ConsumeToken(TokenType.COLON);

            List<ExpressionNode> expressions = new List<ExpressionNode>();

            if (MatchToken(TokenType.DOLLAR))
            {
                expressions.Add(new LiteralNode(_current_token, "\n"));
                ConsumeToken(TokenType.DOLLAR);

                while (MatchToken(TokenType.AMPERSAND))
                {
                    ConsumeToken(TokenType.AMPERSAND);

                    if (MatchToken(TokenType.NEWLINE))
                        throw new Exception($"({_current_token.Row},{_current_token.Column}): Unexpected {_current_token.Token_Type} token expected expression token");
                    if (MatchToken(TokenType.DOLLAR))
                    {
                        expressions.Add(new LiteralNode(_current_token, "\n"));
                        ConsumeToken(TokenType.DOLLAR);
                    }
                    else
                        expressions.Add(ParseExpression());
                }

                if (!MatchToken(TokenType.NEWLINE))
                    throw new Exception($"({_current_token.Row},{_current_token.Column}): Unexpected {_current_token.Token_Type} token expected {TokenType.NEWLINE} token");

                return new DisplayNode(display_token, expressions);
            }
            else if (MatchToken(TokenType.ESCAPE) || MatchToken(TokenType.IDENTIFIER) || MatchToken(TokenType.INTLITERAL) || MatchToken(TokenType.FLOATLITERAL)
                || MatchToken(TokenType.CHARLITERAL) || MatchToken(TokenType.BOOLLITERAL) || MatchToken(TokenType.STRINGLITERAL)
                || MatchToken(TokenType.MINUS) || MatchToken(TokenType.PLUS) || MatchToken(TokenType.NOT))
            {
                expressions.Add(ParseExpression());

                while (MatchToken(TokenType.AMPERSAND))
                {
                    ConsumeToken(TokenType.AMPERSAND);

                    if (MatchToken(TokenType.NEWLINE))
                        throw new Exception($"({_current_token.Row},{_current_token.Column}): Unexpected {_current_token.Token_Type} token expected expression token");

                    if (MatchToken(TokenType.DOLLAR))
                    {
                        expressions.Add(new LiteralNode(_current_token, "\n"));
                        ConsumeToken(TokenType.DOLLAR);
                    }
                    else
                        expressions.Add(ParseExpression());
                }

                if (!MatchToken(TokenType.NEWLINE))
                    throw new Exception($"({_current_token.Row},{_current_token.Column}): Unexpected {_current_token.Token_Type} token expected {TokenType.NEWLINE} token");
                
                return new DisplayNode(display_token, expressions);
            }
            else
                throw new Exception($"({_current_token.Row},{_current_token.Column}): Unexpected {_current_token.Token_Type} token expected expression token");
        }

        private StatementNode ParseScanStatement()
        {
            Token scan_token = _current_token;
            ConsumeToken(TokenType.SCAN);
            ConsumeToken(TokenType.COLON);

            List<string> identifiers = new List<string>();

            identifiers.Add(_current_token.Code);
            ConsumeToken(TokenType.IDENTIFIER);
            
            while (MatchToken(TokenType.COMMA)) 
            {
                ConsumeToken(TokenType.COMMA);
                identifiers.Add(_current_token.Code);
                ConsumeToken(TokenType.IDENTIFIER);
            }

            return new ScanNode(scan_token, identifiers);
        }

        private StatementNode ParseIfStatement()
        {
            bool is_else = false;
            List<ExpressionNode> conditions = new List<ExpressionNode>();
            List<ProgramNode> statement_blocks = new List<ProgramNode>();
            List<Token> tokens = new List<Token>();
            tokens.Add(_current_token);
            ConsumeToken(TokenType.IF);
            conditions.Add(ParseConditionExpression());

            statement_blocks.Add(ParseProgram(TokenType.IF));

            while (MatchToken(TokenType.ELSE))
            {
                if (is_else)
                    throw new Exception($"({_current_token.Row}, {_current_token.Column}): Invalid syntax {_current_token.Token_Type}");

                tokens.Add(_current_token);
                ConsumeToken(TokenType.ELSE);

                if (MatchToken(TokenType.IF))
                {
                    ConsumeToken(TokenType.IF);
                    conditions.Add(ParseConditionExpression());
                }
                else
                {
                    conditions.Add(null);
                    is_else = true;
                }

                statement_blocks.Add(ParseProgram(TokenType.IF));
            }

            return new ConditionalNode(tokens, conditions, statement_blocks);
        }

        private StatementNode ParseWhileStatement()
        {
            Token while_token = _current_token;
            ConsumeToken(TokenType.WHILE);

            ExpressionNode condition = ParseConditionExpression();

            ProgramNode statement_block = ParseProgram(TokenType.WHILE);

            return new LoopNode(while_token, condition, statement_block);
        }

        private ExpressionNode ParseExpression()
        {
            ExpressionNode expression;
            Debug.WriteLine(_current_token);
            if (MatchToken(TokenType.ESCAPE))
            {
                Token escape_token = _current_token;
                ConsumeToken(TokenType.ESCAPE);
                return new LiteralNode(escape_token, escape_token.Value);
            }
            else if (MatchToken(TokenType.OPENPARENTHESIS))
            {
                expression = ParseParenthesisExpression();
                return expression;
            }
            else if (MatchToken(TokenType.PLUS) || MatchToken(TokenType.MINUS) || MatchToken(TokenType.NOT))
            {
                expression = ParseUnaryExpression();
                return expression;
            } 
            else if (MatchToken(TokenType.IDENTIFIER) || MatchToken(TokenType.INTLITERAL) || MatchToken(TokenType.FLOATLITERAL)
                || MatchToken(TokenType.CHARLITERAL) || MatchToken(TokenType.BOOLLITERAL) || MatchToken(TokenType.STRINGLITERAL))
            {
                expression = ParseBinaryExpression();
                return expression;
            }
            else
                throw new Exception($"({_current_token.Row}, {_current_token.Column}): Unexpected {_current_token.Token_Type} token expected expression token.");
        }
        
        private ExpressionNode ParseParenthesisExpression()
        {
            Token open_parenthesis = _current_token;
            ConsumeToken(TokenType.OPENPARENTHESIS);

            ExpressionNode expression = ParseExpression();

            Token close_parenthesis = _current_token;
            ConsumeToken(TokenType.CLOSEPARENTHESIS);

            int precedence = Grammar.BinaryPrecedence(_current_token.Token_Type);

            if (precedence > 0)
            {
                ParenthesisNode paren_expr = new ParenthesisNode(open_parenthesis, expression, close_parenthesis);
                return ParseBinaryExpression(paren_expr);

            }
            return new ParenthesisNode(open_parenthesis, expression, close_parenthesis);
        }

        private ExpressionNode ParseConditionExpression()
        {
            Token open_parenthesis = _current_token;
            ConsumeToken(TokenType.OPENPARENTHESIS);

            ExpressionNode expression = ParseExpression();

            Token close_parenthesis = _current_token;
            ConsumeToken(TokenType.CLOSEPARENTHESIS);

            return new ParenthesisNode(open_parenthesis, expression, close_parenthesis);
        }

        private ExpressionNode ParseUnaryExpression()
        {
            Token unary_token = _current_token;
            ConsumeToken(unary_token.Token_Type);
            ExpressionNode expression = null;
            if (MatchToken(TokenType.OPENPARENTHESIS))
                expression = ParseExpression();
            else
                expression = ParseTerm();

            UnaryNode unary_expr = new UnaryNode(unary_token, expression);

            if (Grammar.BinaryPrecedence(_current_token.Token_Type) > 0)
                return ParseBinaryExpression(unary_expr);

            return unary_expr;
        }

        private ExpressionNode ParseBinaryExpression(ExpressionNode prev_left = null)
        {
            ExpressionNode left;
            if (prev_left != null)
                left = prev_left;
            else
                left = ParseTerm();

            int precedence = Grammar.BinaryPrecedence(_current_token.Token_Type);

            while (precedence > 0)
            {
                Token binary_token = _current_token;
                ConsumeToken(binary_token.Token_Type);

                ExpressionNode right = ParseTerm();

                int next_precedence = Grammar.BinaryPrecedence(_current_token.Token_Type);
                
                if (next_precedence > precedence)
                    right = ParseBinaryExpression(right);
                left = new BinaryNode(left, binary_token, right);

                precedence = Grammar.BinaryPrecedence(_current_token.Token_Type);
            }

            return left;
        }

        private ExpressionNode ParseTerm()
        {
            if (MatchToken(TokenType.IDENTIFIER))
            {
                Token identifier_token = _current_token;
                ConsumeToken(TokenType.IDENTIFIER);
                return new IdentifierNode(identifier_token, identifier_token.Code);
            }
            else if (MatchToken(TokenType.INTLITERAL) || MatchToken(TokenType.FLOATLITERAL) || MatchToken(TokenType.CHARLITERAL)
                || MatchToken(TokenType.BOOLLITERAL) || MatchToken(TokenType.STRINGLITERAL))
            {
                Token literal_token = _current_token;
                ConsumeToken(literal_token.Token_Type);
                return new LiteralNode(literal_token, literal_token.Value);
            }
            else
                return ParseExpression();
        }


        private void ConsumeToken(TokenType token_type)
        {
            if (MatchToken(token_type))
            {
                Token prev_token = _current_token;
                _current_token = _lexer.GetNextToken();
                if (MatchToken(TokenType.ERROR))
                {
                    if (prev_token.Token_Type == TokenType.INT || prev_token.Token_Type == TokenType.FLOAT || prev_token.Token_Type == TokenType.CHAR || prev_token.Token_Type == TokenType.BOOL)
                    {
                        if (_current_token.Value.ToString().Contains("Invalid keyword") || _current_token.Value.ToString().Contains("Invalid data type"))
                        {
                            _current_token.Token_Type = TokenType.IDENTIFIER;
                            _current_token.Value = null;
                        }
                    }
                    else if (_variable_names.Contains(_current_token.Code))
                    {
                        _current_token.Token_Type = TokenType.IDENTIFIER;
                        _current_token.Value = null;
                    }
                    else
                        throw new Exception($"({_current_token.Row}, {_current_token.Column}): {_current_token.Value}");
                }
            }
            else
                throw new Exception($"({_current_token.Row},{_current_token.Column}): Unexpected token {_current_token.Token_Type} token expected {token_type} token");
        }

        private bool MatchToken(TokenType token_type)
        {
            return _current_token.Token_Type == token_type;
        }

        private (string, ExpressionNode) GetVariable()
        {
            Token identifier = _current_token;

            ConsumeToken(TokenType.IDENTIFIER);

            if (MatchToken(TokenType.EQUAL))
            {
                ConsumeToken(TokenType.EQUAL);
                return (identifier.Code, ParseExpression());
            }
            return (identifier.Code, null);
        }

    }
}
