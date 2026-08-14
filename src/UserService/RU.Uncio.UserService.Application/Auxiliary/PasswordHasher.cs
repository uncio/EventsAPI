using System.Security.Cryptography;
using System.Text;

namespace RU.Uncio.UserService.Application.Auxiliary
{
    /// <summary>
    /// Hashes and verifies a string
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Returns hashed string
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string ConvertPass(this string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        /// <summary>
        /// Verifies a string with a hashed string
        /// </summary>
        /// <param name="password"></param>
        /// <param name="checkSum"></param>
        /// <returns></returns>

        public static bool VerifyPassword(string password, string checkSum) => checkSum.Equals(ConvertPass(password));
    }
}
