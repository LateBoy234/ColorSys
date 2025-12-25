using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.Permission
{
    public static  class AuthServiceExtensions
    {
        /// <summary>
        /// 默认注册内存实现；正式项目可 Replace 自己的
        /// </summary>
        public static IServiceCollection AddLoginModule(this IServiceCollection services)
        {
            services.AddSingleton<IAuthService, InMemoryAuthService>();
            return services;
        }
    }
}
