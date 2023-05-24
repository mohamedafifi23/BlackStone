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
            string paymentToken = await GetPaymentToken();

            return $"{_paymobConfig.IFrameUrl}/{_paymobConfig.IFrameId}?payment_token={paymentToken}";  
        }

        public async Task<string> GetPaymentToken()
        {
            int orderPriceInCents = 9898*100;

            var authToken = await CreateAuthenticationToken();
            var orderId = await RegisterPaymentOrder(authToken, orderPriceInCents);
            
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

            //_httpClient.BaseAddress = new Uri(_paymobConfig.BaseUrl);
            var jsonApiKey = new
            {
                auth_token = authToken,
                delivery_needed = "false",
                amount_cents = orderPriceInCents.ToString(),
                currency = "EGP",
                items = new List<string>()
            };
        var response = await _httpClient.PostAsJsonAsync("ecommerce/orders", jsonApiKey);

            if (response.IsSuccessStatusCode)
            {
                //var res1 = await response.Content.ReadAsStringAsync();
                var res = await response.Content.ReadFromJsonAsync<RegisteredPaymentOrder>();
                orderId = res.Id.ToString();
            }

            return orderId;
        }

        private async Task<string> CreatePaymentToken(string authToken, string orderId, int orderPriceInCents)
        {
            string paymentToken = "";

            //_httpClient.BaseAddress = new Uri(_paymobConfig.BaseUrl);
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
        var response = await _httpClient.PostAsJsonAsync("acceptance/payment_keys", jsonApiKey);

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadFromJsonAsync<PaymentToken>();
                paymentToken = res.Token;
            }

            return paymentToken;
        }

        public async Task<string> GetWalletPaymentToken()
        {
            int orderPriceInCents = 9898 * 100;

            var authToken = await CreateAuthenticationToken();
            var orderId = await RegisterPaymentOrder(authToken, orderPriceInCents);

            return await CreateWalletPaymentToken(authToken, orderId, orderPriceInCents);
        }

        private async Task<string> CreateWalletPaymentToken(string authToken, string orderId, int orderPriceInCents)
        {
            string paymentToken = "";

            //_httpClient.BaseAddress = new Uri(_paymobConfig.BaseUrl);
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
                integration_id = _paymobConfig.MobileWalletIdntegrationId
            };
            var response = await _httpClient.PostAsJsonAsync("acceptance/payment_keys", jsonApiKey);

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadFromJsonAsync<PaymentToken>();
                paymentToken = res.Token;
            }

            return paymentToken;
        }

        public async Task<string> CreateWalletRedirectUrl()
        {
            string redirectUrl = "";

            string paymentToken = await GetWalletPaymentToken();

            var walletHttpClient = _httpClientFactory.CreateClient();
            walletHttpClient.BaseAddress = new Uri(_paymobConfig.MobileWalletUrl);  

            var postData = new
            {
                source = new {
                    identifier = "01010101010",//get user phone number
                    subtype = "WALLET"
                },
                payment_token = paymentToken
            };

            var response = await walletHttpClient.PostAsJsonAsync("", postData);
            if (response.IsSuccessStatusCode)
            {
                var res2 = await response.Content.ReadAsStringAsync();
                var res = await response.Content.ReadFromJsonAsync<WalletPayRequestResponse>();

                redirectUrl = res.RedirectUrl;
            }

            return redirectUrl;
        }
    }
}
