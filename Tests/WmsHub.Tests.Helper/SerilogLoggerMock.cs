using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WmsHub.Tests.Helper;

public class SerilogLoggerMock : ILogger
{
  public List<string> Messages { get; set; } = new List<string>();
  public List<Exception> Exceptions { get; set; } = new List<Exception>();

  public void Debug(string messageTemplate) => Messages.Add(messageTemplate);

  public void Debug<T>(string messageTemplate, T propertyValue)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue);
    Messages.Add(messageTemplate);
  }

  public void Debug<T0, T1>(
    string messageTemplate, T0 propertyValue0, T1 propertyValue1)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
    Messages.Add(messageTemplate);
  }

  public void Debug<T0, T1, T2>(
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1,
    T2 propertyValue2)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  public void Debug(string messageTemplate, params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
    Messages.Add(messageTemplate);
  }

  public void Debug(Exception exception, string messageTemplate) => Messages.Add(messageTemplate);

  public void Debug<T>(
    Exception exception,
    string messageTemplate,
    T propertyValue)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue);
    Messages.Add(messageTemplate);
  }

  public void Debug<T0, T1>(
    Exception exception,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
    Messages.Add(messageTemplate);
  }

  public void Debug<T0, T1, T2>(
    Exception exception,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1,
    T2 propertyValue2)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  public void Debug(
    Exception exception,
    string messageTemplate,
    params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
    Messages.Add(messageTemplate);
  }

  public void Error(string messageTemplate) => Messages.Add(messageTemplate);

  public void Error<T>(string messageTemplate, T propertyValue)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue);
    Messages.Add(messageTemplate);
  }

  public void Error<T0, T1>(
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
    Messages.Add(messageTemplate);
  }

  public void Error<T0, T1, T2>(
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1,
    T2 propertyValue2)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  public void Error(string messageTemplate, params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
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
    InsertTemplateParameters(ref messageTemplate, propertyValue);
    Messages.Add(messageTemplate);
  }

  public void Error<T0, T1>(
    Exception exception,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
    Messages.Add(messageTemplate);
  }

  public void Error<T0, T1, T2>(
    Exception exception,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1,
    T2 propertyValue2)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  public void Error(
    Exception exception,
    string messageTemplate,
    params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
    Messages.Add(messageTemplate);
  }

  public void Fatal(string messageTemplate) => Messages.Add(messageTemplate);

  public void Fatal<T>(string messageTemplate, T propertyValue)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue);
    Messages.Add(messageTemplate);
  }

  public void Fatal<T0, T1>(
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
    Messages.Add(messageTemplate);
  }

  public void Fatal<T0, T1, T2>(
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1,
    T2 propertyValue2)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  public void Fatal(string messageTemplate, params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
    Messages.Add(messageTemplate);
  }

  public void Fatal(Exception exception, string messageTemplate) => Messages.Add(messageTemplate);

  public void Fatal<T>(
    Exception exception,
    string messageTemplate,
    T propertyValue)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue);
    Messages.Add(messageTemplate);
  }

  public void Fatal<T0, T1>(
    Exception exception,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
    Messages.Add(messageTemplate);
  }

  public void Fatal<T0, T1, T2>(
    Exception exception,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1,
    T2 propertyValue2)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  public void Fatal(
    Exception exception,
    string messageTemplate,
    params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
    Messages.Add(messageTemplate);
  }

  public ILogger ForContext(ILogEventEnricher enricher) => this;

  public ILogger ForContext(IEnumerable<ILogEventEnricher> enrichers) => this;

  public ILogger ForContext(
    string propertyName,
    object value,
    bool destructureObjects = false) => this;

  public ILogger ForContext<TSource>() => this;

  public ILogger ForContext(Type source) => this;

  public void Information(string messageTemplate) => Messages.Add(messageTemplate);

  public void Information<T>(
    string messageTemplate,
    T propertyValue)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue);
    Messages.Add(messageTemplate);
  }

  public void Information<T0, T1>(
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
    Messages.Add(messageTemplate);
  }

  public void Information<T0, T1, T2>(
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1,
    T2 propertyValue2)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  public void Information(
    string messageTemplate,
    params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
    Messages.Add(messageTemplate);
  }

  public void Information(Exception exception, string messageTemplate) => Messages.Add(messageTemplate);

  public void Information<T>(
    Exception exception,
    string messageTemplate,
    T propertyValue)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue);
    Messages.Add(messageTemplate);
  }

  public void Information<T0, T1>(
    Exception exception,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
    Messages.Add(messageTemplate);
  }

  public void Information<T0, T1, T2>(
    Exception exception,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1,
    T2 propertyValue2)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  void ILogger.Information(
    Exception exception,
    string messageTemplate,
    params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
    Messages.Add(messageTemplate);
  }

  public bool IsEnabled(LogEventLevel level) => true;

  public void Verbose(string messageTemplate) => Messages.Add(messageTemplate);

  public void Verbose<T>(string messageTemplate, T propertyValue)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue);
    Messages.Add(messageTemplate);
  }

  public void Verbose<T0, T1>(
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
    Messages.Add(messageTemplate);
  }

  public void Verbose<T0, T1, T2>(
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1,
    T2 propertyValue2)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  public void Verbose(string messageTemplate, params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
    Messages.Add(messageTemplate);
  }

  public void Verbose(Exception exception, string messageTemplate) => Messages.Add(messageTemplate);

  public void Verbose<T>(
    Exception exception,
    string messageTemplate,
    T propertyValue)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue);
    Messages.Add(messageTemplate);
  }

  public void Verbose<T0, T1>(
    Exception exception,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
    Messages.Add(messageTemplate);
  }

  public void Verbose<T0, T1, T2>(
    Exception exception,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1,
    T2 propertyValue2)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  public void Verbose(
    Exception exception,
    string messageTemplate,
    params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
    Messages.Add(messageTemplate);
  }

  public void Warning(string messageTemplate) => Messages.Add(messageTemplate);

  public void Warning<T>(string messageTemplate, T propertyValue)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue);
    Messages.Add(messageTemplate);
  }

  public void Warning<T0, T1>(
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
    Messages.Add(messageTemplate);
  }

  public void Warning<T0, T1, T2>(
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1,
    T2 propertyValue2)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  public void Warning(string messageTemplate, params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
    Messages.Add(messageTemplate);
  }

  public void Warning(Exception exception, string messageTemplate) => Messages.Add(messageTemplate);

  public void Warning<T>(
    Exception exception,
    string messageTemplate,
    T propertyValue)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue);
    Messages.Add(messageTemplate);
  }

  public void Warning<T0, T1>(
    Exception exception,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
    Messages.Add(messageTemplate);
  }

  public void Warning<T0, T1, T2>(
    Exception exception,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1,
    T2 propertyValue2)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  public void Warning(
    Exception exception,
    string messageTemplate,
    params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
    Messages.Add(messageTemplate);
  }

  public void Write(LogEvent logEvent)
  { }

  public void Write(LogEventLevel level, string messageTemplate) => Messages.Add(messageTemplate);

  public void Write<T>(
    LogEventLevel level,
    string messageTemplate,
    T propertyValue)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue);
    Messages.Add(messageTemplate);
  }

  public void Write<T0, T1>(
    LogEventLevel level,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
    Messages.Add(messageTemplate);
  }

  public void Write<T0, T1, T2>(
    LogEventLevel level,
    string messageTemplate,
    T0 propertyValue0,
    T1 propertyValue1,
    T2 propertyValue2)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  public void Write(
    LogEventLevel level,
    string messageTemplate,
    params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
    Messages.Add(messageTemplate);
  }

  public void Write(
    LogEventLevel level,
    Exception exception,
    string messageTemplate) => Messages.Add(messageTemplate);

  public void Write<T>(
    LogEventLevel level,
    Exception exception,
    string messageTemplate,
    T propertyValue)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValue);
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
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1);
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
    InsertTemplateParameters(ref messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    Messages.Add(messageTemplate);
  }

  public void Write(
    LogEventLevel level,
    Exception exception,
    string messageTemplate,
    params object[] propertyValues)
  {
    InsertTemplateParameters(ref messageTemplate, propertyValues);
    Messages.Add(messageTemplate);
  }

  private static void InsertTemplateParameters(ref string template, params object[] propertyValues)
  {
    MatchCollection matches = Regex.Matches(template, @"\{([^\}]+)\}");

    for (int i = 0; i < matches.Count; i++)
    {
      template = template.Replace(matches[i].Value, propertyValues[i].ToString());
    }
  }
}
