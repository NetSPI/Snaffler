# Snaffler Callback System

This document explains how to use Snaffler as a library with callback functionality to receive real-time logging events.

## Overview

The callback system allows you to register handlers that will be called whenever Snaffler processes a logging event. This is useful when using Snaffler as a library in your own applications, as you can receive and process events in real-time without having to parse log files.

## Basic Usage

### 1. Create a SnaffleRunner Instance

```csharp
var runner = new SnaffleRunner();
```

### 2. Register Callbacks

#### Simple Action-Based Callback
```csharp
var callback = runner.RegisterCallback(eventArgs =>
{
    Console.WriteLine($"[LOG] {eventArgs.LogLevel}: {eventArgs.FormattedMessage}");
});
```

#### Custom Callback Class
```csharp
public class MyCallback : ISnafflerCallback
{
    public void OnLogEvent(SnafflerLogEventArgs eventArgs)
    {
        // Handle the event
        Console.WriteLine($"Received: {eventArgs.Message.Type}");
    }
}

var myCallback = new MyCallback();
runner.RegisterCallback(myCallback);
```

### 3. Run Snaffler
```csharp
runner.Run(args);
```

### 4. Unregister Callbacks (Optional)
```csharp
runner.UnregisterCallback(callback);
```

## Event Types

The callback system supports all Snaffler message types:

- **Trace**: Detailed debugging information
- **Debug**: Debug information
- **Info**: General information messages
- **FileResult**: When interesting files are found
- **ShareResult**: When network shares are discovered
- **DirResult**: When directories are processed
- **Error**: Error messages
- **Fatal**: Fatal errors
- **Finish**: When Snaffler completes

## Event Data Structure

### SnafflerLogEventArgs Properties

- `Message`: The original `SnafflerMessage` object containing raw data
- `FormattedMessage`: The formatted log message string
- `LogLevel`: Simplified log level enum
- `ProcessedAt`: Timestamp when the event was processed
- `HostString`: Host information (user@hostname)

### Accessing Specific Result Data

#### File Results
```csharp
if (eventArgs.Message.Type == SnafflerMessageType.FileResult)
{
    var fileResult = eventArgs.Message.FileResult;
    Console.WriteLine($"File: {fileResult.FileInfo.FullName}");
    Console.WriteLine($"Rule: {fileResult.MatchedRule?.RuleName}");
    Console.WriteLine($"Size: {fileResult.FileInfo.Length} bytes");
    
    if (fileResult.TextResult != null)
    {
        Console.WriteLine($"Matches: {string.Join(", ", fileResult.TextResult.MatchedStrings)}");
    }
}
```

#### Share Results
```csharp
if (eventArgs.Message.Type == SnafflerMessageType.ShareResult)
{
    var shareResult = eventArgs.Message.ShareResult;
    Console.WriteLine($"Share: {shareResult.SharePath}");
    Console.WriteLine($"Readable: {shareResult.RootReadable}");
    Console.WriteLine($"Writable: {shareResult.RootWritable}");
    Console.WriteLine($"Triage: {shareResult.Triage}");
}
```

#### Directory Results
```csharp
if (eventArgs.Message.Type == SnafflerMessageType.DirResult)
{
    var dirResult = eventArgs.Message.DirResult;
    Console.WriteLine($"Directory: {dirResult.DirPath}");
    Console.WriteLine($"Triage: {dirResult.Triage}");
}
```

## Advanced Examples

### Filtering by Log Level
```csharp
runner.RegisterCallback(eventArgs =>
{
    if (eventArgs.LogLevel == SnafflerLogLevel.Error || 
        eventArgs.LogLevel == SnafflerLogLevel.Fatal)
    {
        // Handle only errors and fatal messages
        LogError(eventArgs.FormattedMessage);
    }
});
```

### Collecting High-Value Files
```csharp
var highValueFiles = new List<FileResult>();

runner.RegisterCallback(eventArgs =>
{
    if (eventArgs.Message.Type == SnafflerMessageType.FileResult &&
        eventArgs.Message.FileResult?.MatchedRule?.Triage == Triage.Red)
    {
        highValueFiles.Add(eventArgs.Message.FileResult);
    }
});
```

### Real-time Database Logging
```csharp
runner.RegisterCallback(eventArgs =>
{
    // Log to database in real-time
    using (var db = new DatabaseContext())
    {
        db.LogEntries.Add(new LogEntry
        {
            Timestamp = eventArgs.ProcessedAt,
            Level = eventArgs.LogLevel.ToString(),
            Message = eventArgs.FormattedMessage,
            MessageType = eventArgs.Message.Type.ToString()
        });
        db.SaveChanges();
    }
});
```

### Custom Security Monitoring
```csharp
public class SecurityMonitor : ISnafflerCallback
{
    private readonly ISecurityAlertService _alertService;
    
    public SecurityMonitor(ISecurityAlertService alertService)
    {
        _alertService = alertService;
    }
    
    public void OnLogEvent(SnafflerLogEventArgs eventArgs)
    {
        switch (eventArgs.Message.Type)
        {
            case SnafflerMessageType.FileResult:
                CheckForSensitiveFiles(eventArgs.Message.FileResult);
                break;
            case SnafflerMessageType.ShareResult:
                CheckForOpenShares(eventArgs.Message.ShareResult);
                break;
        }
    }
    
    private void CheckForSensitiveFiles(FileResult fileResult)
    {
        if (fileResult?.MatchedRule?.Triage == Triage.Red)
        {
            _alertService.SendAlert($"Sensitive file found: {fileResult.FileInfo.FullName}");
        }
    }
    
    private void CheckForOpenShares(ShareResult shareResult)
    {
        if (shareResult?.RootWritable == true)
        {
            _alertService.SendAlert($"Writable share found: {shareResult.SharePath}");
        }
    }
}
```

## Thread Safety

The callback system is thread-safe. Callbacks are executed synchronously in the context of the logging thread, so:

1. Keep callback processing lightweight to avoid blocking the logging system
2. For heavy processing, consider queuing work to a background thread
3. Handle exceptions in your callbacks to prevent breaking the logging system

## Error Handling

Exceptions thrown in callbacks are caught and logged to prevent them from breaking the Snaffler execution. However, it's good practice to handle exceptions within your callbacks:

```csharp
runner.RegisterCallback(eventArgs =>
{
    try
    {
        // Your callback logic here
        ProcessEvent(eventArgs);
    }
    catch (Exception ex)
    {
        // Log or handle the exception
        Console.WriteLine($"Callback error: {ex.Message}");
    }
});
```

## Performance Considerations

- Callbacks are called synchronously, so slow callbacks can impact Snaffler's performance
- For CPU-intensive or I/O operations, consider using async patterns or background threads
- The callback system has minimal overhead when no callbacks are registered
- Use specific event type filtering to avoid unnecessary processing

## Complete Example

See `LibraryUsageExample.cs` for a complete working example demonstrating various callback patterns and use cases.
