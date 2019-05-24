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
            var response = await MeetingInfo.GetMeeting(startTime: DateTime.UtcNow, endTime: DateTime.UtcNow.AddDays(1));
            return new OkObjectResult(response);
        }
    }
}
