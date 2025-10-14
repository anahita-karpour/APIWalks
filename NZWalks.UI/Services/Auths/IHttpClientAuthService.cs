namespace NZWalks.UI.Services.Auths
{
    public interface IHttpClientAuthService
    {
        //provides an authorized HttpClient
        Task<HttpClient> GetAuthorizedClient();

        // Creates an HttpRequestMessage with the Authorization header
        //HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string requestUri, HttpContent content = null);

        // Sends an authorized HTTP request
        Task<HttpResponseMessage> SendAuthorizedRequest(HttpMethod method, string requestUri, HttpContent content = null);
        // OVERLOAD for requests with a body (POST/PUT)
        Task<HttpResponseMessage> SendAuthorizedRequest(HttpMethod method, string requestUri, object body);

    }
}
