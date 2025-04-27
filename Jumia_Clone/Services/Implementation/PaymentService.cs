using Jumia_Clone.Models.DTOs.PaymentDTOs;
using Jumia_Clone.Services.Interfaces;
using System.Text.Json;

namespace Jumia_Clone.Services.Implementation
{
    public class PaymobPaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _integrationId;

        public PaymobPaymentService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiKey = _configuration["Paymob:ApiKey"];
            _integrationId = _configuration["Paymob:IntegrationId"];
        }

        public async Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto request)
        {
            try
            {
                // Step 1: Authentication Request
                var authToken = await GetAuthenticationTokenAsync();

                // Step 2: Order Registration
                var orderId = await RegisterOrderAsync(authToken, request);

                // Step 3: Payment Key Generation
                var paymentKey = await GeneratePaymentKeyAsync(authToken, orderId, request);

                // Step 4: Generate Payment URL based on payment method
                var paymentUrl = await GetPaymentUrlAsync(paymentKey, request.PaymentMethod);

                return new PaymentResponseDto
                {
                    Success = true,
                    PaymentUrl = paymentUrl,
                    TransactionId = orderId.ToString(),
                    Message = "Payment initiated successfully"
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = $"Payment initiation failed: {ex.Message}"
                };
            }
        }

        private async Task<string> GetAuthenticationTokenAsync()
        {
            var authRequest = new
            {
                api_key = _apiKey
            };

            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens", authRequest);
            var result = await response.Content.ReadFromJsonAsync<JsonDocument>();
            return result.RootElement.GetProperty("token").GetString();
        }

        private async Task<string> RegisterOrderAsync(string authToken, PaymentRequestDto request)
        {
            var orderRequest = new
            {
                auth_token = authToken,
                delivery_needed = "false",
                amount_cents = (int)(request.Amount * 100),
                currency = request.Currency,
                items = new[]
                {
                    new
                    {
                        name = $"Order #{request.OrderId}",
                        amount_cents = (int)(request.Amount * 100),
                        description = "Order payment",
                        quantity = 1
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/ecommerce/orders", orderRequest);
            var result = await response.Content.ReadFromJsonAsync<JsonDocument>();
            return result.RootElement.GetProperty("id").GetInt64().ToString();
        }

        private async Task<string> GeneratePaymentKeyAsync(string authToken, string orderId, PaymentRequestDto request)
        {
            var paymentKeyRequest = new
            {
                auth_token = authToken,
                amount_cents = (int)(request.Amount * 100),
                expiration = 3600,
                order_id = orderId,
                billing_data = new
                {
                    first_name = "NA",
                    last_name = "NA",
                    email = "NA@email.com",
                    phone_number = "NA",
                    street = "NA",
                    city = "NA",
                    country = "NA",
                    state = "NA",
                    postal_code = "NA",
                    building = "NA",
                    floor = "NA",
                    apartment = "NA"
                },
                currency = request.Currency,
                integration_id = _integrationId
            };

            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/acceptance/payment_keys", paymentKeyRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Payment key generation failed. Status: {response.StatusCode}, Error: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<JsonDocument>();
            return result.RootElement.GetProperty("token").ToString();
        }
        private async Task<string> GetPaymentUrlAsync(string paymentKey, string paymentMethod)
        {
            // Different URLs based on payment method
            switch (paymentMethod.ToLower())
            {
                case "card":
                    return $"https://accept.paymob.com/api/acceptance/iframes/{_configuration["Paymob:CardIframeId"]}?payment_token={paymentKey}";
                case "vodafone":
                    return $"https://accept.paymob.com/api/acceptance/payments/pay?payment_token={paymentKey}&source=mobile_wallet";
                case "paypal":
                    return $"https://accept.paymob.com/api/acceptance/payments/pay?payment_token={paymentKey}&source=paypal";
                default:
                    throw new ArgumentException("Invalid payment method");
            }
        }

        public async Task<bool> ValidatePaymentCallback(string payload)
        {
            // Implement callback validation logic here
            return true;
        }
    }
}
