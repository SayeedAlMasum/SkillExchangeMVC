//BkashService.cs
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SkillExchangeMVC.Models.BkashModels;
using System.Text;

namespace SkillExchangeMVC.Services
{
    public interface IBkashService
    {
        Task<BkashTokenResponse?> GetTokenAsync();
        Task<BkashCreatePaymentResponse?> CreatePaymentAsync(BkashCreatePaymentRequest request, string token);
        Task<BkashExecutePaymentResponse?> ExecutePaymentAsync(BkashExecutePaymentRequest request, string token);
        Task<BkashQueryPaymentResponse?> QueryPaymentAsync(string paymentId, string token);
        Task<BkashRefundResponse?> RefundPaymentAsync(BkashRefundRequest request, string token);
    }

    public class BkashService : IBkashService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _appKey;
        private readonly string _appSecret;

        public BkashService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            
            // For sandbox environment
            _baseUrl = _configuration["Bkash:BaseUrl"] ?? "https://tokenized.sandbox.bka.sh/v1.2.0-beta";
            _username = _configuration["Bkash:Username"] ?? "";
            _password = _configuration["Bkash:Password"] ?? "";
            _appKey = _configuration["Bkash:AppKey"] ?? "";
            _appSecret = _configuration["Bkash:AppSecret"] ?? "";
        }

        public async Task<BkashTokenResponse?> GetTokenAsync()
        {
            try
            {
                var requestData = new
                {
                    app_key = _appKey,
                    app_secret = _appSecret
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("username", _username);
                _httpClient.DefaultRequestHeaders.Add("password", _password);

                var response = await _httpClient.PostAsync($"{_baseUrl}/tokenized/checkout/token/grant", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<BkashTokenResponse>(responseContent);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<BkashCreatePaymentResponse?> CreatePaymentAsync(BkashCreatePaymentRequest request, string token)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("authorization", token);
                _httpClient.DefaultRequestHeaders.Add("x-app-key", _appKey);

                var response = await _httpClient.PostAsync($"{_baseUrl}/tokenized/checkout/create", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<BkashCreatePaymentResponse>(responseContent);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<BkashExecutePaymentResponse?> ExecutePaymentAsync(BkashExecutePaymentRequest request, string token)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("authorization", token);
                _httpClient.DefaultRequestHeaders.Add("x-app-key", _appKey);

                var response = await _httpClient.PostAsync($"{_baseUrl}/tokenized/checkout/execute", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<BkashExecutePaymentResponse>(responseContent);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<BkashQueryPaymentResponse?> QueryPaymentAsync(string paymentId, string token)
        {
            try
            {
                var requestData = new { paymentID = paymentId };
                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("authorization", token);
                _httpClient.DefaultRequestHeaders.Add("x-app-key", _appKey);

                var response = await _httpClient.PostAsync($"{_baseUrl}/tokenized/checkout/payment/status", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<BkashQueryPaymentResponse>(responseContent);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<BkashRefundResponse?> RefundPaymentAsync(BkashRefundRequest request, string token)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("authorization", token);
                _httpClient.DefaultRequestHeaders.Add("x-app-key", _appKey);

                var response = await _httpClient.PostAsync($"{_baseUrl}/tokenized/checkout/payment/refund", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<BkashRefundResponse>(responseContent);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}