using System.Linq;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace _122_Zyamilov_Chzhen.Pages
{
    public partial class DiagrammPage : Page
    {
        public DiagrammPage()
        {
            InitializeComponent();
            LoadChart();
        }

        private void LoadChart()
        {
            var payments = Entities.GetContext().Payment.ToList();
            var series = new SeriesCollection();

            var grouped = payments.GroupBy(p => p.Category.Name);
            foreach (var group in grouped)
            {
                series.Add(new PieSeries
                {
                    Title = group.Key,
                    Values = new ChartValues<double> { group.Sum(p => (double)p.Price) },
                    DataLabels = true
                });
            }

            PieChart.Series = series;
        }
    }
}