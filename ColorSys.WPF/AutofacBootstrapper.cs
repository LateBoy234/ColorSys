using Autofac;
using CColorSys.WPF.Interface;
using ColorSys.HardwareContract;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using ColorSys.HardwareImplementation.Communication.SeriaPort;
using ColorSys.HardwareImplementation.Communication.TCP;
using ColorSys.HardwareImplementation.Device;
using ColorSys.HardwareImplementation.Device.Hub;
using ColorSys.Permission;
using ColorSys.Permission.ViewModels;
using ColorSys.WPF.Implementation;
using ColorSys.WPF.ViewModels;
using ColorSys.WPF.Views;
using System.Diagnostics.Metrics;

namespace ColorSys.WPF
{
    public static class AutofacBootstrapper
    {
        public static IContainer Build()
        {
            var builder = new ContainerBuilder();


            // ① 参数：具体类 + 接口同时注册，供构造函数精确匹配
            builder.RegisterType<SerialParameters>()
                   .AsSelf().As<ICommParameters>().SingleInstance();
            builder.RegisterType<TcpParameters>()
                   .AsSelf().As<ICommParameters>().SingleInstance();

            // ② 通信：命名注册（key = rtu / tcp / bluetooth）
            builder.RegisterType<ModbusRtuSerial>()
                   .Named<ICommunication>("USB")
                   .WithParameter((p, c) => p.ParameterType == typeof(SerialParameters),
                                  (p, c) => c.Resolve<SerialParameters>());
            builder.RegisterType<TcpCommunication>()
                   .Named<ICommunication>("TCP")
                   .WithParameter((p, c) => p.ParameterType == typeof(TcpParameters),
                                  (p, c) => c.Resolve<TcpParameters>());
            builder.RegisterType<BluetoothComm>()
                   .Named<ICommunication>("bluetooth");

            // ③ 设备：命名注册（key = PTS-rtu / PTS-tcp / CR-rtu ...）
            builder.RegisterType<PTSInstrument>()
                   .Named<IDevice>("PTS-USB")
                   .WithParameter((p, c) => p.ParameterType == typeof(ICommunication),
                                  (p, c) => c.ResolveNamed<ICommunication>("USB"));
            builder.RegisterType<PTSInstrument>()
                   .Named<IDevice>("PTS-TCP")
                   .WithParameter((p, c) => p.ParameterType == typeof(ICommunication),
                                  (p, c) => c.ResolveNamed<ICommunication>("TCP"));

            // 把“命名工厂”显式注册成 Func<string, IDevice>
            builder.Register<Func<string, IDevice>>(c =>
            {
                var ctx = c.Resolve<IComponentContext>();
                return key => ctx.ResolveNamed<IDevice>(key);
            }).SingleInstance();
            //builder.RegisterType<CrInstrument>()
            //       .Named<IDevice>("CR-rtu")
            //       .WithParameter((p, c) => p.ParameterType == typeof(ICommunication),
            //                      (p, c) => c.ResolveNamed<ICommunication>("rtu"));

            // ④ 会话级单例：保存“已连接”的那一份设备
            builder.RegisterType<ConnectedDeviceHub>()
                   .As<IConnectedDeviceHub>()
                   .SingleInstance();

            // Services
            builder.RegisterType<InMemoryAuthService>()
                   .As<IAuthService>()
                   .SingleInstance();

            builder.RegisterType<NavigationService>()
                   .As<INavigationService>()
                   .SingleInstance();

            // ViewModels
            builder.RegisterType<MainWindowViewmodel>().InstancePerDependency();
            builder.RegisterType<LoginViewModel>().InstancePerDependency();
            builder.RegisterType<ConnectViewModel>();

            // Views
            builder.RegisterType<MainWindow>().SingleInstance();
            builder.RegisterType<LoginWindow>().InstancePerDependency();
            builder.RegisterType<ConnectView>().InstancePerDependency();

            return builder.Build();
        }
    }
}
