using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AppointmentManagementAPI.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public string RequestorName { get; set; }

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime ScheduledDate { get; set; } // Always in UTC

        public string Status { get; set; }
    }
}
