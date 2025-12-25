

using Autofac;
using ColorSys.WPF.ViewModels;
using System.IO;
using System.Windows;

namespace ColorSys.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IContainer Container { get; private set; } = null!;

        protected override  void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //全局异常处理，防止闪退
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            Container = AutofacBootstrapper.Build();
            var main = Container.Resolve<MainWindow>();
            main.DataContext = Container.Resolve<MainWindowViewmodel>();
            main.Show();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //处理UI线程一场
            HandleException(e.Exception);
            e.Handled = true;//标记为已处理
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if(e.ExceptionObject is Exception ex)
            {
                HandleException(ex);
            }
        }

        private void HandleException(Exception ex)
        {
            //做异常处理；记录日志，
            LogException(ex);
          //  File.AppendAllLines()

            MessageBox.Show("发生异常，请调试或联系管理员", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void LogException(Exception ex)
        {
            string errmessage = $"{DateTime.Now}:{ex.Message}\n{ex.StackTrace}";
            File.AppendAllText("error.log", errmessage);
        }
    }

}
