using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Search;

internal class SearchController(HttpClient http)
{
    public async ValueTask<SearchResponse> GetSearchResponseAsync(
        string searchQuery,
        SearchFilter searchFilter,
        string? continuationToken,
        string hl,
        string gl,
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.youtube.com/youtubei/v1/search"
        );

        var filter = searchFilter.ToString();

        request.Content = new StringContent(
            // lang=json
            $$"""
            {
              "query": {{Json.Encode(searchQuery)}},
              "params": {{Json.Encode(!string.IsNullOrWhiteSpace(filter) ? filter : null)}},
              "continuation": {{Json.Encode(continuationToken)}},
              "context": {
                "client": {
                  "clientName": "WEB",
                  "clientVersion": "2.20210408.08.00",
                  "hl": {{Json.Encode(hl)}},
                  "gl": {{Json.Encode(gl)}},
                  "persist_hl": "1",
                  "utcOffsetMinutes": 0
                }
              }
            }
            """
        );

        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return SearchResponse.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
