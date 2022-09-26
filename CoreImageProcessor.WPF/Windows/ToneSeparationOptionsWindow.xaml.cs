using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CoreImageProcessor.Windows
{
    public class Interval
    {
        public Interval(byte from, byte to, byte value)
        {
            From = from;
            To = to;
            Value = value;
        }

        public byte From
        {
            get;
            set;
        }

        public byte To
        {
            get;
            set;
        }

        public byte Value
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interaktionslogik für ToneSeparationDialog.xaml
    /// </summary>
    public partial class ToneSeparationOptionsWindow : Window
    {
        public ToneSeparationOptionsWindow()
        {
            DefaultValue = 0;

            InitializeComponent();
            DataContext = this;

            Intervals = new ObservableCollection<Interval>();
        }

        public ObservableCollection<Interval> Intervals
        {
            get;
        }

        public byte DefaultValue
        {
            get;
            set;
        }

        public byte[] CalculateLookupTable()
        {
            byte[] lookupTable = new byte[256];

            //This is not the most efficient method, because the collection is sorted, which means that values are 
            //asigned twice for some indices, but it is the best maintainable and least error prune solution for the future.
            
            for (int i = 0; i < lookupTable.Length; ++i)
            {
                lookupTable[i] = DefaultValue;
            }

            foreach (Interval interval in Intervals)
            {
                for (int i = interval.From; i <= interval.To; ++i)
                {
                    lookupTable[i] = interval.Value;
                }
            }

            return lookupTable;
        }

        private void Clear()
        {
            inputFrom.Text = string.Empty;
            inputTo.Text = string.Empty;
            inputValue.Text = string.Empty;
        }

        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            if (byte.TryParse(inputFrom.Text, out byte from))
            {
                if (byte.TryParse(inputTo.Text, out byte to))
                {
                    if (from <= to)
                    {
                        if (byte.TryParse(inputTo.Text, out byte value))
                        {
                            if (Intervals.Count == 0 || Intervals[Intervals.Count-1].To < from)
                            {
                                Intervals.Add(new Interval(from, to, value));
                                Clear();
                            }
                            else
                            {
                                int insertIndex;
                                short lastTo = -1;
                                short nextFrom = 256;

                                for (insertIndex = 0; insertIndex < Intervals.Count; ++insertIndex)
                                {
                                    if (Intervals[insertIndex].From > from)
                                    {
                                        nextFrom = Intervals[insertIndex].From;
                                        break;
                                    }

                                    lastTo = Intervals[insertIndex].To;
                                }

                                if (from > lastTo && to < nextFrom)
                                {
                                    Intervals.Insert(insertIndex, new Interval(from, to, value));
                                    Clear();
                                }
                                else
                                {
                                    MessageBox.Show("The new interval overlaps with one or more previous defined interval/s.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("The 'value' field does not contain an valid byte value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("The 'from' field value has to be lower or equal to the 'to' fields value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("The 'to' field does not contain an valid byte value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("The 'from' field does not contain an valid byte value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
