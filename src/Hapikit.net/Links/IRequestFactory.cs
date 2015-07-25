using System.Net.Http;

namespace Hapikit.Links
{
    public interface IRequestFactory
    {
        string LinkRelation { get; }
        HttpRequestMessage CreateRequest();
    }
}
