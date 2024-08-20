using Microsoft.Extensions.Logging;
using System;

namespace DurMailLog
{
  internal class LogMessageEntry(LogLevel logLevel, EventId eventId, string message,
    Exception? exception, string categoryName)
  {

    #region Properties

    public LogLevel LogLevel { get; set; } = logLevel;
    public EventId EventId { get; set; } = eventId;
    public string Message { get; set; } = message;
    public Exception? Exception { get; set; } = exception;
    public string CategoryName { get; set; } = categoryName;

    #endregion

  }
}
