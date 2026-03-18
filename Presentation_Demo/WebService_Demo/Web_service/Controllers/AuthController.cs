using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FirebaseService _firebase;

    public AuthController(FirebaseService firebase)
    {
        _firebase = firebase;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // ❌ check rỗng
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new
            {
                success = false,
                message = "Thiếu dữ liệu"
            });
        }

        // 🔍 tìm user
        var user = await _firebase.FindUser(request.Username);

        if (user == null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Không tồn tại user"
            });
        }

        // 🔐 check password
        if (user.password != request.Password)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Sai mật khẩu"
            });
        }

        // ✅ OK
        return Ok(new
        {
            success = true,
            message = "Đăng nhập thành công"
        });
    }
}
