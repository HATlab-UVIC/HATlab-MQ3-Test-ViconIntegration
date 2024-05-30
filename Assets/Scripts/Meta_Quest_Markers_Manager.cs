using Meta.WitAi.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using UnityVicon;

public class Meta_Quest_Markers_Manager : MonoBehaviour
{
    [SerializeField] List<Meta_Quest_Markers> TrackedMetaQuestSubjects;
    [SerializeField] Transform CenterEyeAnchor;
    Meta_Quest_Markers Root_Meta_Quest_Marker;

    void Start()
    {
        GameEvents.OnCalibrationInvoked += Calibrate;
        foreach (var TrackedSubject in TrackedMetaQuestSubjects)
        {
            if (TrackedSubject.IsRoot)
            {
                Root_Meta_Quest_Marker = TrackedSubject;
                break;
            }
        }
        if (Root_Meta_Quest_Marker == null) Debug.LogError("There is no Root in Tracked Meta Quest Subjects");
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
            if (TrackedSubject.IsRoot)
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
        Vector3 ViconWorldScaleIPD;
        Vector3 ViconWorldTransformation;
        Vector3 RealWorldScaleIPD;
        Vector3 RealWorldTransformation;

        ViconWorldScaleIPD = Root_Meta_Quest_Marker.transform.GetChild(1).position - Root_Meta_Quest_Marker.transform.GetChild(0).position;
        ViconWorldTransformation = Root_Meta_Quest_Marker.transform.GetChild(1).position - ViconWorldScaleIPD / 2;
        RealWorldScaleIPD = CenterEyeAnchor.right * 13;
        RealWorldTransformation = CenterEyeAnchor.position;

        float ViconToWorldScale = 0.13f / ViconWorldScaleIPD.magnitude;
        float angleX, angleY, angleZ;

        angleX = Vector3.Angle(Vector3.Cross( ViconWorldScaleIPD, new Vector3(0, 1, 0) ), new Vector3(1, 0, 0));
        angleY = Vector3.Angle(Vector3.Cross(ViconWorldScaleIPD, new Vector3(0, 1, 0)), new Vector3(0, 1, 0));
        angleZ = Vector3.Angle(Vector3.Cross(ViconWorldScaleIPD, new Vector3(0, 1, 0)), new Vector3(0, 0, 1));

        Debug.LogError($"angleX: {angleX}\nangleY: {angleY}\nangleZ: {angleZ}");

        Matrix4x4 TranslateViconToOrigin = new Matrix4x4();
        TranslateViconToOrigin.SetColumn(0, new Vector4(1, 0, 0, - ViconWorldTransformation.x));
        TranslateViconToOrigin.SetColumn(1, new Vector4(0, 1, 0, - ViconWorldTransformation.y));
        TranslateViconToOrigin.SetColumn(2, new Vector4(0, 0, 1, - ViconWorldTransformation.z));
        TranslateViconToOrigin.SetColumn(3, new Vector4(0, 0, 0, 1));

        Matrix4x4 TranslateOriginToWorld = new Matrix4x4();
        TranslateOriginToWorld.SetColumn(0, new Vector4(1, 0, 0, RealWorldTransformation.x));
        TranslateOriginToWorld.SetColumn(1, new Vector4(0, 1, 0, RealWorldTransformation.y));
        TranslateOriginToWorld.SetColumn(2, new Vector4(0, 0, 1, RealWorldTransformation.z));
        TranslateOriginToWorld.SetColumn(3, new Vector4(0, 0, 0, 1));

        Matrix4x4 SwizzleYandZ = new Matrix4x4();
        SwizzleYandZ.SetColumn(0, new Vector4(1, 0, 0, 0));
        SwizzleYandZ.SetColumn(1, new Vector4(0, 0, 1, 0));
        SwizzleYandZ.SetColumn(2, new Vector4(0, 1, 0, 0));
        SwizzleYandZ.SetColumn(3, new Vector4(0, 0, 0, 1));

        Matrix4x4 RotateX = new Matrix4x4();
        RotateX.SetColumn(0, new Vector4(1, 0, 0, 0));
        RotateX.SetColumn(1, new Vector4(0, Mathf.Cos(angleX), Mathf.Sin(angleX), 0));
        RotateX.SetColumn(2, new Vector4(0, Mathf.Sin(angleX), Mathf.Cos(angleX), 0));
        RotateX.SetColumn(3, new Vector4(0, 0, 0, 1));

        Matrix4x4 RotateY = new Matrix4x4();
        RotateY.SetColumn(0, new Vector4(Mathf.Cos(angleY), 0, Mathf.Sin(angleY), 0));
        RotateY.SetColumn(1, new Vector4(0, 1, 0, 0));
        RotateY.SetColumn(2, new Vector4(-Mathf.Sin(angleZ), 0, 0));
        RotateY.SetColumn(3, new Vector4(0, 0, 0, 1));

        Matrix4x4 RotateZ = new Matrix4x4();
        RotateZ.SetColumn(0, new Vector4(Mathf.Cos(angleZ), -Mathf.Sin(angleZ), 0, 0));
        RotateZ.SetColumn(1, new Vector4(Mathf.Sin(angleZ), Mathf.Cos(angleZ), 0, 0));
        RotateZ.SetColumn(2, new Vector4(0, 0, 1, 0));
        RotateZ.SetColumn(3, new Vector4(0, 0, 0, 1));

        Matrix4x4 RotateYby90 = new Matrix4x4();
        RotateYby90.SetColumn(0, new Vector4(0, -1, 0, 0));
        RotateYby90.SetColumn(1, new Vector4(1, 0, 0, 0));
        RotateYby90.SetColumn(2, new Vector4(0, 0, 1, 0));
        RotateYby90.SetColumn(3, new Vector4(0, 0, 0, 1));

        Debug.LogError($"Calibrate Rotation Matrix \n{RotateZ * (RotateY * RotateX)}");

        // need to rotate and scale


        /*Vector3 axis = Vector3.Cross(ViconWorldScaleIPD.normalized, RealWorldScaleIPD.normalized);
        float angle = Mathf.Acos(Vector3.Dot(ViconWorldScaleIPD, RealWorldScaleIPD)) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, axis);
        Matrix4x4 transformMatrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
        Root_Meta_Quest_Marker.CalibratedTransformMatrix = transformMatrix;*/

        /*foreach (var TrackedSubject in TrackedMetaQuestSubjects)
        {
            if (TrackedSubject.IsRoot)
            {
                for (int i = 0; i < TrackedSubject.NumberOfMarkers; i++)
                {
                    Debug.LogError($"Root\nMarker number: {i} position: {TrackedSubject.transform.GetChild(i).position} rotation {TrackedSubject.transform.GetChild(i).rotation}");
                }
            }
            else
            {
                for (int i = 0; i < TrackedSubject.NumberOfMarkers; i++)
                {
                    Debug.LogError($"Marker number: {i} position: {TrackedSubject.transform.GetChild(i).position} rotation {TrackedSubject.transform.GetChild(i).rotation}");
                }

            }
        }*/


        foreach (var TrackedSubject in TrackedMetaQuestSubjects)
        {
            TrackedSubject.CalibratedSwizzleMatrix = SwizzleYandZ;
            TrackedSubject.CalibratedRotateMatrix = RotateZ * (RotateY * RotateX);
            TrackedSubject.CalibratedRotateXMatrix = RotateX;
            TrackedSubject.CalibratedRotateYMatrix = RotateY;
            TrackedSubject.CalibratedRotateZMatrix = RotateZ;
        }
    }
}
