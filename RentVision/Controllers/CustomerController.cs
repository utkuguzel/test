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
using RentVision.Controllers;
using System.Linq;

namespace Twinvision.Piranha.RentVision.Controllers
{
    public class UserPlanMetaData
    {
        public string CustomerId { get; set; }
        public string Email { get; set; }
        public Plan Plan { get; set; }
        public bool IsUpgrade { get; set; } = false;
        public string UpgradePrice { get; set; }
    }

    [Route("[controller]")]
    public class CustomerController : Controller
    {
        public ApiHelper _apiHelper { get; set; }
        private readonly IApi _api;
        private readonly IHttpClientFactory _clientFactory;

        public CustomerController(IApi api, IHttpClientFactory clientFactory)
        {
            _api = api;
            _clientFactory = clientFactory;
            _apiHelper = new ApiHelper(api, clientFactory);
        }

        public async Task<CustomerResponse> CreateCustomerAsync(string email, string businessUnitName, HttpContext context)
        {
            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email },
                { "businessUnitName", businessUnitName }
            };
            var createCustomerCall = await _apiHelper.SendApiCallAsync(ApiCalls.MollieCreateCustomer, urlParameters, context: context);
            var createCustomerResponse = await createCustomerCall.Content.ReadAsStringAsync();
            if (createCustomerCall.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<CustomerResponse>(createCustomerResponse);
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

        public async Task<CustomerResponse> GetCustomerAsync(string email)
        {
            var customerList = await GetCustomerListAsync();
            var customer = customerList.FirstOrDefault(c => c.Email == email);
            return customer;
        }

        public async Task<CustomerResponse> GetCustomerFromIdAsync(string customerId)
        {
            var customerList = await GetCustomerListAsync();
            var customer = customerList.FirstOrDefault(c => c.Id == customerId);
            return customer;
        }

        public async Task<PaymentResponse> GetLatestCustomerPayment(string customerId)
        {
            var paymentListResponse = await GetPaymentListAsync();
            var latestPayment = paymentListResponse.Where(m => m.CustomerId == customerId).OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            return latestPayment;
        }

        [HttpGet("payment/create/{customerId}")]
        public async Task<ActionResult> CreatePaymentAsync(string customerId)
        {
            var planController = new PlanController(_api, _clientFactory);
            var customer = await GetCustomerFromIdAsync(customerId);
            if (customer == null)
            {
                return BadRequest("Invalid customer ID specified");
            }
            var latestPayment = await GetLatestCustomerPayment(customer.Id);
            if (latestPayment == null)
            {
                return BadRequest($"No payments found for customer {customer.Email}");
            }
            var latestPaymentMetadata = latestPayment.GetMetadata<UserPlanMetaData>();
            var plan = latestPaymentMetadata.Plan;
            if (plan == null)
            {
                return BadRequest("Invalid plan ID specified");
            }
            var paymentResponse = await CreatePaymentRequestAsync(plan, customer.Email, customer.Id, HttpContext, latestPayment.Amount.Value, latestPaymentMetadata);
            return Redirect(paymentResponse.Links.Checkout.Href);
        }

        public async Task<PaymentResponse> CreatePaymentRequestAsync(Plan plan, string email, string customerId, HttpContext context, string price = null, UserPlanMetaData metadata = null )
        {
            price = price ?? plan.Price.ToString();

            UserPlanMetaData metadataRequest = new UserPlanMetaData()
            {
                CustomerId = customerId,
                Email = email,
                Plan = plan
            };

            if (metadata != null)
            {
                metadataRequest = metadata;
            }

            PaymentRequest paymentRequest = new PaymentRequest()
            {
                CustomerId = $"{customerId}",
                SequenceType = SequenceType.First,
                Amount = new Amount(Currency.EUR, price),
                Description = $"RentVision - {plan.Name}",
                RedirectUrl = $"{Website.Url}/setup",
                WebhookUrl = $"{BackOffice.Url}/{ApiCalls.PaymentWebhook.FullUrl()}"
            };

            paymentRequest.SetMetadata(metadataRequest);

            var paymentRequestSerialized = JsonConvert.SerializeObject(paymentRequest);
            var apiRequest = await _apiHelper.SendApiCallAsync(ApiCalls.MollieCreatePayment, json: paymentRequestSerialized, context: context);
            var apiResponse = await apiRequest.Content.ReadAsStringAsync();
            if (!apiRequest.IsSuccessStatusCode)
            {
                throw new Exception(apiResponse);
            }

            var paymentResponse = JsonConvert.DeserializeObject<PaymentResponse>(apiResponse);

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