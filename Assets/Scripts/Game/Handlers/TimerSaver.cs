using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace RML.Core
{

public class TimerSaveData
{
    [JsonProperty("left_time")]
    public float LeftTime { get; set; }
}


public class TimersSaveData
{
    [JsonProperty("timers")]
    public Dictionary<int, TimerSaveData> TimerSaveData { get; set; }
}

public sealed class TimerSaver : Saver<TimersSaveData>
{
    protected override string DirectoryName => "TimersSaveData";
    protected override string FileName => "TimersSaveData";
}

}

