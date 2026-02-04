using Autofac;
using CColorSys.WPF.Interface;
using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Service;
using ColorSys.HardwareContract.Strategy;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using ColorSys.HardwareImplementation.Communication.SeriaPort;
using ColorSys.HardwareImplementation.Communication.TCP;
using ColorSys.HardwareImplementation.Device;
using ColorSys.Permission;
using ColorSys.Permission.ViewModels;
using ColorSys.WPF.Implementation;
using ColorSys.WPF.Service;
using ColorSys.WPF.ViewModels;
using ColorSys.WPF.Views;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace ColorSys.WPF
{
    public static class AutofacBootstrapper
    {
        public static IContainer Build()
        {
            var builder = new ContainerBuilder();
            // 自动扫描所有策略
            var assemblies = new[]
                             {
                                Assembly.GetExecutingAssembly(),                                    // ColorSys.WPF
                                typeof(ICommStrategy).Assembly,                                     // ColorSys.HardwareContract（接口）
                                Assembly.Load("ColorSys.HardwareImplementation") // 策略实现
                            };

            // 通讯策略
            builder.RegisterAssemblyTypes(assemblies)
                   .Where(t => t.IsAssignableTo<ICommStrategy>())
                   .As<ICommStrategy>()
                   .SingleInstance();

            // 设备策略 ← 加这一行
            builder.RegisterAssemblyTypes(assemblies)
                   .Where(t => t.IsAssignableTo<IDeviceStrategy>())
                   .As<IDeviceStrategy>()
                   .SingleInstance();

            builder.RegisterType<DeviceConnectionService>()
          .As<IDeviceConnectionService>()
          .SingleInstance();

            // Services
            builder.RegisterType<InMemoryAuthService>()
                   .As<IAuthService>()
                   .SingleInstance();

            builder.RegisterType<NavigationService>()
                   .As<INavigationService>()
                   .SingleInstance();

            // ViewModels
            builder.RegisterType<MainWindowViewmodel>().InstancePerLifetimeScope();
            builder.RegisterType<LoginViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<ConnectViewModel>().InstancePerLifetimeScope();

            // Views
            builder.RegisterType<MainWindow>().SingleInstance();
            builder.RegisterType<LoginWindow>().SingleInstance();
            builder.RegisterType<ConnectView>().SingleInstance();

            return builder.Build();
        }
    }
}
