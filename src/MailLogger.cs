﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace DurMailLog
{
  class MailLogger(string categoryName, MailLoggerProcessor processor,
    IExternalScopeProvider externalScopeProvider,
    IOptionsMonitor<MailLoggerConfiguration> options) : ILogger, IDisposable
  {

    #region Properties

    private readonly IOptionsMonitor<MailLoggerConfiguration> _options = options;
    private readonly MailLoggerProcessor _processor = processor;
    private readonly string _categoryName = categoryName;
    private readonly IExternalScopeProvider _externalScopeProvider = externalScopeProvider;

    #endregion


    #region Functions

    public void Dispose()
    {
      this._processor.Dispose();
    }


    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
      return this._externalScopeProvider?.Push(state) ?? new NullDisposable();
    }


    public bool IsEnabled(LogLevel logLevel)
    {
      var config = this._options.CurrentValue;
      if (String.IsNullOrWhiteSpace(config.Host) || String.IsNullOrWhiteSpace(config.User) ||
        string.IsNullOrWhiteSpace(config.Password))
      { return false; }
      else if (config.To.Count + config.CC.Count + config.Bcc.Count == 0)
      { return false; }

      return true;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="logLevel"></param>
    /// <param name="eventId"></param>
    /// <param name="state"></param>
    /// <param name="exception"></param>
    /// <param name="formatter"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
      Exception? exception, Func<TState, Exception?, string> formatter)
    {
      if (!IsEnabled(logLevel))
      {
        return;
      }

      ArgumentNullException.ThrowIfNull(formatter);

      var message = formatter(state, exception);
      if (string.IsNullOrEmpty(message))
      {
        return;
      }
      //https://stackoverflow.com/questions/59919244/how-should-async-logging-to-database-be-implemented-in-asp-net-core-application
      //messages should be put into queue to avoid low performance
      this._processor.Enqueue(new LogMessageEntry(logLevel, eventId, message,
        exception, this._categoryName));
    }

    #endregion

  }
}
