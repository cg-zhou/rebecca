using Newtonsoft.Json;
using StdEx.Media.Tmdb.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace StdEx.Media.Tmdb
{
    public class TmdbUtils
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;
        private readonly string _baseApiUrl;
        private readonly string _baseImageUrl;
        private readonly string _language;
        private readonly string _apiKeyType;

        public TmdbUtils(string token, int timeoutSeconds = 10)
            : this(new TmdbConfig { BearerToken = token }, timeoutSeconds)
        {
        }

        public TmdbUtils(TmdbConfig config, int timeoutSeconds = 10)
        {
            _token = config.BearerToken;
            _baseApiUrl = config.BaseApiUrl;
            _baseImageUrl = config.BaseImageUrl;
            _language = config.Language;
            _apiKeyType = config.ApiKeyType ?? "v4";

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };

            // 通用 HTTP 头
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");

            // 根据 API 密钥类型设置不同的认证方式
            if (_apiKeyType.Equals("v3", StringComparison.OrdinalIgnoreCase))
            {
                // v3 API Key 不使用 Authorization 头，而是在 URL 中添加 api_key 参数
            }
            else
            {
                // v4 Bearer Token 使用 Authorization 头
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }
        }

        public async Task<MovieNfo> GetMovieNfo(string movieName)
        {
            // 构造搜索 URL，根据 API 密钥类型添加不同的参数
            var searchUrl = $"{_baseApiUrl}/search/movie?query={Uri.EscapeDataString(movieName)}&language={_language}";
            
            // 如果使用 v3 API Key，在 URL 中添加 api_key 参数
            if (_apiKeyType.Equals("v3", StringComparison.OrdinalIgnoreCase))
            {
                searchUrl += $"&api_key={_token}";
            }
            
            var searchResponse = await GetJsonAsync<TmdbSearchResponse>(searchUrl);

            if (searchResponse?.Results == null || !searchResponse.Results.Any())
            {
                throw new Exception($"Movie not found: {movieName}");
            }

            var movieId = searchResponse.Results.First().Id;
            var movieUrl = $"{_baseApiUrl}/movie/{movieId}?append_to_response=credits&language={_language}";
            
            // 如果使用 v3 API Key，在 URL 中添加 api_key 参数
            if (_apiKeyType.Equals("v3", StringComparison.OrdinalIgnoreCase))
            {
                movieUrl += $"&api_key={_token}";
            }
            
            var movie = await GetJsonAsync<TmdbMovie>(movieUrl);

            return CreateMovieNfo(movie);
        }

        private async Task<T> GetJsonAsync<T>(string url) where T : class
        {
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API request failed: {response.StatusCode}, Content: {content}");
            }

            var obj = JsonConvert.DeserializeObject<T>(content);
            if (obj == null)
            {
                throw new Exception($"The desrialized object of response is null: {url}");
            }
            return obj;
        }

        private MovieNfo CreateMovieNfo(TmdbMovie movie)
        {
            return new MovieNfo
            {
                Title = movie.Title,
                OriginalTitle = movie.OriginalTitle,
                SortTitle = movie.Title,
                Ratings = new Ratings
                {
                    Rating = new Rating { Value = movie.VoteAverage }
                },
                Year = DateTime.Parse(movie.ReleaseDate).Year,
                Plot = movie.Overview,
                Art = new Art
                {
                    Poster = $"{_baseImageUrl}{movie.PosterPath}",
                    Fanart = $"{_baseImageUrl}{movie.BackdropPath}"
                },
                Id = movie.Id,
                Genre = string.Join(" / ", movie.Genres.Select(g => g.Name)),
                Director = string.Join(" / ", movie.Credits.Crew
                    .Where(c => c.Job == "Director")
                    .Select(c => c.Name)),
                Premiered = movie.ReleaseDate
            };
        }
    }
}