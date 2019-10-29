using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using RentVision.Helpers;

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

        public async Task<JsonResult> CreateCustomerAsync(string email, string businessUnitName, HttpContext context)
        {
            var apiLoginKey = context.Session.GetString("ApiLoginKey") ?? CookieHelper.GetCookie("ApiLoginKey", HttpContext);
            CustomerRequest customerRequest = new CustomerRequest()
            {
                Email = $"{email}",
                Name = $"{businessUnitName}",
                Locale = Locale.nl_NL
            };

            ICustomerClient customerClient = new CustomerClient(MollieKeyTest);
            CustomerResponse customerResponse = await customerClient.CreateCustomerAsync(customerRequest);

            // Set customer MollieId in db
            var urlParameters = new Dictionary<string, string>()
            {
                { "MollieId", customerResponse.Id }
            };

            var setMollieIdResult = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.SetMollieId, HttpMethod.Post, urlParameters, context: context);

            return new JsonResult(customerResponse.Id);
        }

        public async Task<ListResponse<PaymentResponse>> GetPaymentListAsync(string customerId = null)
        {
            IPaymentClient paymentClient = new PaymentClient(MollieKeyTest);
            ListResponse<PaymentResponse> response = null;
            if (customerId != null)
            {
                response = await paymentClient.GetPaymentListAsync(profileId: customerId);
            }
            else
            {
                response = await paymentClient.GetPaymentListAsync();
            }
            return response;
        }

        public async Task<ListResponse<CustomerResponse>> GetCustomerListAsync()
        {
            ICustomerClient customerClient = new CustomerClient(MollieKeyTest);
            ListResponse<CustomerResponse> response = await customerClient.GetCustomerListAsync();
            return response;
        }

        public bool DoesCustomerExist(string customer, ListResponse<CustomerResponse> customerList)
        {
            return customerList.Items.Exists(c => c.Email == customer || c.Name == customer);
        }

        public async Task<PaymentResponse> CreatePaymentRequest(UserPlan plan, string email, string customerId, HttpContext context)
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
                RedirectUrl = $"{Configuration.Website.Url}/paid",
                WebhookUrl = $"{Configuration.Website.Url}/api/{Configuration.ApiCalls.PaymentWebhook}"
            };

            // Set the metadata
            paymentRequest.SetMetadata(metadataRequest);

            // When we retrieve the payment response, we can convert our metadata back to our custom class
            IPaymentClient paymentClient = new PaymentClient(MollieKeyTest);
            PaymentResponse result = await paymentClient.CreatePaymentAsync(paymentRequest);

            context.Session.SetString("paymentId", result.Id.ToString());
            CookieHelper.SetCookie("paymentId", result.Id.ToString(), context);
           
            return result;
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