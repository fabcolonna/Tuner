using Microsoft.UI.Xaml;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TunerWinUI.AudioVisualizers
{
    public class RTPlotter : DependencyObject, IVisualizer, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public PlotModel Model { get; private set; }

        public string Title { get; set; } = "";

        public Axis XAxis { get; set; } = new LinearAxis { IsAxisVisible = false };

        public Axis YAxis { get; set; } = new LinearAxis { IsAxisVisible = false };

        public ObservableCollection<DataPoint> Data { get; } = new();

        public RTPlotter()
        {
            Model = new()
            {
                Title = Title,
                Axes = { XAxis, YAxis },
                Series =
                {
                    new LineSeries()
                    {
                        MarkerType = MarkerType.None,
                        Points =
                        {
                            new DataPoint(0, 0),
                            new DataPoint(10, 18),
                            new DataPoint(20, 12),
                            new DataPoint(30, 8),
                            new DataPoint(40, 15),
                        },
                        Selectable = false,
                    }
                }
            };
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
