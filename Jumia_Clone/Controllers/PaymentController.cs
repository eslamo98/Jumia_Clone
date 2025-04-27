using Jumia_Clone.Models.DTOs.PaymentDTOs;
using Jumia_Clone.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jumia_Clone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("initiate")]
        public async Task<ActionResult<PaymentResponseDto>> InitiatePayment([FromBody] PaymentRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _paymentService.InitiatePaymentAsync(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("callback")]
        public async Task<IActionResult> PaymentCallback([FromBody] object payload)
        {
            var isValid = await _paymentService.ValidatePaymentCallback(payload.ToString());

            if (!isValid)
                return BadRequest("Invalid payment callback");

            return Ok();
        }
    }
}
