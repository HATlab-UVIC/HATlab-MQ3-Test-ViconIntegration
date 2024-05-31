using Meta.WitAi.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using UnityVicon;

public class Meta_Quest_Markers_Manager : MonoBehaviour
{
    [SerializeField] List<Meta_Quest_Markers> TrackedMetaQuestSubjects;
    [SerializeField] Transform CenterEyeAnchor;
    Meta_Quest_Markers Root_Meta_Quest_Marker;

    Vector3 TransformVector;
    Vector3 ViconWorldScaleIPD; // InterPupillary Distance in Vicon Coordinate System
    Vector3 ViconWorldTransformation;
    Vector3 RealWorldScaleIPD; // InterPupillary Distance in Meta Quest Coordinate System. This is matching the real world coordinate system
    Vector3 RealWorldTransformation;

    Matrix4x4 RotateX;
    Matrix4x4 RotateY;
    Matrix4x4 RotateZ;
    Matrix4x4 ScaleMatrix;
    Matrix4x4 SwizzleYandZ;

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
        foreach (var TrackedSubject in TrackedMetaQuestSubjects)
        {
            if (TrackedSubject.IsRoot)
            {
                for (int i = 0; i < TrackedSubject.NumberOfMarkers; i++)
                {
                    // Debug.LogError($"Root\nMarker number: {i} position: {TrackedSubject.transform.GetChild(i).position} rotation {TrackedSubject.transform.GetChild(i).rotation}");
                }
            }
            else
            {
                for (int i = 0; i < TrackedSubject.NumberOfMarkers; i++)
                {
                    // Debug.LogError($"Marker number: {i} position: {TrackedSubject.transform.GetChild(i).position} rotation {TrackedSubject.transform.GetChild(i).rotation}");
                }

            }
        }



        ViconWorldScaleIPD = Root_Meta_Quest_Marker.transform.GetChild(1).position - Root_Meta_Quest_Marker.transform.GetChild(0).position;
        ViconWorldTransformation = (Root_Meta_Quest_Marker.transform.GetChild(0).position + Root_Meta_Quest_Marker.transform.GetChild(1).position) / 2;
        RealWorldScaleIPD = CenterEyeAnchor.right * 13; // The distance between marker HMD1 and marker HMD2 is around 13cm
        RealWorldTransformation = CenterEyeAnchor.position;

        // Swizzle Vicon Coordinate System into World Coordinate System
        ViconWorldScaleIPD = new Vector3(ViconWorldScaleIPD.x, ViconWorldScaleIPD.z, ViconWorldScaleIPD.y);
        ViconWorldTransformation = new Vector3(ViconWorldTransformation.x, ViconWorldTransformation.z, ViconWorldTransformation.y);

        float ViconToWorldScale = 0.13f / ViconWorldScaleIPD.magnitude;
        float angleX, angleY, angleZ;

        // Calculate angles in 3 axis between ViconWorldScaleIPD and RealWorldScaleIPD
        angleX = Vector3.Angle(new Vector3(0, RealWorldScaleIPD.y, RealWorldScaleIPD.z), new Vector3(0, ViconWorldScaleIPD.y, ViconWorldScaleIPD.z)) * Mathf.PI / 180;
        angleY = -Vector3.Angle(new Vector3(RealWorldScaleIPD.x, 0, RealWorldScaleIPD.z), new Vector3(ViconWorldScaleIPD.x, 0, ViconWorldScaleIPD.z)) * Mathf.PI / 180;
        angleZ = Vector3.Angle(new Vector3(RealWorldScaleIPD.x, RealWorldScaleIPD.y, 0), new Vector3(ViconWorldScaleIPD.x, ViconWorldScaleIPD.y, 0)) * Mathf.PI / 180;

        // Debug.LogError($"ViconWorldScaleIPD: {ViconWorldScaleIPD.normalized} RealWorldScaleIPD: {RealWorldScaleIPD.normalized}");
        // Debug.LogError($"angleX: {angleX * 180 / Mathf.PI} angleY: {angleY * 180 / Mathf.PI} angleZ: {angleZ * 180 / Mathf.PI}");

        /*Matrix4x4 TranslateViconToOrigin = new Matrix4x4();
        TranslateViconToOrigin.SetColumn(0, new Vector4(1, 0, 0, - ViconWorldTransformation.x));
        TranslateViconToOrigin.SetColumn(1, new Vector4(0, 1, 0, - ViconWorldTransformation.y));
        TranslateViconToOrigin.SetColumn(2, new Vector4(0, 0, 1, - ViconWorldTransformation.z));
        TranslateViconToOrigin.SetColumn(3, new Vector4(0, 0, 0, 1));

        Matrix4x4 TranslateOriginToWorld = new Matrix4x4();
        TranslateOriginToWorld.SetColumn(0, new Vector4(1, 0, 0, RealWorldTransformation.x));
        TranslateOriginToWorld.SetColumn(1, new Vector4(0, 1, 0, RealWorldTransformation.y));
        TranslateOriginToWorld.SetColumn(2, new Vector4(0, 0, 1, RealWorldTransformation.z));
        TranslateOriginToWorld.SetColumn(3, new Vector4(0, 0, 0, 1));*/

        SwizzleYandZ = new Matrix4x4();
        SwizzleYandZ.SetColumn(0, new Vector4(1, 0, 0, 0));
        SwizzleYandZ.SetColumn(1, new Vector4(0, 0, 1, 0));
        SwizzleYandZ.SetColumn(2, new Vector4(0, 1, 0, 0));
        SwizzleYandZ.SetColumn(3, new Vector4(0, 0, 0, 1));


        // Rotational 4x4 Matrix transformation. This will cause gimbal lock sometime
        RotateX = new Matrix4x4();
        RotateX.SetColumn(0, new Vector4(1, 0, 0, 0));
        RotateX.SetColumn(1, new Vector4(0, Mathf.Cos(angleX), -Mathf.Sin(angleX), 0));
        RotateX.SetColumn(2, new Vector4(0, Mathf.Sin(angleX), Mathf.Cos(angleX), 0));
        RotateX.SetColumn(3, new Vector4(0, 0, 0, 1));

        RotateY = new Matrix4x4();
        RotateY.SetColumn(0, new Vector4(Mathf.Cos(angleY), 0, Mathf.Sin(angleY), 0));
        RotateY.SetColumn(1, new Vector4(0, 1, 0, 0));
        RotateY.SetColumn(2, new Vector4(-Mathf.Sin(angleY), 0, Mathf.Cos(angleY), 0));
        RotateY.SetColumn(3, new Vector4(0, 0, 0, 1));

        RotateZ = new Matrix4x4();
        RotateZ.SetColumn(0, new Vector4(Mathf.Cos(angleZ), -Mathf.Sin(angleZ), 0, 0));
        RotateZ.SetColumn(1, new Vector4(Mathf.Sin(angleZ), Mathf.Cos(angleZ), 0, 0));
        RotateZ.SetColumn(2, new Vector4(0, 0, 1, 0));
        RotateZ.SetColumn(3, new Vector4(0, 0, 0, 1));

        ScaleMatrix = new Matrix4x4();
        ScaleMatrix.SetColumn(0, new Vector4(0.001f, 0, 0, 0));
        ScaleMatrix.SetColumn(1, new Vector4(0, 0.001f, 0, 0));
        ScaleMatrix.SetColumn(2, new Vector4(0, 0, 0.001f, 0));
        ScaleMatrix.SetColumn(3, new Vector4(0, 0, 0, 1));


        // Apply linear transform matrixes to ViconWorldTransformation to calculate real world distance between center eye anchor and IPD center of front markers
        ViconWorldTransformation = ScaleMatrix.MultiplyPoint3x4(RotateY.MultiplyPoint3x4(ViconWorldTransformation));
        TransformVector = RealWorldTransformation - ViconWorldTransformation + new Vector3(0, 0, 0.065f);

        Debug.LogError($"TransformVector: {TransformVector} RealWorldTransformation: {RealWorldTransformation} ViconWorldTransformation: {ViconWorldTransformation}");



        // Debug.LogError($"Calibrate Rotation Matrix \n{RotateZ * (RotateY * RotateX)}");
        // Debug.LogError($"RotateY Matrix \n{RotateY}");

        // need to rotate and scale

        foreach (var TrackedSubject in TrackedMetaQuestSubjects)
        {
            TrackedSubject.CalibrateSwizzleMatrix = SwizzleYandZ;
            // TrackedSubject.CalibratedRotateMatrix = RotateZ * (RotateY * RotateX);
            // TrackedSubject.CalibratedRotateXMatrix = RotateX;
            TrackedSubject.CalibrateRotateYMatrix = RotateY;
            // TrackedSubject.CalibratedRotateZMatrix = RotateZ;
            TrackedSubject.CalibrateScaleMatrix = ScaleMatrix;
            TrackedSubject.CalibrateTransformMatrix = TransformVector;
        }
    }
}
