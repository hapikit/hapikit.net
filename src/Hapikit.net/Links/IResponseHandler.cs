using System.Net.Http;
using System.Threading.Tasks;

namespace Hapikit.Links
{
    public interface IResponseHandler
    {
        Task<HttpResponseMessage> HandleResponseAsync(string linkRelation, HttpResponseMessage responseMessage);
    }
}
