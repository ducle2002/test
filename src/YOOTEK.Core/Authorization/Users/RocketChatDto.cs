

using Newtonsoft.Json;

namespace Yootek.Authorization.Users
{
    public class RocketChatReponseDto
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("data")]
        public DataRocketChat Data { get; set; }
    }
    public class DataRocketChat
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }
        [JsonProperty("authToken")]
        public string AuthToken { get; set; }
        [JsonProperty("me")]
        public MeRocketChat Me { get; set; }
    }

    public class MeRocketChat
    {
        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }
    }

    public class RegisterRocketChatReponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("user")]
        public object User { get; set; }
    }
}
