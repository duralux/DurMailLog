using System;

namespace DurMailLog
{
  class NullDisposable : IDisposable
  {
    public static readonly NullDisposable Instance = new();

    public void Dispose()
    {
    }
  }
}
