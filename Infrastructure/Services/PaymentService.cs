using Core.IServices;
using Core.ServiceHelpers.PaymentService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymobConfiguration _paymobConfig;
        private readonly ILogger<PaymentService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;

        public PaymentService(PaymobConfiguration paymobConfig, ILogger<PaymentService> logger
            , IHttpClientFactory httpClientFactory)
        {
            _paymobConfig = paymobConfig;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient();
        }

        public async Task<string> CreateIFrameUrlForCardPayment()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetPaymentToken()
        {
            int orderPriceInCents = 9898*100;

            string authToken = await CreateAuthenticationToken();
            string orderId = await RegisterPaymentOrder(authToken, orderPriceInCents);
            
            return await CreatePaymentToken(authToken, orderId, orderPriceInCents);
        }

        private async Task<string> CreateAuthenticationToken()
        {
            string authToken = "";

            _httpClient.BaseAddress = new Uri(_paymobConfig.BaseUrl);
            var jsonApiKey = new { api_key = _paymobConfig.ApiKey };
            var response = await _httpClient.PostAsJsonAsync("auth/tokens", jsonApiKey);

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
                authToken = res.Token;
            }

            return authToken;
        }

        private async Task<string> RegisterPaymentOrder(string authToken, int orderPriceInCents)
        {
            string orderId = "";

            _httpClient.BaseAddress = new Uri(_paymobConfig.BaseUrl);
            var jsonApiKey = new
            {
                auth_token = authToken,
                delivery_needed = "false",
                amount_cents = orderPriceInCents.ToString(),
                currency = "EGP",
                items = new List<string>()
            };
        var response = await _httpClient.PostAsJsonAsync("auth/tokens", jsonApiKey);

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadFromJsonAsync<RegisteredPaymentOrder>();
                orderId = res.OrderId;
            }

            return orderId;
        }

        private async Task<string> CreatePaymentToken(string authToken, string orderId, int orderPriceInCents)
        {
            string paymentToken = "";

            _httpClient.BaseAddress = new Uri(_paymobConfig.BaseUrl);
            var jsonApiKey = new
            {
                auth_token = authToken,
                amount_cents = orderPriceInCents,
                expiration = 3600,
                order_id = orderId,
                billing_data = new
                {
                    apartment = "803",
                    email = "mohammedafifi153@gmail.com",
                    floor = "42",
                    first_name = "Clifford",
                    street = "Ethan Land",
                    building = "8028",
                    phone_number = "+86(8)9135210487",
                    shipping_method = "PKG",
                    postal_code = "01898",
                    city = "Jaskolskiburgh",
                    country = "CR",
                    last_name = "Nicolas",
                    state = "Utah"
                },
                currency = "EGP",
                integration_id = _paymobConfig.IntegrationId
            };
        var response = await _httpClient.PostAsJsonAsync("auth/tokens", jsonApiKey);

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadFromJsonAsync<PaymentToken>();
                paymentToken = res.Token;
            }

            return paymentToken;
        }
    }
}
