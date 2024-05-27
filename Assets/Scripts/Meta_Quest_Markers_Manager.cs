using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVicon;

public class Meta_Quest_Markers_Manager : MonoBehaviour
{
    [SerializeField] List<Meta_Quest_Markers> TrackedMetaQuestSubjects;

    void Start()
    {
        GameEvents.OnCalibrationInvoked += Calibrate;
    }

    void OnDestroy()
    {
        GameEvents.OnCalibrationInvoked -= Calibrate;
    }


    public void Calibrate()
    {
        Debug.LogError("Calibrate");
        foreach (var TrackedSubject in TrackedMetaQuestSubjects)
        {
            if (TrackedSubject.isRoot)
            {
                for (int i = 0; i < TrackedSubject.NumberOfMarkers; i ++)
                {
                    Debug.LogError($"Root\nMarker number: {i} position: {TrackedSubject.transform.GetChild(i).position} rotation {TrackedSubject.transform.GetChild(i).rotation}");
                }
            }
            else
            {
                for (int i = 0; i < TrackedSubject.NumberOfMarkers; i ++)
                {
                    Debug.LogError($"Marker number: {i} position: {TrackedSubject.transform.GetChild(i).position} rotation {TrackedSubject.transform.GetChild(i).rotation}");
                }

            }
        }
    }
}
