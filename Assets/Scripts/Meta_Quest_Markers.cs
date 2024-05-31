using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ViconDataStreamSDK.CSharp;
using Unity.VisualScripting;
using UnityEngine.XR;

namespace UnityVicon
{
    /// <summary>
    ///  Track subject with "HMD" segment by individual markers
    ///  Maximum fps fixed at 60 fps. ViconDataStreamClient.cs adjusted for this use
    /// </summary>
    public class Meta_Quest_Markers : MonoBehaviour
    {
        public bool IsRoot = false;

        [HideInInspector] public Matrix4x4 CalibrateSwizzleMatrix = Matrix4x4.identity;
        [HideInInspector] public Matrix4x4 CalibrateRotateXMatrix = Matrix4x4.identity;
        [HideInInspector] public Matrix4x4 CalibrateRotateYMatrix = Matrix4x4.identity;
        [HideInInspector] public Matrix4x4 CalibrateRotateZMatrix = Matrix4x4.identity;
        [HideInInspector] public Matrix4x4 CalibrateScaleMatrix = Matrix4x4.identity;
        [HideInInspector] public Vector3 CalibrateTransformMatrix = Vector3.zero;
        [HideInInspector] public uint NumberOfMarkers;


        [SerializeField] string SubjectName;
        [SerializeField] string SegmentName;
        Transform Headset;

        bool IsScaled = true;
        bool MarkerEnabled = false;
        Vector3 CalibratedPosition = Vector3.one;
        Quaternion CalibratedRotation = Quaternion.identity;

        public ViconDataStreamClient Client;

        public Meta_Quest_Markers()
        {
        }

        void Start()
        {
            Application.targetFrameRate = 60;
            Headset = transform;
        }

        void OnDestroy()
        {
        }

        void Update()
        {
            if (!MarkerEnabled)
            {
                Client.EnableLabeledMarkerData();
                NumberOfMarkers = Client.GetNumberOfMarkers(SubjectName);
            }
            else { MarkerEnabled = true; }
            Output_GetSubjectRootSegmentName OGSRSN = Client.GetSubjectRootSegmentName(SubjectName);
            List<Output_GetMarkerName> OGMN = new List<Output_GetMarkerName>();
            // Debug.Log("numberOfMarkers: " + NumberOfMarkers);

            // Use FindAndTransform instead of FindAndTransformMarker for tracking segments instead of markers
            for (uint i = 0; i < NumberOfMarkers; i++)
            {
                FindAndTransformMarker(Headset, strip(Client.GetMarkerNameFromIndex(SubjectName, i)));
            }
        }

        void FindAndTransformMarker(Transform root, string MarkerName)
        {
            if (root.gameObject.name == MarkerName)
            {
                // root.position = CalibratedRotateMatrix.MultiplyPoint3x4( CalibratedSwizzleMatrix.MultiplyPoint3x4( Client.GetMarkerGlobalTranslationVector3(SubjectName, MarkerName) ) );
                // root.position = CalibratedRotateMatrix.MultiplyPoint3x4( Client.GetMarkerGlobalTranslationVector3(SubjectName, MarkerName) );
                // root.position = CalibratedSwizzleMatrix.MultiplyPoint3x4( Client.GetMarkerGlobalTranslationVector3(SubjectName, MarkerName) );
                root.position =
                    CalibrateScaleMatrix.MultiplyPoint3x4(
                    CalibrateRotateZMatrix.MultiplyPoint3x4(
                    CalibrateRotateYMatrix.MultiplyPoint3x4(
                    CalibrateRotateXMatrix.MultiplyPoint3x4(
                    CalibrateSwizzleMatrix.MultiplyPoint3x4(Client.GetMarkerGlobalTranslationVector3(SubjectName, MarkerName)))))) + CalibrateTransformMatrix;
                double[] rot = Client.GetSegmentRotation(SubjectName, SegmentName).Rotation;
                root.rotation = new Quaternion((float)rot[0], (float)rot[1], (float)rot[2], (float)rot[3]);
                double[] scale = Client.GetSegmentScale(SubjectName, SegmentName).Scale;
                // root.localScale = new Vector3((float)scale[0], (float)scale[1], (float)scale[2]);
                return;
            }
            else
            {
                int childCount = root.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    // target not found. Recursively search
                    FindAndTransformMarker(root.GetChild(i), MarkerName);
                }
            }
        }

        string strip(string BoneName)
        {
            if (BoneName.Contains(":"))
            {
                string[] results = BoneName.Split(':');
                return results[1];
            }
            return BoneName;
        }

        void TransformChildren(Transform iTransform)
        {
            int ChildCount = iTransform.childCount;
            for (int i = 0; i < ChildCount; ++i)
            {
                Transform Child = iTransform.GetChild(i);
                ApplyBoneTransform(Child);
                TransformChildren(Child);
            }
        }
        // map the orientation back for forward

        void ApplyBoneTransform(Transform Bone)
        {
            string BoneName = strip(Bone.gameObject.name);
            // update the bone transform from the data stream
            Output_GetSegmentLocalRotationQuaternion ORot = Client.GetSegmentRotation(SubjectName, BoneName);
            // Debug.Log("Result: " + ORot.Result + " SubjectName:" + SubjectName);
            // Debug.Log("Segment Name:" + BoneName);
            // Debug.Log("Debug:" + "," + (float)ORot.Rotation[0] + "," + (float)ORot.Rotation[1] + "," + (float)ORot.Rotation[2] + "," + (float)ORot.Rotation[3]);
            if (ORot.Result == Result.Success)
            {
                // mapping back to default data stream axis
                Quaternion Rot = new Quaternion((float)ORot.Rotation[0], (float)ORot.Rotation[1], (float)ORot.Rotation[2], (float)ORot.Rotation[3]);
                // mapping right hand to left hand flipping x
                Bone.localRotation = new Quaternion(Rot.x, -Rot.y, -Rot.z, Rot.w);
            }

            Output_GetSegmentLocalTranslation OTran;
            if (IsScaled)
            {
                OTran = Client.GetScaledSegmentTranslation(SubjectName, BoneName);
            }
            else
            {
                OTran = Client.GetSegmentTranslation(SubjectName, BoneName);
            }

            if (OTran.Result == Result.Success)
            {
                //Vector3 Translate = new Vector3(-(float)OTran.Translation[2] * 0.001f, -(float)OTran.Translation[0] * 0.001f, (float)OTran.Translation[1] * 0.001f);
                Vector3 Translate = new Vector3((float)OTran.Translation[0] * 0.001f, (float)OTran.Translation[1] * 0.001f, (float)OTran.Translation[2] * 0.001f);
                Bone.localPosition = new Vector4(-Translate.x, Translate.y, Translate.z, 1);
            }

            // If there's a scale for this subject in the datastream, apply it here.
            if (IsScaled)
            {
                Output_GetSegmentStaticScale OScale = Client.GetSegmentScale(SubjectName, BoneName);
                if (OScale.Result == Result.Success)
                {
                    Bone.localScale = new Vector3((float)OScale.Scale[0], (float)OScale.Scale[1], (float)OScale.Scale[2]);
                }
            }
        }
    } //end of program
}// end of namespace