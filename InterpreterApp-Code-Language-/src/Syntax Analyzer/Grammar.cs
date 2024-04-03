using System.Text.RegularExpressions;
using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Type;

namespace InterpreterApp.src.SyntaxAnalyzer
{

    public static class Grammar
    {


        public static Token WordToken(string input, int line, int column)
        {

            Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
            {
                {"BEGIN", TokenType.BEGIN}, {"END", TokenType.END}, {"CODE", TokenType.CODE}, {"IF", TokenType.IF}, 
                {"ELSE", TokenType.ELSE}, {"WHILE", TokenType.WHILE}, {"DISPLAY", TokenType.DISPLAY}, {"SCAN", TokenType.SCAN},
                {"AND", TokenType.AND}, {"OR", TokenType.OR}, {"NOT", TokenType.NOT}
            };

            Dictionary<string, TokenType> data_types = new Dictionary<string, TokenType>()
            {
                {"INT", TokenType.INT}, {"FLOAT", TokenType.FLOAT}, {"CHAR", TokenType.CHAR}, {"BOOL", TokenType.BOOL}
            };

            if (keywords.ContainsKey(input.ToUpper()))
            {
                if (keywords.ContainsKey(input))
                    return new Token(keywords[input], input, null, line, column);
                else
                    return new Token(TokenType.ERROR, input, $"Invalid keyword '{input}' should be {input.ToUpper()}", line, column);
            }
            else if (data_types.ContainsKey(input.ToUpper()))
            {
                if (data_types.ContainsKey(input))
                    return new Token(data_types[input], input, null, line, column);
                else
                    return new Token(TokenType.ERROR, input, $"Invalid data type '{input}' should be {input.ToUpper()}", line, column);
            }
            else
                return new Token(TokenType.IDENTIFIER, input, null, line, column);
        }

        public static int BinaryPrecedence(TokenType token_type)
        {
            switch (token_type)
            {
                case TokenType.OR:
                    return 1;
                case TokenType.AND:
                    return 2;
                case TokenType.LESSTHAN:
                case TokenType.LESSEQUAL:
                case TokenType.GREATERTHAN:
                case TokenType.GREATEREQUAL:
                case TokenType.EQUALTO:
                case TokenType.NOTEQUAL:
                    return 4;
                case TokenType.PLUS:
                case TokenType.MINUS:
                    return 5;
                case TokenType.PERCENT:
                    return 6;
                case TokenType.STAR:
                case TokenType.SLASH:
                    return 7;
                default:
                    return 0;
            }
        }

        public static DataType dataType(Token token)
        {
            TokenType token_type = token.Token_Type;

            switch (token_type)
            {
                case TokenType.INT:
                case TokenType.INTLITERAL:
                    return DataType.Int;
                case TokenType.FLOAT:
                case TokenType.FLOATLITERAL:
                    return DataType.Float;
                case TokenType.CHAR:
                case TokenType.CHARLITERAL:
                    return DataType.Char;
                case TokenType.BOOL:
                case TokenType.BOOLLITERAL:
                    return DataType.Bool;
                default:
                    throw new Exception($"Unknown data type");
            }
        }

        public static DataType dataType(TokenType token_type)
        {
            switch (token_type)
            {
                case TokenType.INT:
                case TokenType.INTLITERAL:
                    return DataType.Int;
                case TokenType.FLOAT:
                case TokenType.FLOATLITERAL:
                    return DataType.Float;
                case TokenType.CHAR:
                case TokenType.CHARLITERAL:
                    return DataType.Char;
                case TokenType.BOOL:
                case TokenType.BOOLLITERAL:
                    return DataType.Bool;
                default:
                    throw new Exception($"Unknown data type");
            }
        }

        public static DataType dataType(object val)
        {
            if (val.GetType() == typeof(int))
                return DataType.Int;
            else if (val.GetType() == typeof(float) || val.GetType() == typeof(double))
                return DataType.Float;
            else if (val.GetType() == typeof(char))
                return DataType.Char;
            else if (val.GetType() == typeof(bool))
                return DataType.Bool;
            else
                return DataType.String;
        }
        

        public static bool ComparisonOperator(TokenType token_type)
        {
            List<TokenType> logical_operator = new List<TokenType>
            {
                TokenType.LESSTHAN, TokenType.GREATERTHAN,
                TokenType.LESSEQUAL, TokenType.GREATEREQUAL,
                TokenType.EQUALTO, TokenType.NOTEQUAL
            };

            return logical_operator.Contains(token_type);
        }

        public static bool MatchDataType(DataType ldt, DataType rdt)
        {
            if ((ldt == DataType.Float && rdt == DataType.Int))
                return true;
            return ldt == rdt;
        }

        public static object Conversion(string val)
        {
            string float_pattern = @"^(?:\+|\-)?\d*\.\d+$";
            string int_pattern = @"^(?:\+|\-)?\d+$"; 
            string char_pattern = @"^'(?:\[[\[\]\&\$\#']\])'|'[^\[\]\&\$\#']'$";
            string bool_pattern = @"^\""TRUE\""$|^\""FALSE\""$";

            Regex float_regex = new Regex(float_pattern);
            Regex int_regex = new Regex(int_pattern);
            Regex char_regex = new Regex(char_pattern);
            Regex bool_regex = new Regex(bool_pattern);

            if (int_regex.IsMatch(val))
                return Convert.ToInt32(val);
            else if (float_regex.IsMatch(val))
                return Convert.ToDouble(val);
            else if (char_regex.IsMatch(val))
                return val;
            else if (bool_regex.IsMatch(val))
                return val == "\"TRUE\"" ? true : false;
            else
                throw new Exception($"Runtime Error: Invalid input {val}.");
        }

        public static bool ArithmeticOperator(TokenType token_type)
        {
            List<TokenType> arithmetic_operator = new List<TokenType>
            {
                TokenType.PLUS, TokenType.MINUS, TokenType.STAR, TokenType.SLASH, TokenType.PERCENT
            };

            return arithmetic_operator.Contains(token_type);
        }
    }
}
