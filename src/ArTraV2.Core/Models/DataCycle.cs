namespace ArTraV2.Core.Models;

public enum DataCycleBase
{
    Tick,
    Second,
    Minute,
    Hour,
    Day,
    Week,
    Month,
    Quarter,
    Year
}

public class DataCycle
{
    public DataCycleBase CycleBase { get; set; } = DataCycleBase.Day;
    public int Multiplier { get; set; } = 1;

    public string DisplayName => Multiplier == 1
        ? CycleBase.ToString()
        : $"{Multiplier} {CycleBase}";

    public static DataCycle Daily => new() { CycleBase = DataCycleBase.Day };
    public static DataCycle Weekly => new() { CycleBase = DataCycleBase.Week };
    public static DataCycle Monthly => new() { CycleBase = DataCycleBase.Month };
    public static DataCycle Minute(int minutes) => new() { CycleBase = DataCycleBase.Minute, Multiplier = minutes };
    public static DataCycle Hourly(int hours = 1) => new() { CycleBase = DataCycleBase.Hour, Multiplier = hours };
}
