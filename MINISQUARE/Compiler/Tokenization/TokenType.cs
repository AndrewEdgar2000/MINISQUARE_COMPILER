using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using static Compiler.Tokenization.TokenType;

namespace Compiler.Tokenization
{
    /// <summary>
    /// The types of token in the language
    /// </summary>
    public enum TokenType
    {
        //Non-terminals
        Identifier, IntLiteral, CharLiteral, Graphic,
        LetterOrDigit, Operator, Letter, Digit,

        //Reserved Words - Terminals
        Begin, Const, Do, Else, End, If, In, Let, Then, Var, While, For,

        //Punctuation - Terminals (Becomes = (:=), Is = (~))
        Colon, Semicolon, Becomes, Is, LeftBracket, RightBracket, Nothing,
        Forever, Comma, Quote,

        //Special Tokens
        EndOfText, Error
    }

    //Utility functions for working with the tokens
    public static class TokenTypes
    {

        //A mapping from keyword to the token type for that keyword
        public static ImmutableDictionary<string, TokenType> Keywords { get; } = new Dictionary<string, TokenType>()
        {
            { "begin", Begin },
            { "const", Const },
            { "do", Do },
            { "else", Else },
            { "end", End },
            { "if", If },
            { "in", In },
            { "let", Let },
            { "then", Then },
            { "var", Var },
            { "while", While },
            {"becomes", Becomes },
            {"forever", Forever },
            {"nothing", Nothing },
            {"for" , For }
        }.ToImmutableDictionary();


        //Checks whether a word is a keyword
        public static bool IsKeyword(StringBuilder word)
        {
            return Keywords.ContainsKey(word.ToString());
        }

        //Gets the token for a keyword
        public static TokenType GetTokenForKeyword(StringBuilder word)
        {
            if (!IsKeyword(word)) throw new ArgumentException("Word is not a keyword");
            return Keywords[word.ToString()];
        }

    }
}
