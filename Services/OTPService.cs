using System.Security.Cryptography;

namespace InkVault.Services
{
    public interface IOTPService
    {
        string GenerateOTP(int length = 6);
        bool ValidateOTP(string providedOtp, string storedOtp);
    }

    public class OTPService : IOTPService
    {
        public string GenerateOTP(int length = 6)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[length];
                rng.GetBytes(tokenData);
                
                var otp = string.Empty;
                foreach (byte b in tokenData)
                {
                    otp += (b % 10).ToString();
                }
                
                return otp;
            }
        }

        public bool ValidateOTP(string providedOtp, string storedOtp)
        {
            if (string.IsNullOrEmpty(providedOtp) || string.IsNullOrEmpty(storedOtp))
                return false;

            return providedOtp == storedOtp;
        }
    }
}
