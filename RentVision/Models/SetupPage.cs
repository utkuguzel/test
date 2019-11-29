using Mollie.Api.Models.Payment;
using Piranha.AttributeBuilder;
using Piranha.Models;
using RentVision.Models.Regions;

namespace RentVision.Models
{
    [PageType(Title = "Setup page")]
    [PageTypeRoute(Title = "Default", Route = "/setup")]
    public class SetupPage  : Page<SetupPage>
    {
        [Region( Title = "Setup fields" )]
        public SetupFields SetupFields { get; set; }
        
        public string Email { get; set; }
        public Controllers.Plan SelectedPlan { get; set; }
        public string Code { get; set; }
        public string MollieCheckoutUrl { get; set; }
        public string MolliePaymentId { get; set; }
        public bool IsUpgrade { get; set; } = false;
        public string UpgradePrice { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public bool FirstPayment { get; set; } = false;
    }
}