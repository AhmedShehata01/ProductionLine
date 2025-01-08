using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace StartUp.BLL.Services.AppSecurity
{
    public class Subscription
    {
        private readonly TimeZoneSettings _timeZoneSettings;

        public Subscription(IOptions<TimeZoneSettings> timeZoneSettings)
        {
            _timeZoneSettings = timeZoneSettings.Value;
        }

        public bool IsSubscriptionActive()
        {
            // Get the current UTC time
            DateTime utcNow = DateTime.UtcNow;

            // Apply the UTC offset from the configuration (e.g., UTC+2 for Egypt)
            DateTime localTime = utcNow.AddHours(_timeZoneSettings.UtcOffset);

            // Hardcoded subscription expiration date (e.g., November 19, 2024)
            DateTime subscriptionExpirationDate = new DateTime(2025, 11, 11);

            // Check if the local time is before the subscription expiration date
            return localTime <= subscriptionExpirationDate;
        }
    }
}
