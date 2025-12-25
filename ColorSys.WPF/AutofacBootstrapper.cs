using Autofac;
using CColorSys.WPF.Interface;
using ColorSys.Permission;
using ColorSys.Permission.ViewModels;
using ColorSys.WPF.Implementation;
using ColorSys.WPF.ViewModels;

namespace ColorSys.WPF
{
    public static class AutofacBootstrapper
    {
        public static IContainer Build()
        {
            var builder = new ContainerBuilder();

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

            // Views
            builder.RegisterType<MainWindow>().SingleInstance();
            builder.RegisterType<LoginWindow>().InstancePerDependency();

            return builder.Build();
        }
    }
}
