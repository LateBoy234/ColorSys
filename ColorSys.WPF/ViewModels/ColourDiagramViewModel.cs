using CommunityToolkit.Mvvm.ComponentModel;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.WPF.ViewModels
{
   public partial  class ColourDiagramViewModel:ObservableObject
    {
        public ColourDiagramViewModel()
        {
            XyAxisModel = CreateQuadrantPlot();
        }

        [ObservableProperty]
        private PlotModel _xyAxisModel;

        public PlotModel CreateQuadrantPlot()
        {
            var plotModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotAreaBorderColor = OxyColors.Transparent
            };

            var xAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
               
                Minimum = -100,
                Maximum = 100,
                MajorStep=20,
                // 关键：让轴在0点交叉
                PositionAtZeroCrossing = true, 
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColors.Black,
                // 刻度朝外，不跨过轴线
                TickStyle = TickStyle.Outside,
                MajorTickSize = 5,

                // 关闭网格线和次刻度
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0
            };
            var yAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                Minimum = -100,
                Maximum = 100,
                MajorStep = 20,  // 刻度间隔20

                PositionAtZeroCrossing = true,

                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColors.Black,
                AxislineThickness = 1,

                TickStyle = TickStyle.Outside,
                MajorTickSize = 5,

                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0
            };

           plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            // 水平分隔线 Y=0
            var horizontalLine = new LineAnnotation
            {
                Type = LineAnnotationType.Horizontal,
                Y = 0,
                Color = OxyColors.Black,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 1,
                // 限制在坐标轴范围内
                MinimumX = -100,
                MaximumX = 100,
                Layer = AnnotationLayer.BelowSeries
            };
            plotModel.Annotations.Add(horizontalLine);

            // 垂直分隔线 X=0
            var verticalLine = new LineAnnotation
            {
                Type = LineAnnotationType.Vertical,
                X = 0,
                Color = OxyColors.Black,
                LineStyle = LineStyle.Solid,
               
                MinimumY = -100,
                MaximumY = 100,
                
                StrokeThickness = 1,
                Layer = AnnotationLayer.BelowSeries  // 确保线在数据点下方
            };
            plotModel.Annotations.Add(verticalLine);

            // 添加图例
            plotModel.IsLegendVisible = true;
            //plotModel.legen = LegendPosition.RightTop;
            //plotModel.LegendOrientation = LegendOrientation.Vertical;
            return plotModel;
        }
    }
}
