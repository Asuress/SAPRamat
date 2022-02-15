using SAPRamat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace SAPRamat
{
    class RodsManager
    {
        private static RodsManager _instance;
        public static RodsManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new RodsManager();
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        public HashSet<int> IDs { get; set; }
        public List<Rod> Rods { get; set; }
        RodsManager()
        {
            if (Rods == null)
            {
                Rods = new List<Rod>();
            }
            if (IDs == null)
            {
                IDs = new HashSet<int>();
            }
        }
        public Rod CreateRod(int startNodeIndex, int endNodeIndex)
        {
            IDs.Add(IDs.Count);
            Rod rod = new Rod(startNodeIndex, endNodeIndex);
            Rods.Add(rod);
            return rod;
        }
        public void RemoveLastRod()
        {
            IDs.Remove(IDs.Count);
            Rods.RemoveAt(Rods.Count - 1);
        }
    }
}
