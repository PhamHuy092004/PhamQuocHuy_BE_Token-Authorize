using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PhamQuocHuy_BE_Token_Authorize.Data;
using PhamQuocHuy_BE_Token_Authorize.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PhamQuocHuy_BE_Token_Authorize.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JwtController : ControllerBase
    {
        public IConfiguration Configuration { get; set; }
        public readonly ApplicationDBContext _context;
        public JwtController(IConfiguration configuration, ApplicationDBContext context)
        {
            Configuration = configuration;
            _context = context;
        }
        [HttpGet("GetUsers")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }
        [HttpPost]
        public async Task<IActionResult> GenerateToken(User user)
        {
            if (user != null && !string.IsNullOrEmpty(user.Username) && !string.IsNullOrEmpty(user.Password))
            {   //Kiểm tra tài khoản và mật khẩu có đúng hay không
                var result = await GetUserInfo(user.Username, user.Password);
                if (result is NotFoundResult)
                {
                    // Nếu không sẽ trả về Unauthorized
                    return Unauthorized("Invalid username or password.");
                }
                // Lấy thông tin người dùng từ kết quả
                var userData = result as OkObjectResult;
                var users = userData.Value as User;
                //Lấy cấu hình JWT từ tệp cấu hình
                var jwt = Configuration.GetSection("Jwt").Get<Jwt>();
                //Tạo danh sách các claim, nơi đây sẽ chứa các cặp key-value chứa các thông tin người dùng
                var claims = new[]
                {
            new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            new Claim("ID", users.ID.ToString()),
            new Claim("Username", users.Username),
            new Claim("Password", users.Password)
        };
                //Tạo khóa bảo mật và thông tin đăng nhập
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                //Tạo token JWT
                var token = new JwtSecurityToken(
                    jwt.Issuer,
                    jwt.Audience,
                    claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: signIn
                );
                return Ok(new JwtSecurityTokenHandler().WriteToken(token)); //Trả về token
            }
            else
            {
                return BadRequest("Invalid user data."); // Mếu các input chứa các giá trị null, hoặc không họp lệ thì sẽ về Status 400 (Bad request)
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserInfo(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }


    }
}
