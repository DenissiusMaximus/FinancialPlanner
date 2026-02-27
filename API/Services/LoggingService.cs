namespace API.Services;

public class LoggingService : ILoggingService
{
    public void WriteLog(string message)
    {
        Console.WriteLine(message);
    }
}