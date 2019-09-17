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

namespace Twinvision.Piranha.RentVision.Controllers
{
    [Route("customer/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        public class UserPlanMetaData
        {
            public Guid UserPlanId { get; set; }
            public Guid UserId { get; set; }
        }

        [Route("createCustomer"), HttpPost]
        public IActionResult CreateCustomer()
        {
            return new JsonResult(HttpStatusCode.OK);
        }

        [Route("createPaymentRequest"), HttpPost]
        public async Task<IActionResult> CreatePaymentRequest(Guid userPlanId)
        {
            // TODO(Jesse): Fetch userPlan data here (name, price)

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
            IPaymentClient paymentClient = new PaymentClient("test_m6hyMf2rFV8UKDDJQwJSzuUVbF5ybr");
            PaymentResponse result = await paymentClient.CreatePaymentAsync(paymentRequest);

            UrlLink checkoutLink = result.Links.Checkout;

            return Redirect(checkoutLink.Href);

            //UserPlanMetaData metadataResponse = result.GetMetadata<UserPlanMetaData>();

            //return new JsonResult(result);
        }
    }
}