

using Autofac;
using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using ColorSys.HardwareImplementation.Communication.CommParamVm;
using ColorSys.HardwareImplementation.Communication.SeriaPort;
using ColorSys.HardwareImplementation.Communication.TCP;
using ColorSys.HardwareImplementation.Device;
using ColorSys.WPF.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using System.Globalization;
using System.IO;
using System.Management;
using System.Windows;

namespace ColorSys.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IContainer Container { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ChangeLanguage("zh-CN");
            //全局异常处理，防止闪退
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            //方式一注册
            Container = AutofacBootstrapper.Build();

           
            var main = Container.Resolve<MainWindow>();
            main.DataContext = Container.Resolve<MainWindowViewmodel>();
            main.Show();
        }

       
        public static void ChangeLanguage(string cultureName)
        {
            var newDict = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Colorsys.WPF;component/Resources/Language/{cultureName}.xaml",
                        UriKind.Absolute)
            };

            // 找到旧的 Language 字典并删除
            var oldDict = Current.Resources.MergedDictionaries
                              .FirstOrDefault(d =>
                                  d.Source?.OriginalString.Contains("Language/") == true);
            if (oldDict != null)
                Current.Resources.MergedDictionaries.Remove(oldDict);

            Current.Resources.MergedDictionaries.Add(newDict);
        }

        #region 防止异常闪退
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //处理UI线程一场
            HandleException(e.Exception);
            e.Handled = true;//标记为已处理
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
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
        #endregion

    }

}
