using ColorSys.Permission.ViewModels;
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
using System.Windows.Shapes;

namespace ColorSys.Permission
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _auth;
        public LoginWindow(IAuthService auth)
        {
            InitializeComponent();
            _auth = auth;
        }

        private  void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = tb_Pwd.Password;
                // 如果登录成功（无异常），就关闭窗口并返回 DialogResult=true
                if (_auth.CurrentUser is not null)
                    DialogResult = true;   // 关闭窗口并返回 true
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    }
}
