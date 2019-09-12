﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System.Collections.Generic;

namespace RentVision.Helpers
{
    public class CultureHelper
    {
        public static string userCulture { get; set; }

        // Exclude default culture(s)
        private static List<string> excludedCultures = new List<string>()
        {
            "en"
        };

        // List of supported cultures
        private static List<string> supportedCultures = new List<string>()
        {
            "nl"
        };

        /// <summary>
        /// Returns the user culture as an ISO 639-1 two-letter code.
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="Context"></param>
        /// <returns></returns>
        public static string GetUserCulture( HttpRequest Request, HttpContext Context )
        {
            var rqf = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.Culture.TwoLetterISOLanguageName;
            //var culture = "nl";

            if (string.IsNullOrWhiteSpace(Context.Session.GetString("language")))
            {
                SetUserCulture(Request, Context, culture);
                userCulture = culture;
            }

            return Context.Session.GetString("language");
        }

        /// <summary>
        /// Sets the user culture in a session. (Initialize session in Startup first)
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="Context"></param>
        /// <param name="Culture"></param>
        /// <returns></returns>
        public static bool SetUserCulture( HttpRequest Request, HttpContext Context, string Culture = "en" )
        {
            Context.Session.SetString("language", Culture);
            userCulture = Culture;

            if ( string.IsNullOrWhiteSpace(Context.Session.GetString("language") ) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the proper path with the users' culture appended to it
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="Context"></param>
        /// <param name="Excludes">A list of excluded culture codes</param>
        /// <returns>A string containing the proper user culture appended to the path</returns>
        public static string GetProperCultureUrl( HttpRequest Request, HttpContext Context, List<string> Excludes = null )
        {
            excludedCultures = Excludes ?? excludedCultures;

            var culture = GetUserCulture(Request, Context);
            var path = Request.Path.Value;
            var culturizedPath = $"~/{culture}{path.Replace("/start", "")}";

            if (culturizedPath == Context.Session.GetString("redirectPath") || !supportedCultures.Contains(culture))
            {
                return null;
            }

            Context.Session.SetString("redirectPath", culturizedPath);

            return (excludedCultures.Contains(culture)) ? null : culturizedPath;
        }
    }
}
