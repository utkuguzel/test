using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mollie.Api;
using Mollie.Api.Models.Payment.Request;
using Mollie.Api.Models;
using Mollie.Api.Client;
using Mollie.Api.Client.Abstract;
using Mollie.Api.Models.Payment.Response;
using Mollie.Api.Models.Url;
using System.Net;
using Mollie.Api.Models.Customer;
using Mollie.Api.Models.Payment;
using Twinvision.Piranha.RentVision.Helpers;
using RentVision.Models;

namespace Twinvision.Piranha.RentVision.Controllers
{
    public class UserPlanMetaData
    {
        public string CustomerId { get; set; }
        public string Email { get; set; }
        public UserPlan Plan { get; set; }
    }

    [Route("[controller]")]
    public class CustomerController : Controller
    {
        public static string MollieKeyLive { get; set; }
        public static string MollieKeyTest { get; set; }

        public static async Task<JsonResult> CreateCustomerAsync(string email, string businessUnitName)
        {
            CustomerRequest customerRequest = new CustomerRequest()
            {
                Email = $"{email}",
                Name = $"{businessUnitName}",
                Locale = Locale.nl_NL
            };

            ICustomerClient customerClient = new CustomerClient(MollieKeyTest);
            CustomerResponse customerResponse = await customerClient.CreateCustomerAsync(customerRequest);

            return new JsonResult(customerResponse.Id);
        }

        public async Task<string> CreatePaymentRequest(UserPlan plan, string email, string customerId)
        {
            // TODO(Jesse): Add a webhook to check on payment status

            UserPlanMetaData metadataRequest = new UserPlanMetaData()
            {
                CustomerId = customerId,
                Email = email,
                Plan = plan
            };

            PaymentRequest paymentRequest = new PaymentRequest()
            {
                CustomerId = $"{customerId}",
                SequenceType = SequenceType.First,
                Amount = new Amount(Currency.EUR, plan.Price.ToString()),
                Description = $"RentVision - {plan.Name}",
                RedirectUrl = $"http://localhost:53352/customer/paid/{customerId}"
            };

            // Set the metadata
            paymentRequest.SetMetadata(metadataRequest);

            // When we retrieve the payment response, we can convert our metadata back to our custom class
            IPaymentClient paymentClient = new PaymentClient(MollieKeyTest);
            PaymentResponse result = await paymentClient.CreatePaymentAsync(paymentRequest);

            UrlLink checkoutLink = result.Links.Checkout;

            return checkoutLink.Href;
        }

        [HttpGet("paid/{customerId}")]
        public JsonResult Paid(string customerId)
        {
            //TODO: Mandate aanmaken

            //ICustomerClient customerClient = new CustomerClient("{yourApiKey}");
            //CustomerResponse customerResponse = await customerClient.GetCustomerAsync(customerId);

            return new JsonResult(HttpStatusCode.OK);
        }
    }
}