using API.Data;
using API.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;

//Controller that allows us to register a new user

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto register)
        {

            //if we have a user, we can return a bad request with a message
            if (await UserExists(register.Username))
            {
                return BadRequest("Username is taken");
            }

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = register.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password)),
                PasswordSalt = hmac.Key,
            };

            //adds new user and saves to DB
            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return new UserDto{
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
            };
        }

        //used to check if a user exists in our DB
        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto login)
        {

            var user = await _context.Users.SingleOrDefaultAsync((x) => x.UserName == login.Username.ToLower());

            if (user == null)
            {
                return Unauthorized("Invalid username");
            }

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));

            for (int i = 0; i < hash.Length; i++)
            {
                if (hash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("Invalid password");
                }
            }

            return new UserDto{
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
            };;
        }

        //works with query strings but not with something passing a JSON object
        // [HttpPost("register")]
        // public async Task<ActionResult<AppUser>> Register(string username, string password){
        //     using var hmac = new HMACSHA512();

        //     var user = new AppUser
        //     {
        //         UserName = username,
        //         PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
        //         PasswordSalt = hmac.Key,
        //     };

        //     _context.Users.Add(user);

        //     await _context.SaveChangesAsync();

        //     return user;
        // }
    }
}