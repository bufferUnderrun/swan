﻿using Swan.Logging;
using System;

namespace Swan.Lite.Logging
{
    /// <summary>
    /// Use this class for text-based logger.
    /// </summary>
    public abstract class TextLogger
    {
        /// <summary>
        /// Gets or sets the logging time format.
        /// set to null or empty to prevent output.
        /// </summary>
        /// <value>
        /// The logging time format.
        /// </value>
        public static string LoggingTimeFormat { get; set; } = "HH:mm:ss.fff";

        /// <summary>
        /// Gets the color of the output of the message (the output message has a new line char in the end).
        /// </summary>
        /// <param name="logEvent">The <see cref="LogMessageReceivedEventArgs" /> instance containing the event data.</param>
        /// <param name="outputMessage">The output message.</param>
        /// <returns>
        /// The color of the console to be used.
        /// </returns>
        protected ConsoleColor GetOutputAndColor(
            LogMessageReceivedEventArgs logEvent, 
            out string outputMessage)
        {
            var (prefix , color) = GetConsoleColorAndPrefix(logEvent.MessageType);

            var loggerMessage = string.IsNullOrWhiteSpace(logEvent.Message)
                ? string.Empty
                : logEvent.Message.RemoveControlCharsExcept('\n');

            outputMessage = CreateOutputMessage(logEvent.Source, loggerMessage, prefix, logEvent.UtcDate);

            // Further format the output in the case there is an exception being logged
            if (logEvent.MessageType == LogLevel.Error && logEvent.Exception != null)
            {
                try
                {
                    outputMessage += $"{logEvent.Exception.Stringify().Indent()}{Environment.NewLine}";
                }
                catch
                {
                    // Ignore  
                }
            }

            return color;
        }

        private static (string Prefix, ConsoleColor color) GetConsoleColorAndPrefix(LogLevel messageType)
        {
            switch (messageType)
            {
                case LogLevel.Debug:
                    return (ConsoleLogger.DebugPrefix, ConsoleLogger.DebugColor);
                case LogLevel.Error:
                    return (ConsoleLogger.ErrorPrefix, ConsoleLogger.ErrorColor);
                case LogLevel.Info:
                    return (ConsoleLogger.InfoPrefix, ConsoleLogger.InfoColor);
                case LogLevel.Trace:
                    return (ConsoleLogger.TracePrefix, ConsoleLogger.TraceColor);
                case LogLevel.Warning:
                    return (ConsoleLogger.WarnPrefix, ConsoleLogger.WarnColor);
                case LogLevel.Fatal:
                    return (ConsoleLogger.FatalPrefix, ConsoleLogger.FatalColor);
                default:
                    return (new string(' ', ConsoleLogger.InfoPrefix.Length), Terminal.Settings.DefaultColor);
            }
        }

        private static string CreateOutputMessage(string sourceName, string loggerMessage, string prefix, DateTime date)
        {
            var friendlySourceName = string.IsNullOrWhiteSpace(sourceName)
                ? string.Empty
                : sourceName.SliceLength(sourceName.LastIndexOf('.') + 1, sourceName.Length);

            var outputMessage = string.IsNullOrWhiteSpace(sourceName)
                ? loggerMessage
                : $"[{friendlySourceName}] {loggerMessage}";

            return string.IsNullOrWhiteSpace(LoggingTimeFormat)
                ? $" {prefix} >> {outputMessage}{Environment.NewLine}"
                : $" {date.ToLocalTime().ToString(LoggingTimeFormat)} {prefix} >> {outputMessage}{Environment.NewLine}";
        }
    }
}
