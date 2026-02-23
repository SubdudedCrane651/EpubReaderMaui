using System.Net;

namespace EpubReaderMaui;

public class LocalHttpServer
{
    private readonly HttpListener _listener;
    private readonly string _filePath;

    public string Url { get; }

    public LocalHttpServer(string filePath, int port = 12345)
    {
        _filePath = filePath;
        Url = $"http://localhost:{port}/book.epub";

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://*:{port}/");
    }

    public void Start()
    {
        _listener.Start();
        Task.Run(async () =>
        {
            while (_listener.IsListening)
            {
                HttpListenerContext ctx;
                try
                {
                    ctx = await _listener.GetContextAsync();
                }
                catch
                {
                    break;
                }

                if (ctx.Request.RawUrl == "/book.epub")
                {
                    try
                    {
                        byte[] bytes = File.ReadAllBytes(_filePath);
                        ctx.Response.ContentType = "application/epub+zip";
                        ctx.Response.ContentLength64 = bytes.Length;
                        await ctx.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                        ctx.Response.OutputStream.Close();
                    }
                    catch
                    {
                        ctx.Response.StatusCode = 500;
                        ctx.Response.Close();
                    }
                }
                else
                {
                    ctx.Response.StatusCode = 404;
                    ctx.Response.Close();
                }
            }
        });
    }

    public void Stop()
    {
        try
        {
            if (_listener.IsListening)
                _listener.Stop();
        }
        catch { }
    }
}