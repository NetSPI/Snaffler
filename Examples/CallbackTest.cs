using System;
using System.Collections.Generic;
using Snaffler;
using SnaffCore.Callbacks;
using SnaffCore.Concurrency;

namespace Examples
{
    /// <summary>
    /// Simple test to verify the callback system is working
    /// </summary>
    public class CallbackTest
    {
        public static void TestCallbacks()
        {
            Console.WriteLine("Testing Snaffler Callback System...");
            
            var runner = new SnaffleRunner();
            var receivedEvents = new List<SnafflerLogEventArgs>();
            
            // Register a test callback
            var callback = runner.RegisterCallback(eventArgs =>
            {
                receivedEvents.Add(eventArgs);
                Console.WriteLine($"[TEST CALLBACK] {eventArgs.LogLevel}: {eventArgs.Message.Type}");
                
                // Print some details based on message type
                switch (eventArgs.Message.Type)
                {
                    case SnafflerMessageType.FileResult when eventArgs.Message.FileResult != null:
                        Console.WriteLine($"  -> File: {eventArgs.Message.FileResult.FileInfo?.FullName}");
                        break;
                    case SnafflerMessageType.ShareResult when eventArgs.Message.ShareResult != null:
                        Console.WriteLine($"  -> Share: {eventArgs.Message.ShareResult.SharePath}");
                        break;
                    case SnafflerMessageType.DirResult when eventArgs.Message.DirResult != null:
                        Console.WriteLine($"  -> Directory: {eventArgs.Message.DirResult.DirPath}");
                        break;
                    case SnafflerMessageType.Info:
                    case SnafflerMessageType.Error:
                    case SnafflerMessageType.Trace:
                    case SnafflerMessageType.Degub:
                        Console.WriteLine($"  -> Message: {eventArgs.Message.Message}");
                        break;
                }
            });
            
            // Register a second callback to test multiple callbacks
            var errorCallback = runner.RegisterCallback(eventArgs =>
            {
                if (eventArgs.LogLevel == SnafflerLogLevel.Error || 
                    eventArgs.LogLevel == SnafflerLogLevel.Fatal)
                {
                    Console.WriteLine($"[ERROR CALLBACK] {eventArgs.FormattedMessage}");
                }
            });
            
            Console.WriteLine("Callbacks registered successfully!");
            Console.WriteLine("Run Snaffler with some arguments to see the callbacks in action.");
            Console.WriteLine("Example: dotnet run -- -s -o snaffler_output.txt");
            Console.WriteLine();
            
            try
            {
                // For testing purposes, we'll run with minimal arguments
                // In a real scenario, you'd pass actual command line arguments
                string[] testArgs = { "--help" }; // This will show help and exit quickly
                
                Console.WriteLine("Running Snaffler with --help argument...");
                runner.Run(testArgs);
                
                Console.WriteLine($"\nTest completed! Received {receivedEvents.Count} callback events.");
                
                // Print summary of received events
                var eventTypeCounts = new Dictionary<SnafflerMessageType, int>();
                foreach (var evt in receivedEvents)
                {
                    if (eventTypeCounts.ContainsKey(evt.Message.Type))
                        eventTypeCounts[evt.Message.Type]++;
                    else
                        eventTypeCounts[evt.Message.Type] = 1;
                }
                
                Console.WriteLine("\nEvent Summary:");
                foreach (var kvp in eventTypeCounts)
                {
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test completed with exception: {ex.Message}");
                Console.WriteLine($"Received {receivedEvents.Count} callback events before exception.");
            }
            finally
            {
                // Clean up
                runner.UnregisterCallback(callback);
                runner.UnregisterCallback(errorCallback);
                Console.WriteLine("Callbacks unregistered.");
            }
        }
        
        /// <summary>
        /// Test the callback system with a custom callback class
        /// </summary>
        public static void TestCustomCallback()
        {
            Console.WriteLine("\nTesting Custom Callback Class...");
            
            var runner = new SnaffleRunner();
            var customCallback = new TestCallback();
            
            runner.RegisterCallback(customCallback);
            
            try
            {
                // Run with help to generate some events
                runner.Run(new[] { "--help" });
                
                Console.WriteLine($"Custom callback received {customCallback.EventCount} events");
                Console.WriteLine($"Info events: {customCallback.InfoEventCount}");
                Console.WriteLine($"Error events: {customCallback.ErrorEventCount}");
            }
            finally
            {
                runner.UnregisterCallback(customCallback);
                Console.WriteLine("Custom callback unregistered.");
            }
        }
        
        public static void Main(string[] args)
        {
            TestCallbacks();
            TestCustomCallback();
            
            Console.WriteLine("\nAll tests completed!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
    
    /// <summary>
    /// Test callback implementation
    /// </summary>
    public class TestCallback : ISnafflerCallback
    {
        private int _eventCount = 0;
        private int _infoEventCount = 0;
        private int _errorEventCount = 0;
        
        public void OnLogEvent(SnafflerLogEventArgs eventArgs)
        {
            _eventCount++;
            
            switch (eventArgs.LogLevel)
            {
                case SnafflerLogLevel.Info:
                    _infoEventCount++;
                    break;
                case SnafflerLogLevel.Error:
                case SnafflerLogLevel.Fatal:
                    _errorEventCount++;
                    break;
            }
            
            Console.WriteLine($"[CUSTOM TEST] Event #{_eventCount}: {eventArgs.Message.Type} - {eventArgs.LogLevel}");
        }
        
        public int EventCount => _eventCount;
        public int InfoEventCount => _infoEventCount;
        public int ErrorEventCount => _errorEventCount;
    }
}
