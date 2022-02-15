using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SAPRamat
{
    class Project : INotifyPropertyChanged
    {
        public NodesManager NodesManager { get; set; }
        public RodsManager RodsManager { get; set; }
        public double MaxStress { get => _maxStress;
            set
            {
                _maxStress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxStress"));
            }
        }
        public double[] Deltas
        {
            get => _deltas;
            private set
            {
                _deltas = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Deltas"));
            }
        }
        public double[] DeltasPoints { get; private set; }
        public double[] NormalForces { get; private set; }
        public double[] NormalForcesPoints { get; private set; }
        public double[] Stresses { get; private set; }
        public double[] StressesPoints { get; private set; }
        public double CalculationStep
        {
            get => _calculationStep;
            set
            {
                if(value > 0)
                {
                    _calculationStep = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CalculationStep"));
                }
                _calculationStep = 0.1;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CalculationStep"));
            }
        }
        private static Project _current;
        private double _maxStress;
        private double[] _deltas;
        private double _calculationStep;

        public event PropertyChangedEventHandler PropertyChanged;

        public static Project Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new Project();
                }
                return _current;
            }
            set
            {
                _current = value;
                NodesManager.Instance = _current.NodesManager;
                RodsManager.Instance = _current.RodsManager;
            }
        }
        public Project()
        {
            NodesManager = NodesManager.Instance;
            RodsManager = RodsManager.Instance;
        }
        private double[,] GenerateExtendedMatrix()
        {
            int n = NodesManager.Nodes.Count;

            double[,] A_matrix = new double[n, n];
            double[,] b_matrix = new double[n, 1];
            double[,] extended_matrix = new double[n, n + 1];

            //generate A matrix
            //rod index
            int r = 0;
            for (int i = 0; i < n; i++)
            {
                for(int j = 0; j < n; j++)
                {
                    //diagonal calclulations
                    if (i == j)
                    {
                        //"central" diagonal
                        if (i != n - 1 && i != 0)
                        {
                            var rod = RodsManager.Rods[r];
                            var rod2 = RodsManager.Rods[r + 1];
                            A_matrix[i, j] = (rod.Elasticity * rod.Area / rod.Length) + (rod2.Elasticity * rod2.Area / rod2.Length);
                            r++;
                        }
                        //two corner values aka right bottom and top left values
                        else
                        {
                            var rod = RodsManager.Rods[r];
                            if (NodesManager.Nodes[rod.EndNodeIndex].IsBase || NodesManager.Nodes[rod.StartNodeIndex].IsBase)
                            {
                                A_matrix[i, j] = 1;
                            }
                            else
                            {
                                A_matrix[i, j] = rod.Elasticity * rod.Area / rod.Length;
                            }
                        }
                    }
                    //values along diagonal
                    else if (System.Math.Abs(j-i) <= 1)
                    {
                        var rod = RodsManager.Rods[r];
                        if (NodesManager.Nodes[rod.StartNodeIndex].IsBase || NodesManager.Nodes[rod.EndNodeIndex].IsBase)
                        {
                            A_matrix[i, j] = 0;
                        }
                        else
                        {
                            A_matrix[i, j] = -rod.Elasticity * rod.Area / rod.Length;
                        }
                    }
                    //any else values
                    else
                    {
                        A_matrix[i, j] = 0;
                    }
                }
            }

            //generate b matrix
            for (int i = 0; i < n; i++)
            {
                if (NodesManager.Nodes[i].IsBase)
                {
                    b_matrix[i, 0] = 0;
                }
                else
                {
                    if (i > 0 && i != n - 1)
                    {
                        var leftRod = RodsManager.Rods[i - 1];
                        var rightRod = RodsManager.Rods[i];
                        b_matrix[i, 0] = leftRod.Force / 2 + rightRod.Force / 2 + NodesManager.Nodes[i].Force;
                    }
                    else if (i == n - 1)
                    {
                        var leftRod = RodsManager.Rods[i - 1];
                        b_matrix[i, 0] = leftRod.Force / 2 + NodesManager.Nodes[i].Force;
                    }
                    else
                    {
                        var rightRod = RodsManager.Rods[i];
                        b_matrix[i, 0] = rightRod.Force / 2 + NodesManager.Nodes[i].Force;
                    }
                }
            }

            //generate extended matrix
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n + 1; j++)
                {
                    if (j == n)
                    {
                        extended_matrix[i, j] = b_matrix[i, 0];
                    }
                    else
                    {
                        extended_matrix[i, j] = A_matrix[i, j];
                    }
                }
            }

            return extended_matrix;
        }

        public bool Calculate()
        {
            Deltas = Math.Gauss(GenerateExtendedMatrix());
            NormalForces = CalculateNormalForces();
            Stresses = CalculateStresses();
            CalculatePoints();

            //
            //double tmp = CalculateMovementInAPoint(RodsManager.Rods[RodsManager.Rods.Count - 1].Length, RodsManager.Rods.Count - 1);

            //
            return true;
        }

        private void CalculatePoints()
        {
            //double i = 0;
            //double summaryLength = 0;
            //foreach (var rod in RodsManager.Rods)
            //{
            //    summaryLength += rod.Length;
            //}
            //double lengthOffset = 0;
            //int index = 0;
            //while (i < summaryLength)
            //{

            //    i += CalculationStep;
            //}
        }

        private double[] CalculateStresses()
        {
            double[] result = new double[RodsManager.Rods.Count * 2];
            int i = 0; //index of rod
            int j = 0; //index of rod's side (2 for each rod)
            foreach (Rod rod in RodsManager.Rods)
            {
                result[j] = CalculateStressInAPoint(0, i);
                result[j + 1] = CalculateStressInAPoint(rod.Length, i);
                j += 2;
                i++;
            }
            return result;
        }

        public double CalculateStressInAPoint(double point, int rodId)
        {
            double result = 0;
            foreach (var index in NodesManager.IDs)
            {
                result = CalculateNormalForceInAPoint(point, rodId) / RodsManager.Rods[rodId].Area;
            }
            return result;
        }

        private double[] CalculateNormalForces()
        {
            double[] result = new double[RodsManager.Rods.Count * 2];
            int i = 0; //index of rod
            int j = 0; //index of rod's side (2 for each rod)
            foreach (Rod rod in RodsManager.Rods)
            {
                result[j] = CalculateNormalForceInAPoint(0, i);
                result[j + 1] = CalculateNormalForceInAPoint(rod.Length, i);
                j += 2;
                i++;
            }
            return result;
        }

        public double CalculateNormalForceInAPoint(double point, int rodId)
        {
            double result;
            var rod = RodsManager.Rods[rodId];
            result = (-rod.Elasticity * rod.Area * Deltas[rodId] / rod.Length) +
                (rod.Elasticity * rod.Area * Deltas[rodId + 1] / rod.Length) +
                (rod.Force * (1 - (2 * point / rod.Length)) / 2);
            return result;
        }

        public double CalculateMovementInAPoint(double point, int rodId)
        {
            double result;
            var rod = RodsManager.Rods[rodId];
            result = ((1 - (point / rod.Length)) * Deltas[rodId]) +
                (point * Deltas[rodId + 1] / rod.Length) +
                (rod.Force * point / (2 * rod.Elasticity * rod.Area) * (1 - point / rod.Length));
            return result;
        }

        public double[,] GetMovementCalculations()
        {
            //double[,] result = new double[0,0];

            List<double> points = new List<double>();
            List<double> calculations = new List<double>();

            foreach (var rod in RodsManager.Rods)
            {
                double p = 0;
                while (p <= rod.Length)
                {
                    points.Add(p);
                    calculations.Add(CalculateMovementInAPoint(p, rod.StartNodeIndex));
                    p += CalculationStep;
                }
            }
            var result = new double[points.Count, 2];
            for (int i = 0; i < result.Length / 2; i++)
            {
                result[i, 0] = points[i];
                result[i, 1] = calculations[i];
            }
            return result;
        }
    }
}
