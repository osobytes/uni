using Newtonsoft.Json;

namespace osobytes.uni.API
{
    public class UniPackage
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("owner")]
        public string Owner { get; set; }
        [JsonProperty("tools")]
        public ToolDefinition[] Tools { get; set; }

    }

    public class ToolDefinition
    {
        [JsonProperty("name")]
        public virtual string Name {  get; set; }
        [JsonProperty("description")]
        public virtual string Description { get; set; }
        [JsonProperty("default")]
        public virtual bool Default { get; set; }  
        [JsonProperty("includes")]
        public virtual string[] Includes { get; set; }
        [JsonProperty("folder")]
        public virtual string Folder { get; set; }
    }
}
