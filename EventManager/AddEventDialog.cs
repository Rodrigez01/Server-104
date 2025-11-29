using System;
using System.Drawing;
using System.Windows.Forms;
using Meridian59EventManager.Models;

namespace Meridian59EventManager
{
    public class AddEventDialog : Form
    {
        public GameEvent? Event { get; private set; }

        private TextBox txtName;
        private ComboBox cmbEventType;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpStartTime;
        private CheckBox chkHasEnd;
        private DateTimePicker dtpEnd;
        private DateTimePicker dtpEndTime;
        private CheckBox chkRecurring;
        private NumericUpDown numRecurrenceHours;
        private Button btnOK;
        private Button btnCancel;

        public AddEventDialog()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Add New Event";
            this.Size = new Size(450, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 20;

            // Event Name
            var lblName = new Label
            {
                Text = "Event Name:",
                Location = new Point(20, y),
                Size = new Size(100, 20)
            };

            txtName = new TextBox
            {
                Location = new Point(130, y),
                Size = new Size(280, 20)
            };

            y += 35;

            // Event Type
            var lblType = new Label
            {
                Text = "Event Type:",
                Location = new Point(20, y),
                Size = new Size(100, 20)
            };

            cmbEventType = new ComboBox
            {
                Location = new Point(130, y),
                Size = new Size(280, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEventType.Items.AddRange(Enum.GetNames(typeof(EventType)));
            cmbEventType.SelectedIndex = 0;

            y += 35;

            // Start Date/Time
            var lblStart = new Label
            {
                Text = "Start Date/Time:",
                Location = new Point(20, y),
                Size = new Size(100, 20)
            };

            dtpStart = new DateTimePicker
            {
                Location = new Point(130, y),
                Size = new Size(150, 20),
                Format = DateTimePickerFormat.Short
            };

            dtpStartTime = new DateTimePicker
            {
                Location = new Point(290, y),
                Size = new Size(120, 20),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };

            y += 35;

            // Has End Checkbox
            chkHasEnd = new CheckBox
            {
                Text = "Schedule End Time",
                Location = new Point(130, y),
                Size = new Size(150, 20)
            };
            chkHasEnd.CheckedChanged += (s, e) =>
            {
                dtpEnd.Enabled = chkHasEnd.Checked;
                dtpEndTime.Enabled = chkHasEnd.Checked;
            };

            y += 30;

            // End Date/Time
            var lblEnd = new Label
            {
                Text = "End Date/Time:",
                Location = new Point(20, y),
                Size = new Size(100, 20)
            };

            dtpEnd = new DateTimePicker
            {
                Location = new Point(130, y),
                Size = new Size(150, 20),
                Format = DateTimePickerFormat.Short,
                Enabled = false
            };

            dtpEndTime = new DateTimePicker
            {
                Location = new Point(290, y),
                Size = new Size(120, 20),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Enabled = false
            };

            y += 35;

            // Recurring Checkbox
            chkRecurring = new CheckBox
            {
                Text = "Recurring Event",
                Location = new Point(130, y),
                Size = new Size(150, 20)
            };
            chkRecurring.CheckedChanged += (s, e) =>
            {
                numRecurrenceHours.Enabled = chkRecurring.Checked;
            };

            y += 30;

            // Recurrence Interval
            var lblRecurrence = new Label
            {
                Text = "Repeat Every (hours):",
                Location = new Point(20, y),
                Size = new Size(130, 20)
            };

            numRecurrenceHours = new NumericUpDown
            {
                Location = new Point(160, y),
                Size = new Size(80, 20),
                Minimum = 1,
                Maximum = 168,
                Value = 24,
                Enabled = false
            };

            y += 50;

            // Buttons
            btnOK = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(240, y),
                Size = new Size(80, 30)
            };
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(330, y),
                Size = new Size(80, 30)
            };

            this.Controls.AddRange(new Control[]
            {
                lblName, txtName,
                lblType, cmbEventType,
                lblStart, dtpStart, dtpStartTime,
                chkHasEnd,
                lblEnd, dtpEnd, dtpEndTime,
                chkRecurring,
                lblRecurrence, numRecurrenceHours,
                btnOK, btnCancel
            });

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            // Set default values
            txtName.Text = "New Event";
            dtpStart.Value = DateTime.Now.AddHours(1);
            dtpStartTime.Value = DateTime.Now.AddHours(1);
            dtpEnd.Value = DateTime.Now.AddHours(2);
            dtpEndTime.Value = DateTime.Now.AddHours(2);
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter an event name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            DateTime start = dtpStart.Value.Date + dtpStartTime.Value.TimeOfDay;
            DateTime? end = null;

            if (chkHasEnd.Checked)
            {
                end = dtpEnd.Value.Date + dtpEndTime.Value.TimeOfDay;

                if (end <= start)
                {
                    MessageBox.Show("End time must be after start time.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }
            }

            Event = new GameEvent
            {
                Name = txtName.Text,
                Type = (EventType)Enum.Parse(typeof(EventType), cmbEventType.SelectedItem?.ToString() ?? "OrcInvasion"),
                ScheduledStart = start,
                ScheduledEnd = end,
                IsRecurring = chkRecurring.Checked,
                RecurrenceInterval = chkRecurring.Checked ? TimeSpan.FromHours((double)numRecurrenceHours.Value) : null
            };
        }
    }
}
