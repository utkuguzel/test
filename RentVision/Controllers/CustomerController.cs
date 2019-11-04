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
using static RentVision.Models.Configuration.Configuration;
using Newtonsoft.Json;

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
        public ApiHelper _apiHelper { get; set; }

        public CustomerController(IApi api, IHttpClientFactory clientFactory)
        {
            _apiHelper = new ApiHelper(api, clientFactory);
        }

        public async Task<string> CreateCustomerAsync(string email, string businessUnitName, HttpContext context)
        {
            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email },
                { "businessUnitName", businessUnitName }
            };
            var createCustomerRequest = await _apiHelper.SendApiCallAsync(ApiCalls.MollieCreateCustomer, urlParameters, context: context);
            var createCustomerResult = await createCustomerRequest.Content.ReadAsStringAsync();
            if (createCustomerRequest.IsSuccessStatusCode)
            {
                return createCustomerResult;
            }
            return null;
        }

        public async Task<List<PaymentResponse>> GetPaymentListAsync()
        {
            var paymentListRequest = await _apiHelper.SendApiCallAsync(ApiCalls.MollieGetPayments);
            var paymentListResponse = await paymentListRequest.Content.ReadAsStringAsync();
            if (paymentListRequest.IsSuccessStatusCode)
            {
                var paymentList = JsonConvert.DeserializeObject<List<PaymentResponse>>(paymentListResponse);
                return paymentList;
            }
            return null;
        }

        public async Task<List<CustomerResponse>> GetCustomerListAsync()
        {
            var customerListRequest = await _apiHelper.SendApiCallAsync(ApiCalls.MollieGetCustomers);
            var customerListResponse = await customerListRequest.Content.ReadAsStringAsync();
            if (customerListRequest.IsSuccessStatusCode)
            {
                var customerList = JsonConvert.DeserializeObject<List<CustomerResponse>>(customerListResponse);
                return customerList;
            }
            return null;
        }

        public bool DoesCustomerExist(string customer, List<CustomerResponse> customerList)
        {
            return customerList.Exists(c => c.Email == customer || c.Name == customer);
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
                RedirectUrl = $"{Website.Url}/setup",
                WebhookUrl = $"{BackOffice.Url}/{ApiCalls.PaymentWebhook.FullUrl()}"
            };

            // Set the metadata
            paymentRequest.SetMetadata(metadataRequest);

            // When we retrieve the payment response, we can convert our metadata back to our custom class
            var paymentClient = new PaymentClient(MollieController.GetMollieKey());
            var paymentResponse = await paymentClient.CreatePaymentAsync(paymentRequest);

            context.Session.SetString("paymentId", paymentResponse.Id.ToString());
            CookieHelper.SetCookie("paymentId", paymentResponse.Id.ToString(), context);
           
            return paymentResponse;
        }

        [HttpGet("deleteTestAccounts")]
        public async Task<IActionResult> DeleteTestAccounts()
        {
            var customerClient = new CustomerClient(MollieController.GetMollieKey());
            ListResponse<CustomerResponse> response = await customerClient.GetCustomerListAsync();

            foreach ( var account in response.Items )
            {
               await customerClient.DeleteCustomerAsync(account.Id);
            }

            return new JsonResult(HttpStatusCode.OK);
        }
    }
}