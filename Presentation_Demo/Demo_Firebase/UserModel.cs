using System.Text.Json.Serialization;

namespace Demo_Firebase
{
    public class UserModel
    {
        [JsonPropertyName("username")]
        public required string Username { get; set; }

        [JsonPropertyName("password")]
        public required string Password { get; set; }
    }
}
