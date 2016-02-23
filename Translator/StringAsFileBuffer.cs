using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator
{
    public class StringAsFileBuffer: ITextBuffer
    {
        string _internal;
        int _position = -1;
        byte[] _byteArray;
        public bool EndReached { get; private set; } = false;

        public StringAsFileBuffer(string s)
        {
            _internal = s;
            _byteArray = Encoding.ASCII.GetBytes(s);
        }

        public byte CurrentByte
        {
            get
            {
                return _byteArray[_position];
            }
        }

        public char CurrentChar
        {
            get
            {
                return _internal[_position];
            }
        }

        public bool TryMoveNext()
        {
            if (_position >= _byteArray.Length - 1)
            {
                EndReached = true;
                return false;
            }
            _position++;
            return true;
        }

       
    }
}
