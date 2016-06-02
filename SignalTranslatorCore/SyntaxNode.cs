using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalTranslatorCore
{
    public struct SyntaxNode
    {
        public string Name;
        public int Value;
        public string Tag;

        public SyntaxNode(string name, int value)
        {
            Name = name;
            Value = value;
            Tag = "";
        }

        public SyntaxNode(string name, string tag)
        {
            Name = name;
            Tag = tag;
            Value = -1;
        }

        public SyntaxNode(string name, int val, string tag)
        {
            Name = name;
            Tag = tag;
            Value = val;
        }

        public SyntaxNode(string name)
        {
            Name = name;
            Value = -1;
            Tag = "";
        }

        public override string ToString()
        {
            if (Value > 0)
                return $"{Value}: {Name} {Tag}";
            else if (Tag != "")
            {
                return $"{Name} ({Tag})";
            }
            return Name;
        }
    }
}
