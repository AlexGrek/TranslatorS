using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator
{
    public interface ITextBuffer
    {
        byte CurrentByte
        {
            get;
        }

        char CurrentChar
        {
            get;
        }

        bool TryMoveNext();

        bool EndReached { get; }
    }
}
