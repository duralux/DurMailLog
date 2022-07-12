using Microsoft.Extensions.Logging;
using System;

namespace DurMailLog
{
  internal class LogMessageEntry
  {

    #region Properties

    public LogLevel LogLevel { get; set; }
    public EventId EventId { get; set; }
    public string Message { get; set; }
    public Exception? Exception { get; set; }
    public string CategoryName { get; set; }

    #endregion


    #region Initialization

    public LogMessageEntry(LogLevel logLevel, EventId eventId, string message,
      Exception? exception, string categoryName)
    {
      this.LogLevel = logLevel;
      this.EventId = eventId;
      this.Message = message;
      this.Exception = exception;
      this.CategoryName = categoryName;
    }

    #endregion

  }
}
