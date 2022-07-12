using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace DurMailLog
{
  public static class MailLoggerExtensions
  {

    #region Static Functions

    public static ILoggingBuilder AddMailLog(this ILoggingBuilder builder)
    {
      if (builder == null)
      { throw new ArgumentNullException(nameof(builder)); }

      builder.Services.TryAddEnumerable(
        ServiceDescriptor.Singleton<ILoggerProvider, MailLoggerProvider>());

      return builder;
    }


    public static ILoggingBuilder AddMailLogger(this ILoggingBuilder builder,
      Action<MailLoggerConfiguration> configure)
    {
      if (configure == null)
      { throw new ArgumentNullException(nameof(configure)); }

      if (builder == null)
      { throw new ArgumentNullException(nameof(builder)); }

      builder.Services.TryAddEnumerable(
        ServiceDescriptor.Singleton<ILoggerProvider, MailLoggerProvider>());

      builder.Services.Configure(configure);
      return builder;
    }

    #endregion

  }
}
