using RU.Uncio.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RU.Uncio.Application.Interfaces
{
    /// <summary>
    /// Token service interface
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Create token for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        string CreateToken(User user);
    }
}
