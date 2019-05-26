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
using CEK.CSharp;
using CEK.CSharp.Models;
using Line.Messaging;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MeetingResponseServer
{
    public static class GetMeetingInfo
    {
        private static string IntroductionMessage { get; } = "こんにちは、デコード2019のデモアプリです。予定を教えてと聞いてください。";
        private static string HelloMessage { get; } = "こんにちは、ちょまどさん！";
        private static string ErrorMessage { get; } = "すみません、わかりませんでした！";

        [FunctionName("GetMeetingInfo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var response = await MeetingInfo.GetMeeting(startTime: DateTime.UtcNow, endTime: DateTime.UtcNow.AddDays(1));
            return new OkObjectResult(response);
        }

        [FunctionName("Line")]
        public static async Task<IActionResult> Line([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ExecutionContext context, ILogger log)
        {
            var client = new ClovaClient();
            var cekRequest = await client.GetRequest(req.Headers["SignatureCEK"], req.Body);
            var cekResponse = new CEKResponse();
            switch (cekRequest.Request.Type)
            {
                case RequestType.LaunchRequest:
                    cekResponse.AddText(IntroductionMessage);
                    cekResponse.ShouldEndSession = false;
                    break;
                case RequestType.IntentRequest:
                    {
                        // intent ごとに処理を振り分け
                        cekRequest.Request.Intent.Slots.TryGetValue(key: "when", value: out var when);
                        var r = await HandleIntentAsync(cekRequest.Request.Intent.Name, when?.Value ?? "今日");
                        cekResponse.AddText(r);

                        // LINE にプッシュ通知する
                        if (r != null)
                        {
                            var config = Models.AuthenticationConfigModel.ReadFromJsonFile("appsettings.json");
                            var secret = config.LineMessagingApiSecret;

                            var messagingClient = new LineMessagingClient(secret);
                            await messagingClient.PushMessageAsync(
                                //to: cekRequest.Session.User.UserId,
                                to: config.MessagingUserId,
                                messages: new List<ISendMessage>
                                {
                                    new TextMessage($"ちょまどさんの予定はこちら！"),
                                    new TextMessage($@"タイトル『{r}』
{r}"),
                                });
                        }
                    }
                    break;
            }
            return new OkObjectResult(cekResponse);
        }


        private static async Task<string> HandleIntentAsync(string intent, string meetingDay)
        {
            switch (intent)
            {
                case "HelloIntent":
                    return HelloMessage;
                // 明日の予定を教えて
                case "AskScheduleIntent":
                    {
                        var start = ConvertJname2Datetime(meetingDay);
                        //var response = await MeetingInfo.GetMeeting(startTime: start, endTime: start.AddDays(1));
                        
                        return $"{meetingDay}のスケジュールを聞かれました";
                    }
                default:
                    return ErrorMessage;
            }
        }

        // "今日" -> DateTime
        private static DateTime ConvertJname2Datetime(string when)
        {
            var utc = DateTime.UtcNow;

            switch (when)
            {
                case "今日":
                    utc = DateTime.UtcNow;
                    break;
                case "明日":
                    utc = DateTime.UtcNow.AddDays(1);
                    break;
            }
            var jstZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, jstZoneInfo);
        }
    }
}
