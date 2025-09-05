using SnaffCore.Concurrency;
using System;

namespace SnaffCore.Callbacks
{
    /// <summary>
    /// Interface for receiving Snaffler logging event callbacks
    /// </summary>
    public interface ISnafflerCallback
    {
        /// <summary>
        /// Called when a logging event occurs
        /// </summary>
        /// <param name="eventArgs">Event arguments containing the logging information</param>
        void OnLogEvent(SnafflerLogEventArgs eventArgs);
    }

    /// <summary>
    /// Event arguments for Snaffler logging events
    /// </summary>
    public class SnafflerLogEventArgs : EventArgs
    {
        /// <summary>
        /// The original message from the queue
        /// </summary>
        public SnafflerMessage Message { get; set; }

        /// <summary>
        /// The formatted log message that would be written to console/file
        /// </summary>
        public string FormattedMessage { get; set; }

        /// <summary>
        /// The log level (corresponds to NLog levels)
        /// </summary>
        public SnafflerLogLevel LogLevel { get; set; }

        /// <summary>
        /// The timestamp when the event was processed
        /// </summary>
        public DateTime ProcessedAt { get; set; }

        /// <summary>
        /// The host string (user@hostname format)
        /// </summary>
        public string HostString { get; set; }

        public SnafflerLogEventArgs(SnafflerMessage message, string formattedMessage, SnafflerLogLevel logLevel, string hostString)
        {
            Message = message;
            FormattedMessage = formattedMessage;
            LogLevel = logLevel;
            HostString = hostString;
            ProcessedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Simplified log levels for callbacks
    /// </summary>
    public enum SnafflerLogLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
        Data
    }
}
