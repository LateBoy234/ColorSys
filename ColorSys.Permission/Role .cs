
namespace ColorSys.Permission
{

    public record User(string Name, UserRole Role);
    public enum UserRole
    {
        /// <summary>
        /// 普通操作员
        /// </summary>
        Operator,
        /// <summary>
        /// 工程师
        /// </summary>
        Engineer,
        /// <summary>
        /// 管理员
        /// </summary>
        Admin

    }
}
