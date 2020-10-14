using Piranha;
using Piranha.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Twinvision.Piranha.RentVision.Models.Extensions
{
    public static class PiranhaApiExtensions
    {
        public async static Task<Site> GetCulturedSite(this IApi api, string culture)
        {
            var sites = await api.Sites.GetAllAsync();
            var site = sites.SingleOrDefault(m => m.Culture == culture);
            if(site == null)
            {
                try
                {
                    site = sites.Single(m => m.Culture == "en");
                }
                catch (Exception ex)
                {
                    throw new Exception("No site with culture 'en' was found, this is not allowed in Piranha.RentVision", ex);
                }
            }
            return site;
        }
    }
}
