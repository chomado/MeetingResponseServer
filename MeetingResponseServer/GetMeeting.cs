using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MeetingResponseServer
{
    public static class MeetingInfo
    {
        public static async Task<IActionResult> GetMeeting(DateTime startTime, DateTime endTime)
        {
            AuthenticationConfig config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");

            // Even if this is a console application here, a daemon application is a confidential client application
            IConfidentialClientApplication app;

            app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority(new Uri(config.Authority))
                .Build();

            // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
            // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
            // a tenant administrator
            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            AuthenticationResult result = null;
            result = await app.AcquireTokenForClient(scopes)
                .ExecuteAsync();

            var httpClient = new HttpClient();
            var apiCaller = new ProtectedApiCallHelper(httpClient);
            var response = await apiCaller.CallWebApiAndProcessResultASync($"https://graph.microsoft.com/v1.0/users/{config.MyUserId}/calendarview?startdatetime={startTime}&enddatetime={endTime}", result.AccessToken);
            return new OkObjectResult(response);
        }
    }
}
