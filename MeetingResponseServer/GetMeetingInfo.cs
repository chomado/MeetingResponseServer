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
using System.Linq;
using Google.Protobuf;
using Google.Cloud.Dialogflow.V2;

namespace MeetingResponseServer
{
    public static class GetMeetingInfo
    {
        private static string[] IntroductionMessage { get; } = { "こんにちは、デコード2019のデモアプリです。予定を教えてと聞いてください。" };
        private static string[] HelloMessage { get; } = { "こんにちは、ちょまどさん！" };
        private static string[] ErrorMessage { get; } = { "すみません、わかりませんでした！" };

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
                    cekResponse.AddText(IntroductionMessage[0]);
                    cekResponse.ShouldEndSession = false;
                    break;
                case RequestType.IntentRequest:
                    {
                        // intent ごとに処理を振り分け
                        cekRequest.Request.Intent.Slots.TryGetValue(key: "when", value: out var when);
                        var texts = await HandleIntentAsync(cekRequest.Request.Intent.Name, when?.Value ?? "今日", Platforms.Clova);

                        if (texts.Any())
                        {
                            foreach (var text in texts)
                            {
                                cekResponse.AddText(text);
                            }
                        }
                        else
                        {
                            cekResponse.AddText("予定はありません。");
                        }

                        // 予定があったら LINE にプッシュ通知する
                        if (texts.Any())
                        {
                            var config = Models.AuthenticationConfigModel.ReadFromJsonFile("appsettings.json");
                            var secret = config.LineMessagingApiSecret;

                            var textMessages = new List<ISendMessage>();

                            textMessages.Add(new TextMessage("ちょまどさんの予定はこちら！"));
                            foreach (var text in texts)
                            {
                                textMessages.Add(new TextMessage(text));
                            }

                            var messagingClient = new LineMessagingClient(secret);
                            await messagingClient.PushMessageAsync(
                                //to: cekRequest.Session.User.UserId,
                                to: config.MessagingUserId,
                                messages: textMessages
                            );
                        }
                    }
                    break;
            }
            return new OkObjectResult(cekResponse);
        }


        [FunctionName("GoogleHome")]
        public static async Task<IActionResult> GoogleHome([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log)
        {
            var parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            var webhookRequest = parser.Parse<WebhookRequest>(await req.ReadAsStringAsync());
            var entities = webhookRequest.QueryResult.Parameters.Fields["when"].StringValue;
            var webhookResponse = new WebhookResponse();
            log.LogInformation(webhookRequest.QueryResult.Intent.DisplayName);
            switch (webhookRequest.QueryResult.Intent.DisplayName)
            {
                case "Default Welcome Intent":
                    webhookResponse.FulfillmentText = IntroductionMessage[0];
                    break;
                default:
                    {
                        var texts = await HandleIntentAsync(webhookRequest.QueryResult.Intent.DisplayName, entities, Platforms.GoogleAssistant);
                        if (texts.Any())
                        {
                            var fulfillmentText = "ちょまどさんの予定をお知らせします。\n";
                            foreach (var text in texts)
                            {
                                fulfillmentText += $"{text}\n";
                            }
                            webhookResponse.FulfillmentText = fulfillmentText;
                        }
                        else
                        {
                            webhookResponse.FulfillmentText = "予定はありません。";
                        }
                    }
                    break;
            }

            return new ProtcolBufJsonResult(webhookResponse, JsonFormatter.Default);
        }


        private static async Task<IEnumerable<string>> HandleIntentAsync(string intent, object meetingDay, Platforms platform)
        {
            switch (intent)
            {
                case "HelloIntent":
                    return HelloMessage;
                // 明日の予定を教えて
                case "AskScheduleIntent":
                    {

                        var start = ParseMeetingDay(meetingDay, platform);
                        var response = await MeetingInfo.GetMeeting(startTime: start, endTime: start.AddDays(1));

                        return ScheduleMessage(response.Value);
                    }
                default:
                    return ErrorMessage;
            }
        }

        private static DateTimeOffset ParseMeetingDay(object meetingDay, Platforms platform)
        {
            if (platform == Platforms.Clova || platform == Platforms.Alexa)
            {
                return ConvertJname2Datetime((string)meetingDay);
            }
            else
            {
                // GoogleAssistant
                // 	2019-05-27T12:00:00+09:00
                var d = DateTimeOffset.Parse((string)meetingDay);
                return new DateTimeOffset(d.Date, d.Offset);
            }
        }

        private static IEnumerable<string> ScheduleMessage(Models.Value[] meeting)
        {
            return meeting
                .Select(x => $"{x.Start.DateTime.ToJst().Hour}時{x.Start.DateTime.ToJst().Minute}分から{x.Subject}があります。");
        }

        // "今日" -> DateTime
        private static DateTimeOffset ConvertJname2Datetime(string when)
        {
            var temp = DateTimeOffset.UtcNow.ToJst();
            var utc = new DateTimeOffset(temp.Date, temp.Offset);

            switch (when)
            {
                case "今日":
                    break;
                case "明日":
                    utc = utc.AddDays(1);
                    break;
            }
            return utc;
        }

        // UTC -> JST
        private static DateTimeOffset ToJst(this DateTimeOffset utc)
        {
            var jstZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            return utc.ToOffset(jstZoneInfo.BaseUtcOffset);
        }
        private static DateTime ToJst(this DateTime utc)
        {
            var jstZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, jstZoneInfo);
        }
    }
}
