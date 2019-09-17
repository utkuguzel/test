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

    [Route("customer/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private static string _mollieKeyLive { get; set; }
        private static string _mollieKeyTest { get; set; }

        public CustomerController( string mollieKeyLive, string mollieKeyTest )
        {
            _mollieKeyLive = mollieKeyLive;
            _mollieKeyTest = mollieKeyTest;
        }

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

            ICustomerClient customerClient = new CustomerClient(_mollieKeyTest);
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
            IPaymentClient paymentClient = new PaymentClient(_mollieKeyTest);
            PaymentResponse result = await paymentClient.CreatePaymentAsync(paymentRequest);

            UrlLink checkoutLink = result.Links.Checkout;

            return Redirect(checkoutLink.Href);
        }
    }
}