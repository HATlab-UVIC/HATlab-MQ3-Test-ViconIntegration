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
    

    void Update()
    {
        if (Client == null) Debug.LogError("ViconDataStreamClient does not exist for analog input");
        if (Client.IsDeviceDataEnabled().Enabled)
        {
            if (Client.GetDeviceCount().DeviceCount > 0)
            {
                Debug.LogError($"Analog device count: {Client.GetDeviceCount().DeviceCount}");
                if (Client.GetDeviceOutputValue(DeviceName, ComponentName).Result == Result.Success)
                {
                    AnalogOutputText.text = Client.GetDeviceOutputValue(DeviceName, ComponentName).Value.ToString("0.00");
                    SpotLight.intensity = (float) Client.GetDeviceOutputValue(DeviceName, ComponentName).Value;
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
    }
}
