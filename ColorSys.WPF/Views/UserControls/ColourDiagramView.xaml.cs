using ColorSys.WPF.ViewModels;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ColorSys.WPF.Views
{
    /// <summary>
    /// ColourDiagramView.xaml 的交互逻辑
    /// </summary>
    public partial class ColourDiagramView : UserControl
    {
        public ColourDiagramView()
        {
            InitializeComponent();
            // 绑定鼠标进入事件到 PlotCommands.HoverSnapTrack 命令
            XyAxisPlotView.Controller = new PlotController();
            XyAxisPlotView.Controller.BindMouseEnter(PlotCommands.HoverSnapTrack);

            ZAxisPlotView.Controller = new PlotController();
            ZAxisPlotView.Controller.BindMouseEnter(PlotCommands.HoverSnapTrack);

            this.DataContext =new  ColourDiagramViewModel();
        }
    }
}
