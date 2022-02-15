using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SAPRamat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly double scaleRate = 1.5;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            Binding stressBinding = new Binding()
            {
                Source = Project.Current,
                Path = new PropertyPath("MaxStress"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            //folder initialization
            if (!Directory.Exists("./saves"))
            {
                Directory.CreateDirectory("./saves");
            }
            if (!Directory.Exists("./assets"))
            {
                Directory.CreateDirectory("./assets");
            }

            stressTextBox.SetBinding(TextBox.TextProperty, stressBinding);

            LeftBase.IsChecked = true;

            InitRodsInfoRow();
            InitNodesInfoRow();

            CreateRod();
            AddRodOnNewRow();

            var nmInstance = NodesManager.Instance;

            for (int i = 0; i < 2; i++)
            {
                AddNodeOnNewRow(nmInstance.Nodes.ElementAt(i));
            }

            Binding leftBinding = new Binding()
            {
                Source = NodesManager.Instance.Nodes.First(),
                Path = new PropertyPath("IsBase"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            LeftBase.SetBinding(CheckBox.IsCheckedProperty, leftBinding);

            Binding rightBinding = new Binding()
            {
                Source = NodesManager.Instance.Nodes.Last(),
                Path = new PropertyPath("IsBase"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            RightBase.SetBinding(CheckBox.IsCheckedProperty, rightBinding);

            InitDrawings();

            Binding deltaBinding = new Binding()
            {
                Source = Project.Current,
                Path = new PropertyPath("Deltas"),
                Mode = BindingMode.OneWay
            };

            
            //listboxCalculations.SetBinding(ListBox.ItemsSourceProperty, deltaBinding);

        }

        private double drawingScale = 30;
        private void InitDrawings()
        {
            Rectangle leftBase = new Rectangle()
            {
                Fill = new ImageBrush() {
                ImageSource = new BitmapImage(new System.Uri("./assets/left base.png", System.UriKind.Relative)),
                },
                //Stroke = Brushes.Black,
                //StrokeThickness = 1,
                Width = 1 * drawingScale,
                Height = RodsManager.Instance.Rods[0].Area * 2 * drawingScale,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            Grid.SetColumn(leftBase, 0);

            Rectangle bar = new Rectangle()
            {
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Width = RodsManager.Instance.Rods[0].Length * 3 * drawingScale,
                Height = RodsManager.Instance.Rods[0].Area * drawingScale
            };
            Grid.SetColumn(bar, CanvasGrid.ColumnDefinitions.Count - 2);

            Rectangle rightBase = new Rectangle()
            {
                Fill = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new System.Uri("./assets/right base.png", System.UriKind.Relative)),
                },
                //Stroke = Brushes.Black,
                //StrokeThickness = 1,
                Width = 1 * drawingScale,
                Height = RodsManager.Instance.Rods[0].Area * 2 * drawingScale,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            Grid.SetColumn(rightBase, CanvasGrid.ColumnDefinitions.Count - 1);

            var rbBinding = new Binding()
            {
                Source = RightBase,
                Path = new PropertyPath("IsChecked"),
                Mode = BindingMode.OneWay,
                Converter = new BooleanToVisibilityConverter()
            };
            var lbBinding = new Binding()
            {
                Source = LeftBase,
                Path = new PropertyPath("IsChecked"),
                Mode = BindingMode.OneWay,
                Converter = new BooleanToVisibilityConverter()
            };

            rightBase.SetBinding(Rectangle.VisibilityProperty, rbBinding);
            leftBase.SetBinding(Rectangle.VisibilityProperty, lbBinding);

            CanvasGrid.Children.Add(leftBase);
            CanvasGrid.Children.Add(bar);
            CanvasGrid.Children.Add(rightBase);
        }

        private void AddNewRodToDrawing(double rodLength, double rodArea)
        {
            Rectangle bar = new Rectangle()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Width = rodLength * 3 * drawingScale,
                Height = rodArea * drawingScale
            };

            CanvasGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            Grid.SetColumn(bar, CanvasGrid.Children.Count - 1);
            Grid.SetColumn(CanvasGrid.Children[CanvasGrid.Children.Count - 1], CanvasGrid.Children.Count);
            CanvasGrid.Children.Insert(CanvasGrid.Children.Count - 1, bar);
        }

        private void RemoveLastRodFromDrawing()
        {
            //move right base to the left by 1 column and delete last column
            Grid.SetColumn(CanvasGrid.Children[CanvasGrid.Children.Count - 1], CanvasGrid.Children.Count - 1);
            CanvasGrid.ColumnDefinitions.RemoveAt(CanvasGrid.ColumnDefinitions.Count - 1);

            CanvasGrid.Children.RemoveAt(CanvasGrid.Children.Count - 2);
        }

        private void InitNodesInfoRow()
        {
            Grid groupBoxNodesGrid = (Grid)((ScrollViewer)groupBoxNodes.Content).Content;

            TextBlock tb = new TextBlock() { Text = "Node #", Tag = "nodeID", Margin = new Thickness(5) };
            Grid.SetRow(tb, 0);
            Grid.SetColumn(tb, 0);
            groupBoxNodesGrid.Children.Add(tb);

            tb = new TextBlock() { Text = "Is base", Tag = "isBase", Margin = new Thickness(5) };
            Grid.SetRow(tb, 0);
            Grid.SetColumn(tb, 1);
            groupBoxNodesGrid.Children.Add(tb);

            //TODO:
            // 1. Rename tag from "force" to "load"
            tb = new TextBlock() { Text = "(F)orce", Tag = "force", Margin = new Thickness(5) };
            Grid.SetRow(tb, 0);
            Grid.SetColumn(tb, 2);
            groupBoxNodesGrid.Children.Add(tb);
        }

        private void AddNodeOnNewRow(Node node)
        {
            Grid groupBoxNodesGrid = (Grid)((ScrollViewer)groupBoxNodes.Content).Content;
            groupBoxNodesGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            int rowCount = groupBoxNodesGrid.RowDefinitions.Count;

            Binding forceBinding = new Binding()
            {
                Source = NodesManager.Instance.Nodes.ElementAt(rowCount - 2),
                Path = new PropertyPath("Force"),
                Mode = BindingMode.TwoWay
            };

            Binding baseBinding = new Binding()
            {
                Source = NodesManager.Instance.Nodes.ElementAt(rowCount - 2),
                Path = new PropertyPath("IsBase"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            Binding idBinding = new Binding()
            {
                Source = NodesManager.Instance.Nodes.ElementAt(rowCount - 2),
                Path = new PropertyPath("Id"),
                Mode = BindingMode.TwoWay
            };

            TextBlock tb = new TextBlock() { Tag = "id"};
            tb.SetBinding(TextBlock.TextProperty, idBinding);
            Grid.SetRow(tb, rowCount - 1);
            Grid.SetColumn(tb, 0);
            groupBoxNodesGrid.Children.Add(tb);

            TextBlock tb1 = new TextBlock() { Tag = "isBase" };
            tb1.SetBinding(TextBlock.TextProperty, baseBinding);
            Grid.SetRow(tb1, rowCount - 1);
            Grid.SetColumn(tb1, 1);
            groupBoxNodesGrid.Children.Add(tb1);

            TextBox tb2 = new TextBox() { Tag = "force" };
            tb2.SetBinding(TextBox.TextProperty, forceBinding);
            Grid.SetRow(tb2, rowCount - 1);
            Grid.SetColumn(tb2, 2);
            groupBoxNodesGrid.Children.Add(tb2);

            ////rebind checkbox to the last element of NodesManager.Instance.Nodes
            //Binding binding = new Binding()
            //{
            //    Source = NodesManager.Instance.Nodes.Last(),
            //    Path = new PropertyPath("IsBase"),
            //    Mode = BindingMode.OneWay,
            //    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            //};
            ////need to remember state becouse after clearing bindings CheckBox forgets its state an resets to default
            //bool tmp = RightBase.IsChecked.Value;
            //BindingOperations.ClearAllBindings(RightBase);
            //RightBase.SetBinding(CheckBox.IsCheckedProperty, binding);
            //RightBase.IsChecked = tmp;
        }

        private Rod CreateRod()
        {
            var nmInstance = NodesManager.Instance;

            //Node startNode = nmInstance.CreateNode(LeftBase.IsChecked.Value);
            Node startNode;
            Node endNode;
            startNode = nmInstance.Nodes.Count > 0
                ? nmInstance.Nodes.ElementAt(nmInstance.Nodes.Count - 1)
                : nmInstance.CreateNode(LeftBase.IsChecked.Value);

            endNode = nmInstance.CreateNode(RightBase.IsChecked.Value);

            //need to remember state becouse after clearing bindings CheckBox forgets its state an resets to default
            bool checkBoxState = RightBase.IsChecked.Value;

            BindingOperations.ClearAllBindings(RightBase);

            RightBase.IsChecked = checkBoxState;

            Binding binding = new Binding()
            {
                Source = NodesManager.Instance.Nodes.Last(),
                Path = new PropertyPath("IsBase"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            RightBase.SetBinding(CheckBox.IsCheckedProperty, binding);
            var rod = Project.Current.RodsManager.CreateRod(startNode.Id, endNode.Id);
            rod.PropertyChanged += RodPropertyChanged;
            return rod;
        }

        private void RodPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Load")
            {
                Rod rod = sender as Rod;
                Rectangle rect = new Rectangle()
                {
                    Width = rod.Length * drawingScale,
                    Height = rod.Area * 2 * drawingScale,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Tag = "Load " + rod.StartNodeIndex
                };
                if (rod.Load > 0)
                {
                    rect.Fill = new ImageBrush()
                    {
                        ImageSource = new BitmapImage(new System.Uri("./assets/load to right.png", System.UriKind.Relative)),
                    };
                    //CanvasGrid.Children.Add()
                }
                else if (rod.Load < 0)
                {
                    rect.Fill = new ImageBrush()
                    {
                        ImageSource = new BitmapImage(new System.Uri("./assets/load to left.png", System.UriKind.Relative)),
                    };
                }
                else
                {
                    
                }
            }
        }

        private void InitRodsInfoRow()
        {
            Grid groupBoxRodsGrid = (Grid)((ScrollViewer)groupBoxRods.Content).Content;

            TextBlock tb = new TextBlock() { Text = "(L)ength", Tag = "length", Margin = new Thickness(5) };
            Grid.SetRow(tb, 0);
            Grid.SetColumn(tb, 1);
            groupBoxRodsGrid.Children.Add(tb);

            tb = new TextBlock() { Text = "(A)rea", Tag = "area", Margin = new Thickness(5) };
            Grid.SetRow(tb, 0);
            Grid.SetColumn(tb, 2);
            groupBoxRodsGrid.Children.Add(tb);

            tb = new TextBlock() { Text = "(E)lasticity", Tag = "elasticity", Margin = new Thickness(5) };
            Grid.SetRow(tb, 0);
            Grid.SetColumn(tb, 3);
            groupBoxRodsGrid.Children.Add(tb);

            tb = new TextBlock() { Text = "(L)oad", Tag = "load", Margin = new Thickness(5) };
            Grid.SetRow(tb, 0);
            Grid.SetColumn(tb, 4);
            groupBoxRodsGrid.Children.Add(tb);
        }

        private void UpdateRodsUI()
        {
            Grid groupBoxRodsGrid = (Grid)((ScrollViewer)groupBoxRods.Content).Content;

            while (RodsManager.Instance.Rods.Count != groupBoxRodsGrid.RowDefinitions.Count - 1)
            {
                if (RodsManager.Instance.Rods.Count > groupBoxRodsGrid.RowDefinitions.Count - 1)
                {
                    AddRodOnNewRow();
                }
                else if (RodsManager.Instance.Rods.Count < groupBoxRodsGrid.RowDefinitions.Count - 1)
                {
                    RemoveLastRodOnRow();
                }
            }

            foreach (UIElement item in groupBoxRodsGrid.Children)
            {
                if(item is TextBox tb)
                {
                    var row = Grid.GetRow(item);
                    if (RodsManager.Instance.Rods.Count > row - 1)
                    {
                        Binding lengthBinding = new Binding()
                        {
                            Source = RodsManager.Instance.Rods.ElementAt(row - 1),
                            Path = new PropertyPath("Length"),
                            Mode = BindingMode.TwoWay
                        };

                        Binding areaBinding = new Binding()
                        {
                            Source = RodsManager.Instance.Rods.ElementAt(row - 1),
                            Path = new PropertyPath("Area"),
                            Mode = BindingMode.TwoWay
                        };

                        Binding elasticityBinding = new Binding()
                        {
                            Source = RodsManager.Instance.Rods.ElementAt(row - 1),
                            Path = new PropertyPath("Elasticity"),
                            Mode = BindingMode.TwoWay
                        };

                        Binding loadBinding = new Binding()
                        {
                            Source = RodsManager.Instance.Rods.ElementAt(row - 1),
                            Path = new PropertyPath("Load"),
                            Mode = BindingMode.TwoWay
                        };

                        switch (tb.Tag)
                        {
                            case "length":
                                tb.SetBinding(TextBox.TextProperty, lengthBinding);
                                break;
                            case "area":
                                tb.SetBinding(TextBox.TextProperty, areaBinding);
                                break;
                            case "elasticity":
                                tb.SetBinding(TextBox.TextProperty, elasticityBinding);
                                break;
                            case "load":
                                tb.SetBinding(TextBox.TextProperty, loadBinding);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void UpdateNodesUI()
        {
            Grid groupBoxNodesGrid = (Grid)((ScrollViewer)groupBoxNodes.Content).Content;
            int index = groupBoxNodesGrid.RowDefinitions.Count - 1;
            while (NodesManager.Instance.Nodes.Count != groupBoxNodesGrid.RowDefinitions.Count - 1)
            {
                if (NodesManager.Instance.Nodes.Count > groupBoxNodesGrid.RowDefinitions.Count - 1)
                {
                    AddNodeOnNewRow(NodesManager.Instance.Nodes[index]);
                }
                else if (NodesManager.Instance.Nodes.Count < groupBoxNodesGrid.RowDefinitions.Count - 1)
                {
                    RemoveLastNodeOnRow();
                }
                index++;
            }

            foreach (UIElement item in groupBoxNodesGrid.Children)
            {
                int row = Grid.GetRow(item);
                if (item is TextBlock tblock && row > 0)
                {
                    Binding baseBinding = new Binding()
                    {
                        Source = NodesManager.Instance.Nodes.ElementAt(row - 1),
                        Path = new PropertyPath("IsBase"),
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };

                    Binding idBinding = new Binding()
                    {
                        Source = NodesManager.Instance.Nodes.ElementAt(row - 1),
                        Path = new PropertyPath("Id"),
                        Mode = BindingMode.TwoWay
                    };

                    switch (tblock.Tag)
                    {
                        case "id":
                            tblock.SetBinding(TextBlock.TextProperty, idBinding);
                            break;
                        case "isBase":
                            tblock.SetBinding(TextBlock.TextProperty, baseBinding);
                            break;
                        default:
                            break;
                    }
                }
                else if (item is TextBox tbox && row > 0)
                {
                    Binding forceBinding = new Binding()
                    {
                        Source = NodesManager.Instance.Nodes.ElementAt(row - 1),
                        Path = new PropertyPath("Force"),
                        Mode = BindingMode.TwoWay
                    };

                    switch (tbox.Tag)
                    {
                        case "force":
                            tbox.SetBinding(TextBox.TextProperty, forceBinding);
                            break;
                        default:
                            break;
                    }
                }
            }

            //Update binding with CheckBox
            Binding leftBinding = new Binding()
            {
                Source = NodesManager.Instance.Nodes.First(),
                Path = new PropertyPath("IsBase"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            LeftBase.SetBinding(CheckBox.IsCheckedProperty, leftBinding);

            Binding rightBinding = new Binding()
            {
                Source = NodesManager.Instance.Nodes.Last(),
                Path = new PropertyPath("IsBase"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            bool tmp = NodesManager.Instance.Nodes.Last().IsBase;
            BindingOperations.ClearAllBindings(RightBase);
            RightBase.SetBinding(CheckBox.IsCheckedProperty, rightBinding);
            RightBase.IsChecked = tmp;
        }

        private void UpdateDrawings()
        {
            //from second element til prelast cuz first and last are bases
            int i = 1;
            while(i < CanvasGrid.Children.Count - 1)
            {
                CanvasGrid.Children.RemoveAt(i);
            }
            foreach (var rod in RodsManager.Instance.Rods)
            {
                AddNewRodToDrawing(rod.Length, rod.Area);
            }
        }

        private void RemoveLastNodeOnRow()
        {
            Grid groupBoxNodesGrid = (Grid)((ScrollViewer)groupBoxNodes.Content).Content;

            List<UIElement> toDelete = new List<UIElement>();

            foreach (UIElement item in groupBoxNodesGrid.Children)
            {
                int row = Grid.GetRow(item);
                if (row == groupBoxNodesGrid.RowDefinitions.Count - 1)
                {
                    toDelete.Add(item);
                }
            }
            foreach (UIElement item in toDelete)
            {
                BindingOperations.ClearAllBindings(item);
                groupBoxNodesGrid.Children.Remove(item);
            }

            groupBoxNodesGrid.RowDefinitions.RemoveAt(groupBoxNodesGrid.RowDefinitions.Count - 1);
        }

        private void RodAddButton_Click(object sender, RoutedEventArgs e)
        {
            var tmp = CreateRod();
            AddRodOnNewRow();
            AddNodeOnNewRow(NodesManager.Instance.Nodes.Last());
            AddNewRodToDrawing(tmp.Length, tmp.Area);
            UpdateDrawings();
        }

        private void RodRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            //check that count of rods more than 0
            if (RodsManager.Instance.Rods.Count > 1)
            {
                //remove last rod and last node attached to this rod
                RodsManager.Instance.RemoveLastRod();
                NodesManager.Instance.RemoveLastNode();

                //remove last row in grid with all its content
                RemoveLastRodOnRow();
                RemoveLastNodeFromRow();
                RemoveLastRodFromDrawing();
            }
            UpdateDrawings();
        }

        private void RemoveLastRodOnRow()
        {
            Grid groupBoxRodsGrid = (Grid)((ScrollViewer)groupBoxRods.Content).Content;

            List<UIElement> toDelete = new List<UIElement>();

            foreach (UIElement item in groupBoxRodsGrid.Children)
            {
                var row = Grid.GetRow(item);
                if (row == groupBoxRodsGrid.RowDefinitions.Count - 1)
                {
                    toDelete.Add(item);
                }
            }
            foreach (var item in toDelete)
            {
                BindingOperations.ClearBinding(item, TextBox.TextProperty);
                groupBoxRodsGrid.Children.Remove(item);
            }

            groupBoxRodsGrid.RowDefinitions.RemoveAt(groupBoxRodsGrid.RowDefinitions.Count - 1);
        }

        private void AddRodOnNewRow()
        {
            Grid groupBoxRodsGrid = (Grid)((ScrollViewer)groupBoxRods.Content).Content;

            Binding lengthBinding = new Binding()
            {
                Source = RodsManager.Instance.Rods.ElementAt(groupBoxRodsGrid.RowDefinitions.Count - 1),
                Path = new PropertyPath("Length"),
                Mode = BindingMode.TwoWay
            };

            Binding areaBinding = new Binding()
            {
                Source = RodsManager.Instance.Rods.ElementAt(groupBoxRodsGrid.RowDefinitions.Count - 1),
                Path = new PropertyPath("Area"),
                Mode = BindingMode.TwoWay
            };

            Binding elasticityBinding = new Binding()
            {
                Source = RodsManager.Instance.Rods.ElementAt(groupBoxRodsGrid.RowDefinitions.Count - 1),
                Path = new PropertyPath("Elasticity"),
                Mode = BindingMode.TwoWay
            };

            Binding loadBinding = new Binding()
            {
                Source = RodsManager.Instance.Rods.ElementAt(groupBoxRodsGrid.RowDefinitions.Count - 1),
                Path = new PropertyPath("Load"),
                Mode = BindingMode.TwoWay
            };

            groupBoxRodsGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            int row = groupBoxRodsGrid.RowDefinitions.Count;

            var tb = new TextBlock() { Text = $"Rod {row - 1}", Margin = new Thickness(5) };
            Grid.SetRow(tb, row - 1);
            Grid.SetColumn(tb, 0);
            groupBoxRodsGrid.Children.Add(tb);

            var tbox = new TextBox() { Text = "1", Tag = "length", Margin = new Thickness(5) };
            tbox.SetBinding(TextBox.TextProperty, lengthBinding);
            Grid.SetRow(tbox, row - 1);
            Grid.SetColumn(tbox, 1);
            groupBoxRodsGrid.Children.Add(tbox);

            tbox = new TextBox() { Text = "1", Tag = "area", Margin = new Thickness(5) };
            tbox.SetBinding(TextBox.TextProperty, areaBinding);
            Grid.SetRow(tbox, row - 1);
            Grid.SetColumn(tbox, 2);
            groupBoxRodsGrid.Children.Add(tbox);

            tbox = new TextBox() { Text = "1", Tag = "elasticity", Margin = new Thickness(5) };
            tbox.SetBinding(TextBox.TextProperty, elasticityBinding);
            Grid.SetRow(tbox, row - 1);
            Grid.SetColumn(tbox, 3);
            groupBoxRodsGrid.Children.Add(tbox);

            tbox = new TextBox() { Text = "1", Tag = "load", Margin = new Thickness(5) };
            tbox.SetBinding(TextBox.TextProperty, loadBinding);
            Grid.SetRow(tbox, row - 1);
            Grid.SetColumn(tbox, 4);
            groupBoxRodsGrid.Children.Add(tbox);
        }

        private void RemoveLastNodeFromRow()
        {
            Grid groupBoxNodesGrid = (Grid)((ScrollViewer)groupBoxNodes.Content).Content;

            List<UIElement> toDelete = new List<UIElement>();

            foreach (UIElement item in groupBoxNodesGrid.Children)
            {
                if (Grid.GetRow(item) == groupBoxNodesGrid.RowDefinitions.Count - 1)
                {
                    toDelete.Add(item);
                }
            }
            foreach (var item in toDelete)
            {
                groupBoxNodesGrid.Children.Remove(item);
            }

            groupBoxNodesGrid.RowDefinitions.RemoveAt(groupBoxNodesGrid.RowDefinitions.Count - 1);
        }

        private void NodeAddButton_Click(object sender, RoutedEventArgs e)
        {
            var node = NodesManager.Instance.CreateNode(RightBase.IsChecked.HasValue ? RightBase.IsChecked.Value : false);
            AddNodeOnNewRow(node);
        }

        private void NodeRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveLastNodeFromRow();
            NodesManager.Instance.RemoveLastNode();
        }

        private void LeftBase_Checked(object sender, RoutedEventArgs e)
        {
            if (NodesManager.Instance.Nodes.Count > 0)
            {
                NodesManager.Instance.Nodes.ElementAt(0).IsBase = true;
            }
        }

        private void LeftBase_Unchecked(object sender, RoutedEventArgs e)
        {
            if(RightBase.IsChecked != true)
            {
                LeftBase.IsChecked = true;
            }
            else
            {
                NodesManager.Instance.Nodes.ElementAt(0).IsBase = false;
            }
        }

        private void RightBase_Checked(object sender, RoutedEventArgs e)
        {
            NodesManager.Instance.Nodes.ElementAt(NodesManager.Instance.Nodes.Count - 1).IsBase = true;
        }

        private void RightBase_Unchecked(object sender, RoutedEventArgs e)
        {
            if (LeftBase.IsChecked != true)
            {
                RightBase.IsChecked = true;
            }
            else
            {
                NodesManager.Instance.Nodes.ElementAt(NodesManager.Instance.Nodes.Count - 1).IsBase = false;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSystem.Save();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSystem.Load();
            UpdateRodsUI();
            UpdateNodesUI();
            UpdateDrawings();
        }

        private void ButtonCalculateClick(object sender, RoutedEventArgs e)
        {
            Project.Current.Calculate();
            double[] a = Project.Current.Deltas;
            listboxCalculationsDeltas.ItemsSource = new List<double>(Project.Current.Deltas);
            listboxCalculationsForces.ItemsSource = Project.Current.NormalForces;
            listboxCalculationsStresses.ItemsSource = Project.Current.Stresses;

            DrawGraphics();
        }

        private void DrawGraphics()
        {
            //DrawMovementGraphic();
            DrawNGraphics();
            DrawSigmaGraphics();
        }

        private void DrawSigmaGraphics()
        {

        }

        private void DrawNGraphics()
        {

        }

        Canvas CurvesCanvas = new Canvas();
        Polyline pl = new Polyline()
        {
            Stroke = Brushes.Red,
            StrokeThickness = 1,
        };
        Line zeroLine = new Line()
        {
            Stroke = Brushes.Black,
            StrokeThickness = 0.5,
        };
        //void DrawMovementGraphic()
        //{
        //    if (CanvasGrid.RowDefinitions.Count < 4)
        //    {
        //        while (CanvasGrid.RowDefinitions.Count != 2)
        //        {
        //            CanvasGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        //        }
        //    }

        //    pl.Points.Clear();

        //    Grid.SetRow(CurvesCanvas, 1);
        //    Grid.SetColumn(CurvesCanvas, 1);
        //    Grid.SetColumnSpan(CurvesCanvas, CanvasGrid.ColumnDefinitions.Count);

        //    try
        //    {
        //        CanvasGrid.Children.Add(CurvesCanvas);
        //    }
        //    catch (System.Exception)
        //    {
        //    }

        //    try
        //    {
        //        CurvesCanvas.Children.Add(pl);
        //    }
        //    catch (System.Exception)
        //    {
        //    }

        //    Project.Current.CalculationStep = 0.1d;
        //    double[,] arr = Project.Current.GetMovementCalculations();
        //    bool isBeginOfRod;
        //    int rodIndex = -1;
        //    double additionalLength = 0;
        //    double maxYVal = arr[0, 1];

        //    for (int i = 0; i < arr.Length / 2; i++)
        //    {
        //        maxYVal = arr[i, 1] > maxYVal ? arr[i, 1] : maxYVal;
        //    }

        //    for (int i = 0; i < arr.Length / 2; i++)
        //    {
        //        isBeginOfRod = arr[i, 0] == 0;

        //        if (isBeginOfRod)
        //        {
        //            rodIndex++;
        //            additionalLength += rodIndex > 0 ? Project.Current.RodsManager.Rods[rodIndex - 1].Length : 0;
        //        }

        //        pl.Points.Add(new Point((additionalLength + arr[i, 0]) * drawingScale * 3, -(arr[i, 1] - maxYVal * 1.2) * drawingScale));
        //    }

        //    additionalLength += Project.Current.RodsManager.Rods[rodIndex - 1].Length;

        //    zeroLine.X1 = 0;
        //    zeroLine.Y1 = maxYVal * drawingScale * 1.2;
        //    zeroLine.X2 = additionalLength * 3 * drawingScale;
        //    zeroLine.Y2 = maxYVal * drawingScale * 1.2;

        //    try
        //    {
        //        CurvesCanvas.Children.Add(zeroLine);
        //    }
        //    catch (System.Exception)
        //    {
        //    }
        //}

        private void DrawingMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //LayoutScaleTransform.CenterX = e.GetPosition(CanvasGrid).X;
            //LayoutScaleTransform.CenterY = e.GetPosition(CanvasGrid).Y;

            if (e.Delta > 0)
            {
                LayoutScaleTransform.ScaleX *= scaleRate;
                LayoutScaleTransform.ScaleY *= scaleRate;
            }
            else
            {
                LayoutScaleTransform.ScaleX /= scaleRate;
                LayoutScaleTransform.ScaleY /= scaleRate;
            }
        }
    }
}
