using System;
using System.Net.Http;
using Hapikit.Links;


namespace Hapikit.RequestBuilders
{
    public class InlineRequestBuilder : DelegatingRequestBuilder
    {
        private readonly Func<HttpRequestMessage, HttpRequestMessage> _customizeRequest;

        public InlineRequestBuilder(Func<HttpRequestMessage, HttpRequestMessage> customizeRequest)
        {
            _customizeRequest = customizeRequest;
        }

        protected override HttpRequestMessage ApplyChanges(ILink link,HttpRequestMessage request)
        {
            return _customizeRequest(request);
        }
    }
}