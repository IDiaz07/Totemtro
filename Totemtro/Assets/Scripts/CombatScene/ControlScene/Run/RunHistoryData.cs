using System;
using System.Collections.Generic;

[Serializable]
public class RunHistoryEntry
{
    public float timeSurvived;
    public int enemiesKilled;
    public int goldEarned;
    public bool extracted;
    public string date;
}

[Serializable]
public class RunHistoryContainer
{
    public List<RunHistoryEntry> runs =
        new List<RunHistoryEntry>();
}