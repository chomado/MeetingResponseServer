using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Identity.Client;
using System.Net.Http;

namespace MeetingResponseServer
{
    public static class GetMeetingInfo
    {
        [FunctionName("GetMeetingInfo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
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
            var response = await apiCaller.CallWebApiAndProcessResultASync($"https://graph.microsoft.com/v1.0/users/{config.MyUserId}/calendarview?startdatetime=2019-05-23T15:00:00.000Z&enddatetime=2019-05-24T14:59:59.999Z", result.AccessToken);
            return new OkObjectResult(response);
        }
    }
}
