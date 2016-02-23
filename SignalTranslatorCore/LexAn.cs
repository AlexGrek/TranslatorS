using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalTranslatorCore
{
    /// <summary>
    /// Lexical analyser
    /// </summary>
    public class LexAn
    {
        [Flags]
        public enum SymbolCat {Unknown, Whitespace, Delimiter, Digit, Identifier,  MultiDelimiter}

        public Table Identifiers { get; private set; }
        public Table Constants { get; private set; }
        public Table Delimiters { get; private set; }
        public Table Keywords { get; private set; }

        public List<int> Output { get; private set; } = new List<int>();

        SymbolCat[] _char;

        public LexAn()
        {
            InitializeTables();
            CreateCharTable();
        }

        private void CreateCharTable ()
        {
            _char = new SymbolCat[256];

            for (int i = 0; i < _char.Length; i++ )
            {
                //digits
                if (i >= 0x30 && i <= 0x39)
                    _char[i] = SymbolCat.Digit;

                //big and small letters
                else if ((i >= 0x41 && i <= 0x5A) || (i >= 0x61 && i <= 0x7A))
                    _char[i] = SymbolCat.Identifier;

                //delimiters
                else if (i == '(' || i == ')' || i == '.' || i == ';' || i == '+' || i == '-' || i == ':'
                    || i == ',' || i == '$' || i == '\\' || i == '=')
                    _char[i] = SymbolCat.Delimiter;

                //multisymbol delimiters
                else if (i == '>' || i == '<')
                    _char[i] = SymbolCat.MultiDelimiter;

                //spaces
                else if (i == 13 || i == 10 || i == 32)
                    _char[i] = SymbolCat.Whitespace;
            }
        }

        private void InitializeTables()
        {
            var keyw = new string[]
            {
                "program",
                "procedure",
                "begin",
                "end",
                "defunc"
            };
            Keywords = new Table(keyw, startIndex: 300);
            Identifiers = new Table(canAdd: true, startIndex: 500);
            Constants = new Table(canAdd: true, startIndex: 400);

            var del = new string[]
            {
                "(", ")", ".", ";", "+", "-", ":", ",", "$", "\\", "=", ">=", "<=", ">", "<", "<>"
            };
            Delimiters = new Table(del);
        }

        public void Scan(ITextBuffer file)
        {
            var str = new StringBuilder();
            if (!file.TryMoveNext())
                throw new ArgumentException("This is an empty file!");

            while (!file.EndReached)
            {
                switch (_char[file.CurrentByte])
                {
                    case SymbolCat.Identifier:
                        str.Append(file.CurrentChar);
                        while (file.TryMoveNext() &&
                            (_char[file.CurrentByte] == SymbolCat.Identifier || _char[file.CurrentByte] == SymbolCat.Digit))
                        {
                            str.Append(file.CurrentChar);
                        }
                        IdentifierOut(str);
                        str.Clear();
                        break;

                    case SymbolCat.Digit:
                        str.Append(file.CurrentChar);
                        while (file.TryMoveNext() && _char[file.CurrentByte] == SymbolCat.Digit)
                        {
                            str.Append(file.CurrentChar);
                        }
                        NumberOut(str);
                        str.Clear();
                        break;

                    case SymbolCat.Delimiter:
                        str.Append(file.CurrentChar);
                        file.TryMoveNext();
                        DelimiterOut(str);
                        str.Clear();
                        break;

                    case SymbolCat.MultiDelimiter:  //handle <= and >= and <>
                        str.Append(file.CurrentChar);
                        while (file.TryMoveNext() && (_char[file.CurrentByte] == SymbolCat.Delimiter || _char[file.CurrentByte] == SymbolCat.MultiDelimiter))
                        {
                            if (file.CurrentChar == '=' || (file.CurrentChar == '>' && str.ToString() == "<"))
                            {
                                str.Append(file.CurrentChar);
                                file.TryMoveNext();
                            }
                            break;
                        }
                        DelimiterOut(str);
                        str.Clear();
                        break;

                    case SymbolCat.Whitespace:
                        file.TryMoveNext();
                        break;  //ignore

                    default:
                        throw new FormatException("Wtf is that? " + file.CurrentChar);
                } 
            }
        }

        private void IdentifierOut(StringBuilder sb)
        {
            var str = sb.ToString();

            //handle keyword
            if (Keywords.ContainsKey(str))
            {
                Output.Add(Keywords[str]);
            }
            else if (Identifiers.ContainsKey(str)) //handle existing user identifier
            {
                Output.Add(Identifiers[str]);
            }
            else //create new identifier
            {
                Identifiers.Add(str);
                Output.Add(Identifiers[str]);
            }
        }

        private void NumberOut(StringBuilder sb)
        {
            var str = sb.ToString();

            if (Constants.ContainsKey(str)) //handle existing user identifier
            {
                Output.Add(Constants[str]);
            }
            else //create new identifier
            {
                Constants.Add(str);
                Output.Add(Constants[str]);
            }
        }

        private void DelimiterOut(StringBuilder sb)
        {
                Output.Add(Delimiters[sb.ToString()]);
        }
    }
}
