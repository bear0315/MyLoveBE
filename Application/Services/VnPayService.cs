using Application.Interfaces;
using Application.Request.Payment;
using Application.Response.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(VnPaymentRequest request, string ipAddress)
        {
            var vnp_TmnCode = _configuration["VnPay:TmnCode"];
            var vnp_HashSecret = _configuration["VnPay:HashSecret"];
            var vnp_Url = _configuration["VnPay:Url"];
            var vnp_ReturnUrl = _configuration["VnPay:ReturnUrl"];

            // Build VNPay data
            var vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString()); 
            vnpay.AddRequestData("vnp_CreateDate", request.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", request.OrderDescription);
            vnpay.AddRequestData("vnp_OrderType", "other"); // Bill payment
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", request.BookingCode); // Unique transaction reference

            // Optional: Specify bank code (if you want to redirect directly to a bank)
            // vnpay.AddRequestData("vnp_BankCode", "NCB");

            var paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return paymentUrl;
        }

        public VnPaymentResponse ProcessPaymentCallback(IQueryCollection queryParams)
        {
            var vnpay = new VnPayLibrary();

            foreach (var (key, value) in queryParams)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            var vnp_HashSecret = _configuration["VnPay:HashSecret"];
            var vnp_SecureHash = queryParams["vnp_SecureHash"];

            // Validate signature
            bool isValidSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

            if (!isValidSignature)
            {
                return new VnPaymentResponse
                {
                    Success = false,
                    Message = "Invalid signature"
                };
            }

            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");

            // Response codes:
            // 00: Success
            // Others: Failed
            bool isSuccess = vnp_ResponseCode == "00" && vnp_TransactionStatus == "00";

            return new VnPaymentResponse
            {
                Success = isSuccess,
                Message = GetResponseMessage(vnp_ResponseCode),
                PaymentMethod = vnpay.GetResponseData("vnp_CardType"),
                OrderDescription = vnpay.GetResponseData("vnp_OrderInfo"),
                OrderId = vnpay.GetResponseData("vnp_TxnRef"),
                TransactionId = vnpay.GetResponseData("vnp_TransactionNo"),
                Token = vnpay.GetResponseData("vnp_SecureHash"),
                VnPayResponseCode = vnp_ResponseCode,
                Amount = Convert.ToDecimal(vnpay.GetResponseData("vnp_Amount")) / 100,
                BankCode = vnpay.GetResponseData("vnp_BankCode"),
                PaymentDate = ParseVnPayDate(vnpay.GetResponseData("vnp_PayDate"))
            };
        }

        public bool ValidateSignature(Dictionary<string, string> responseData, string secureHash)
        {
            var vnp_HashSecret = _configuration["VnPay:HashSecret"];
            var vnpay = new VnPayLibrary();

            foreach (var (key, value) in responseData)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            return vnpay.ValidateSignature(secureHash, vnp_HashSecret);
        }

        private string GetResponseMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
                "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
                "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
                "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
                "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
                "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
                "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
                "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
                "75" => "Ngân hàng thanh toán đang bảo trì.",
                "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
                "99" => "Các lỗi khác",
                _ => "Lỗi không xác định"
            };
        }

        private DateTime ParseVnPayDate(string vnPayDate)
        {
            try
            {
                return DateTime.ParseExact(vnPayDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            }
            catch
            {
                return DateTime.UtcNow;
            }
        }
    }

    // VnPay Library Helper Class
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayComparer());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayComparer());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            var data = new StringBuilder();

            foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            var queryString = data.ToString();

            baseUrl += "?" + queryString;
            var signData = queryString;

            if (signData.Length > 0)
            {
                signData = signData.Remove(signData.Length - 1, 1);
            }

            var vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnp_SecureHash;

            return baseUrl;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();
            var myChecksum = HmacSHA512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        private string GetResponseData()
        {
            var data = new StringBuilder();
            if (_responseData.ContainsKey("vnp_SecureHashType"))
            {
                _responseData.Remove("vnp_SecureHashType");
            }

            if (_responseData.ContainsKey("vnp_SecureHash"))
            {
                _responseData.Remove("vnp_SecureHash");
            }

            foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }

            return data.ToString();
        }
    }

    public class VnPayComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
