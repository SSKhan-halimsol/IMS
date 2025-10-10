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
        private PlotModel _barChartModel;
        public PlotModel BarChartModel
        {
            get { return _barChartModel; }
            set { _barChartModel = value; OnPropertyChanged(nameof(BarChartModel)); }
        }

        private PlotModel _pieChartModel;
        public PlotModel PieChartModel
        {
            get { return _pieChartModel; }
            set { _pieChartModel = value; OnPropertyChanged("PieChartModel"); }
        }

        private PlotModel _applicantVolumeChartModel;
        public PlotModel ApplicantVolumeChartModel
        {
            get { return _applicantVolumeChartModel; }
            set { _applicantVolumeChartModel = value; OnPropertyChanged("ApplicantVolumeChartModel"); }
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
                    BuildApplicantVolumeChart(applicants);
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
                var model = new PlotModel
                {
                    Title = "",
                    Background = OxyColors.Transparent,
                    PlotAreaBorderColor = OxyColor.FromRgb(30, 41, 59),
                    TextColor = OxyColor.FromRgb(203, 213, 225),
                    TitleColor = OxyColors.White,
                    PlotAreaBorderThickness = new OxyThickness(1)
                };

                var today = DateTime.Today;
                var designations = new[] { "QA Engineer", ".NET Developer", "Angular Developer", "PHP Developer" };

                var designationAxis = new CategoryAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Designation",
                    TextColor = OxyColor.FromRgb(148, 163, 184),
                    TitleColor = OxyColor.FromRgb(203, 213, 225),
                    AxislineColor = OxyColor.FromRgb(51, 65, 85),
                    TicklineColor = OxyColor.FromRgb(51, 65, 85),
                    MajorGridlineStyle = LineStyle.Dot,
                    MajorGridlineColor = OxyColor.FromRgb(30, 41, 59),
                    FontSize = 11
                };

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
                    AbsoluteMinimum = 0,
                    TextColor = OxyColor.FromRgb(148, 163, 184),
                    TitleColor = OxyColor.FromRgb(203, 213, 225),
                    AxislineColor = OxyColor.FromRgb(51, 65, 85),
                    TicklineColor = OxyColor.FromRgb(51, 65, 85),
                    MajorGridlineStyle = LineStyle.Dot,
                    MajorGridlineColor = OxyColor.FromRgb(30, 41, 59),
                    FontSize = 11
                };
                model.Axes.Add(countAxis);

                var barSeries = new BarSeries
                {
                    Title = "Applicants Today",
                    FillColor = OxyColor.FromRgb(16, 185, 129), // Green gradient color
                    StrokeColor = OxyColor.FromRgb(5, 150, 105),
                    StrokeThickness = 2
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
                BarChartModel = model;
            }
        }

        private void BuildPieChart(IEnumerable<Applicant> applicants)
        {
            var model = new PlotModel
            {
                Title = "",
                Background = OxyColors.Transparent,
                TextColor = OxyColor.FromRgb(203, 213, 225),
                TitleColor = OxyColors.White,
                PlotAreaBorderThickness = new OxyThickness(0)
            };

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

            // Modern color palette for pie chart
            var colors = new[]
            {
                OxyColor.FromRgb(16, 185, 129),   // Green
                OxyColor.FromRgb(59, 130, 246),   // Blue
                OxyColor.FromRgb(139, 92, 246),   // Purple
                OxyColor.FromRgb(245, 158, 11),   // Orange
                OxyColor.FromRgb(239, 68, 68),    // Red
                OxyColor.FromRgb(236, 72, 153)    // Pink
            };

            var pieSeries = new PieSeries
            {
                StrokeThickness = 3.0,
                Stroke = OxyColor.FromRgb(20, 31, 58),
                InsideLabelPosition = 0.7,
                AngleSpan = 360,
                StartAngle = 0,
                InsideLabelColor = OxyColors.White,
                OutsideLabelFormat = "{1}: {2:0}",
                TextColor = OxyColor.FromRgb(203, 213, 225),
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };

            int colorIndex = 0;
            foreach (var g in grouped)
            {
                pieSeries.Slices.Add(new PieSlice(g.Month, g.Count)
                {
                    Fill = colors[colorIndex % colors.Length],
                    IsExploded = false
                });
                colorIndex++;
            }

            model.Series.Add(pieSeries);
            PieChartModel = model;
        }

        private void BuildApplicantVolumeChart(IEnumerable<Applicant> applicants)
        {
            using (var context = new IMSDbContext())
            {
                applicants = context.Applicants.ToList();

                var model = new PlotModel
                {
                    Title = "",
                    Background = OxyColors.Transparent,
                    PlotAreaBorderColor = OxyColor.FromRgb(30, 41, 59),
                    TextColor = OxyColor.FromRgb(203, 213, 225),
                    TitleColor = OxyColors.White,
                    PlotAreaBorderThickness = new OxyThickness(1)
                };

                var designations = new[] { "QA Engineer", ".NET Developer", "Angular Developer", "PHP Developer" };

                // Y-axis (Designation)
                var designationAxis = new CategoryAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Designation",
                    TextColor = OxyColor.FromRgb(148, 163, 184),
                    TitleColor = OxyColor.FromRgb(203, 213, 225),
                    AxislineColor = OxyColor.FromRgb(51, 65, 85),
                    TicklineColor = OxyColor.FromRgb(51, 65, 85),
                    MajorGridlineStyle = LineStyle.Dot,
                    MajorGridlineColor = OxyColor.FromRgb(30, 41, 59),
                    FontSize = 11
                };

                foreach (var d in designations)
                    designationAxis.Labels.Add(d);
                model.Axes.Add(designationAxis);

                // X-axis (Count)
                var countAxis = new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "Total Applicants",
                    Minimum = 0,
                    MajorStep = 1,
                    MinorStep = 1,
                    AbsoluteMinimum = 0,
                    TextColor = OxyColor.FromRgb(148, 163, 184),
                    TitleColor = OxyColor.FromRgb(203, 213, 225),
                    AxislineColor = OxyColor.FromRgb(51, 65, 85),
                    TicklineColor = OxyColor.FromRgb(51, 65, 85),
                    MajorGridlineStyle = LineStyle.Dot,
                    MajorGridlineColor = OxyColor.FromRgb(30, 41, 59),
                    FontSize = 11
                };
                model.Axes.Add(countAxis);

                // Create bar series with gradient effect
                var barSeries = new BarSeries
                {
                    Title = "Applicants",
                    FillColor = OxyColor.FromRgb(16, 185, 129),
                    StrokeColor = OxyColor.FromRgb(5, 150, 105),
                    StrokeThickness = 2
                };

                // Count total applicants for each designation
                foreach (var designation in designations)
                {
                    int count = applicants.Count(a =>
                        string.Equals(a.AppliedFor, designation, StringComparison.OrdinalIgnoreCase));

                    barSeries.Items.Add(new BarItem(count));
                }

                model.Series.Add(barSeries);
                ApplicantVolumeChartModel = model;
            }
        }

        public void RefreshCharts()
        {
            LoadCharts();
        }
    }
}