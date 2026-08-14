using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using RU.Uncio.UserService.Application.DTO;
using RU.Uncio.UserService.Application.Interfaces;
using RU.Uncio.UserService.Application.Auxiliary;
using System.ComponentModel.DataAnnotations;
using System.Net;
using RU.Uncio.UserService.Domain.Models;

namespace RU.Uncio.UserService.Presentation.Controllers
{
    /// <summary>
    /// Users controller
    /// </summary>
    /// <param name="userService"></param>
    /// <param name="logger"></param>
    [ApiController]
    [Route("/")]    
    public class UsersController(IUserService userService, ILogger<UsersController> logger) : ControllerBase
    {
        /// <summary>
        /// Returns all users
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("/users")]
        public async Task<ActionResult<ApiResult<List<UserDTO>>>> GetUsersAsync(CancellationToken token)
        {
            var users = await userService.GetAllUsersAsync(token);

            var result = users.Select(ev => ev.MapToDto()).ToList();

            return Ok(new ApiResult<List<UserDTO>>
            {
                Data = result,
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Getting all users from DB"
            });
        }

        /// <summary>
        /// Creates user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status201Created)]
        [Consumes("application/json")]
        [HttpPost, Route("auth/register")]
        public async Task<ActionResult<ApiResult<UserDTO>>> CreateUser([FromBody] UserDTO user, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                logger.LogError($"Validation failed: {String.Join(";",
                    ModelState.SelectMany(x => x.Value?.Errors.Select(z => $"{x.Key} : {z.ErrorMessage}") ?? new List<string>()))}");
                throw new ValidationException($"Validation failed: {String.Join(";",
                    ModelState.SelectMany(x => x.Value?.Errors.Select(z => $"{x.Key} : {z.ErrorMessage}") ?? new List<string>()))}");
            }

            var newUser = new User(user.Login, user.Password.ConvertPass(), user.Role) { Name = user.Name! };
            await userService.SaveUserAsync(newUser, token);

            return CreatedAtAction(nameof(CreateUser), new ApiResult<UserDTO>
            {
                Data = newUser.MapToDto(),
                Success = true,
                StatusCode = HttpStatusCode.Created,
                Message = $"User {user.Name} : {user.Login} added to DB"
            });
        }

        /// <summary>
        /// Authentication
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ApiBaseResult), StatusCodes.Status200OK)]
        [HttpPost, Route("auth/login")]
        public async Task<ActionResult<ApiBaseResult>> Login([FromBody] LoginRequest request, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authUser = await userService.VerifyUserAsync(request.Email, request.Password, token);

            if (authUser != null)
            {
                return Ok(new ApiResult<string>
                {
                    Data = authUser,
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    Message = $"User Token"
                });
            }
            else
            {
                return NotFound(new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.Forbidden,
                    Message = $"User with login {request.Email} is not found in DB or password is incorrect"
                });
            }
        }
    }
}
