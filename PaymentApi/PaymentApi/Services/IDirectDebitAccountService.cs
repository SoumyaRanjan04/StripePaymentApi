using PaymentApi.Model;

namespace PaymentApi.Services
{
    public interface IDirectDebitAccountService
    {
        Task<string> CreateCheckOutSession(CheckOutSessionModel model);
        Task<string> CreateSubscription(DirectDebitAccountRequest model);
    }
}
