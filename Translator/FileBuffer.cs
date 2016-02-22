using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Translator
{
    class FileBuffer: IDisposable
    {
        FileStream _stream;
        byte _current;

        public FileBuffer(string path)
        {
            _stream = new FileStream(path, FileMode.Open);
        }

        public byte CurrentByte {
            get
            {
                return _current;
            }
        }

        public char CurrentChar
        {
            get
            {
                return (char)_current;
            }
        }

        public bool TryMoveNext()
        {
            if (_stream.Position >= _stream.Length)
                return false;

            _current = (byte)_stream.ReadByte();

            return true;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
