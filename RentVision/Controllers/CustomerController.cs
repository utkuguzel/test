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

namespace Twinvision.Piranha.RentVision.Controllers
{
    public class UserPlanMetaData
    {
        public Guid UserPlanId { get; set; }
        public Guid UserId { get; set; }
    }

    [Route("[controller]")]
    [ApiController]
    public class CustomerController : Controller
    {
        public static string MollieKeyLive { get; set; }
        public static string MollieKeyTest { get; set; }

        // Test siiiiiiii
        [Route("list"), HttpGet]
        public IActionResult List()
        {
            return new JsonResult(HttpStatusCode.OK);
        }

        [Route("createCustomer"), HttpPost]
        public async Task<IActionResult> CreateCustomerAsync(string email, string businessUnitName)
        {
            CustomerRequest customerRequest = new CustomerRequest()
            {
                Email = $"{email}",
                Name = $"{businessUnitName}",
                Locale = Locale.nl_NL
            };

            ICustomerClient customerClient = new CustomerClient(MollieKeyTest);
            CustomerResponse customerResponse = await customerClient.CreateCustomerAsync(customerRequest);

            return new JsonResult(HttpStatusCode.OK);
        }

        [Route("createPaymentRequest"), HttpPost]
        public async Task<IActionResult> CreatePaymentRequest(Guid userPlanId)
        {
            // TODO(Jesse): Fetch userPlan data here (name, price)
            // Also: add a webhook to check on payment status

            UserPlanMetaData metadataRequest = new UserPlanMetaData()
            {
                UserPlanId = userPlanId,
                UserId = Guid.NewGuid()
            };

            PaymentRequest paymentRequest = new PaymentRequest()
            {
                Amount = new Amount(Currency.EUR, "100.00"),
                Description = "{description}",
                RedirectUrl = "http://google.com"
            };

            // Set the metadata
            paymentRequest.SetMetadata(metadataRequest);

            // When we retrieve the payment response, we can convert our metadata back to our custom class
            IPaymentClient paymentClient = new PaymentClient(MollieKeyTest);
            PaymentResponse result = await paymentClient.CreatePaymentAsync(paymentRequest);

            UrlLink checkoutLink = result.Links.Checkout;

            return Redirect(checkoutLink.Href);
        }
    }
}