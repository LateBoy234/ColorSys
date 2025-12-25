using Autofac;
using CColorSys.WPF.Interface;
using ColorSys.Permission;
using ColorSys.Permission.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.WPF.Implementation
{
    public sealed class NavigationService : INavigationService
    {
        private readonly ILifetimeScope _scope;

        public NavigationService(ILifetimeScope scope)
        {
            _scope= scope;
        }
        public bool ShowLoginDialog()
        {
            var win = _scope.Resolve<LoginWindow>();
            var vm = _scope.Resolve<LoginViewModel>();
            win.DataContext = vm;
            return win.ShowDialog() == true;
        }
    }
}
