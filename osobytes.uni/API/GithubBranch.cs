using Newtonsoft.Json;

namespace osobytes.uni.API
{
    public class GithubBranch
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("protected")]
        public bool Protected { get; set; }
    }
}
