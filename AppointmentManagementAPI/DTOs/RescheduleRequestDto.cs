using Newtonsoft.Json;

namespace AppointmentManagementAPI.DTOs
{
    public class RescheduleRequestDto
    {
        [JsonProperty("newDate")]
        public string NewDate { get; set; } // String type to avoid deserialization issues
    }
}
