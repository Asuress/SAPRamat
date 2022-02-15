using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Numerics;
using SAPRamat;
using System.ComponentModel;

namespace SAPRamat
{
    public class Rod : INotifyPropertyChanged
    {
        private double _area;
        private double _length;
        private double _elasticity;
        private double _force;
        private double _load;

        public event PropertyChangedEventHandler PropertyChanged;

        public double Area
        {
            get => _area;
            set
            {
                if (!(value <= 0))
                {
                    _area = value;
                }
                else
                {
                    _area = 1;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Area"));
            }
        }
        public double Length
        {
            get => _length;
            set
            {
                if (value > 0)
                {
                    _length = value;
                }
                else
                {
                    _length = 1;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Length"));
            }
        }
        public double Elasticity
        {
            get => _elasticity;
            set
            {
                if (value > 0)
                {
                    _elasticity = value;
                }
                else
                {
                    _elasticity = 1;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Elasticity"));
            }
        }
        public double Force
        {
            get => _load * _length;
            set
            {
                _force = _load * _length;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Force"));
            }
        }
        public double Load
        {
            get => _load;
            set
            {
                _load = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Load"));
            }
        }
        public int StartNodeIndex { get; set; }
        public int EndNodeIndex { get; set; }
        private Rod()
        {
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            Area = 1;
            Length = 1;
            Elasticity = 1;
            Force = 0;
            Load = 0;
        }

        public Rod(Rod rod, int startNode, int endNode)
        {
            SetDefaultValues();
            Area = rod.Area;
            Length = rod.Length;
            Elasticity = rod.Elasticity;
            Load = rod.Load;
            StartNodeIndex = startNode;
            EndNodeIndex = endNode;
        }
        public Rod(int startNode, int endNode)
        {
            SetDefaultValues();
            StartNodeIndex = startNode;
            EndNodeIndex = endNode;
        }
    }
}
