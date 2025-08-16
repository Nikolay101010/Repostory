using System.Text.Json.Serialization;

namespace Story.Models
{
    internal class ApiResponseDTO
    {
        //check if you need something here
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("storyid")]
        public string? StoryId { get; set; }
    }
}
