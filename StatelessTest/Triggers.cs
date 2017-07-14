namespace StatelessTest
{

    /// <summary>
    /// An enum containing the possible events that trigger a potential change in occupancy state
    /// </summary>
    public enum Trigger
    {
        DoorOpens,
        DoorCloses,
        SensorActivity,
        AlarmFullSet,
        AlarmPartSet,
        AlarmUnset,
        OccupancyTimerExpires,
        ChildOccupied
    }
}
