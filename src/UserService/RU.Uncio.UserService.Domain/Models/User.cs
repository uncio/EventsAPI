namespace RU.Uncio.UserService.Domain.Models
{
    /// <summary>
    /// User model
    /// </summary>
    public class User
    {
        /// <summary>
        /// User ID
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// USer ID
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// User login name
        /// </summary>
        public string Login { get; set; } = null!;
        /// <summary>
        /// User hashed password
        /// </summary>
        public string HashedPassword { get; set; } = null!;
        /// <summary>
        /// User role
        /// </summary>
        public Roles Role { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public User() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="role"></param>
        public User(string login, string password, Roles role = Roles.User)
        {
            Id = Guid.NewGuid();
            Login = login;
            HashedPassword = password;
            Role = role;
        }
    }
}
