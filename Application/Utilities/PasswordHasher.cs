using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities
{
    public static class PasswordHasher
    {
        private const int WorkFactor = 12;

        /// <summary>
        /// Hash password bằng BCrypt
        /// </summary>
        /// <param name="password">Mật khẩu cần hash</param>
        /// <returns>Password hash</returns>
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        /// <summary>
        /// Xác thực password với hash
        /// </summary>
        /// <param name="password">Mật khẩu nhập vào</param>
        /// <param name="passwordHash">Hash đã lưu trong database</param>
        /// <returns>True nếu khớp, False nếu không khớp</returns>
        public static bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra độ mạnh của password
        /// </summary>
        /// <param name="password">Mật khẩu cần kiểm tra</param>
        /// <param name="errorMessage">Thông báo lỗi nếu không hợp lệ</param>
        /// <returns>True nếu hợp lệ, False nếu không hợp lệ</returns>
        public static bool IsPasswordValid(string password, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Mật khẩu không được để trống";
                return false;
            }

            if (password.Length < 8)
            {
                errorMessage = "Mật khẩu phải có ít nhất 8 ký tự";
                return false;
            }

            if (password.Length > 100)
            {
                errorMessage = "Mật khẩu không được vượt quá 100 ký tự";
                return false;
            }

            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;
            bool hasSpecial = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (!char.IsLetterOrDigit(c)) hasSpecial = true;
            }

            if (!hasUpper)
            {
                errorMessage = "Mật khẩu phải chứa ít nhất một chữ hoa";
                return false;
            }

            if (!hasLower)
            {
                errorMessage = "Mật khẩu phải chứa ít nhất một chữ thường";
                return false;
            }

            if (!hasDigit)
            {
                errorMessage = "Mật khẩu phải chứa ít nhất một chữ số";
                return false;
            }

            if (!hasSpecial)
            {
                errorMessage = "Mật khẩu phải chứa ít nhất một ký tự đặc biệt";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
