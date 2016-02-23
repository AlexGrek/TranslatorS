using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SignalTranslatorCore
{
    class FileBuffer: IDisposable, ITextBuffer
    {
        FileStream _stream;
        byte _current;
        bool _ended;

        public FileBuffer(string path)
        {
            _stream = new FileStream(path, FileMode.Open);
            _ended = false;
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
            {
                _ended = true;
                return false;
            }

            _current = (byte)_stream.ReadByte();

            return true;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public bool EndReached { get { return _ended; } }
    }
}
