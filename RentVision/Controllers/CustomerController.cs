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
using Mollie.Api.Models.Mandate;
using Mollie.Api.Models.List;
using Mollie.Api.Models.Subscription;

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

        public async Task<string> CreatePaymentRequest(UserPlan plan, string email, string customerId, HttpContext context)
        {
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
                Amount = new Amount(Currency.EUR, "0.01"),
                Description = $"RentVision - {plan.Name}",
                RedirectUrl = $"http://localhost:53352/customer/paid"
            };

            // Set the metadata
            paymentRequest.SetMetadata(metadataRequest);

            // When we retrieve the payment response, we can convert our metadata back to our custom class
            IPaymentClient paymentClient = new PaymentClient(MollieKeyTest);
            PaymentResponse result = await paymentClient.CreatePaymentAsync(paymentRequest);

            context.Session.SetString("paymentId", result.Id.ToString());
           
            UrlLink checkoutLink = result.Links.Checkout;

            return checkoutLink.Href;
        }

        [HttpGet("paid/{paymentId?}")]
        public async Task<JsonResult> PaidAsync(string paymentId = null)
        {
            paymentId = ( paymentId != null ) ? paymentId : HttpContext.Session.GetString("paymentId");

            if ( paymentId != null )
            {
                IPaymentClient paymentClient = new PaymentClient(MollieKeyTest);
                PaymentResponse result = await paymentClient.GetPaymentAsync(paymentId);

                if ( result.Status == PaymentStatus.Paid )
                {
                    // TODO: Verplaatsen naar andere methode

                    // Create customer subscription
                    UserPlanMetaData metaDataResponse = result.GetMetadata<UserPlanMetaData>();

                    IMandateClient mandateclient = new MandateClient(MollieKeyTest);
                    MandateResponse mandateResponse = await mandateclient.GetMandateAsync(metaDataResponse.CustomerId, result.MandateId);

                    if ( mandateResponse.Status == MandateStatus.Valid )
                    {
                        int payInterval = metaDataResponse.Plan.payInterval;
                        string price = metaDataResponse.Plan.Price.ToString();
                        string payIntervalProper = (payInterval == 1) ? "12 months" : "1 month";

                        ISubscriptionClient subscriptionClient = new SubscriptionClient(MollieKeyTest);
                        SubscriptionRequest subscriptionRequest = new SubscriptionRequest()
                        {
                            Amount = new Amount(Currency.EUR, price),
                            Interval = payIntervalProper,
                            Description = $"RentVision Subscription - {metaDataResponse.Plan.Name} ({payIntervalProper})"
                        };

                        SubscriptionResponse subscriptionResponse = await subscriptionClient.CreateSubscriptionAsync(metaDataResponse.CustomerId, subscriptionRequest);

                        if ( subscriptionResponse.Status == SubscriptionStatus.Active )
                        {
                            return new JsonResult(new { StatusCode = HttpStatusCode.OK, Value = $"{subscriptionResponse.Description} - {Enum.GetName(typeof(SubscriptionStatus), subscriptionResponse.Status)}" });
                        }
                    }

                    return new JsonResult(mandateResponse);
                }

                // Clear paymentId from session
                HttpContext.Session.Remove("paymentId");

                return new JsonResult( new { StatusCode = HttpStatusCode.OK, Value = Enum.GetName(typeof(PaymentStatus), result.Status) });
            }

            return new JsonResult(HttpStatusCode.BadRequest);
        }

        [HttpGet("deleteTestAccounts")]
        public async Task<IActionResult> DeleteTestAccounts()
        {
            ICustomerClient customerClient = new CustomerClient(MollieKeyTest);
            ListResponse<CustomerResponse> response = await customerClient.GetCustomerListAsync();

            foreach ( var account in response.Items )
            {
               await customerClient.DeleteCustomerAsync(account.Id);
            }

            return new JsonResult(HttpStatusCode.OK);
        }
    }
}