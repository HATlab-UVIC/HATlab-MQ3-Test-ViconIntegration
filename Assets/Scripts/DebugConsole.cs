using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI DebugText;

    static event Action<object, LogType> OnDebugMessage;
    public static void Log(object message) => OnDebugMessage.Invoke(message, LogType.Log);
    public static void Error(object message) => OnDebugMessage.Invoke(message, LogType.Error);
    public static void Warn(object message) => OnDebugMessage.Invoke(message, LogType.Warning);
    public static void Success(object message) => OnDebugMessage.Invoke(message, LogType.Assert);

    void OnEnable() => OnDebugMessage += HandleDebugMessage;
    void OnDisable() => OnDebugMessage -= HandleDebugMessage;

    private void HandleDebugMessage(object msg, LogType logType)
    {
        if (msg == null) return;

        Color color = logType switch
        {
            LogType.Log => Color.white,
            LogType.Error => Color.red,
            LogType.Warning => Color.yellow,
            LogType.Assert => Color.green,
            _ => Color.white
        };

        DebugText.text += $"{Time.time:000.000}\n";
        DebugText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{msg}</color>\n";
    }
}
