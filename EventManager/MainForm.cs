using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Meridian59EventManager.Core;
using Meridian59EventManager.Models;

namespace Meridian59EventManager
{
    public partial class MainForm : Form
    {
        private AdminSocketConnector? _connector;
        private EventScheduler? _scheduler;

        private TextBox txtHost;
        private NumericUpDown numPort;
        private Button btnConnect;
        private Button btnDisconnect;
        private ListBox lstEvents;
        private Button btnAddEvent;
        private Button btnStartNow;
        private Button btnCancel;
        private Button btnRemove;
        private Button btnClearCompleted;
        private RichTextBox txtLog;
        private Label lblStatus;
        private Button btnRefreshStatus;
        private Button btnCheckActiveEvents;
        private GroupBox grpConnection;
        private GroupBox grpEvents;
        private GroupBox grpLog;
        private Button btnScheduleEvent;
        private DataGridView dgvActiveEvents;
        private Button btnStopActive;
        private Button btnRefreshActiveEvents;
        private Button btnTrackInstances;
        private GroupBox grpActiveEvents;

        public MainForm()
        {
            InitializeComponents();
            UpdateConnectionStatus(false);
        }

        private void InitializeComponents()
        {
            this.Text = "Meridian 59 Event Manager";
            this.Size = new Size(900, 850);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Connection Group
            grpConnection = new GroupBox
            {
                Text = "Server Connection",
                Location = new Point(10, 10),
                Size = new Size(860, 80)
            };

            var lblHost = new Label
            {
                Text = "Host:",
                Location = new Point(10, 25),
                Size = new Size(50, 20)
            };

            txtHost = new TextBox
            {
                Text = "127.0.0.1",
                Location = new Point(70, 23),
                Size = new Size(100, 20)
            };

            var lblPort = new Label
            {
                Text = "Port:",
                Location = new Point(180, 25),
                Size = new Size(40, 20)
            };

            numPort = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 65535,
                Value = 9998,
                Location = new Point(230, 23),
                Size = new Size(70, 20)
            };

            btnConnect = new Button
            {
                Text = "Connect",
                Location = new Point(310, 20),
                Size = new Size(80, 25)
            };
            btnConnect.Click += BtnConnect_Click;

            btnDisconnect = new Button
            {
                Text = "Disconnect",
                Location = new Point(400, 20),
                Size = new Size(90, 25),
                Enabled = false
            };
            btnDisconnect.Click += BtnDisconnect_Click;

            lblStatus = new Label
            {
                Text = "Status: Not Connected",
                Location = new Point(500, 25),
                Size = new Size(200, 20),
                ForeColor = Color.Red
            };

            btnRefreshStatus = new Button
            {
                Text = "Refresh Status",
                Location = new Point(610, 20),
                Size = new Size(100, 25),
                Enabled = false
            };
            btnRefreshStatus.Click += BtnRefreshStatus_Click;

            btnCheckActiveEvents = new Button
            {
                Text = "Refresh Active Events",
                Location = new Point(720, 20),
                Size = new Size(130, 25),
                Enabled = false
            };
            btnCheckActiveEvents.Click += BtnCheckActiveEvents_Click;

            grpConnection.Controls.AddRange(new Control[]
            {
                lblHost, txtHost, lblPort, numPort, btnConnect, btnDisconnect, lblStatus, btnRefreshStatus, btnCheckActiveEvents
            });

            // Events Group
            grpEvents = new GroupBox
            {
                Text = "Scheduled Events",
                Location = new Point(10, 100),
                Size = new Size(860, 300)
            };

            lstEvents = new ListBox
            {
                Location = new Point(10, 25),
                Size = new Size(700, 260)
            };
            lstEvents.DoubleClick += LstEvents_DoubleClick;

            btnAddEvent = new Button
            {
                Text = "Add Event",
                Location = new Point(720, 25),
                Size = new Size(120, 30)
            };
            btnAddEvent.Click += BtnAddEvent_Click;

            btnScheduleEvent = new Button
            {
                Text = "Schedule Event",
                Location = new Point(720, 65),
                Size = new Size(120, 30)
            };
            btnScheduleEvent.Click += BtnScheduleEvent_Click;

            btnStartNow = new Button
            {
                Text = "Start Now",
                Location = new Point(720, 105),
                Size = new Size(120, 30),
                Enabled = false
            };
            btnStartNow.Click += BtnStartNow_Click;

            btnCancel = new Button
            {
                Text = "Stop/Cancel Event",
                Location = new Point(720, 145),
                Size = new Size(120, 30),
                Enabled = false
            };
            btnCancel.Click += BtnCancel_Click;

            btnRemove = new Button
            {
                Text = "Remove",
                Location = new Point(720, 185),
                Size = new Size(120, 30)
            };
            btnRemove.Click += BtnRemove_Click;

            btnClearCompleted = new Button
            {
                Text = "Clear Completed",
                Location = new Point(720, 225),
                Size = new Size(120, 30)
            };
            btnClearCompleted.Click += BtnClearCompleted_Click;

            grpEvents.Controls.AddRange(new Control[]
            {
                lstEvents, btnAddEvent, btnScheduleEvent, btnStartNow, btnCancel, btnRemove, btnClearCompleted
            });

            // Active Events Group
            grpActiveEvents = new GroupBox
            {
                Text = "Active Events on Server",
                Location = new Point(10, 410),
                Size = new Size(860, 140)
            };

            dgvActiveEvents = new DataGridView
            {
                Location = new Point(10, 25),
                Size = new Size(700, 100),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true
            };
            dgvActiveEvents.Columns.Add("EventType", "Event Type");
            dgvActiveEvents.Columns.Add("ObjectId", "Object ID");
            dgvActiveEvents.Columns["EventType"].Width = 200;
            dgvActiveEvents.Columns["ObjectId"].Width = 100;

            btnRefreshActiveEvents = new Button
            {
                Text = "Refresh Active Events",
                Location = new Point(720, 25),
                Size = new Size(130, 30),
                Enabled = false
            };
            btnRefreshActiveEvents.Click += BtnCheckActiveEvents_Click;

            btnTrackInstances = new Button
            {
                Text = "Track Instances",
                Location = new Point(720, 60),
                Size = new Size(130, 30),
                Enabled = false
            };
            btnTrackInstances.Click += BtnTrackInstances_Click;

            btnStopActive = new Button
            {
                Text = "Stop Selected",
                Location = new Point(720, 95),
                Size = new Size(130, 30),
                Enabled = false
            };
            btnStopActive.Click += BtnStopActive_Click;

            grpActiveEvents.Controls.AddRange(new Control[]
            {
                dgvActiveEvents, btnRefreshActiveEvents, btnTrackInstances, btnStopActive
            });

            // Log Group
            grpLog = new GroupBox
            {
                Text = "Activity Log",
                Location = new Point(10, 560),
                Size = new Size(860, 240)
            };

            txtLog = new RichTextBox
            {
                Location = new Point(10, 25),
                Size = new Size(830, 200),
                ReadOnly = true,
                Font = new Font("Consolas", 9)
            };

            grpLog.Controls.Add(txtLog);

            // Add all to form
            this.Controls.AddRange(new Control[]
            {
                grpConnection, grpEvents, grpActiveEvents, grpLog
            });
        }

        private async void BtnConnect_Click(object? sender, EventArgs e)
        {
            try
            {
                btnConnect.Enabled = false;

                _connector = new AdminSocketConnector(txtHost.Text, (int)numPort.Value);
                _connector.MessageReceived += Connector_MessageReceived;
                _connector.ErrorOccurred += Connector_ErrorOccurred;

                bool connected = await _connector.ConnectAsync();

                if (connected)
                {
                    _scheduler = new EventScheduler(_connector);
                    _scheduler.EventStarted += Scheduler_EventStarted;
                    _scheduler.EventEnded += Scheduler_EventEnded;
                    _scheduler.EventFailed += Scheduler_EventFailed;
                    _scheduler.LogMessage += Scheduler_LogMessage;
                    _scheduler.Start();

                    UpdateConnectionStatus(true);
                    Log("Successfully connected to server");
                }
                else
                {
                    UpdateConnectionStatus(false);
                    Log("Failed to connect to server");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateConnectionStatus(false);
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }

        private void BtnDisconnect_Click(object? sender, EventArgs e)
        {
            _scheduler?.Stop();
            _scheduler?.Dispose();
            _connector?.Disconnect();
            _connector?.Dispose();

            _scheduler = null;
            _connector = null;

            UpdateConnectionStatus(false);
            Log("Disconnected from server");
        }

        private async void BtnRefreshStatus_Click(object? sender, EventArgs e)
        {
            if (_connector == null) return;

            try
            {
                string status = await _connector.GetServerStatusAsync();
                Log("Server Status:\n" + status);
            }
            catch (Exception ex)
            {
                Log($"Failed to get status: {ex.Message}");
            }
        }

        private async void BtnCheckActiveEvents_Click(object? sender, EventArgs e)
        {
            if (_connector == null) return;

            Log("=== Checking Active Events ===");
            try
            {
                dgvActiveEvents.Rows.Clear();
                var activeEvents = await _connector.CheckAllActiveEventsAsync();

                if (activeEvents.Count == 0)
                {
                    Log("No active events found.");
                }
                else
                {
                    Log($"Found {activeEvents.Count} active event type(s):");
                    foreach (var kvp in activeEvents)
                    {
                        Log($"  {kvp.Key}: {kvp.Value.Count} instance(s) - Objects: {string.Join(", ", kvp.Value)}");

                        foreach (int objectId in kvp.Value)
                        {
                            dgvActiveEvents.Rows.Add(kvp.Key, objectId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"ERROR checking active events: {ex.Message}");
            }
        }

        private async void BtnTrackInstances_Click(object? sender, EventArgs e)
        {
            if (_connector == null || _scheduler == null) return;

            Log("=== Tracking Active Event Instances ===");

            try
            {
                var activeEvents = await _connector.CheckAllActiveEventsAsync();

                if (activeEvents.Count == 0)
                {
                    Log("No active events found to track.");
                    return;
                }

                int trackedCount = 0;

                foreach (var kvp in activeEvents)
                {
                    string eventType = kvp.Key;
                    List<int> objectIds = kvp.Value;

                    // Find matching scheduled events
                    foreach (var evt in _scheduler.Events)
                    {
                        if (evt.GetBlakodClassName() == eventType && evt.Status == EventStatus.Active)
                        {
                            evt.ServerObjectIds.Clear();
                            evt.ServerObjectIds.AddRange(objectIds);
                            trackedCount++;
                            Log($"✓ Tracked {objectIds.Count} instance(s) for {evt.Name}: {string.Join(", ", objectIds)}");
                            break;
                        }
                    }
                }

                if (trackedCount > 0)
                {
                    RefreshEventList();
                    Log($"=== Tracking Complete: {trackedCount} event(s) updated ===");
                }
                else
                {
                    Log("No matching events found in scheduler.");
                }
            }
            catch (Exception ex)
            {
                Log($"ERROR tracking instances: {ex.Message}");
            }
        }

        private async void BtnStopActive_Click(object? sender, EventArgs e)
        {
            if (_connector == null) return;
            if (dgvActiveEvents.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select one or more events to stop.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int totalSelected = dgvActiveEvents.SelectedRows.Count;
            if (MessageBox.Show($"Stop {totalSelected} selected event instance(s)?", "Confirm Stop",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            Log($"=== Stopping {totalSelected} event instance(s) ===");

            int successCount = 0;
            int failCount = 0;

            foreach (DataGridViewRow row in dgvActiveEvents.SelectedRows)
            {
                string eventType = row.Cells["EventType"].Value?.ToString() ?? "";
                int objectId = int.Parse(row.Cells["ObjectId"].Value?.ToString() ?? "0");

                try
                {
                    Log($"Stopping {eventType} (Object {objectId})...");

                    string command = $"Send o {objectId} NotifyEngineEndEvent";
                    string response = await _connector.SendCommandAsync(command);

                    // Check for success: "Message NotifyEngineEndEvent completed" and "$ 0"
                    if (response.Contains("NotifyEngineEndEvent completed") && response.Contains("$ 0"))
                    {
                        successCount++;
                        Log($"✓ Successfully stopped {eventType} (Object {objectId})");
                    }
                    else
                    {
                        failCount++;
                        Log($"✗ Failed to stop {eventType} (Object {objectId}): {response}");
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    Log($"✗ Error stopping {eventType} (Object {objectId}): {ex.Message}");
                }
            }

            Log($"=== Stop Complete: {successCount} succeeded, {failCount} failed ===");

            // Refresh the active events list
            await Task.Delay(500);
            BtnCheckActiveEvents_Click(sender, e);
        }

        private void BtnAddEvent_Click(object? sender, EventArgs e)
        {
            using var dialog = new AddEventDialog();
            if (dialog.ShowDialog() == DialogResult.OK && dialog.Event != null)
            {
                _scheduler?.AddEvent(dialog.Event);
                RefreshEventList();
                Log($"Added event: {dialog.Event}");
            }
        }

        private async void BtnScheduleEvent_Click(object? sender, EventArgs e)
        {
            if (lstEvents.SelectedItem is not EventListItem item) return;
            var evt = item.Event;
            if (_connector == null) return;

            try
            {
                bool success = await _connector.ScheduleEventAsync(evt);
                if (success)
                {
                    Log($"Event scheduled on server: {evt.Name}");
                }
                else
                {
                    Log($"Failed to schedule event: {evt.LastError}");
                }
            }
            catch (Exception ex)
            {
                Log($"Error scheduling event: {ex.Message}");
            }
        }

        private async void BtnStartNow_Click(object? sender, EventArgs e)
        {
            if (lstEvents.SelectedItem is not EventListItem item) return;
            var evt = item.Event;

            if (MessageBox.Show($"Start event '{evt.Name}' immediately?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                bool success = await _scheduler!.StartEventNowAsync(evt);
                if (success)
                {
                    RefreshEventList();
                }
            }
        }

        private async void BtnCancel_Click(object? sender, EventArgs e)
        {
            if (lstEvents.SelectedItem is not EventListItem item) return;
            var evt = item.Event;

            string action = evt.Status == EventStatus.Active ? "Stop" : "Cancel";
            if (MessageBox.Show($"{action} event '{evt.Name}'?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                await _scheduler!.CancelEventAsync(evt);
                RefreshEventList();
                Log($"Event {action.ToLower()}ed: {evt.Name}");
            }
        }

        private void BtnRemove_Click(object? sender, EventArgs e)
        {
            if (lstEvents.SelectedItem is not EventListItem item) return;
            var evt = item.Event;

            _scheduler?.RemoveEvent(evt);
            RefreshEventList();
            Log($"Removed event: {evt}");
        }

        private void BtnClearCompleted_Click(object? sender, EventArgs e)
        {
            _scheduler?.ClearCompleted();
            RefreshEventList();
        }

        private void LstEvents_DoubleClick(object? sender, EventArgs e)
        {
            if (lstEvents.SelectedItem is not EventListItem item) return;

            using var dialog = new EventDetailsDialog(item.Event);
            dialog.ShowDialog();
        }

        private void UpdateConnectionStatus(bool connected)
        {
            lblStatus.Text = connected ? "Status: Connected" : "Status: Not Connected";
            lblStatus.ForeColor = connected ? Color.Green : Color.Red;

            btnConnect.Enabled = !connected;
            btnDisconnect.Enabled = connected;
            btnRefreshStatus.Enabled = connected;
            btnCheckActiveEvents.Enabled = connected;
            btnRefreshActiveEvents.Enabled = connected;
            btnTrackInstances.Enabled = connected;
            btnStopActive.Enabled = connected;
            btnAddEvent.Enabled = connected;
            btnScheduleEvent.Enabled = connected;
            btnStartNow.Enabled = connected;
            btnCancel.Enabled = connected;
        }

        private void RefreshEventList()
        {
            lstEvents.Items.Clear();
            if (_scheduler != null)
            {
                foreach (var evt in _scheduler.Events.OrderBy(e => e.ScheduledStart))
                {
                    string display = evt.ToString();
                    if (evt.ServerObjectIds.Count > 0)
                    {
                        display += $" [Instances: {string.Join(", ", evt.ServerObjectIds)}]";
                    }
                    lstEvents.Items.Add(new EventListItem(evt, display));
                }
            }
        }

        private class EventListItem
        {
            public GameEvent Event { get; }
            public string Display { get; }

            public EventListItem(GameEvent evt, string display)
            {
                Event = evt;
                Display = display;
            }

            public override string ToString() => Display;
        }

        private void Connector_MessageReceived(object? sender, string message)
        {
            this.Invoke(() => Log(message));
        }

        private void Connector_ErrorOccurred(object? sender, string error)
        {
            this.Invoke(() => Log($"ERROR: {error}"));
        }

        private void Scheduler_EventStarted(object? sender, GameEvent evt)
        {
            this.Invoke(() =>
            {
                RefreshEventList();
                Log($"Event STARTED: {evt.Name}");
            });
        }

        private void Scheduler_EventEnded(object? sender, GameEvent evt)
        {
            this.Invoke(() =>
            {
                RefreshEventList();
                Log($"Event ENDED: {evt.Name}");
            });
        }

        private void Scheduler_EventFailed(object? sender, GameEvent evt)
        {
            this.Invoke(() =>
            {
                RefreshEventList();
                Log($"Event FAILED: {evt.Name} - {evt.LastError}");
            });
        }

        private void Scheduler_LogMessage(object? sender, string message)
        {
            this.Invoke(() => Log(message));
        }

        private void Log(string message)
        {
            txtLog.AppendText(message + Environment.NewLine);
            txtLog.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _scheduler?.Stop();
            _scheduler?.Dispose();
            _connector?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
