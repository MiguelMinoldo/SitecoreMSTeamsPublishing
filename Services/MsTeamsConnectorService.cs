using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MessageCardModel;
using Sitecore.Configuration;

namespace MSTeamsPublishing.Services
{
    public class MsTeamsConnectorService : IMsTeamsConnectorService
    {
        public async Task ProcessAsync(MessageCard card)
        {
            var requestUri = Settings.GetSetting("MSTeamsPublishing.TeamsWebhookUrl", string.Empty);
            var converted = card.ToJson();

            converted = converted.Replace("Default", "default");
            converted = converted.Replace("\"@type\":2", "\"@type\":\"OpenUri\"");

            using (var client = new HttpClient())
            using (var content = new StringContent(converted, Encoding.UTF8, "application/json"))
            using (var response = await client.PostAsync(requestUri, content).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
            }
        }
    }
}