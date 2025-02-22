using Microsoft.AspNetCore.Builder;
using System.Net.Http;

namespace Rebecca.Services
{
    public static class ApplicationExtensions
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static void UseProxyToDevServer(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                try
                {
                    if (context.Request.Path.ToString().StartsWith("/api"))
                    {
                        await next(context);
                        return;
                    }

                    var targetUri = new Uri($"http://localhost:4074{context.Request.Path}{context.Request.QueryString}");
                    
                    // 创建新的请求消息
                    var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);
                    
                    // 复制原始请求头
                    foreach (var header in context.Request.Headers)
                    {
                        if (!header.Key.StartsWith(":") && 
                            !new[] { "Host", "Connection" }.Contains(header.Key))
                        {
                            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                        }
                    }

                    // 如果有请求体，复制请求体
                    if (context.Request.Body != null && context.Request.Body.CanRead)
                    {
                        requestMessage.Content = new StreamContent(context.Request.Body);
                        if (context.Request.ContentType != null)
                        {
                            requestMessage.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(context.Request.ContentType);
                        }
                    }

                    var response = await _httpClient.SendAsync(requestMessage);
                    
                    // 设置响应状态码
                    context.Response.StatusCode = (int)response.StatusCode;
                    
                    // 复制响应头
                    foreach (var header in response.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }
                    if (response.Content != null)
                    {
                        foreach (var header in response.Content.Headers)
                        {
                            context.Response.Headers[header.Key] = header.Value.ToArray();
                        }

                        // 只在非304状态时复制响应体
                        if (response.StatusCode != System.Net.HttpStatusCode.NotModified)
                        {
                            await response.Content.CopyToAsync(context.Response.Body);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Proxy error: {ex}");
                    await next(context);
                }
            });
        }
    }
}
