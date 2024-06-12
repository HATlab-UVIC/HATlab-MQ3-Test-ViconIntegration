using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public static void InvokeCalibration() => OnCalibrationInvoked?.Invoke();
    public static event Action OnCalibrationInvoked;
}
