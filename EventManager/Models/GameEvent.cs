using System;
using System.Collections.Generic;

namespace Meridian59EventManager.Models
{
    public enum EventStatus
    {
        Scheduled,
        Active,
        Completed,
        Cancelled,
        Failed
    }

    public enum EventType
    {
        OrcInvasion,
        RatInvasion,
        SpiderInvasion,
        SkeletonInvasion,
        TriggeredInvasion,
        ChaosNight,
        WarEvent,
        NodeAttack,
        EasterEggHunt,
        Qormas,
        Custom
    }

    public class GameEvent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public EventType Type { get; set; }
        public string BlakodClass { get; set; } = string.Empty;
        public DateTime ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
        public EventStatus Status { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public int? ServerEventId { get; set; }
        public List<int> ServerObjectIds { get; set; } = new(); // Store all object instances
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }
        public string? LastError { get; set; }
        public bool IsRecurring { get; set; }
        public TimeSpan? RecurrenceInterval { get; set; }

        public GameEvent()
        {
            Status = EventStatus.Scheduled;
            ScheduledStart = DateTime.Now;
        }

        public string GetBlakodClassName()
        {
            return Type switch
            {
                EventType.OrcInvasion => "OrcInvasion",
                EventType.RatInvasion => "RatInvasion",
                EventType.SpiderInvasion => "SpiderInvasion",
                EventType.SkeletonInvasion => "SkeletonInvasion",
                EventType.TriggeredInvasion => "TriggeredInvasion",
                EventType.ChaosNight => "ChaosNight",
                EventType.WarEvent => "WarEvent",
                EventType.NodeAttack => "NodeAttack",
                EventType.EasterEggHunt => "EasterEggHunt",
                EventType.Qormas => "Qormas",
                EventType.Custom => BlakodClass,
                _ => "OrcInvasion"
            };
        }

        public int GetBlakodClassId()
        {
            return Type switch
            {
                EventType.OrcInvasion => 13155,
                EventType.RatInvasion => 13154,
                EventType.SpiderInvasion => 13156,
                EventType.SkeletonInvasion => 13157,
                EventType.TriggeredInvasion => 13183,
                EventType.ChaosNight => 11494,
                EventType.EasterEggHunt => 13180,
                EventType.Qormas => 13182,
                EventType.WarEvent => 0, // Unknown - needs to be found
                EventType.NodeAttack => 0, // Unknown - needs to be found
                EventType.Custom => 0,
                _ => 13154 // Default to RatInvasion
            };
        }

        public override string ToString()
        {
            return $"{Name} ({Type}) - {ScheduledStart:g} [{Status}]";
        }
    }
}
