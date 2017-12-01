using System;
using System.Collections.Generic;
using System.Text;

namespace GeneticTree.BooleanLogicParser
{

    /// <summary>
    /// Derived from https://github.com/spavkov/BooleanLogicExpressionParser
    /// </summary>
    public class Tokenizer
    {
        private string _text;

        private int i = 0;
        public Tokenizer(string text)
        {
            _text = text;
        }

        public IEnumerable<Token> Tokenize()
        {

            var tokens = new List<Token>();
            while (_text.Length > i)
            {
                while (Char.IsWhiteSpace((char) _text[i]))
                {
                    i++;
                }

                if (_text.Length <= i)
                    break;

                var c = (char) _text[i];
                switch (c)
                {
                    case '!':
                        tokens.Add(new NegationToken());
                        i++;
                        break;
                    case '(':
                        tokens.Add(new OpenParenthesisToken());
                        i++;
                        break;
                    case ')':
                        tokens.Add(new ClosedParenthesisToken());
                        i++;
                        break;
                    default:
                        if (Char.IsLetter(c))
                        {
                            var token = ParseKeyword();
                            tokens.Add(token);
                        }
                        else
                        {
                            var remainingText = _text.Substring(i) ?? string.Empty;
                            throw new Exception(string.Format("Unknown grammar found at position {0} : '{1}'", _text.Length - remainingText.Length, remainingText));
                        }
                        break;
                }
            }
            return tokens;
        }

        private Token ParseKeyword()
        {
            var text = "";
            while (_text.Length > i && Char.IsLetter((char) _text[i]))
            {
                text = text + ((char) _text[i]);
                i++;
            }

            var potentialKeyword = text.ToString().ToLower();

            switch (potentialKeyword)
            {
                case "true":
                    return new TrueToken();
                case "false":
                    return new FalseToken();
                case "and":
                    return new AndToken();
                case "or":
                    return new OrToken();
                default:
                    throw new Exception("Expected keyword (True, False, And, Or) but found "+ potentialKeyword);
            }
        }
    }
}