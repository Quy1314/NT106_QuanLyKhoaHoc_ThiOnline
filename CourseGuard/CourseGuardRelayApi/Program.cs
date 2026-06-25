using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

// Configure port from Railway environment variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

var app = builder.Build();

// Enable WebSockets Middleware
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(60)
});

// examId -> (studentId -> WebSocket)
var students = new ConcurrentDictionary<int, ConcurrentDictionary<int, WebSocket>>();
// examId -> list of teacher WebSockets
var teachers = new ConcurrentDictionary<int, ConcurrentBag<WebSocket>>();

app.Map("/relay", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Only WebSocket connections are accepted at this endpoint.");
        return;
    }

    var query = HttpUtility.ParseQueryString(context.Request.QueryString.ToString());
    string? role = query["role"];
    string? examIdStr = query["examId"];
    string? studentIdStr = query["studentId"];

    if (string.IsNullOrEmpty(role) || !int.TryParse(examIdStr, out int examId))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Missing or invalid role/examId.");
        return;
    }

    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

    if (role == "student" && int.TryParse(studentIdStr, out int studentId))
    {
        var examStudents = students.GetOrAdd(examId, _ => new ConcurrentDictionary<int, WebSocket>());
        examStudents[studentId] = webSocket;
        Console.WriteLine($"Student {studentId} joined exam proctoring room {examId}");

        var buffer = new byte[1024 * 1024 * 5]; // 5MB buffer for screenshots
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Binary && result.Count > 0)
                {
                    // Forward student's frame to all teachers subscribed to this exam room
                    if (teachers.TryGetValue(examId, out var examTeachers))
                    {
                        var activeTeachers = examTeachers.Where(t => t.State == WebSocketState.Open).ToList();
                        foreach (var teacherWs in activeTeachers)
                        {
                            try
                            {
                                await teacherWs.SendAsync(
                                    new ArraySegment<byte>(buffer, 0, result.Count),
                                    WebSocketMessageType.Binary,
                                    true,
                                    CancellationToken.None);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error forwarding to teacher: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error on student {studentId} socket connection: {ex.Message}");
        }
        finally
        {
            examStudents.TryRemove(studentId, out _);
            Console.WriteLine($"Student {studentId} disconnected from exam {examId}");
        }
    }
    else if (role == "teacher")
    {
        var examTeachers = teachers.GetOrAdd(examId, _ => new ConcurrentBag<WebSocket>());
        examTeachers.Add(webSocket);
        Console.WriteLine($"Teacher joined monitor room for exam {examId}");

        try
        {
            var buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                // Teacher is listener but can send command to update streaming rate
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }

                // If teacher sends a text command, e.g. "studentId:intervalMs"
                if (result.MessageType == WebSocketMessageType.Text && result.Count > 0)
                {
                    string command = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var parts = command.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int targetStudentId) && int.TryParse(parts[1], out int intervalMs))
                    {
                        if (students.TryGetValue(examId, out var examStudents) && examStudents.TryGetValue(targetStudentId, out var studentWs))
                        {
                            if (studentWs.State == WebSocketState.Open)
                            {
                                try
                                {
                                    byte[] cmdBytes = System.Text.Encoding.UTF8.GetBytes($"RATE:{intervalMs}");
                                    await studentWs.SendAsync(new ArraySegment<byte>(cmdBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                                    Console.WriteLine($"Forwarded command RATE:{intervalMs} to student {targetStudentId} in exam {examId}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error forwarding command to student: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error on teacher monitor socket: {ex.Message}");
        }
        finally
        {
            // The teacher socket will be filtered out in the broadcast loop once State != Open
            Console.WriteLine($"Teacher disconnected from monitor room for exam {examId}");
        }
    }
});

app.Run();
