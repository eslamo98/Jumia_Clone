using Jumia_Clone.Models.DTOs.PaymentDTOs;

namespace Jumia_Clone.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto request);
        Task<bool> ValidatePaymentCallback(string payload);
    }
}
