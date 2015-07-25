﻿using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Hapikit.Links;
using Hapikit.RequestBuilders;

namespace LinkTests
{
    public class AcceptHeaderRequestBuilder : DelegatingRequestBuilder
    {
        private readonly IEnumerable<MediaTypeWithQualityHeaderValue> _AcceptHeader;

        public AcceptHeaderRequestBuilder(IEnumerable<MediaTypeWithQualityHeaderValue> acceptHeaders )
        {
            _AcceptHeader = acceptHeaders;
        }

        protected override HttpRequestMessage ApplyChanges(ILink link, HttpRequestMessage request)
        {
            request.Headers.Accept.Clear();
            foreach (var headerValue in _AcceptHeader)
            {
                request.Headers.Accept.Add(headerValue);
            }
            return request;
        }
    }
}