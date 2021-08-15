using Newtonsoft.Json;

namespace osobytes.uni.API
{
    public class PublicRepositoryResponse
    {
        [JsonProperty("id")]
        public virtual long Id { get; set; }

        [JsonProperty("full_name")]
        public virtual string FullName { get; set; }
        [JsonProperty("private")]
        public virtual bool Private { get; set; }
        [JsonProperty("branches_url")]
        public virtual string BranchesUrl { get; set; }
        [JsonProperty("blobs_url")]
        public virtual string BlobsUrl { get; set; }
    }
}
