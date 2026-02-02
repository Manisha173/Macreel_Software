using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;


namespace Macreel_Software.Services.OTPVerification
{
    public class OTPVerificationService
    {
        private readonly IDistributedCache _cache;

        public OTPVerificationService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<(string FlowId, string Otp)> GenerateOtpAsync(string email)
        {
            string otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            string flowId = Guid.NewGuid().ToString("N");

            await _cache.SetStringAsync(
                $"FP_EMAIL_{flowId}",
                email,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
                });

            await _cache.SetStringAsync(
                $"FP_OTP_{flowId}",
                otp,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

            
            return (flowId, otp);
        }

        public async Task<bool> VerifyOtpAsync(string flowId, string otp)
        {
            var cacheOtp = await _cache.GetStringAsync($"FP_OTP_{flowId}");
            if (string.IsNullOrEmpty(cacheOtp)) return false;
            if (cacheOtp != otp) return false;

            await _cache.SetStringAsync(
                $"FP_VERIFIED_{flowId}",
                "true",
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            await _cache.RemoveAsync($"FP_OTP_{flowId}");
            return true;
        }

        public async Task<bool> IsFlowVerifiedAsync(string flowId)
        {
            var verified = await _cache.GetStringAsync($"FP_VERIFIED_{flowId}");
            return verified == "true";
        }

        public async Task<string> GetEmailByFlowIdAsync(string flowId)
        {
            return await _cache.GetStringAsync($"FP_EMAIL_{flowId}");
        }

        public async Task ClearFlowAsync(string flowId)
        {
            await _cache.RemoveAsync($"FP_VERIFIED_{flowId}");
            await _cache.RemoveAsync($"FP_EMAIL_{flowId}");
        }
    }
}
