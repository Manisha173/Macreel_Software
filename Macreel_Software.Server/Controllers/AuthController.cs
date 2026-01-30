using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using Macreel_Software.DAL;
using Macreel_Software.DAL.Auth;
using Macreel_Software.Models;
using Macreel_Software.Models.Common;
using Macreel_Software.Services.MailSender;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Macreel_Software.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;
        private readonly JwtTokenProvider _jwtProvider;
        private readonly IMemoryCache _cache;
        private readonly MailSender _mailservice;
        private readonly PasswordEncrypt _pass;

        public AuthController(IAuthServices authServices,JwtTokenProvider jwtProvider, PasswordEncrypt pass,IMemoryCache cache,MailSender sender)
        {
            _authServices = authServices;
            _jwtProvider = jwtProvider;
            _cache = cache;
            _mailservice=sender;
            _pass=pass;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.UserName) || string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest(new { Status = 400, Message = "Username and password are required." });
            }

            var user = await _authServices.ValidateUserAsync(model.UserName, model.Password);

            if (user == null)
                return Unauthorized(new { Status = 401, Message = "Invalid username or password." });

            var accessToken = _jwtProvider.CreateToken(user);
            var refreshToken = _jwtProvider.GenerateRefreshToken();
            var refreshExpire = DateTime.UtcNow.AddDays(2);

            await _authServices.SaveRefreshTokenAsync(user.UserId, refreshToken, refreshExpire);

            Response.Cookies.Append("access_token", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(30)
            });

            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(2)
            });

            return Ok(new
            {
                Status = 200,
                Message = "Login successful",
                Data = new { token = accessToken }
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("Refresh token missing");

            var tokenData = await _authServices.GetRefreshTokenAsync(refreshToken);
            if (tokenData == null || tokenData.Expiry < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token");

            var user = await _authServices.GetUserByIdAsync(tokenData.UserId);
            if (user == null)
                return Unauthorized();

            var newAccessToken = _jwtProvider.CreateToken(user);

            Response.Cookies.Append("access_token", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,               
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(30)
            });

            return Ok(new { message = "Token refreshed",token = newAccessToken });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshToken))
                await _authServices.RevokeRefreshTokenAsync(refreshToken);

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");

            return Ok();
        }
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                userId,
                role
            });
        }

        [HttpPost("GetOTPForResetPassword")]
        public async Task<IActionResult> GetOTPforPassReset([FromBody] ForgetPasswordRequest data)
        {
            try
            {
                var user = await _authServices.CheckUserExistOrNot(data.Email);
                if (user == null)
                {
                    return NotFound(new
                    {
                        status = false,
                        message = "User not found"
                    });
                }

                string otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

                string cacheKey = $"OTP_{data.Email}";
                _cache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

                MailRequest mailRequest = new MailRequest
                {
                    ToEmail = data.Email,
                    Subject = "OTP Verification for Change Password",
                    BodyType = MailBodyType.ForgotPassword,
                    otp = otp
                };

                var status = await _mailservice.SendMailAsync(mailRequest);

                if (status!=null)
                {
                    return Ok(new
                    {
                        status = true,
                        statusCode = 200,
                        message = "OTP sent successfully on your registered email"
                    });
                }

                return StatusCode(500, new
                {
                    status = false,
                    statusCode = 500,
                    message = "Failed to send OTP"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    statusCode = 500,
                    message = "Something went wrong",
                    error = ex.Message 
                });
            }
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] verifyOtpRequest data)
        {
            try
            {
                var cacheKey = $"OTP_{data.Email}";

                if (!_cache.TryGetValue(cacheKey, out string cacheOtp))
                {
                    return BadRequest(new
                    {
                        status = false,
                        message = "OTP expired or invalid"
                    });
                }

                if (cacheOtp != data.Otp)
                {
                    return BadRequest(new
                    {
                        status = false,
                        message = "Incorrect OTP"
                    });
                }

                _cache.Set($"OTP_Verified_{data.Email}", true, TimeSpan.FromMinutes(10));

                _cache.Remove(cacheKey);

                return Ok(new
                {
                    status = true,
                    message = "OTP verified successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    message = "Something went wrong while verifying OTP",
                    error = ex.Message 
                });
            }
        }

        //[HttpPost("reset-password")]
        //public IActionResult ResetPassword(ResetPasswordRequest data)
        //{
        //    var verifyKey = $"OTP_Verify_{data.Email}";

        //    if (!_cache.TryGetValue(verifyKey, out bool isVerified) || !isVerified)
        //        return BadRequest(new
        //        {
        //            status=false,
        //            message="OTP not verified!!"
        //        });


        //    var userId = _authServices.GetUserIdByEmailId(data.Email);
        //    string pass = data.NewPassword;

        //    data.NewPassword = _pass.EncryptPassword(data.NewPassword!);

         


        //}

    }
}
