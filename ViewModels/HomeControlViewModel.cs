using IMS.Data;
using IMS.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IMS.ViewModels
{
    public class HomeControlViewModel : BaseViewModel
    {
        private PlotModel _lineChartModel;
        public PlotModel LineChartModel
        {
            get { return _lineChartModel; }
            set { _lineChartModel = value; OnPropertyChanged("LineChartModel"); }
        }

        private PlotModel _pieChartModel;
        public PlotModel PieChartModel
        {
            get { return _pieChartModel; }
            set { _pieChartModel = value; OnPropertyChanged("PieChartModel"); }
        }

        public HomeControlViewModel()
        {
            LoadCharts();
        }

        private async void LoadCharts()
        {
            try
            {
                var applicants = await ApplicantRepository.GetAllAsync();
                if (applicants != null && applicants.Any())
                {
                    BuildApplicantsBarChart(applicants);
                    BuildPieChart(applicants);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No applicants found.");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading charts: " + ex.Message);
            }
        }


        private void BuildApplicantsBarChart(IEnumerable<Applicant> applicants)
        {
            using (var context = new IMSDbContext())
            {
                applicants = context.Applicants.ToList();
                var model = new PlotModel { Title = "" };

                var today = DateTime.Today;
                var designations = new[] { "QA Engineer", ".NET Developer", "Angular Developer", "PHP Developer" };

                var designationAxis = new CategoryAxis { Position = AxisPosition.Left, Title = "Designation" };
                foreach (var d in designations)
                    designationAxis.Labels.Add(d);
                model.Axes.Add(designationAxis);

                var countAxis = new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "Applicants",
                    Minimum = 0,
                    MajorStep = 1,
                    MinorStep = 1,
                    AbsoluteMinimum = 0
                };
                model.Axes.Add(countAxis);

                // Bar series
                var barSeries = new BarSeries
                {
                    Title = "Applicants Today",
                    FillColor = OxyColors.LimeGreen,
                };

                foreach (var designation in designations)
                {
                    int count = applicants.Count(a =>
                        a.Date.HasValue &&
                        a.Date.Value.Date == today &&
                        string.Equals(a.AppliedFor, designation, StringComparison.OrdinalIgnoreCase));

                    barSeries.Items.Add(new BarItem(count));
                }

                model.Series.Add(barSeries);
                LineChartModel = model; // binding property
            }
        }


        private void BuildPieChart(IEnumerable<Applicant> applicants)
        {
            var model = new PlotModel { Title = "" };

            var today = DateTime.Today;
            var startMonth = today.AddMonths(-6);

            var grouped = applicants
                .Where(a => a.Date.HasValue && a.Date.Value >= startMonth)
                .GroupBy(a => new { a.Date.Value.Year, a.Date.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                    Count = g.Count()
                });

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1.0,
                InsideLabelPosition = 0.8,
                AngleSpan = 360,
                StartAngle = 0
            };

            foreach (var g in grouped)
            {
                pieSeries.Slices.Add(new PieSlice(g.Month, g.Count));
            }

            model.Series.Add(pieSeries);
            PieChartModel = model;
        }


        public void RefreshCharts()
        {
            LoadCharts();
        }
    }
}
