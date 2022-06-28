using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace WmsHub.Tests.Helper
{
  public class SerilogLoggerMock : ILogger
  {
    public List<string> Messages { get; set; } = new List<string>();
    public List<Exception> Exceptions { get; set; } = new List<Exception>();

    public void Debug(string messageTemplate)
    {
      Messages.Add(messageTemplate);
    }

    public void Debug<T>(string messageTemplate, T propertyValue)
    {
      string serialisedObject = JsonConvert.SerializeObject(propertyValue);
      Messages.Add($"{messageTemplate}:{serialisedObject}");
    }

    public void Debug<T0, T1>(
      string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
      Messages.Add(messageTemplate);
    }

    public void Debug<T0, T1, T2>(
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    public void Debug(string messageTemplate, params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }

    public void Debug(Exception exception, string messageTemplate)
    {
      Messages.Add(messageTemplate);
    }

    public void Debug<T>(
      Exception exception,
      string messageTemplate,
      T propertyValue)
    {
      Messages.Add(messageTemplate);
    }

    public void Debug<T0, T1>(
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1)
    {
      Messages.Add(messageTemplate);
    }

    public void Debug<T0, T1, T2>(
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    public void Debug(
      Exception exception,
      string messageTemplate,
      params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }

    public void Error(string messageTemplate)
    {
      Messages.Add(messageTemplate);
    }

    public void Error<T>(string messageTemplate, T propertyValue)
    {
      Messages.Add(messageTemplate);
    }

    public void Error<T0, T1>(
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1)
    {
      Messages.Add(messageTemplate);
    }

    public void Error<T0, T1, T2>(
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    public void Error(string messageTemplate, params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }

    public void Error(Exception exception, string messageTemplate)
    {
      Messages.Add(exception.Message);
      Exceptions.Add(exception);
    }

    public void Error<T>(
      Exception exception,
      string messageTemplate,
      T propertyValue)
    {
      Messages.Add(messageTemplate);
    }

    public void Error<T0, T1>(
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1)
    {
      Messages.Add(messageTemplate);
    }

    public void Error<T0, T1, T2>(
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    public void Error(
      Exception exception,
      string messageTemplate,
      params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }

    public void Fatal(string messageTemplate)
    {
      Messages.Add(messageTemplate);
    }

    public void Fatal<T>(string messageTemplate, T propertyValue)
    {
      Messages.Add(messageTemplate);
    }

    public void Fatal<T0, T1>(
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1)
    {
      Messages.Add(messageTemplate);
    }

    public void Fatal<T0, T1, T2>(
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    public void Fatal(string messageTemplate, params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }

    public void Fatal(Exception exception, string messageTemplate)
    {
      Messages.Add(messageTemplate);
    }

    public void Fatal<T>(
      Exception exception,
      string messageTemplate,
      T propertyValue)
    {
      Messages.Add(messageTemplate);
    }

    public void Fatal<T0, T1>(
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1)
    {
      Messages.Add(messageTemplate);
    }

    public void Fatal<T0, T1, T2>(
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    public void Fatal(
      Exception exception,
      string messageTemplate,
      params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }

    public ILogger ForContext(ILogEventEnricher enricher)
    {
      return this;
    }

    public ILogger ForContext(IEnumerable<ILogEventEnricher> enrichers)
    {
      return this;
    }

    public ILogger ForContext(
      string propertyName,
      object value,
      bool destructureObjects = false)
    {
      return this;
    }

    public ILogger ForContext<TSource>()
    {
      return this;
    }

    public ILogger ForContext(Type source)
    {
      return this;
    }

    public void Information(string messageTemplate)
    {
      Messages.Add(messageTemplate);
    }

    public void Information<T>(
      string messageTemplate,
      T propertyValue)
    {
      Messages.Add(messageTemplate);
    }

    public void Information<T0, T1>(
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1)
    {
      Messages.Add(messageTemplate);
    }

    public void Information<T0, T1, T2>(
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    public void Information(
      string messageTemplate,
      params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }

    public void Information(Exception exception, string messageTemplate)
    {
      Messages.Add(messageTemplate);
    }

    public void Information<T>(
      Exception exception,
      string messageTemplate,
      T propertyValue)
    {
      Messages.Add(messageTemplate);
    }

    public void Information<T0, T1>(
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1)
    {
      Messages.Add(messageTemplate);
    }

    public void Information<T0, T1, T2>(
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    void ILogger.Information(
      Exception exception,
      string messageTemplate,
      params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }

    public bool IsEnabled(LogEventLevel level)
    {
      return true;
    }

    public void Verbose(string messageTemplate)
    {
      Messages.Add(messageTemplate);
    }

    public void Verbose<T>(string messageTemplate, T propertyValue)
    {
      string serialisedObject = JsonConvert.SerializeObject(propertyValue);
      Messages.Add($"{messageTemplate}:{serialisedObject}");
    }

    public void Verbose<T0, T1>(
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1)
    {
      Messages.Add(messageTemplate);
    }

    public void Verbose<T0, T1, T2>(
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    public void Verbose(string messageTemplate, params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }

    public void Verbose(Exception exception, string messageTemplate)
    {
      Messages.Add(messageTemplate);
    }

    public void Verbose<T>(
      Exception exception,
      string messageTemplate,
      T propertyValue)
    {
      Messages.Add(messageTemplate);
    }

    public void Verbose<T0, T1>(
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1)
    {
      Messages.Add(messageTemplate);
    }

    public void Verbose<T0, T1, T2>(
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    public void Verbose(
      Exception exception,
      string messageTemplate,
      params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }

    public void Warning(string messageTemplate)
    {
      Messages.Add(messageTemplate);
    }

    public void Warning<T>(string messageTemplate, T propertyValue)
    {
      Messages.Add(messageTemplate);
    }

    public void Warning<T0, T1>(
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1)
    {
      Messages.Add(messageTemplate);
    }

    public void Warning<T0, T1, T2>(
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    public void Warning(string messageTemplate, params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }

    public void Warning(Exception exception, string messageTemplate)
    {
      Messages.Add(messageTemplate);
    }

    public void Warning<T>(
      Exception exception,
      string messageTemplate,
      T propertyValue)
    {
      Messages.Add(messageTemplate);
    }

    public void Warning<T0, T1>(
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1)
    {
      Messages.Add(messageTemplate);
    }

    public void Warning<T0, T1, T2>(
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    public void Warning(
      Exception exception,
      string messageTemplate,
      params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }

    public void Write(LogEvent logEvent)
    { }

    public void Write(LogEventLevel level, string messageTemplate)
    {
      Messages.Add(messageTemplate);
    }

    public void Write<T>(
      LogEventLevel level,
      string messageTemplate,
      T propertyValue)
    {
      Messages.Add(messageTemplate);
    }

    public void Write<T0, T1>(
      LogEventLevel level,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1)
    {
      Messages.Add(messageTemplate);
    }

    public void Write<T0, T1, T2>(
      LogEventLevel level,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    public void Write(
      LogEventLevel level,
      string messageTemplate,
      params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }

    public void Write(
      LogEventLevel level,
      Exception exception,
      string messageTemplate)
    {
      Messages.Add(messageTemplate);
    }

    public void Write<T>(
      LogEventLevel level,
      Exception exception,
      string messageTemplate,
      T propertyValue)
    {
      Messages.Add(exception.Message);
      Exceptions.Add(exception);
    }

    public void Write<T0, T1>(
      LogEventLevel level,
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1)
    {
      Messages.Add(exception.Message);
      Exceptions.Add(exception);
    }

    public void Write<T0, T1, T2>(
      LogEventLevel level,
      Exception exception,
      string messageTemplate,
      T0 propertyValue0,
      T1 propertyValue1,
      T2 propertyValue2)
    {
      Messages.Add(messageTemplate);
    }

    public void Write(
      LogEventLevel level,
      Exception exception,
      string messageTemplate,
      params object[] propertyValues)
    {
      Messages.Add(messageTemplate);
    }
  }
}
