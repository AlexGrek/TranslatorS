using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalTranslatorCore
{
    public class SyntaxException: Exception
    {
        public string Expected { get; internal set; }
        public string Got { get; internal set; }
        public int Line { get; internal set; }
        public int Lexeme { get; internal set; }
        public string Summary { get; internal set; }
        public string Method { get; internal set; }
        public bool Critical = false;

        public SyntaxException()
        {
            Summary = "Unknown error";
        }

        public SyntaxException(string message): base(message)
        {
            Summary = message;
        }

        public SyntaxException(string exp, string got, int line, int lex, bool critical = false)
        {
            Expected = exp;
            Got = got;
            Line = line;
            Lexeme = lex;
            Critical = critical;
        }

        public SyntaxException(string exp, string got, int line, int lex, string method, bool critical = false) 
            :this(exp, got, line, lex, critical)
        {
            Method = method;
        }

        public override string Message
        {
            get
            {
                if (Summary != null)
                    return Summary;
                else
                    return $"Syntax error on line {Line}: {Expected} expected, but {Got} found."
                        + Method == null ? "" : $" In method {Method}."; 
            }
        }
    }
}
