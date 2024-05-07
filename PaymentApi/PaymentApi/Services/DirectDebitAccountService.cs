using Microsoft.Extensions.Options;
using PaymentApi.Model;
using Stripe;
using Stripe.Checkout;

namespace PaymentApi.Services
{
    public class DirectDebitAccountService : IDirectDebitAccountService
    {
        private readonly StripeClient _stripeClient;
        public DirectDebitAccountService(IOptions<StripeOptions> stripeOptions)
        {
            _stripeClient = new StripeClient(stripeOptions.Value.SecretKey);

        }
        #region CheckOut
        public async Task<string> CreateCheckOutSession(CheckOutSessionModel model)
        {
            var currency = "GBP";
            StripeConfiguration.ApiKey = _stripeClient.ApiKey;
            var option = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "bacs_debit"
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency,
                            UnitAmount = Convert.ToInt32(model.Amount) * 100,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = model.Name,
                                Description =model.Description,
                                Images=model.Images,

                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel",
                CustomerEmail = model.Email,
                BillingAddressCollection = "required",
                ShippingAddressCollection = new SessionShippingAddressCollectionOptions
                {
                    AllowedCountries = new List<string> { "GB" }
                }

            };
            var service = new SessionService();
            try
            {
                var session = await service.CreateAsync(option);
                return session.Url;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion

        #region SubscriptionAccount
        public async Task<string> CreateSubscription(DirectDebitAccountRequest model)
        {
            string paymentMethodId = await CreatePaymentMethod(model);

            string customerId = await CreateCustomer(model, paymentMethodId);
            string planId = await CreatePlanId(model.ProductDetails.ProductName, model.ProductDetails.Amount, model.ProductDetails.Currency, model.ProductDetails.Interval, model.ProductDetails.IntervalCount);

            return await CreateSubscription(customerId, planId, paymentMethodId);
        }
        #endregion

        #region MethodForPlanCreate
        private async Task<string> CreatePlanId(string productName, long Amount, string currency, string interval, int intervalcount)
        {
            try
            {
                var options = new PlanCreateOptions
                {
                    Product = new PlanProductOptions
                    {
                        Name = productName,
                    },
                    Amount = Amount,
                    Currency = currency,
                    Interval = interval,
                    IntervalCount = intervalcount,
                };

                var service = new PlanService(_stripeClient);
                var plan = service.Create(options);

                return plan.Id;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion

        #region MethodForCreateSubscription
        private async Task<string> CreateSubscription(string customerId, string planId, string paymentMethodId)
        {
            try
            {
                var options = new SubscriptionCreateOptions
                {
                    DefaultPaymentMethod= paymentMethodId,
                    Customer = customerId,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Plan = planId,
                        }
                    },
                    TrialPeriodDays = null,  
                    TrialFromPlan = false,
                    CollectionMethod = "charge_automatically",
                    PaymentBehavior= "default_incomplete",

                };

                var service = new SubscriptionService(_stripeClient);
                var subscription = service.Create(options);
                var invoiceService = new InvoiceService(_stripeClient);
                var invoice = await invoiceService.GetAsync(subscription.LatestInvoiceId);
                return invoice.HostedInvoiceUrl;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion

        #region MethodForCustomerCreate
        private async Task<string> CreateCustomer(DirectDebitAccountRequest model, string PaymentmetyhodId)
        {
            try
            {
                var customerService = new CustomerService(_stripeClient);
                var existingCustomer = await customerService.ListAsync(new CustomerListOptions { Email = model.BillingDetails.Email, Limit = 1 });
                if (existingCustomer.Any())
                {
                    var updateOptions = new CustomerUpdateOptions
                    {
                        InvoiceSettings = new CustomerInvoiceSettingsOptions
                        {
                            DefaultPaymentMethod = PaymentmetyhodId
                        }
                    };
                    var updatedCustomer = await customerService.UpdateAsync(existingCustomer.First().Id, updateOptions);
                    return updatedCustomer.Id;
                }
                var customerCreateOptions = new CustomerCreateOptions
                {
                    PaymentMethod = PaymentmetyhodId,
                    Email = model.BillingDetails.Email,
                    Name = model.BillingDetails.Name,
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = PaymentmetyhodId
                    }
                };
                var newCustomer = await customerService.CreateAsync(customerCreateOptions);
                return newCustomer.Id;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion

        #region MethodForCreatepaymentMethod
        private async Task<string> CreatePaymentMethod(DirectDebitAccountRequest model)
        {
            try
            {

                var options = new PaymentMethodCreateOptions
                {
                    Type = "bacs_debit",
                    BacsDebit = new PaymentMethodBacsDebitOptions
                    {
                        AccountNumber = model.AccountNumber,
                        SortCode = model.SortCode,
                    },
                    BillingDetails = new PaymentMethodBillingDetailsOptions
                    {
                        Name = model.BillingDetails.Name,
                        Email = model.BillingDetails.Email,
                        Phone = model.BillingDetails.Phone,
                        Address = new AddressOptions
                        {
                            City = model.BillingDetails.Address.City,
                            Country = model.BillingDetails.Address.Country,
                            Line1 = model.BillingDetails.Address.Line1,
                            PostalCode = model.BillingDetails.Address.PostalCode,
                            State = model.BillingDetails.Address.State
                        }
                    }
                };
                var paymentMethodService = new PaymentMethodService(_stripeClient);
                var paymentMethod = await paymentMethodService.CreateAsync(options);
                return paymentMethod.Id;
            }
            catch (Exception ex)
            {
                return ex.Message;

            }
        }
