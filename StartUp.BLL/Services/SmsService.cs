using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace StartUp.BLL.Services
{
    public class SmsService : ISmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromPhoneNumber;

        public SmsService(IConfiguration configuration)
        {
            _accountSid = configuration["TwilioSettings:AccountSid"];
            _authToken = configuration["TwilioSettings:AuthToken"];
            _fromPhoneNumber = configuration["TwilioSettings:FromPhoneNumber"];
        }

        public async Task SendSmsAsync(string to, string message)
        {
            TwilioClient.Init(_accountSid, _authToken);

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromPhoneNumber),
                to: new PhoneNumber(to)
            );
        }
    }

    public interface ISmsService
    {
        Task SendSmsAsync(string to, string message);
    }
}
