using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DurMailLog
{
  public class MailLoggerProvider : ILoggerProvider, ISupportExternalScope
  {

    #region Properties

    private readonly IOptionsMonitor<MailLoggerConfiguration> _options;
    private readonly MailLoggerProcessor _processor;
    private readonly ConcurrentDictionary<string, MailLogger> _loggers;
    private IExternalScopeProvider _scopeProvider;


    #endregion


    #region Initialization

    public MailLoggerProvider(IOptionsMonitor<MailLoggerConfiguration> options)
    {
      this._options = options;
      this._scopeProvider = new LoggerExternalScopeProvider();
      this._processor = new MailLoggerProcessor(this._options);

      this._loggers = new ConcurrentDictionary<string, MailLogger>();
    }


    public void Dispose()
    {
      foreach (var (_, logger) in this._loggers)
      {
        logger.Dispose();
      }
      this._processor.Dispose();
      GC.SuppressFinalize(this);
    }

    #endregion


    #region Functions

    public ILogger CreateLogger(string categoryName)
    {
      return this._loggers.GetOrAdd(categoryName,
        new MailLogger(categoryName, this._processor, this._scopeProvider, this._options));
    }


    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
      this._scopeProvider = scopeProvider;
    }

    #endregion

  }
}
