using Compiler.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenization
{
    /// <summary>
    /// A tokenizer for the reader language
    /// </summary>
    public class Tokenizer
    {
        //Declare error reporter
        public ErrorReporter Reporter { get; }

        //The reader getting the characters from the file
        public IFileReader Reader { get; }

        //Characters currently in the token
        public StringBuilder TokenSpelling { get; } = new StringBuilder();

        //Constructor for creating a tokeniser
        public Tokenizer(IFileReader reader, ErrorReporter reporter)
        {
            Reporter = reporter;
            Reader = reader;
        }


        public List<Token> GetAllTokens()
        {
            List<Token> tokens = new List<Token>();
            Token token = GetNextToken();
            while (token.Type != TokenType.EndOfText)
            {
                tokens.Add(token);
                token = GetNextToken();
            }
            tokens.Add(token);
            Reader.Close();
            return tokens;
        }


        //Scan the next token
        private Token GetNextToken()
        {
            // Skip forward over any white spcae and comments
            SkipSeparators();

            // Remember the starting position of the token
            Position tokenStartPosition = Reader.CurrentPosition;

            // Scan the token and work out its type
            TokenType tokenType = ScanToken();

            // Create the token
            Token token = new Token(tokenType, TokenSpelling.ToString(), tokenStartPosition);
            Debugger.Write($"Scanned {token}");

            // Report an error if necessary
            if (tokenType == TokenType.Error)
            {
                Position position = token.Position;
                // Report the error here
                Reporter.ErrorReport("Token type error", position);
            }

            return token;
        }

        //Checks whether a character is white space or not
        private static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n';
        }

        //Skip forward until the next character is not whitespace or a comment
        private void SkipSeparators()
        {
            while (Reader.Current == '$' || IsWhiteSpace(Reader.Current))
            {
                if (Reader.Current == '$')
                    Reader.SkipRestOfLine();
                else
                    Reader.MoveNext();
            }
        }


        //Appends the current character to the current token then moves to the next character
        private void TakeIt()
        {
            TokenSpelling.Append(Reader.Current);
            Reader.MoveNext();
        }


        //Checks whether a character is an operator
        private static bool IsOperator(char c)
        {
            switch (c)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '<':
                case '>':
                case '=':
                case '\\':
                    return true;
                default:
                    return false;
            }
        }

        private TokenType ScanToken()
        {
            TokenSpelling.Clear();
            if (char.IsLetter(Reader.Current))
            {
                TakeIt();
                while (char.IsLetterOrDigit(Reader.Current))
                    TakeIt();
                if (TokenTypes.IsKeyword(TokenSpelling))
                    return TokenTypes.GetTokenForKeyword(TokenSpelling);
                else
                    return TokenType.Identifier;
            }
            else if (char.IsDigit(Reader.Current))
            {
                TakeIt();
                while (char.IsDigit(Reader.Current))
                    TakeIt();
                return TokenType.IntLiteral;
            }
            else if (IsOperator(Reader.Current))
            {
                // Read an operator
                TakeIt();
                return TokenType.Operator;
            }
            else if (Reader.Current == ':')
            {
                //Reads a Colon
                TakeIt();
                return TokenType.Colon;
            }
            else if (Reader.Current == ';')
            {
                // Read a ;
                TakeIt();
                return TokenType.Semicolon;
            }
            else if (Reader.Current == '~')
            {
                // Read a ~
                TakeIt();
                return TokenType.Is;
            }
            else if (Reader.Current == ',')
            {
                //Read a ,
                TakeIt();
                return TokenType.Comma;
            }
            else if (Reader.Current == '(')
            {
                // Read a (
                TakeIt();
                return TokenType.LeftBracket;
            }
            else if (Reader.Current == ')')
            {
                // Read a )
                TakeIt();
                return TokenType.RightBracket;
            }
            else if (Reader.Current == '\"')
            {
                // Read a "
                TakeIt();
                // Take whatever the character is
                TakeIt();
                // Try getting the closing "
                if (Reader.Current == '\"')
                {
                    TakeIt();
                    return TokenType.CharLiteral;
                }
                else
                {
                    // Could do some better error handling here but we weren't asked to
                    return TokenType.Error;
                }
            }
            else if (Reader.Current == default(char))
            {
                // Read the end of the file
                TakeIt();
                return TokenType.EndOfText;
            }
            else
            {
                // Encountered a character we weren't expecting
                TakeIt();
                return TokenType.Error;
            }
        }

    }
}

