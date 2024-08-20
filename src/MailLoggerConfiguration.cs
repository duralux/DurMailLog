using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DurMailLog
{
  public class MailLoggerConfiguration
  {

    public const string NAME = "Logging:MailLog";


    #region Properties

    public string? Host { get; set; }
    public int Port { get; set; } = 25;
    public bool UseSsl { get; set; } = true;
    public string? User { get; set; }
    public string? Password { get; set; }
    public int IntervalSeconds { get; set; } = 1;
    public Dictionary<string, LogLevel> LogLevel { get; set; }

    public string? From { get; set; }
    public Dictionary<string, Dictionary<string, LogLevel>> To { get; set; }
    public Dictionary<string, Dictionary<string, LogLevel>> CC { get; set; }
    public Dictionary<string, Dictionary<string, LogLevel>> Bcc { get; set; }

    public string? LogSplit { get; set; }



    #endregion


    #region Initialization

    public MailLoggerConfiguration()
    {
      this.LogLevel = [];
      this.To = [];
      this.CC = [];
      this.Bcc = [];
    }


    public MailLoggerConfiguration(IConfiguration configuration) : this()
    {
      var options = configuration
        .GetSection("Logging:MailLog")
        .Get<MailLoggerConfiguration>();

      if (options != null)
      {
        this.Host = options.Host;
        this.Port = options.Port;
        this.UseSsl = options.UseSsl;
        this.User = options.User;
        this.Password = options.Password;
        this.IntervalSeconds = options.IntervalSeconds;
        this.From = options.From ?? options.User;
        this.LogLevel = options.LogLevel;
        this.To = options.To;
        this.CC = options.CC;
        this.Bcc = options.Bcc;
      }
    }

    #endregion

  }
}
