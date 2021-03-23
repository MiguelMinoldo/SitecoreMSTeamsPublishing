using System.Threading.Tasks;
using MessageCardModel;

namespace MSTeamsPublishing.Services
{
    public interface IMsTeamsConnectorService
    {
        Task ProcessAsync(MessageCard card);
    }
}