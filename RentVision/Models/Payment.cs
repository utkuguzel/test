using RentVision.Models.Regions;
using Piranha.AttributeBuilder;
using Piranha.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Mollie.Api.Models.Payment;

namespace RentVision.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid RentVisionAccountId { get; set; }
        public string MolliePaymentId { get; set; }
        public string Value { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public PaymentStatus? Status { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ExpirationDate { get; set; }
    }
}