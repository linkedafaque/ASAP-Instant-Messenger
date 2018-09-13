using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Messaging_App.AzureModel
{
    public class SendData: TableEntity
    {
        public SendData()
        {
            this.PartitionKey = "default";
        }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        
        [JsonProperty(PropertyName = "from")]
        public string From { get; set; }

        [JsonProperty(PropertyName = "to")]
        public string To { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "time")]
        public string Time { get; set; }

        [JsonProperty(PropertyName = "read")]
        public bool Read { get; set; }

        [JsonProperty(PropertyName = "secret")]
        public bool Secret { get; set; }
    }
}