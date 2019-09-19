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
using RentVision.Models.Configuration;
using System.Net.Http;
using Piranha;

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
        public ApiHelper _apiHelper { get; set; }

        public CustomerController(IApi api, IHttpClientFactory clientFactory)
        {
            _apiHelper = new ApiHelper(api, clientFactory);
        }

        public async Task<JsonResult> CreateCustomerAsync(string email, string businessUnitName )
        {
            CustomerRequest customerRequest = new CustomerRequest()
            {
                Email = $"{email}",
                Name = $"{businessUnitName}",
                Locale = Locale.nl_NL
            };

            ICustomerClient customerClient = new CustomerClient(MollieKeyTest);
            CustomerResponse customerResponse = await customerClient.CreateCustomerAsync(customerRequest);

            // Set customer mollyId in db
            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email },
                { "mollyId", customerResponse.Id }
            };

            var setMollyIdResult = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.SetMollyId, HttpMethod.Post, urlParameters, context: HttpContext);

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
                Amount = new Amount(Currency.EUR, plan.Price.ToString()),
                Description = $"RentVision - {plan.Name}",
                RedirectUrl = $"http://localhost:53352/customer/paid",
                WebhookUrl = $"{Configuration.BackOffice.Protocol}://{Configuration.BackOffice.HostName}/mollie/PaymentWebhook"
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
                    // Create subscription if initial payment is paid
                    UserPlanMetaData metaDataResponse = result.GetMetadata<UserPlanMetaData>();
                    var subscriptionResponse = await CreateCustomerSubscriptionAsync(metaDataResponse, result);

                    if ( subscriptionResponse.Status == SubscriptionStatus.Active )
                    {
                        var urlParameters = new Dictionary<string, string>()
                        {
                            { "email", metaDataResponse.Email },
                            { "userPlanName", metaDataResponse.Plan.Name }
                        };

                        var setUserPlanResult = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.SetPlan, HttpMethod.Post, urlParameters, context: HttpContext);

                        if ( !setUserPlanResult.IsSuccessStatusCode )
                        {
                            return new JsonResult(new { StatusCode = HttpStatusCode.BadRequest, Value = "Failed to set user plan" });
                        }
                    }
                }

                // Clear paymentId from session
                HttpContext.Session.Remove("paymentId");

                return new JsonResult( new { StatusCode = HttpStatusCode.OK, Value = Enum.GetName(typeof(PaymentStatus), result.Status) });
            }

            return new JsonResult(HttpStatusCode.BadRequest);
        }

        public async Task<SubscriptionResponse> CreateCustomerSubscriptionAsync(UserPlanMetaData metaDataResponse, PaymentResponse result)
        {
            IMandateClient mandateclient = new MandateClient(MollieKeyTest);
            MandateResponse mandateResponse = await mandateclient.GetMandateAsync(metaDataResponse.CustomerId, result.MandateId);

            if (mandateResponse.Status == MandateStatus.Valid)
            {
                int payInterval = metaDataResponse.Plan.payInterval;
                string price = metaDataResponse.Plan.Price.ToString();
                string payIntervalProper = (payInterval == 1) ? "12 months" : "1 month";

                ISubscriptionClient subscriptionClient = new SubscriptionClient(MollieKeyTest);

                // TODO: WebhookUrl naar backoffice toevoegen
                ////////////////////////////////////////////////
                
                SubscriptionRequest subscriptionRequest = new SubscriptionRequest()
                {
                    Amount = new Amount(Currency.EUR, price),
                    Interval = payIntervalProper,
                    StartDate = DateTime.Now.AddMonths(payInterval == 1 ? 12 : 1),
                    Description = $"RentVision Subscription - {metaDataResponse.Plan.Name} ({payIntervalProper})",
                    WebhookUrl = $"{Configuration.BackOffice.Protocol}://{Configuration.BackOffice.HostName}/mollie/PaymentWebhook"
                };

                SubscriptionResponse subscriptionResponse = await subscriptionClient.CreateSubscriptionAsync(metaDataResponse.CustomerId, subscriptionRequest);

                return subscriptionResponse;
            }

            return null;
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