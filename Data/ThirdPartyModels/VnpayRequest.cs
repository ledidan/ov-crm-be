using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Data.ThirdPartyModels
{
    public class VnpayRequest
    {
        public string Version { get; set; } = "2.1.0";
        public string TmnCode { get; set; }
        public decimal Amount { get; set; }
        public string Command { get; set; } = "pay";
        public string CreateDate { get; set; }
        public string CurrCode { get; set; } = "VND";
        public string IpAddr { get; set; }
        public string Locale { get; set; } = "vn";
        public string OrderInfo { get; set; }
        public string OrderType { get; set; } = "250000";
        public string ReturnUrl { get; set; }
        public string TxnRef { get; set; }
        public string BankCode { get; set; }
        public bool Tokenize { get; set; }
        public string Token { get; set; }
        public string SecureHash { get; set; }
        public string ResponseCode { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }

        public Dictionary<string, string> ToQueryParams(string hashSecret)
        {
            var paramsDict = new Dictionary<string, string>
            {
                { "vnp_Version", Version },
                { "vnp_TmnCode", TmnCode },
                { "vnp_Amount", ((int)(Amount * 100)).ToString() },
                { "vnp_Command", Command },
                { "vnp_CreateDate", CreateDate },
                { "vnp_CurrCode", CurrCode },
                { "vnp_IpAddr", IpAddr },
                { "vnp_Locale", Locale },
                { "vnp_OrderInfo", OrderInfo },
                { "vnp_OrderType", OrderType },
                { "vnp_ReturnUrl", ReturnUrl },
                { "vnp_TxnRef", TxnRef }
            };

            if (!string.IsNullOrEmpty(BankCode))
                paramsDict.Add("vnp_BankCode", BankCode);
            if (Tokenize)
                paramsDict.Add("vnp_Token", "1");
            if (!string.IsNullOrEmpty(Token))
                paramsDict.Add("vnp_Token", Token);
            if (!string.IsNullOrEmpty(ResponseCode))
                paramsDict.Add("vnp_ResponseCode", ResponseCode);
            if (!string.IsNullOrEmpty(CardNumber))
                paramsDict.Add("vnp_CardNumber", CardNumber);
            if (!string.IsNullOrEmpty(ExpiryDate))
                paramsDict.Add("vnp_ExpiryDate", ExpiryDate);

            paramsDict = paramsDict.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                                   .OrderBy(kvp => kvp.Key)
                                   .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var queryString = string.Join("&", paramsDict.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            paramsDict["vnp_SecureHash"] = ComputeHmacSha512(hashSecret, queryString);

            return paramsDict;
        }

        public static VnpayRequest FromWebhookData(Dictionary<string, string> webhookData)
        {
            if (webhookData == null)
                throw new ArgumentNullException(nameof(webhookData), "Webhook data không được null nha!");

            var missingKeys = new List<string>();
            if (!webhookData.ContainsKey("vnp_SecureHash")) missingKeys.Add("vnp_SecureHash");
            if (!webhookData.ContainsKey("vnp_TxnRef")) missingKeys.Add("vnp_TxnRef");
            if (!webhookData.ContainsKey("vnp_ResponseCode")) missingKeys.Add("vnp_ResponseCode");

            if (missingKeys.Any())
            {
                Console.WriteLine($"Webhook thiếu các key bắt buộc: {string.Join(", ", missingKeys)}");
            }

            return new VnpayRequest
            {
                TxnRef = webhookData.GetValueOrDefault("vnp_TxnRef"),
                ResponseCode = webhookData.GetValueOrDefault("vnp_ResponseCode"),
                SecureHash = webhookData.GetValueOrDefault("vnp_SecureHash"),
                Token = webhookData.GetValueOrDefault("vnp_Token"),
                CardNumber = webhookData.GetValueOrDefault("vnp_CardNumber"),
                ExpiryDate = webhookData.GetValueOrDefault("vnp_ExpiryDate"),
                TmnCode = webhookData.GetValueOrDefault("vnp_TmnCode"),
                Amount = webhookData.ContainsKey("vnp_Amount") ? decimal.Parse(webhookData["vnp_Amount"]) / 100 : 0,
                OrderInfo = webhookData.GetValueOrDefault("vnp_OrderInfo"),
                BankCode = webhookData.GetValueOrDefault("vnp_BankCode")
            };
        }

        private string ComputeHmacSha512(string key, string data)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}