using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Meridian59EventManager.Models;

namespace Meridian59EventManager.Core
{
    public class AdminSocketConnector : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private bool _connected;

        public event EventHandler<string>? MessageReceived;
        public event EventHandler<string>? ErrorOccurred;

        public bool IsConnected => _connected && _client?.Connected == true;

        public AdminSocketConnector(string host = "127.0.0.1", int port = 9998)
        {
            _host = host;
            _port = port;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_host, _port);
                _stream = _client.GetStream();
                _connected = true;

                OnMessageReceived($"Connected to {_host}:{_port}");
                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Connection failed: {ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
            _connected = false;
            OnMessageReceived("Disconnected");
        }

        public async Task<string> SendCommandAsync(string command)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Not connected to server");
            }

            try
            {
                // Sende Command
                string cmdWithNewline = command.EndsWith("\r\n") ? command : command + "\r\n";
                byte[] data = Encoding.ASCII.GetBytes(cmdWithNewline);
                await _stream!.WriteAsync(data, 0, data.Length);

                OnMessageReceived($"Sent: {command}");

                // Empfange Response - wir müssen mehrfach lesen, da der Server
                // erst das Echo sendet, dann die eigentliche Antwort
                byte[] buffer = new byte[8192];
                StringBuilder fullResponse = new StringBuilder();

                // Lese bis zu 3x oder bis zum Prompt
                for (int i = 0; i < 3; i++)
                {
                    int bytes = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytes > 0)
                    {
                        string chunk = Encoding.ASCII.GetString(buffer, 0, bytes);
                        fullResponse.Append(chunk);

                        // Wenn wir den Prompt sehen, sind wir fertig
                        if (chunk.Contains(">") || chunk.Contains("completed"))
                        {
                            break;
                        }
                    }

                    // Kurze Pause zwischen den Lesevorgängen
                    await Task.Delay(100);
                }

                string response = fullResponse.ToString();
                OnMessageReceived($"Received: {response}");

                return response;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Command failed: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> StartEventAsync(GameEvent evt)
        {
            try
            {
                int classId = evt.GetBlakodClassId();
                string className = evt.GetBlakodClassName();

                if (classId == 0)
                {
                    OnErrorOccurred($"Unknown class ID for event type: {evt.Type}");
                    evt.Status = EventStatus.Failed;
                    evt.LastError = "Unknown class ID";
                    return false;
                }

                // Use System object's StartEvent method with class name
                // Format: Send o 0 startevent event_type class <ClassName>
                // Note: lowercase class name required
                string command = $"Send o 0 startevent event_type class {className.ToLower()}";

                OnMessageReceived($"Starting {className}: {command}");
                string response = await SendCommandAsync(command);

                // Check for success - absence of error indicators means success
                bool success = !response.Contains("Error") &&
                              !response.Contains("Failure") &&
                              !response.Contains("Failed") &&
                              !response.Contains("Invalid");

                if (success)
                {
                    evt.Status = EventStatus.Active;
                    evt.ActualStart = DateTime.Now;

                    // Find the new instance object ID
                    await Task.Delay(200); // Wait for object creation
                    string instancesCmd = $"show instances {className}";
                    string instancesResponse = await SendCommandAsync(instancesCmd);
                    var matches = System.Text.RegularExpressions.Regex.Matches(instancesResponse, @"OBJECT\s+(\d+)");

                    evt.ServerObjectIds.Clear();
                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        int objId = int.Parse(match.Groups[1].Value);
                        evt.ServerObjectIds.Add(objId);
                    }

                    OnMessageReceived($"Event started successfully: {evt.Name} (Objects: {string.Join(", ", evt.ServerObjectIds)})");
                }
                else
                {
                    evt.Status = EventStatus.Failed;
                    evt.LastError = response;
                    OnErrorOccurred($"Event failed to start: {response}");
                }

                return success;
            }
            catch (Exception ex)
            {
                evt.Status = EventStatus.Failed;
                evt.LastError = ex.Message;
                OnErrorOccurred($"Failed to start event: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EndEventAsync(GameEvent evt)
        {
            try
            {
                string className = evt.GetBlakodClassName();
                int stoppedCount = 0;

                // Always query for current instances from server to get the latest object IDs
                List<int> objectIdsToStop = new List<int>();

                string showCommand = $"show instances {className}";
                OnMessageReceived($"Finding current {className} instances: {showCommand}");
                string showResponse = await SendCommandAsync(showCommand);

                var matches = System.Text.RegularExpressions.Regex.Matches(showResponse, @"OBJECT\s+(\d+)");
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    objectIdsToStop.Add(int.Parse(match.Groups[1].Value));
                }

                if (objectIdsToStop.Count == 0)
                {
                    OnMessageReceived($"No active {className} instances found on server");
                    return false;
                }

                OnMessageReceived($"Found {objectIdsToStop.Count} {className} instance(s): {string.Join(", ", objectIdsToStop)}");

                // Stop all object instances using NotifyEngineEndEvent
                foreach (int objectId in objectIdsToStop)
                {
                    OnMessageReceived($"Stopping {className} instance: Object {objectId}");

                    // Use NotifyEngineEndEvent directly on the object
                    string command = $"Send o {objectId} NotifyEngineEndEvent";
                    OnMessageReceived($"Command: {command}");
                    string response = await SendCommandAsync(command);

                    // Check for "Message NotifyEngineEndEvent completed" and "$ 0"
                    if (response.Contains("NotifyEngineEndEvent completed") && response.Contains("$ 0"))
                    {
                        stoppedCount++;
                        OnMessageReceived($"✓ Event instance stopped: Object {objectId}");
                    }
                    else
                    {
                        OnErrorOccurred($"✗ Failed to stop Object {objectId}: {response}");
                    }
                }

                if (stoppedCount > 0)
                {
                    evt.ServerObjectIds.Clear(); // Clear stored IDs after stopping
                    OnMessageReceived($"Event stopped successfully: {evt.Name} ({stoppedCount} instance(s))");
                    return true;
                }
                else
                {
                    OnErrorOccurred($"Failed to stop any {className} instances");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Failed to end event: {ex.Message}");
                return false;
            }
        }

        public async Task<Dictionary<string, List<int>>> CheckAllActiveEventsAsync()
        {
            var activeEvents = new Dictionary<string, List<int>>();

            string[] eventTypes = { "Qormas", "OrcInvasion", "RatInvasion", "SpiderInvasion",
                                   "SkeletonInvasion", "TriggeredInvasion", "ChaosNight", "EasterEggHunt" };

            foreach (string eventType in eventTypes)
            {
                try
                {
                    string command = $"show instances {eventType}";
                    string response = await SendCommandAsync(command);

                    var matches = System.Text.RegularExpressions.Regex.Matches(response, @"OBJECT\s+(\d+)");
                    if (matches.Count > 0)
                    {
                        var objectIds = new List<int>();
                        foreach (System.Text.RegularExpressions.Match match in matches)
                        {
                            objectIds.Add(int.Parse(match.Groups[1].Value));
                        }
                        activeEvents[eventType] = objectIds;
                        OnMessageReceived($"Found {eventType}: {string.Join(", ", objectIds)}");
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccurred($"Error checking {eventType}: {ex.Message}");
                }
            }

            return activeEvents;
        }

        private int? ParseObjectIdFromResponse(string response)
        {
            // Try to parse object ID from response
            // Common patterns: "Object 1234", "#1234", or just the number
            var match = System.Text.RegularExpressions.Regex.Match(response, @"(?:Object\s+)?(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int objectId))
            {
                return objectId;
            }
            return null;
        }

        public async Task<bool> EndEventAsync(int eventObjectId)
        {
            try
            {
                // Direct end using object ID
                string command = $"Send o {eventObjectId} NotifyEngineEndEvent";
                string response = await SendCommandAsync(command);

                bool success = !response.Contains("Error") && !response.Contains("Failure");

                if (success)
                {
                    OnMessageReceived($"Event stopped successfully (Object ID: {eventObjectId})");
                }

                return success;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Failed to end event: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ScheduleEventAsync(GameEvent evt)
        {
            try
            {
                string className = evt.GetBlakodClassName();
                TimeSpan timeUntilStart = evt.ScheduledStart - DateTime.Now;

                if (timeUntilStart.TotalMinutes < 0)
                {
                    // Event ist in der Vergangenheit - starte sofort
                    return await StartEventAsync(evt);
                }

                // Berechne Zeit in Minuten
                int minutes = (int)Math.Ceiling(timeUntilStart.TotalMinutes);

                // SEND OBJECT 3 ScheduleEventInMinutes iClass=&<ClassName> minutes=<N>
                string command = $"SEND OBJECT 3 ScheduleEventInMinutes iClass=&{className} minutes={minutes}";

                string response = await SendCommandAsync(command);
                bool success = !response.Contains("Error");

                if (success)
                {
                    evt.Status = EventStatus.Scheduled;
                }
                else
                {
                    evt.Status = EventStatus.Failed;
                    evt.LastError = response;
                }

                return success;
            }
            catch (Exception ex)
            {
                evt.Status = EventStatus.Failed;
                evt.LastError = ex.Message;
                OnErrorOccurred($"Failed to schedule event: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetServerStatusAsync()
        {
            return await SendCommandAsync("SHOW STATUS");
        }

        public async Task<string> GetActiveTimersAsync()
        {
            return await SendCommandAsync("SHOW TIMERS");
        }

        public async Task<string> GetActiveEventsAsync()
        {
            return await SendCommandAsync("SHOW OBJECT 3");
        }

        public async Task<bool> BroadcastMessageAsync(string message)
        {
            try
            {
                string command = $"SEND USERS {message}";
                string response = await SendCommandAsync(command);
                return !response.Contains("Error");
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Failed to broadcast: {ex.Message}");
                return false;
            }
        }

        protected virtual void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }

        protected virtual void OnErrorOccurred(string error)
        {
            ErrorOccurred?.Invoke(this, error);
        }

        public void Dispose()
        {
            Disconnect();
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}
