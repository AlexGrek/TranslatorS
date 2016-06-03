using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalTranslatorCore
{
    public class SemanticException: Exception
    {
        string _msg;

        public SemanticException(string msg)
        {
            _msg = msg;
        }

        public override string Message
        {
            get
            {
                return _msg;
            }
        }
    }
}
