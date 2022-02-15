using System;
using System.Collections.Generic;
using System.Text;

namespace SAPRamat
{
    class NodesManager
    {
        private static NodesManager _instance;
        public static NodesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NodesManager();
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        NodesManager()
        {
            if (IDs == null)
                IDs = new HashSet<int>();
            if (Nodes == null)
                Nodes = new List<Node>();
        }
        public HashSet<int> IDs { get; set; }
        public List<Node> Nodes { get; set; }
        public Node CreateNode(bool isBase)
        {
            int newId = IDs.Count;
            IDs.Add(newId);
            var node = new Node(isBase, newId);
            Nodes.Add(node);
            if (Nodes.Count > 2)
            {
                Nodes[Nodes.Count - 2].IsBase = false;
            }
            return node;
        }
        public void RemoveLastNode()
        {
            int index = Nodes.Count - 1;
            Nodes[index - 1].IsBase = Nodes[index].IsBase;
            Nodes.RemoveAt(index);
            IDs.Remove(IDs.Count - 1);
        }
    }
}
