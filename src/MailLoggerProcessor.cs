using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DurMailLog
{
  class MailLoggerProcessor : IDisposable
  {

    #region Properties

    private readonly ConcurrentQueue<LogMessageEntry> _queue = new();
    private readonly Thread _thread;
    private readonly IOptionsMonitor<MailLoggerConfiguration> _options;

    #endregion


    #region Initialization

    public MailLoggerProcessor(IOptionsMonitor<MailLoggerConfiguration> optConfiguration)
    {
      this._options = optConfiguration;

      this._thread = new Thread(ScanningQueueThread)
      {
        IsBackground = true,
        Name = $"{nameof(MailLoggerProcessor)} Thread",

      };
      this._thread.Start();
    }

    public void Dispose()
    {
      try
      {
        _thread.Join(TimeSpan.FromSeconds(2));
      }
      catch (ThreadStateException) { }
    }

    #endregion


    #region Functions

    public void Enqueue(LogMessageEntry logItem)
    {
      this._queue.Enqueue(logItem);
    }


    private void ScanningQueueThread()
    {
      var options = _options.CurrentValue;
      using var client = new SmtpClient();
      
      while (true)
      {
        Thread.Sleep(TimeSpan.FromSeconds(options.IntervalSeconds));
        while (_queue.TryDequeue(out LogMessageEntry? logItem))
        {
          if (logItem == null)
          { continue; }

          try
          {
            var message = CreateMessage(options, logItem);
            if (message == null)
            { break; }

            if (!client.IsConnected)
            {
              client.Connect(options.Host, options.Port, options.UseSsl);
            }
            if (!client.IsAuthenticated)
            {
              client.Authenticate(options.User, options.Password);
            }
            client.Send(message);
          }
          catch
          {
          }
        }
      }
    }


    private static MimeMessage? CreateMessage(MailLoggerConfiguration options,
      LogMessageEntry logItem)
    {
      var message = new MimeMessage();
      message.From.Add(MailboxAddress.Parse(options.From));


      bool hasReceivers = false;
      foreach (var to in GetMailboxAddresses(
        options.To, logItem.LogLevel, logItem.CategoryName))
      {
        hasReceivers = true;
        message.To.Add(to);
      }

      foreach (var cc in GetMailboxAddresses(
        options.CC, logItem.LogLevel, logItem.CategoryName))
      {
        hasReceivers = true;
        message.Cc.Add(cc);
      }

      foreach (var bcc in GetMailboxAddresses(
        options.Bcc, logItem.LogLevel, logItem.CategoryName))
      {
        hasReceivers = true;
        message.Bcc.Add(bcc);
      }

      if (!hasReceivers)
      { return null!; }

      string subject = $"[{logItem.LogLevel}][{Environment.MachineName}]" +
        $"[{AppDomain.CurrentDomain.FriendlyName}][{logItem.CategoryName}]";

      if (subject != null)
      {
        message.Subject = subject;
      }

      var log = new StringBuilder()
        .AppendLine($"<h1>{logItem.CategoryName}</h1>")r
        .AppendLine($"EventId: <b>[{logItem.EventId}]</b><br />")
        .AppendLine($"Time: <b>{DateTime.Now}</b><br />")
        .AppendLine($"Host: <b>{Environment.MachineName}</b><br />")
        .AppendLine($"App: <b>{AppDomain.CurrentDomain.FriendlyName}</b><br />")
        .AppendLine($"PID: <b>{Environment.ProcessId}</b><br />")
        .AppendLine("<br />")
        .AppendLine($"<p><span style=\"font-family: monospace\">");

      var logLevel = LogLevel.Information;
      foreach (var logRow in logItem.Message.Split(Environment.NewLine + "\""))
      {
        foreach(var ll in Enum.GetValues<LogLevel>().OrderByDescending(l => l))
        {
          if(logRow.Split(";").Contains("\"" + ll + "\""))
          {
            logLevel = ll;
          }
        }

        string style = String.Empty;
        if((int)logLevel >= (int)LogLevel.Error)
        {
          style = "style=\"color: red\"";
        } else if ((int)logLevel >= (int)LogLevel.Warning)
        {
          style = "style=\"color: orange\"";
        }
        log.AppendLine($"<span {style}>" + (logRow.StartsWith("\"") ? "" : "\"") + logRow.Replace(Environment.NewLine, "<br />") + "</span><br />");
      }
      //$"{logItem.Message.Replace(Environment.NewLine, "<br />" + Environment.NewLine)}</span></p>");

      if (logItem.Exception != null)
      {
        log.AppendLine("<br />")
          .AppendLine($"<p><span style=\"color: red; font-family: monospace\">" +
            $"{logItem.Exception.Message.Replace(Environment.NewLine, "<br />" + Environment.NewLine)}</span></p>");
      }

      var bb = new BodyBuilder
      {
        HtmlBody = log.ToString().Trim()
      };
      message.Body = bb.ToMessageBody();

      message.Headers.Add(HeaderId.XMailer, "DurLogMailer");
      return message;
    }


    private static IEnumerable<MailboxAddress> GetMailboxAddresses(
      Dictionary<string, Dictionary<string, LogLevel>> mailAddresses,
      LogLevel logLevel, string categoryName)
    {
      var ret = new List<MailboxAddress>();
      foreach (var (mailAdress, categoryLevel) in mailAddresses)
      {
        bool add = false;
        string? tmpCategory = categoryName;
        do
        {
          if (tmpCategory == null)
          { break; }

          if (categoryLevel.TryGetValue(tmpCategory, out LogLevel mailLevel))
          {
            if (logLevel >= mailLevel)
            {
              add = true;
              break;
            }
            else
            {
              break;
            }
          }

          if (tmpCategory.Split(".").Length <= 1)
          {
            tmpCategory = "Default";
          }
          else if (tmpCategory != "Default")
          {
            tmpCategory = String.Join(".", tmpCategory.Split(".")[0..^1]);
          }
          else if (tmpCategory == "Default")
          {
            tmpCategory = null;
          }
        } while (!add);

        if (add)
        {
          ret.Add(MailboxAddress.Parse(mailAdress));
        }
      }

      return ret;
    }

    #endregion

  }
}
