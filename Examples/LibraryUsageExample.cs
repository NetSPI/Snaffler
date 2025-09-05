using System;
using System.Collections.Generic;
using Snaffler;
using SnaffCore.Callbacks;
using SnaffCore.Concurrency;
using SnaffCore.Classifiers;

namespace Examples
{
    /// <summary>
    /// Example of how to use Snaffler as a library with callbacks
    /// </summary>
    public class LibraryUsageExample
    {
        public static void Main(string[] args)
        {
            // Create a new SnaffleRunner instance
            var runner = new SnaffleRunner();

            // Example 1: Simple action-based callback
            var actionCallback = runner.RegisterCallback(eventArgs =>
            {
                Console.WriteLine($"[CALLBACK] {eventArgs.LogLevel}: {eventArgs.FormattedMessage}");
            });

            // Example 2: Custom callback class
            var customCallback = new CustomCallback();
            runner.RegisterCallback(customCallback);

            // Example 3: File result specific callback
            var fileResultCallback = runner.RegisterCallback(eventArgs =>
            {
                if (eventArgs.Message.Type == SnafflerMessageType.FileResult && eventArgs.Message.FileResult != null)
                {
                    var fileResult = eventArgs.Message.FileResult;
                    Console.WriteLine($"[FILE FOUND] {fileResult.FileInfo.FullName} - Rule: {fileResult.MatchedRule?.RuleName}");
                }
            });

            try
            {
                // Run Snaffler with the provided arguments
                runner.Run(args);
            }
            finally
            {
                // Clean up callbacks when done
                runner.UnregisterCallback(actionCallback);
                runner.UnregisterCallback(customCallback);
                runner.UnregisterCallback(fileResultCallback);
            }

            Console.WriteLine("Snaffling completed!");
        }
    }

    /// <summary>
    /// Example of a custom callback implementation
    /// </summary>
    public class CustomCallback : ISnafflerCallback
    {
        private readonly List<SnafflerLogEventArgs> _events = new List<SnafflerLogEventArgs>();

        public void OnLogEvent(SnafflerLogEventArgs eventArgs)
        {
            // Store all events for later analysis
            _events.Add(eventArgs);

            // Handle different types of events
            switch (eventArgs.Message.Type)
            {
                case SnafflerMessageType.FileResult:
                    HandleFileResult(eventArgs);
                    break;
                case SnafflerMessageType.ShareResult:
                    HandleShareResult(eventArgs);
                    break;
                case SnafflerMessageType.DirResult:
                    HandleDirResult(eventArgs);
                    break;
                case SnafflerMessageType.Error:
                    HandleError(eventArgs);
                    break;
                case SnafflerMessageType.Fatal:
                    HandleFatal(eventArgs);
                    break;
            }
        }

        private void HandleFileResult(SnafflerLogEventArgs eventArgs)
        {
            var fileResult = eventArgs.Message.FileResult;
            if (fileResult != null)
            {
                Console.WriteLine($"[CUSTOM] Found file: {fileResult.FileInfo.Name}");
                Console.WriteLine($"[CUSTOM] Size: {fileResult.FileInfo.Length} bytes");
                Console.WriteLine($"[CUSTOM] Modified: {fileResult.FileInfo.LastWriteTime}");
                
                if (fileResult.TextResult != null)
                {
                    Console.WriteLine($"[CUSTOM] Matched text: {string.Join(", ", fileResult.TextResult.MatchedStrings)}");
                }
            }
        }

        private void HandleShareResult(SnafflerLogEventArgs eventArgs)
        {
            var shareResult = eventArgs.Message.ShareResult;
            if (shareResult != null)
            {
                Console.WriteLine($"[CUSTOM] Found share: {shareResult.SharePath}");
                Console.WriteLine($"[CUSTOM] Triage: {shareResult.Triage}");
                Console.WriteLine($"[CUSTOM] Readable: {shareResult.RootReadable}, Writable: {shareResult.RootWritable}");
            }
        }

        private void HandleDirResult(SnafflerLogEventArgs eventArgs)
        {
            var dirResult = eventArgs.Message.DirResult;
            if (dirResult != null)
            {
                Console.WriteLine($"[CUSTOM] Found directory: {dirResult.DirPath}");
                Console.WriteLine($"[CUSTOM] Triage: {dirResult.Triage}");
            }
        }

        private void HandleError(SnafflerLogEventArgs eventArgs)
        {
            Console.WriteLine($"[CUSTOM ERROR] {eventArgs.Message.Message}");
        }

        private void HandleFatal(SnafflerLogEventArgs eventArgs)
        {
            Console.WriteLine($"[CUSTOM FATAL] {eventArgs.Message.Message}");
            Console.WriteLine($"[CUSTOM] Total events processed: {_events.Count}");
        }

        public int EventCount => _events.Count;
        public List<SnafflerLogEventArgs> Events => new List<SnafflerLogEventArgs>(_events);
    }

    /// <summary>
    /// Example of a specialized callback for security monitoring
    /// </summary>
    public class SecurityMonitorCallback : ISnafflerCallback
    {
        private readonly List<string> _highValueFiles = new List<string>();
        private readonly List<string> _sensitiveShares = new List<string>();

        public void OnLogEvent(SnafflerLogEventArgs eventArgs)
        {
            switch (eventArgs.Message.Type)
            {
                case SnafflerMessageType.FileResult:
                    AnalyzeFileForSecurity(eventArgs.Message.FileResult);
                    break;
                case SnafflerMessageType.ShareResult:
                    AnalyzeShareForSecurity(eventArgs.Message.ShareResult);
                    break;
            }
        }

        private void AnalyzeFileForSecurity(FileResult fileResult)
        {
            if (fileResult?.MatchedRule?.Triage == Triage.Red)
            {
                _highValueFiles.Add(fileResult.FileInfo.FullName);
                Console.WriteLine($"[SECURITY ALERT] High-value file detected: {fileResult.FileInfo.FullName}");
            }
        }

        private void AnalyzeShareForSecurity(ShareResult shareResult)
        {
            if (shareResult?.RootWritable == true && shareResult.Triage == Triage.Red)
            {
                _sensitiveShares.Add(shareResult.SharePath);
                Console.WriteLine($"[SECURITY ALERT] Writable sensitive share: {shareResult.SharePath}");
            }
        }

        public List<string> HighValueFiles => new List<string>(_highValueFiles);
        public List<string> SensitiveShares => new List<string>(_sensitiveShares);
    }
}
