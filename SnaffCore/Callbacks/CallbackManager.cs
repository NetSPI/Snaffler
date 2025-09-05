using System;
using System.Collections.Generic;

namespace SnaffCore.Callbacks
{
    /// <summary>
    /// Manages multiple callbacks for Snaffler logging events
    /// </summary>
    public class CallbackManager
    {
        private readonly List<ISnafflerCallback> _callbacks = new List<ISnafflerCallback>();
        private readonly object _lock = new object();

        /// <summary>
        /// Register a callback to receive logging events
        /// </summary>
        /// <param name="callback">The callback to register</param>
        public void RegisterCallback(ISnafflerCallback callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock (_lock)
            {
                if (!_callbacks.Contains(callback))
                {
                    _callbacks.Add(callback);
                }
            }
        }

        /// <summary>
        /// Unregister a callback
        /// </summary>
        /// <param name="callback">The callback to unregister</param>
        public void UnregisterCallback(ISnafflerCallback callback)
        {
            if (callback == null)
                return;

            lock (_lock)
            {
                _callbacks.Remove(callback);
            }
        }

        /// <summary>
        /// Register a simple action-based callback
        /// </summary>
        /// <param name="callback">Action to call on log events</param>
        /// <returns>A wrapper that can be used to unregister the callback</returns>
        public ISnafflerCallback RegisterCallback(Action<SnafflerLogEventArgs> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            var wrapper = new ActionCallbackWrapper(callback);
            RegisterCallback(wrapper);
            return wrapper;
        }

        /// <summary>
        /// Trigger all registered callbacks with the given event args
        /// </summary>
        /// <param name="eventArgs">The event arguments</param>
        public void TriggerCallbacks(SnafflerLogEventArgs eventArgs)
        {
            if (eventArgs == null)
                return;

            List<ISnafflerCallback> currentCallbacks;
            lock (_lock)
            {
                currentCallbacks = new List<ISnafflerCallback>(_callbacks);
            }

            foreach (var callback in currentCallbacks)
            {
                try
                {
                    callback.OnLogEvent(eventArgs);
                }
                catch (Exception ex)
                {
                    // Don't let callback exceptions break the logging system
                    // In a real implementation you might want to log this somewhere
                    Console.WriteLine($"Callback error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get the number of registered callbacks
        /// </summary>
        public int CallbackCount
        {
            get
            {
                lock (_lock)
                {
                    return _callbacks.Count;
                }
            }
        }

        /// <summary>
        /// Clear all registered callbacks
        /// </summary>
        public void ClearCallbacks()
        {
            lock (_lock)
            {
                _callbacks.Clear();
            }
        }
    }

    /// <summary>
    /// Wrapper to allow Action-based callbacks
    /// </summary>
    internal class ActionCallbackWrapper : ISnafflerCallback
    {
        private readonly Action<SnafflerLogEventArgs> _action;

        public ActionCallbackWrapper(Action<SnafflerLogEventArgs> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void OnLogEvent(SnafflerLogEventArgs eventArgs)
        {
            _action(eventArgs);
        }
    }
}
