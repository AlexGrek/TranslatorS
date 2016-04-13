using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalTranslatorCore
{
    public class Tree<T>
    {
        public TreeNode<T> Root { get; set; }

        public Tree()
        {
            Root = new TreeNode<T>();
        }

        public Tree(T rootContent)
        {
            Root = new TreeNode<T>(rootContent);
        }

        public override string ToString()
        {
            return Root.ToString();
        }
    }
}
