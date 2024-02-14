using Microsoft.Extensions.Configuration;
namespace VDownload.Sources.Twitch.Configuration.Models{ 

    public class GetVideoToken
    {
        [ConfigurationKeyName("operation_name")]
        public string OperationName { get; set; }

        [ConfigurationKeyName("query")]
        public string Query { get; set; }
    }

}