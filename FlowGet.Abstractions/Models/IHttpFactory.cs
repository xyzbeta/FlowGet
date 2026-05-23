using System;
using System.Collections.Generic;
using System.Text;

namespace FlowGet.Abstractions.Models
{
    public interface IHttpFactory
    {
        event Action? ProxyChanged;
        HttpClient GetClient(string name);

        void CloseClient(string name);

        void Configure(string name, Action<HttpClient, HttpClientHandler> configure);

        void Remove(string name);
    }
}
