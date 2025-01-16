using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sysnapse.OrdersExample.Test;

internal class TestLogger: ILogger, IDisposable
{
    private readonly ITestOutputHelper _output;
    private bool disposedValue;

    //CTOR
    public TestLogger(ITestOutputHelper output)
    {
        _output = output;
    }


    /// <summary>
    /// Write log entry to test output
    /// </summary>
    /// <typeparam name="TState">type of object to be written</typeparam>
    /// <param name="logLevel">level of log entry</param>
    /// <param name="eventId">event id</param>
    /// <param name="state">entry to be written</param>
    /// <param name="exception">exception related to entry</param>
    /// <param name="formatter">formatting function for state and exception</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter) 
    {
        _output.WriteLine($"LogLevel: {logLevel}: {state}");
    }

    /// <summary>
    ///  Is loglevel set
    /// </summary>
    /// <param name="logLevel"></param>
    /// <returns></returns>
    public bool IsEnabled(LogLevel logLevel) => (int)logLevel > 0;


    /// <summary>
    /// Logging scope
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => this;

    /// <summary>
    /// Logging scope
    /// </summary>
    /// <param name="messageFormat"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public IDisposable? BeginScope(string messageFormat, params object[] args) => this;

    #region dispose
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~TestLogger()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}

internal class TestLogger<T> : ILogger<T>, IDisposable
    where T : class
{
    private readonly ITestOutputHelper _output;
    private bool disposedValue;

    //CTOR
    public TestLogger(ITestOutputHelper output)
    {
        _output = output;
    }


    /// <summary>
    /// Write log entry to test output
    /// </summary>
    /// <typeparam name="TState">type of object to be written</typeparam>
    /// <param name="logLevel">level of log entry</param>
    /// <param name="eventId">event id</param>
    /// <param name="state">entry to be written</param>
    /// <param name="exception">exception related to entry</param>
    /// <param name="formatter">formatting function for state and exception</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    {
        _output.WriteLine($"LogLevel: {logLevel}: {state}");
    }

    /// <summary>
    ///  Is loglevel set
    /// </summary>
    /// <param name="logLevel"></param>
    /// <returns></returns>
    public bool IsEnabled(LogLevel logLevel) => (int)logLevel > 0;


    /// <summary>
    /// Logging scope
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => this;

    /// <summary>
    /// Logging scope
    /// </summary>
    /// <param name="messageFormat"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public IDisposable? BeginScope(string messageFormat, params object[] args) => this;

    #region dispose
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~TestLogger()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
