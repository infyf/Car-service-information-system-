using AutoServ.Core.Interfaces;
using AutoServ.Core.Models;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AutoServ.Infrastructure.Services
{
    public class LiqPayService : IPaymentService
    {
        private readonly string _publicKey;
        private readonly string _privateKey;

        public LiqPayService(IConfiguration config)
        {
            _publicKey = config["LiqPaySettings:PublicKey"];
            _privateKey = config["LiqPaySettings:PrivateKey"];
        }

        public Result<(string Data, string Signature)> GeneratePaymentData(int orderId, decimal amount, string description)
        {
            // 1. Формуємо словник параметрів
            var paramsDict = new Dictionary<string, string>
            {
                { "action", "pay" },
                { "amount", amount.ToString("F2") },
                { "currency", "UAH" },
                { "description", description },
                { "order_id", orderId.ToString() },
                { "version", "3" },
                { "public_key", _publicKey },
                { "sandbox", "1" } 
            };

            
            string json = JsonSerializer.Serialize(paramsDict);

            
            string data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            
            string signature = GenerateSignature(data);

            return Result<(string, string)>.Success((data, signature));
        }

        public Result<int> ProcessCallback(string data, string signature)
        {
            
            string expectedSignature = GenerateSignature(data);
            if (expectedSignature != signature)
            {
                return Result<int>.Failure("Невірний цифровий підпис.");
            }


            string jsonStr;
            try
            {
                byte[] bytes = Convert.FromBase64String(data);
                jsonStr = Encoding.UTF8.GetString(bytes);
            }
            catch (Exception)
            {
                return Result<int>.Failure("Помилка декодування даних.");
            }

 
            using var doc = JsonDocument.Parse(jsonStr);
            JsonElement root = doc.RootElement;

            string status = root.GetProperty("status").GetString();
            int orderId = root.GetProperty("order_id").GetInt32();

      
            if (status == "success" || status == "sandbox")
            {
                return Result<int>.Success(orderId);
            }
            else
            {
                return Result<int>.Failure($"Оплата невдала. Статус: {status}");
            }
        }

        // приватний метод для генерації SHA1 підпису (офіційна формула LiqPay)
        private string GenerateSignature(string data)
        {
            string stringToSign = _privateKey + data + _privateKey;

            using (var sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}