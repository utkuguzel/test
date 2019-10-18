using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System.Collections.Generic;

namespace RentVision.Helpers
{
    public class CookieHelper
    {
        public static bool SetCookie(string key, string value, HttpContext context, CookieOptions options = null)
        {
            options = options ?? new CookieOptions();
            context.Response.Cookies.Append(key, value, options: options);

            return true;
        }

        public static string GetCookie(string key, HttpContext context)
        {
            return context.Request.Cookies[key] ?? null;
        }
    }
}
