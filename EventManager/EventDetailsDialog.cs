using System;
using System.Drawing;
using System.Windows.Forms;
using Meridian59EventManager.Models;

namespace Meridian59EventManager
{
    public class EventDetailsDialog : Form
    {
        private readonly GameEvent _event;

        public EventDetailsDialog(GameEvent evt)
        {
            _event = evt;
            InitializeComponents();
            LoadEventDetails();
        }

        private void InitializeComponents()
        {
            this.Text = "Event Details";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var txtDetails = new RichTextBox
            {
                Location = new Point(10, 10),
                Size = new Size(460, 350),
                ReadOnly = true,
                Font = new Font("Consolas", 9)
            };

            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(390, 370),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };

            this.Controls.AddRange(new Control[] { txtDetails, btnClose });
            this.AcceptButton = btnClose;

            // Store reference for loading
            this.Tag = txtDetails;
        }

        private void LoadEventDetails()
        {
            var txt = (RichTextBox)this.Tag!;

            txt.Clear();
            txt.SelectionFont = new Font("Consolas", 10, FontStyle.Bold);
            txt.AppendText($"{_event.Name}\n");
            txt.AppendText("".PadRight(50, '=') + "\n\n");

            txt.SelectionFont = new Font("Consolas", 9, FontStyle.Regular);

            AppendDetail(txt, "Event Type", _event.Type.ToString());
            AppendDetail(txt, "Blakod Class", _event.GetBlakodClassName());
            AppendDetail(txt, "Status", _event.Status.ToString());
            AppendDetail(txt, "Scheduled Start", _event.ScheduledStart.ToString("g"));

            if (_event.ScheduledEnd.HasValue)
            {
                AppendDetail(txt, "Scheduled End", _event.ScheduledEnd.Value.ToString("g"));
            }

            if (_event.ActualStart.HasValue)
            {
                AppendDetail(txt, "Actual Start", _event.ActualStart.Value.ToString("g"));
            }

            if (_event.ActualEnd.HasValue)
            {
                AppendDetail(txt, "Actual End", _event.ActualEnd.Value.ToString("g"));
            }

            if (_event.ServerEventId.HasValue)
            {
                AppendDetail(txt, "Server Event ID", _event.ServerEventId.Value.ToString());
            }

            AppendDetail(txt, "Recurring", _event.IsRecurring ? "Yes" : "No");

            if (_event.IsRecurring && _event.RecurrenceInterval.HasValue)
            {
                AppendDetail(txt, "Recurrence Interval", FormatTimeSpan(_event.RecurrenceInterval.Value));
            }

            if (!string.IsNullOrEmpty(_event.LastError))
            {
                txt.AppendText("\n");
                txt.SelectionColor = Color.Red;
                txt.SelectionFont = new Font("Consolas", 9, FontStyle.Bold);
                txt.AppendText("Last Error:\n");
                txt.SelectionFont = new Font("Consolas", 9, FontStyle.Regular);
                txt.AppendText(_event.LastError + "\n");
                txt.SelectionColor = Color.Black;
            }

            if (_event.Parameters.Count > 0)
            {
                txt.AppendText("\n");
                txt.SelectionFont = new Font("Consolas", 9, FontStyle.Bold);
                txt.AppendText("Parameters:\n");
                txt.SelectionFont = new Font("Consolas", 9, FontStyle.Regular);

                foreach (var param in _event.Parameters)
                {
                    txt.AppendText($"  {param.Key} = {param.Value}\n");
                }
            }
        }

        private void AppendDetail(RichTextBox txt, string label, string value)
        {
            txt.SelectionFont = new Font("Consolas", 9, FontStyle.Bold);
            txt.AppendText($"{label.PadRight(20)}: ");
            txt.SelectionFont = new Font("Consolas", 9, FontStyle.Regular);
            txt.AppendText($"{value}\n");
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalDays >= 1)
                return $"{ts.TotalDays:F1} days";
            if (ts.TotalHours >= 1)
                return $"{ts.TotalHours:F1} hours";
            if (ts.TotalMinutes >= 1)
                return $"{ts.TotalMinutes:F1} minutes";
            return $"{ts.TotalSeconds:F0} seconds";
        }
    }
}
