using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalTranslatorCore
{
    public class TreeNode<T>
    {
        public T Content { get; set; }
        private List<TreeNode<T>> _nodes = new List<TreeNode<T>>(); 

        public TreeNode() { }

        public TreeNode(T content)
        {
            Content = content;
        }

        public TreeNode<T> AddNode(TreeNode<T> node)
        {
            _nodes.Add(node);
            return node;
        }

        public TreeNode<T> AddNode(T nodeContent)
        {
            var node = new TreeNode<T>(nodeContent);
            _nodes.Add(node);
            return node;
        }

        public TreeNode<T> RemoveNode(TreeNode<T> node)
        {
            _nodes.Remove(node);
            return node;
        }

        public List<TreeNode<T>> Nodes { get
            {
                return _nodes;
            }
        }

        public void ClearNodes()
        {
            _nodes.Clear();
        }

        public TreeNode<T> AddTo(TreeNode<T> node)
        {
            node.AddNode(this);
            return this;
        }

        public override string ToString()
        {
            if (Nodes.Count == 0)
                return Content?.ToString();
            else
            {
                StringBuilder sb = new StringBuilder();
                if (Content != null)
                    sb.Append(Content.ToString());
                sb.Append(" [ ");
                foreach (var n in _nodes)
                {
                    sb.Append(n.ToString());
                    sb.Append(" ");
                }
                sb.Append("]");
                return sb.ToString();
            }
        }
    }
}
