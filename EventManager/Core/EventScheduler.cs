using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meridian59EventManager.Models;

namespace Meridian59EventManager.Core
{
    public class EventScheduler : IDisposable
    {
        private readonly AdminSocketConnector _connector;
        private readonly List<GameEvent> _events;
        private readonly System.Threading.Timer _checkTimer;
        private readonly object _lock = new();
        private bool _isRunning;

        public event EventHandler<GameEvent>? EventStarted;
        public event EventHandler<GameEvent>? EventEnded;
        public event EventHandler<GameEvent>? EventFailed;
        public event EventHandler<string>? LogMessage;

        public IReadOnlyList<GameEvent> Events
        {
            get
            {
                lock (_lock)
                {
                    return _events.ToList();
                }
            }
        }

        public EventScheduler(AdminSocketConnector connector)
        {
            _connector = connector;
            _events = new List<GameEvent>();
            _checkTimer = new System.Threading.Timer(CheckScheduledEvents, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                // Check every minute
                _checkTimer.Change(TimeSpan.Zero, TimeSpan.FromMinutes(1));
                Log("Scheduler started - checking every minute");
            }
        }

        public void Stop()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _checkTimer.Change(Timeout.Infinite, Timeout.Infinite);
                Log("Scheduler stopped");
            }
        }

        public void AddEvent(GameEvent evt)
        {
            lock (_lock)
            {
                _events.Add(evt);
                Log($"Event added: {evt}");
            }
        }

        public void RemoveEvent(GameEvent evt)
        {
            lock (_lock)
            {
                _events.Remove(evt);
                Log($"Event removed: {evt}");
            }
        }

        public void ClearCompleted()
        {
            lock (_lock)
            {
                var completed = _events.Where(e =>
                    e.Status == EventStatus.Completed ||
                    e.Status == EventStatus.Cancelled).ToList();

                foreach (var evt in completed)
                {
                    _events.Remove(evt);
                }

                if (completed.Count > 0)
                {
                    Log($"Removed {completed.Count} completed events");
                }
            }
        }

        private void CheckScheduledEvents(object? state)
        {
            // Run async work in a separate task to avoid blocking the timer
            _ = CheckScheduledEventsAsync();
        }

        private async Task CheckScheduledEventsAsync()
        {
            if (!_connector.IsConnected)
            {
                return;
            }

            // Get current time without seconds for comparison
            DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                       DateTime.Now.Hour, DateTime.Now.Minute, 0);

            List<GameEvent> dueEvents;
            lock (_lock)
            {
                dueEvents = _events
                    .Where(e => e.Status == EventStatus.Scheduled &&
                               TruncateToMinute(e.ScheduledStart) <= now)
                    .ToList();
            }

            foreach (var evt in dueEvents)
            {
                try
                {
                    evt.Status = EventStatus.Active;
                    Log($"Starting event: {evt.Name}");

                    bool success = await _connector.StartEventAsync(evt);

                    if (success)
                    {
                        Log($"Event started successfully: {evt.Name}");
                        EventStarted?.Invoke(this, evt);

                        // Event will be automatically ended by the main scheduler loop
                        // when ScheduledEnd time is reached
                    }
                    else
                    {
                        evt.Status = EventStatus.Failed;
                        Log($"Event failed to start: {evt.Name} - {evt.LastError}");
                        EventFailed?.Invoke(this, evt);
                    }
                }
                catch (Exception ex)
                {
                    evt.Status = EventStatus.Failed;
                    evt.LastError = ex.Message;
                    Log($"Error starting event: {evt.Name} - {ex.Message}");
                    EventFailed?.Invoke(this, evt);
                }
            }

            // Check for events that should end
            List<GameEvent> endingEvents;
            lock (_lock)
            {
                endingEvents = _events
                    .Where(e => e.Status == EventStatus.Active &&
                               e.ScheduledEnd.HasValue &&
                               TruncateToMinute(e.ScheduledEnd.Value) <= now)
                    .ToList();
            }

            foreach (var evt in endingEvents)
            {
                await EndEventAsync(evt);
            }
        }

        private void ScheduleRecurringEvent(GameEvent original)
        {
            if (!original.RecurrenceInterval.HasValue)
                return;

            var nextEvent = new GameEvent
            {
                Name = original.Name,
                Type = original.Type,
                BlakodClass = original.BlakodClass,
                ScheduledStart = original.ScheduledStart + original.RecurrenceInterval.Value,
                ScheduledEnd = original.ScheduledEnd.HasValue
                    ? original.ScheduledEnd.Value + original.RecurrenceInterval.Value
                    : null,
                Status = EventStatus.Scheduled,
                Parameters = new Dictionary<string, object>(original.Parameters),
                IsRecurring = true,
                RecurrenceInterval = original.RecurrenceInterval
            };

            AddEvent(nextEvent);
            Log($"Scheduled recurring event: {nextEvent}");
        }

        private async Task EndEventAsync(GameEvent evt)
        {
            try
            {
                // Try to end using event object if we have the ID
                bool success = false;

                if (evt.ServerEventId.HasValue)
                {
                    success = await _connector.EndEventAsync(evt.ServerEventId.Value);
                }
                else
                {
                    // Otherwise use the event class to find and end it
                    success = await _connector.EndEventAsync(evt);
                }

                if (success)
                {
                    evt.Status = EventStatus.Completed;
                    evt.ActualEnd = DateTime.Now;
                    Log($"Event ended: {evt.Name}");
                    EventEnded?.Invoke(this, evt);

                    // Schedule next occurrence if this is a recurring event
                    if (evt.IsRecurring && evt.RecurrenceInterval.HasValue)
                    {
                        ScheduleRecurringEvent(evt);
                    }
                }
                else
                {
                    Log($"Failed to end event: {evt.Name}");
                }
            }
            catch (Exception ex)
            {
                Log($"Error ending event: {evt.Name} - {ex.Message}");
            }
        }

        public async Task<bool> StartEventNowAsync(GameEvent evt)
        {
            try
            {
                evt.Status = EventStatus.Active;
                bool success = await _connector.StartEventAsync(evt);

                if (success)
                {
                    EventStarted?.Invoke(this, evt);
                }
                else
                {
                    EventFailed?.Invoke(this, evt);
                }

                return success;
            }
            catch (Exception ex)
            {
                evt.Status = EventStatus.Failed;
                evt.LastError = ex.Message;
                EventFailed?.Invoke(this, evt);
                return false;
            }
        }

        public async Task<bool> CancelEventAsync(GameEvent evt)
        {
            if (evt.Status == EventStatus.Active && evt.ServerEventId.HasValue)
            {
                await _connector.EndEventAsync(evt.ServerEventId.Value);
            }

            evt.Status = EventStatus.Cancelled;
            Log($"Event cancelled: {evt.Name}");
            return true;
        }

        private void Log(string message)
        {
            LogMessage?.Invoke(this, $"[{DateTime.Now:HH:mm}] {message}");
        }

        private DateTime TruncateToMinute(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day,
                              dateTime.Hour, dateTime.Minute, 0);
        }

        public void Dispose()
        {
            Stop();
            _checkTimer.Dispose();
        }
    }
}
