﻿using System;
using System.Net.Http;

namespace Hapikit.Cache
{
    public class CacheKey
    {
        private readonly Uri _uri;
        private readonly HttpMethod _method;


        public CacheKey(Uri uri, HttpMethod method)
        {
            _uri = uri;
            _method = method;
            if (_method == HttpMethod.Post)  // A response to a POST can be returned to a GET method
            {
                _method = HttpMethod.Get; 
            }
        }

        public override bool Equals(object obj)
        {
            var key2 = (CacheKey) obj; 
            return key2._uri == _uri && key2._method == _method;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + _uri.GetHashCode();
            hash = (hash * 7) + _method.GetHashCode();
            return hash;
        }
    }
}