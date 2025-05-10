using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Data.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace ServerLibrary.Libraries
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());
        private readonly ILogger<VnPayLibrary> _logger;

        public VnPayLibrary(ILogger<VnPayLibrary> logger)
        {
            _logger = logger;
        }

        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    AddResponseData(key, value);
                }
            }

            var dataLog = string.Join(", ", _responseData.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            _logger.LogInformation($"Webhook VNPAY params: {dataLog}");

            var missingKeys = new List<string>();
            if (!_responseData.ContainsKey("vnp_SecureHash")) missingKeys.Add("vnp_SecureHash");
            if (!_responseData.ContainsKey("vnp_TxnRef")) missingKeys.Add("vnp_TxnRef");
            if (!_responseData.ContainsKey("vnp_ResponseCode")) missingKeys.Add("vnp_ResponseCode");

            if (missingKeys.Any())
            {
                _logger.LogWarning($"Webhook thiếu key: {string.Join(", ", missingKeys)}");
                return new PaymentResponseModel { Success = false, VnPayResponseCode = "99", Message = "Missing required keys" };
            }

            var orderId = GetResponseData("vnp_TxnRef");
            var vnPayTranId = GetResponseData("vnp_TransactionNo");
            var vnpResponseCode = GetResponseData("vnp_ResponseCode");
            var vnpSecureHash = GetResponseData("vnp_SecureHash");
            var orderInfo = GetResponseData("vnp_OrderInfo");

            var checkSignature = ValidateSignature(vnpSecureHash, hashSecret);
            _logger.LogInformation($"Webhook signature check: {(checkSignature ? "Valid" : "Invalid")}");

            if (!checkSignature)
            {
                return new PaymentResponseModel
                {
                    Success = false,
                    VnPayResponseCode = "97",
                    Message = "Invalid signature"
                };
            }

            return new PaymentResponseModel
            {
                Success = vnpResponseCode.Equals("00"),
                PaymentMethod = "VnPay",
                OrderDescription = orderInfo,
                OrderId = orderId,
                PaymentId = vnPayTranId,
                TransactionId = vnPayTranId,
                Token = vnpSecureHash,
                VnPayResponseCode = vnpResponseCode,
                Message = "Confirm Success"
            };
        }

        public string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;
                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    }
                    ipAddress = remoteIpAddress?.ToString() ?? "127.0.0.1";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Lỗi lấy IP: {ex.Message}");
                ipAddress = "127.0.0.1";
            }
            _logger.LogWarning($"IP Address: {ipAddress}");
            return ipAddress;
        }

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData[key] = value;
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData[key] = value;
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var queryString = string.Join("&", _requestData
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

            var signData = queryString;
            var vnpSecureHash = HmacSha512(vnpHashSecret, signData);

            var url = $"{baseUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";
            _logger.LogInformation($"VNPAY PayUrl: {url}");
            _logger.LogInformation($"VNPAY SecureHash: {vnpSecureHash}");

            return url;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseDataRaw();
            var myChecksum = HmacSha512(secretKey, rspRaw);
            _logger.LogInformation($"Webhook raw data: {rspRaw}");
            _logger.LogInformation($"Webhook computed hash: {myChecksum}, received: {inputHash}");
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSha512(string key, string inputData)
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

        private string GetResponseDataRaw()
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
                data.Length--; // Xóa '&' cuối
            }

            return data.ToString();
        }
    }

    public class VnPayCompare : IComparer<string>
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