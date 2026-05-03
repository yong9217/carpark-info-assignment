using System.Net;
using System.Text.Json;
using System.Text;

public class HttpListenerServer
{
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Dictionary<string, Func<HttpListenerContext, Task>> _routes;
    private DatabaseConnector _con; //The database connection to get data from

    public HttpListenerServer(string[] prefixes, DatabaseConnector con)
    {
        _listener = new HttpListener();
        _cancellationTokenSource = new CancellationTokenSource();
        _routes = new Dictionary<string, Func<HttpListenerContext, Task>>();
        _con = con;

        // Add URL prefixes
        foreach (string prefix in prefixes)
        {
            _listener.Prefixes.Add(prefix);
        }

        SetupDefaultRoutes();
    }

    private void SetupDefaultRoutes()
    {
        // GET routes
        _routes["GET /FreeParking"] = HandleFreeParking;
        _routes["GET /NightParking"] = HandleNightParking;
        _routes["GET /gantry/height/{height}"] = HandleGantryHeight;
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine("HTTP Server started on:");
        foreach (string prefix in _listener.Prefixes)
        {
            Console.WriteLine($"  {prefix}");
        }

        // Handle requests concurrently
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++) //10 concurrent connections at a time
        {
            tasks.Add(HandleIncomingConnections());
        }

        await Task.WhenAll(tasks);
    }

    private async Task HandleIncomingConnections()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                // Handle each request asynchronously
                _ = Task.Run(() => ProcessRequest(context));
            }
            catch (ObjectDisposedException)
            {
                // Listener was stopped
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting context: {ex.Message}");
            }
        }
    }

    private async Task ProcessRequest(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;

            // Add CORS headers
            AddCorsHeaders(response);

            // Handle OPTIONS preflight request for CORS
            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = 200;
                response.Close();
                return;
            }

            Console.WriteLine($"{request.HttpMethod} {request.Url.LocalPath}");

            // Route the request
            string routeKey = $"{request.HttpMethod} {request.Url.LocalPath}";
            
            if (_routes.TryGetValue(routeKey, out var handler))
            {
                await handler(context);
            }
            else if (IsParameterizedRoute(request, out var paramHandler))
            {
                await paramHandler(context);
            }
            else
            {
                await HandleNotFound(context);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing request: {ex.Message}");
            try
            {
                context.Response.StatusCode = 500;
                await WriteJsonResponse(context.Response, new { error = "Internal server error" });
            }
            catch
            {
                // Ignore if response is already closed
            }
        }
    }

    //Parameterized routes are handled separately
    private bool IsParameterizedRoute(HttpListenerRequest request, out Func<HttpListenerContext, Task> handler)
    {
        handler = null;
        var path = request.Url.LocalPath;
        var method = request.HttpMethod;

        if (method == "GET" && path.StartsWith("/gantry/height/") && path.Length > "/gantry/height/".Length)
        {
            handler = HandleGantryHeight;
            return true;
        }

        return false;
    }

    private void AddCorsHeaders(HttpListenerResponse response)
    {
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET");
    }

    // Route Handlers
    private async Task HandleFreeParking(HttpListenerContext context)
    {
        List<Lot> lots = new List<Lot>{};

        foreach (CarPark cp in _con.getFreeParking())
        {
            lots.Add(DatabaseConnector.CarParkToLot(cp));
        }

        await WriteJsonResponse(context.Response, lots);
    }

    ////Handlers for the various GET calls

    private async Task HandleNightParking(HttpListenerContext context)
    {
        List<Lot> lots = new List<Lot>{};

        foreach (CarPark cp in _con.getNightParking())
        {
            lots.Add(DatabaseConnector.CarParkToLot(cp)); //Have to convert to non-circular reference
        }

        await WriteJsonResponse(context.Response, lots);
    }

    private async Task Handleheight(HttpListenerContext context)
    {
        List<Lot> lots = new List<Lot>{};

        foreach (CarPark cp in _con.getMatchHeight(Convert.ToSingle(2.2)))
        {
            lots.Add(DatabaseConnector.CarParkToLot(cp));
        }

        await WriteJsonResponse(context.Response, lots);
    }

    private async Task HandleGantryHeight(HttpListenerContext context)
    {
        var path = context.Request.Url.LocalPath;
        var segments = path.Split('/');
        
        if (segments.Length >= 4 && double.TryParse(segments[3], out double height)) //attempt to read a double in the 4th segment (localhos/<port> : 1st/gantry : 2nd/height : 3rd/<height> : 4th)
        {
            List<Lot> lots = new List<Lot>{};

            foreach (CarPark cp in _con.getMatchHeight(Convert.ToSingle(height)))
            {
                lots.Add(DatabaseConnector.CarParkToLot(cp));
            }

            await WriteJsonResponse(context.Response, lots);
        }
        else
        {
            context.Response.StatusCode = 400;
            await WriteJsonResponse(context.Response, new { error = "Invalid vehicle height" });
        }
    }

    private async Task HandleNotFound(HttpListenerContext context)
    {
        context.Response.StatusCode = 404;
        await WriteJsonResponse(context.Response, new { error = "Endpoint not found" });
    }

    // Helper Methods
    private async Task WriteResponse(HttpListenerResponse response, string content)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(content);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();
    }

    private async Task WriteJsonResponse(HttpListenerResponse response, object data)
    {
        response.ContentType = "application/json";
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true 
        });
        await WriteResponse(response, json);
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _listener?.Stop();
        _listener?.Close();
    }
}