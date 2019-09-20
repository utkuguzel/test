using Microsoft.AspNetCore.Mvc;
using Piranha.AttributeBuilder;
using Piranha.Models;
using RentVision.Models.Regions;
using System;
using System.Collections.Generic;

namespace RentVision.Models
{
    [PageType(Title = "Plans page")]
    [PageTypeRoute(Title = "Default", Route = "/plans")]
    public class PlansPage  : Page<PlansPage>
    {
        [Region(Display = RegionDisplayMode.Setting)]
        public Hero Hero { get; set; }

        [Region(ListTitle = "Title")]
        public IList<Plan> Plans { get; set; }

        [Region(ListTitle = "Title")]
        public IList<PlanFeature> Features { get; set; }

        public IList<UserPlan> UserPlans { get; set; }

        public PlansPage()
        {
            Plans = new List<Plan>();
            Features = new List<PlanFeature>();
            UserPlans = new List<UserPlan>();
        }
    }

    public class UserPlan
    {
        public Guid UserPlanId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int PayInterval { get; set; }
    }
}