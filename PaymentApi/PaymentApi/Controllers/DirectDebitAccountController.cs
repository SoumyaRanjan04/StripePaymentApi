using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentApi.Model;
using PaymentApi.Services;

namespace PaymentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DirectDebitAccountController : ControllerBase
    {
        private readonly IDirectDebitAccountService _directDebitAccountService;

        public DirectDebitAccountController(IDirectDebitAccountService directDebitAccountService)
        {
            _directDebitAccountService = directDebitAccountService;
        }
        [HttpPost("Checkout")]
        public async Task<IActionResult> Checkout(CheckOutSessionModel model)
        {
            var result = await _directDebitAccountService.CreateCheckOutSession(model);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [HttpPost("createSubscribeAcc")]
        public async Task<IActionResult> CreateDirectDebit([FromBody] DirectDebitAccountRequest request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var res = await _directDebitAccountService.CreateSubscription(request); 
                    return Ok(res);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
            return BadRequest(ModelState);
        }
    }
}
