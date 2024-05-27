using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public static void InvokeCalibration(Vector3 position, Quaternion rotation) => OnCalibrationInvoked?.Invoke(position, rotation);
    public static event Action<Vector3, Quaternion> OnCalibrationInvoked;
}
