using System.Text;

namespace Compiler.Tokenization
{
    /// <summary>
    /// A token in the source language
    /// </summary>
    public class Token
    {
        //The type of token
        public TokenType Type { get; }
        //The actual text associated with the token
        public string Spelling { get; }
        //The start position of the token in the source file
        public Position Position { get; }

        //Creates a token in the source language
        public Token(TokenType type, string spell, Position pos)
        {
            Spelling = spell;
            Type = type;
            Position = pos;
        }
        //ToString method to produce information on the class 
        public override string ToString()
        {
            return $"type = {Type}, spelling = \"{Spelling}\", position = {Position}";
        }
    }
}
