using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.ComponentModel;

namespace SAPRamat
{
    public class Node : INotifyPropertyChanged
    {
        private bool _isBase;
        private int _id;
        private double _force;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsBase
        {
            get => _isBase;
            set
            {
                _isBase = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsBase"));
            }
        }
        public int Id
        {
            get => _id;
            set
            {
                if (value >= 0)
                {
                    _id = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Id"));
                }
            }
        }
        public double Force
        {
            get => _force;
            set
            {
                _force = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Force"));
            }
        }
        public Node()
        {
            setDefaultValues();
        }
        public Node(bool isBase, int id)
        {
            setDefaultValues();
            Id = id;
            IsBase = isBase;
        }
        private void setDefaultValues()
        {
            IsBase = false;
            Force = 0;
        }
        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
