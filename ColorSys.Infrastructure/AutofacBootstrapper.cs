using Autofac;
using System.ComponentModel;

namespace ColorSys.Infrastructure
{
    public static class AutofacBootstrapper
    {
        public static IContainer Build()
        {
            var builder = new ContainerBuilder();

            // Services
            builder.RegisterType<AuthService>()
                   .As<IAuthService>()
                   .SingleInstance();

            builder.RegisterType<NavigationService>()
                   .As<INavigationService>()
                   .SingleInstance();

            // ViewModels
            builder.RegisterType<MainViewModel>().InstancePerDependency();
            builder.RegisterType<LoginViewModel>().InstancePerDependency();

            // Views
            builder.RegisterType<MainWindow>().SingleInstance();
            builder.RegisterType<LoginWindow>().InstancePerDependency();

            return builder.Build();
        }
    }
}
