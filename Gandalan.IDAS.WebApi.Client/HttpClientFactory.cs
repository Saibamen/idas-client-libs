using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Gandalan.IDAS.WebApi.Client;

public class HttpClientConfig : ICloneable
{
    /// <summary>
    /// Stammadresse der Web-API. Die Resource-Parameter der einzelnen Übertragungsmethoden
    /// werden angehängt. Beispiel: http://192.168.217.10/neurosAPI/api/
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Proxy-Informationen (optional)
    /// </summary>
    public IWebProxy Proxy { get; set; }

    /// <summary>
    /// Liste der zusätzlich zu übermittelnden Header. Werden bei jeder Anfrage mitgeschickt,
    /// z.B. für Authentifizierungs-Header
    /// </summary>
    public Dictionary<string, string> AdditionalHeaders { get; private set; } = [];

    public ICredentials Credentials { get; set; }

    /// <summary>
    /// User-Agent Header. Wird bei jeder Anfrage mitgeschickt.
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    /// AcceptEncoding Header wird auf GZIP gesetzt.
    /// </summary>
    public bool UseCompression { get; set; }

    public object Clone()
    {
        return new HttpClientConfig
        {
            BaseUrl = BaseUrl,
            Proxy = Proxy,
            Credentials = Credentials,
            UserAgent = UserAgent,
            UseCompression = UseCompression,
            AdditionalHeaders = new Dictionary<string, string>(AdditionalHeaders)
        };
    }
}

public class HttpClientFactory
{
    private HttpClientFactory()
    {
    }

    public static HttpClient GetInstance(HttpClientConfig config)
    {
        return createWebClient(config); // this is not best practice!!
        /*
        if (!_clients.ContainsKey(config.BaseUrl))
        {
            var client = createWebClient(config);
            _clients[client.BaseAddress.ToString()] = client;
        }
        return _clients[config.BaseUrl];
        */
    }

    /// <summary>
    /// Erstellt und konfiguriert eine neue WebClient-Instanz
    /// </summary>
    private static HttpClient createWebClient(HttpClientConfig config)
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = config.UseCompression ? DecompressionMethods.GZip | DecompressionMethods.Deflate : DecompressionMethods.None,
            Credentials = config.Credentials,
            Proxy = config.Proxy
        };

        var client = new HttpClient(handler);
        client.BaseAddress = new Uri(config.BaseUrl);
        client.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = new TimeSpan(0, 5, 0); // 5 Minute

        if (!string.IsNullOrEmpty(config.UserAgent))
        {
            client.DefaultRequestHeaders.UserAgent.TryParseAdd(config.UserAgent);
        }

        foreach (var key in config.AdditionalHeaders.Keys)
        {
            client.DefaultRequestHeaders.Add(key, config.AdditionalHeaders[key]);
        }

        return client;
    }
}
