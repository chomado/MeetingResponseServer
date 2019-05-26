using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

//    var meetingModel = MeetingModel.FromJson(jsonString);

namespace MeetingResponseServer.Models
{

    public partial class MeetingModel
    {
        [JsonProperty("value")]
        public MeetingModelValue Value { get; set; }

        [JsonProperty("formatters")]
        public List<object> Formatters { get; set; }

        [JsonProperty("contentTypes")]
        public List<object> ContentTypes { get; set; }

        [JsonProperty("declaredType")]
        public object DeclaredType { get; set; }

        [JsonProperty("statusCode")]
        public long StatusCode { get; set; }
    }

    public partial class MeetingModelValue
    {
        [JsonProperty("@odata.context")]
        public Uri OdataContext { get; set; }

        [JsonProperty("value")]
        public List<ValueElement> Value { get; set; }
    }

    public partial class ValueElement
    {
        [JsonProperty("@odata.etag")]
        public string OdataEtag { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("createdDateTime")]
        public DateTimeOffset CreatedDateTime { get; set; }

        [JsonProperty("lastModifiedDateTime")]
        public DateTimeOffset LastModifiedDateTime { get; set; }

        [JsonProperty("changeKey")]
        public string ChangeKey { get; set; }

        [JsonProperty("categories")]
        public List<object> Categories { get; set; }

        [JsonProperty("originalStartTimeZone")]
        public string OriginalStartTimeZone { get; set; }

        [JsonProperty("originalEndTimeZone")]
        public string OriginalEndTimeZone { get; set; }

        [JsonProperty("iCalUId")]
        public string ICalUId { get; set; }

        [JsonProperty("reminderMinutesBeforeStart")]
        public long ReminderMinutesBeforeStart { get; set; }

        [JsonProperty("isReminderOn")]
        public bool IsReminderOn { get; set; }

        [JsonProperty("hasAttachments")]
        public bool HasAttachments { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("bodyPreview")]
        public string BodyPreview { get; set; }

        [JsonProperty("importance")]
        public string Importance { get; set; }

        [JsonProperty("sensitivity")]
        public string Sensitivity { get; set; }

        [JsonProperty("isAllDay")]
        public bool IsAllDay { get; set; }

        [JsonProperty("isCancelled")]
        public bool IsCancelled { get; set; }

        [JsonProperty("isOrganizer")]
        public bool IsOrganizer { get; set; }

        [JsonProperty("responseRequested")]
        public bool ResponseRequested { get; set; }

        [JsonProperty("seriesMasterId")]
        public object SeriesMasterId { get; set; }

        [JsonProperty("showAs")]
        public string ShowAs { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("webLink")]
        public Uri WebLink { get; set; }

        [JsonProperty("onlineMeetingUrl")]
        public object OnlineMeetingUrl { get; set; }

        [JsonProperty("recurrence")]
        public object Recurrence { get; set; }

        [JsonProperty("responseStatus")]
        public Status ResponseStatus { get; set; }

        [JsonProperty("body")]
        public Body Body { get; set; }

        [JsonProperty("start")]
        public End Start { get; set; }

        [JsonProperty("end")]
        public End End { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("locations")]
        public List<Location> Locations { get; set; }

        [JsonProperty("attendees")]
        public List<Attendee> Attendees { get; set; }

        [JsonProperty("organizer")]
        public Organizer Organizer { get; set; }
    }

    public partial class Attendee
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("emailAddress")]
        public EmailAddress EmailAddress { get; set; }
    }

    public partial class EmailAddress
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }
    }

    public partial class Status
    {
        [JsonProperty("response")]
        public string Response { get; set; }

        [JsonProperty("time")]
        public DateTimeOffset Time { get; set; }
    }

    public partial class Body
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public partial class End
    {
        [JsonProperty("dateTime")]
        public DateTimeOffset DateTime { get; set; }

        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }
    }

    public partial class Location
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("locationUri")]
        public string LocationUri { get; set; }

        [JsonProperty("locationType")]
        public string LocationType { get; set; }

        [JsonProperty("uniqueId")]
        public string UniqueId { get; set; }

        [JsonProperty("uniqueIdType")]
        public string UniqueIdType { get; set; }

        [JsonProperty("address")]
        public Address Address { get; set; }

        [JsonProperty("coordinates")]
        public Coordinates Coordinates { get; set; }
    }

    public partial class Address
    {
        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("countryOrRegion")]
        public string CountryOrRegion { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }
    }

    public partial class Coordinates
    {
    }

    public partial class Organizer
    {
        [JsonProperty("emailAddress")]
        public EmailAddress EmailAddress { get; set; }
    }

}
