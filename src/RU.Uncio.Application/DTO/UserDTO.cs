using RU.Uncio.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RU.Uncio.Application.DTO
{
    /// <summary>
    /// Data transfer object for User model
    /// </summary>
    public class UserDTO
    {
        /// <summary>
        /// User ID
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// User name
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// User login name
        /// </summary>
        [Required]
        [MinLength(8)]
        public required string Login { get; set; }
        /// <summary>
        /// User password
        /// </summary>
        [Required]
        [MinLength(8)]
        public required string Password { get; set; }
        /// <summary>
        /// User role
        /// </summary>
        public Roles Role { get; set; }
    }
}
