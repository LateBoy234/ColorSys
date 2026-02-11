using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace ColorSys.WPF.ViewModels
{
    public partial class ColourDiagramViewModel : ObservableObject, IRecipient<LabValueMode>
    {
        public ColourDiagramViewModel()
        {
            XyAxisModel = CreateQuadrantPlot();
            ZAxisModel = CreateBrightnessPlot();
            AddColorData(); // 添加颜色数据
            InitializeSampleSeries(); // 初始化样品数据系列
           // 注册消息接收
            WeakReferenceMessenger.Default.Register(this);
        }

        [ObservableProperty]
        private PlotModel _xyAxisModel;
        [ObservableProperty]
        private PlotModel _zAxisModel;

        // 样品数据系列
        private ScatterSeries StandardSampleSeries;
        private ScatterSeries TestSampleSeries;

        private ScatterSeries StandardLSampleSeries;
        private ScatterSeries TestLSampleSeries;

        #region ab颜色空间图

        /// <summary>
        /// 创建一个以原点为中心，分为四个象限的 ab 颜色空间图。X轴表示 a* 值，Y轴表示 b* 值。图中包含水平和垂直的分隔线，将图分成四个象限，分别代表不同的颜色区域。
        /// </summary>
        /// <returns></returns>
        public PlotModel CreateQuadrantPlot()
        {
            var plotModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotAreaBorderColor = OxyColors.Transparent
            };

            // 系列1：标样（蓝色菱形）
            var standardSeries = new ScatterSeries
            {
                Title = "标样",                    // ← 图例显示的标题
                MarkerType = MarkerType.Diamond,
                MarkerFill = OxyColors.Red,
                MarkerSize = 8
            };

            // 系列2：试样（绿色圆点）
            var sampleSeries = new ScatterSeries
            {
                Title = "试样",                    // ← 图例显示的标题
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Yellow,
                MarkerSize = 8
            };

            plotModel.Series.Add(standardSeries);
            plotModel.Series.Add(sampleSeries);

            // 添加图例 - 显示在图表顶部中央
            plotModel.Legends.Add(new Legend
            {
                LegendPosition = LegendPosition.TopCenter,      // 顶部中央
                LegendPlacement = LegendPlacement.Outside,      // 图表外部
                LegendOrientation = LegendOrientation.Horizontal, // 水平排列
                LegendFontSize = 8,
                LegendSymbolLength = 16                         // 符号长度
            });

            // X轴 (a轴)
            var xAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Minimum = -100,
                Maximum = 100,
                MajorStep = 20,
                PositionAtZeroCrossing = true,
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColors.Black,
                TickStyle = TickStyle.Outside,
                MajorTickSize = 5,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0
            };

            // Y轴 (b轴)
            var yAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                Minimum = -100,
                Maximum = 100,
                MajorStep = 20,
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
            plotModel.Annotations.Add(new LineAnnotation
            {
                Type = LineAnnotationType.Horizontal,
                Y = 0,
                Color = OxyColors.Black,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 1,
                MinimumX = -100,
                MaximumX = 100,
                Layer = AnnotationLayer.BelowSeries
            });

            // 垂直分隔线 X=0
            plotModel.Annotations.Add(new LineAnnotation
            {
                Type = LineAnnotationType.Vertical,
                X = 0,
                Color = OxyColors.Black,
                LineStyle = LineStyle.Solid,
                MinimumY = -100,
                MaximumY = 100,
                StrokeThickness = 1,
                Layer = AnnotationLayer.BelowSeries
            });

            return plotModel;
        }

        // 添加颜色数据
        private void AddColorData()
        {
            double minA = -100, maxA = 100;
            double minB = -100, maxB = 100;

            using (var stream = new MemoryStream())
            {
                var oxyImage = new OxyImage(CreateLabDiagramWithCoords().ToArray());

                var imageAnnotation = new ImageAnnotation
                {
                    ImageSource = oxyImage,
                    X = new PlotLength(minA, PlotLengthUnit.Data),
                    Y = new PlotLength(maxB, PlotLengthUnit.Data),
                    Width = new PlotLength(maxA - minA, PlotLengthUnit.Data),
                    Height = new PlotLength(minB - maxB, PlotLengthUnit.Data),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Layer = AnnotationLayer.BelowAxes,
                    Interpolate = false
                };

                XyAxisModel.Annotations.Add(imageAnnotation);
            }
            AddSaturationCircles();
            AddLabels();
            XyAxisModel.InvalidatePlot(true);
            ZAxisModel.InvalidatePlot(true);
        }

        /// <summary>
        /// 创建一个以原点为中心的 Lab 色轮图，包含 a* 和 b* 轴的颜色渐变。图像尺寸为 1000x1000 像素，内圆半径为 500 像素，使用径向渐变填充颜色。外圈为白色背景。该图将用于 ab 颜色空间的背景显示，帮助用户理解不同 a* 和 b* 值对应的颜色位置。
        /// </summary>
        /// <returns></returns>
        static MemoryStream CreateLabDiagramWithCoords()
        {
            const int W = 1000;
            const int H = 1000;
            const int Cx = W / 2;
            const int Cy = H / 2;
            const int InnerR = 500;          // 内圆半径（渐变区域）

            Bitmap bmp = new Bitmap(W, H, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // 1. 整图先刷白（外圈背景）
                g.Clear(Color.White);
                // 2. 内圆 1000×1000 的径向渐变（Lab 色轮）
                RectangleF innerRect = new RectangleF(Cx - InnerR, Cy - InnerR, InnerR * 2, InnerR * 2);
                // ========== 32等分 ==========
                int segments = 32;
                float angleStep = 360f / segments;  // 每份11.25度

              //  批量存储绘图数据，适合循环绘制色环
                PointF[] points = new PointF[segments];
                Color[] colors = new Color[segments];

                for (int i = 0; i < segments; i++)
                {
                    float angleDeg = i * angleStep;   // 0, 11.25, 22.5, 33.75...
                    float angleRad = (float)(angleDeg * Math.PI / 180.0);

                    float x = Cx + InnerR * (float)Math.Cos(angleRad);
                    float y = Cy - InnerR * (float)Math.Sin(angleRad);
                    points[i] = new PointF(x, y);

                    // HSL色相均匀分布：0-360度
                    colors[i] = ColorFromHsl(angleDeg, 1.0f, 0.5f);
                }
                //路径渐变画刷
                using (PathGradientBrush brush = new PathGradientBrush(points, WrapMode.Tile))
                {
                    brush.CenterPoint = new PointF(Cx, Cy);
                    brush.CenterColor = Color.White;
                    brush.SurroundColors = colors;

                    g.FillEllipse(brush, innerRect);
                }
            }
            return ConvertToImageSource(bmp);
        }
        // HSL转RGB辅助方法
        static Color ColorFromHsl(float hue, float saturation, float lightness)
        {
            // 确保 hue 在 0-360 范围内
            hue = hue % 360;
            if (hue < 0) hue += 360;

            float c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
            float x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
            float m = lightness - c / 2;

            float r, g, b;
            int sector = (int)(hue / 60) % 6;

            switch (sector)
            {
                case 0: r = c; g = x; b = 0; break;  // 0-60°: 红→黄
                case 1: r = x; g = c; b = 0; break;  // 60-120°: 黄→绿
                case 2: r = 0; g = c; b = x; break;  // 120-180°: 绿→青
                case 3: r = 0; g = x; b = c; break;  // 180-240°: 青→蓝
                case 4: r = x; g = 0; b = c; break;  // 240-300°: 蓝→紫
                default: r = c; g = 0; b = x; break; // 300-360°: 紫→红
            }

            return Color.FromArgb(
                255,                                    // Alpha (不透明)
                (int)((r + m) * 255),                   // Red
                (int)((g + m) * 255),                   // Green  
                (int)((b + m) * 255));                  // Blue
        }
        /// <summary>
        /// 将bitmap图转换成存储流数据
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static MemoryStream ConvertToImageSource(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();

            src.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            //src.Save($"H:/{Guid.NewGuid()}.bmp");
            return ms;

        }

        // 添加饱和度圆环
        private void AddSaturationCircles()
        {
            double[] radii = { 20, 40, 60, 80, 100 };
            foreach (var r in radii)
            {
                XyAxisModel.Annotations.Add(new EllipseAnnotation
                {
                    X = 0,
                    Y = 0,
                    Width = r * 2,
                    Height = r * 2,
                    Fill = OxyColors.Transparent,
                    Stroke = OxyColors.Gray,
                    StrokeThickness = 0.5,
                    Layer = AnnotationLayer.BelowSeries
                });
            }
        }

        // 添加文字标注
        private void AddLabels()
        {
            // -a 绿
            XyAxisModel.Annotations.Add(new TextAnnotation
            {
                Text = "-a*\n绿",
                TextPosition = new DataPoint(-90, 15),
                TextColor = OxyColors.Black,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Stroke = OxyColors.Transparent
            });

            // +a 红
            XyAxisModel.Annotations.Add(new TextAnnotation
            {
                Text = "+a*\n红",
                TextPosition = new DataPoint(90, 15),
                TextColor = OxyColors.Black,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Stroke = OxyColors.Transparent
            });

            // -b 蓝
            XyAxisModel.Annotations.Add(new TextAnnotation
            {
                Text = "-b*蓝",
                TextPosition = new DataPoint(15, -90),
                TextColor = OxyColors.Black,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Stroke = OxyColors.Transparent
            });

            // +b 黄
            XyAxisModel.Annotations.Add(new TextAnnotation
            {
                Text = "+b*黄",
                TextPosition = new DataPoint(15, 90),
                TextColor = OxyColors.Black,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Stroke = OxyColors.Transparent
            });
        }
        #endregion

        #region 亮度条

        /// <summary>
        /// 创建亮度条
        /// </summary>
        /// <returns></returns>
        public PlotModel CreateBrightnessPlot()
        {
            var plotModel = new PlotModel
            {
                Title = "L*",
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotAreaBorderColor = OxyColors.Transparent,
                Padding = new OxyThickness(0)
            };

            // X轴
            var xAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 1,  
                IsAxisVisible = false//不显示刻度和数值
            };

            // Y轴 
            var yAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 100,
                MajorStep = 20,
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColors.Black,
                AxislineThickness = 1,
                TickStyle = TickStyle.Outside,
                MajorTickSize = 5,
                MajorGridlineStyle = LineStyle.None
            };

          
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            // 创建 1x100 像素的渐变位图
            var bitmap = CreateGradientBitmap(1, 100);
            var oxyImage = new OxyImage(bitmap);

            // 用 ImageAnnotation 作为背景
            var bgImage = new ImageAnnotation
            {
                ImageSource = oxyImage,
                X = new PlotLength(0, PlotLengthUnit.Data),
                Y = new PlotLength(100, PlotLengthUnit.Data),
                Width = new PlotLength(1, PlotLengthUnit.Data),
                Height = new PlotLength(-100, PlotLengthUnit.Data),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Layer = AnnotationLayer.BelowAxes,
                Interpolate = true
            };

            plotModel.Annotations.Add(bgImage);


            return plotModel;
        }

        private byte[] CreateGradientBitmap(int width, int height)
        {
            using (var bmp = new Bitmap(width, height))
            {
                for (int y = 0; y < height; y++)
                {
                    byte gray = (byte)(y * 255 / (height - 1)); // 0-255
                                                                // 从下到上：黑(0) -> 白(255)
                                                                // 但 Y 轴 0 在底部，所以 y=0 应该是黑色
                    byte invertedGray = (byte)(255 - gray); // 翻转：y=0(底部)=黑, y=99(顶部)=白

                    for (int x = 0; x < width; x++)
                    {
                        bmp.SetPixel(x, y, Color.FromArgb(invertedGray, invertedGray, invertedGray));
                    }
                }

                using (var stream = new System.IO.MemoryStream())
                {
                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return stream.ToArray();
                }
            }
        }
        #endregion


        #region Lab值数据的接收
        public void Receive(LabValueMode message)
        {
            
            if (message.LabValueType == LabValueEnum.Standard)
            {
                AddStandardSample(message);
            }
            else
            {
                AddTestSample(message);
            }
        }

        // 添加标样数据
        public void AddStandardSample(LabValueMode message)
        {
            // 每次添加标样数据前先清除之前的样品数据
            ClearSamples(); 
            // 添加数据点到标样系列
            StandardLSampleSeries.Points.Add(new ScatterPoint(0.5, message.LValue, tag: message));

            // 添加数据点到标样系列
            StandardSampleSeries.Points.Add(new ScatterPoint(message.AValue, message.BValue, tag: message));

            // 刷新图表
            XyAxisModel.InvalidatePlot(true);
            ZAxisModel.InvalidatePlot(true);
        }

        // 添加试样数据
        public void AddTestSample(LabValueMode message)
        {

            // 添加数据点到试样系列
            TestLSampleSeries.Points.Add(new ScatterPoint(0.5, message.LValue, tag: message));
            // 添加数据点到试样系列
            TestSampleSeries.Points.Add(new ScatterPoint(message.AValue, message.BValue,tag: message));

            // 刷新图表
            XyAxisModel.InvalidatePlot(true);
            ZAxisModel.InvalidatePlot(true);
        }
        // 清除所有样品数据
        public void ClearSamples()
        {
            StandardSampleSeries.Points.Clear();
            TestSampleSeries.Points.Clear();
            StandardLSampleSeries.Points.Clear();
            TestLSampleSeries.Points.Clear();
            XyAxisModel.InvalidatePlot(true);
            ZAxisModel.InvalidatePlot(true);
        }
        // 初始化样品数据系列
        private void InitializeSampleSeries()
        {
            // 标样数据系列 - 红色圆圈
            StandardSampleSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Diamond,
                MarkerSize = 7,
                MarkerFill = OxyColors.Red,
                MarkerStroke = OxyColors.DarkRed,
                MarkerStrokeThickness = 2,
                Selectable = true
            };

            // 试样数据系列 - 黄色圆圈
            TestSampleSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 5,
                MarkerFill = OxyColors.Yellow,
                MarkerStroke = OxyColors.Orange,
                MarkerStrokeThickness = 2,
                Selectable = true
            };

            // 标样L数据系列 - 红色圆圈
            StandardLSampleSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Diamond,
                MarkerSize = 7,
                MarkerFill = OxyColors.Red,
                MarkerStroke = OxyColors.DarkRed,
                MarkerStrokeThickness = 2,
                Selectable = true
            };

            // 试样L数据系列 - 黄色圆圈
            TestLSampleSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 5,
                MarkerFill = OxyColors.Yellow,
                MarkerStroke = OxyColors.Orange,
                MarkerStrokeThickness = 2,
                Selectable = true
            };

            // 添加到图表模型
            XyAxisModel.Series.Add(StandardSampleSeries);
            XyAxisModel.Series.Add(TestSampleSeries);
            ZAxisModel.Series.Add(StandardLSampleSeries);
            ZAxisModel.Series.Add(TestLSampleSeries);

            // 刷新图表
            XyAxisModel.InvalidatePlot(true);
        }
        #endregion



    }


}