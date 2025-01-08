using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using StartUp.BLL.Services.AppSecurity;

namespace StartUp.BLL.Middleware
{
    public class SubscriptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Subscription _subscriptionService;

        public SubscriptionMiddleware(RequestDelegate next, Subscription subscriptionService)
        {
            _next = next;
            _subscriptionService = subscriptionService;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.Value.ToLower();

            // Prevent redirection loop if the user is already on the SubscriptionExpired page
            if (!_subscriptionService.IsSubscriptionActive() && !path.Contains("/home/subscriptionexpired"))
            {
                // If subscription is expired and user is not on SubscriptionExpired page, redirect
                httpContext.Response.Redirect("/Home/SubscriptionExpired");
                return; // Stop further processing of the request
            }

            // If the user is logged in and subscription is active, continue with the request
            await _next(httpContext);
        }
    }
}
