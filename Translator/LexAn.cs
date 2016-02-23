using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator
{
    /// <summary>
    /// Lexical analyser
    /// </summary>
    public class LexAn
    {
        [Flags]
        public enum SymbolCat {Unknown, Whitespace, Delimiter, Digit, Identifier,  MultiDelimiter}

        public Table Identifiers { get; private set; }
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
            Identifiers = new Table(canAdd: true, startIndex: 400);

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

        }

        private void NumberOut(StringBuilder sb)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <returns>true if multichar delimiter detected, false if it's just one-symbol delimiters</returns>
        private bool TryDelimiterOut(StringBuilder sb)
        {
            //it's multichar delimiter or one single-char delimiter
            if (Delimiters.ContainsKey(sb.ToString()))
            {
                Output.Add(Delimiters[sb.ToString()]);
                sb.Clear();
                return true;
            }
            else
            {
                Output.Add(Delimiters[sb[0].ToString()]);
                sb.Remove(0, 1); //remove delimiter that is already added
                return false;
            }
        }

        private void DelimitersOut(StringBuilder sb)
        {
            foreach (char ch in sb.ToString())
                Output.Add(Delimiters[ch.ToString()]);

        }

        private void DelimiterOut(StringBuilder sb)
        {
                Output.Add(Delimiters[sb.ToString()]);
        }
    }
}
