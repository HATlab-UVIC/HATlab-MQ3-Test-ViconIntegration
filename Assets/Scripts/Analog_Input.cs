using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ViconDataStreamSDK.CSharp;

public class Analog_Input : MonoBehaviour
{
    public ViconDataStreamClient Client;
    [SerializeField] string DeviceName;
    [SerializeField] string ComponentName;
    [SerializeField] TextMeshProUGUI AnalogOutputText;
    [SerializeField] Light SpotLight;
    bool IsReadingInput = false;

    private void OnDestroy()
    {
        StopCoroutine(check());
    }

    void Start()
    {
        if (Client == null) DebugConsole.Error("ViconDataStreamClient does not exist for analog input");
        if (Client.IsDeviceDataEnabled().Enabled)
        {
            if (Client.GetDeviceCount().DeviceCount > 0)
            {
                DebugConsole.Log($"Analog device count: {Client.GetDeviceCount().DeviceCount}");
                if (Client.GetDeviceOutputValue(DeviceName, ComponentName).Result == Result.Success)
                {
                    AnalogOutputText.text = Client.GetDeviceOutputValue(DeviceName, ComponentName).Value.ToString("0.00");
                    SpotLight.intensity = (float)Client.GetDeviceOutputValue(DeviceName, ComponentName).Value;
                    IsReadingInput = true;
                }
            }
            else
            {
                Debug.LogError("There is no analog device");
                AnalogOutputText.text = "There is no analog device";
            }
        }
        else
        {
            DebugConsole.Error("Client Device Data is not enabled");
            Debug.LogError("Client Device Data is not enabled");
            Client.EnableDeviceData();
        }

        StartCoroutine(check());
    }

    void Update()
    {
        if (IsReadingInput && Client.IsDeviceDataEnabled().Enabled)
        {
            if (Client.GetDeviceOutputValue(DeviceName, ComponentName).Result == Result.Success)
            {
                AnalogOutputText.text = Client.GetDeviceOutputValue(DeviceName, ComponentName).Value.ToString("0.00");
                SpotLight.intensity = (float)Client.GetDeviceOutputValue(DeviceName, ComponentName).Value;
                IsReadingInput = true;
            }
        }
    }

    IEnumerator check()
    {
        if (Client.IsDeviceDataEnabled().Enabled)
        {
            DebugConsole.Log("Device Data is enabled");
            if (Client.GetDeviceCount().DeviceCount > 0)
            {
                DebugConsole.Log($"Device count is: {Client.GetDeviceCount().DeviceCount}");
                if (Client.GetDeviceOutputValue(DeviceName, ComponentName).Result == Result.Success)
                {
                    DebugConsole.Success("Client successfully reading device output value");
                    IsReadingInput = true;
                }
                else
                {
                    DebugConsole.Error($"Client unsuccessfully reading device output value.\nGetDeviceOutputValue Result: {Client.GetDeviceOutputValue(DeviceName, ComponentName).Result.ToString()}");
                    IsReadingInput = false;
                }
            }
            else
            {
                DebugConsole.Log($"Device count is: {Client.GetDeviceCount().DeviceCount}");
                IsReadingInput = false;
            }
        }
        else
        {
            IsReadingInput = false;
            DebugConsole.Error("Device Data is not enabled");
            DebugConsole.Log("Trying enabling Deice Data...");
            Client.EnableDeviceData();
            if (Client.IsDeviceDataEnabled().Enabled)
            {
                DebugConsole.Success("Device Data is enabled");
            }
            else
            {
                DebugConsole.Error("Device Data is cannot be enabled");
            }
        }
        yield return new WaitForSeconds(1f);
    }
}
