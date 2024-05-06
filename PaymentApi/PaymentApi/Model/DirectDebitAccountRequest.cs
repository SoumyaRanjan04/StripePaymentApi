using System.ComponentModel.DataAnnotations;

namespace PaymentApi.Model
{
    public class DirectDebitAccountRequest
    {
        public string AccountNumber { get; set; }
        public string SortCode { get; set; }
        public decimal Amount { get; set; }
        public BillingDetails BillingDetails { get; set; }
        public ProductDetails ProductDetails { get; set; }
    }

    public class BillingDetails
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Address Address { get; set; }
    }

    public class Address
    {
        public string City { get; set; }
        public string Country { get; set; }
        public string Line1 { get; set; }
        public string PostalCode { get; set; }
        public string State { get; set; }
    }
    public class ProductDetails
    {
        public string ProductName { get; set; }
        public DateTime StartDate { get; set; }
        public long Amount { get; set; }
        public string Currency {  get; set; }
        public string Interval { get; set; }
        public int IntervalCount { get; set; }
    }

    public class CheckOutSessionModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public List<string> Images { get; set; }
        public string Amount { get; set; }
    }
    public class DirectDebitRequest
    {
        public string Amount { get; set; }
        public string Iban { get; set; }
        public string CustomerName { get; set; }
    }

}
