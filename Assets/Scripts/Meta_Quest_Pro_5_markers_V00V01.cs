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
    ///  Track "Meta_Quest_Pro_5_markers_V00" subject with "HMD" segment by individual markers
    ///  Maximum fps fixed at 60 fps. ViconDataStreamClient.cs adjusted for this use
    /// </summary>
    public class Meta_Quest_Pro_5_markers_V00V01 : MonoBehaviour
    {
        [SerializeField] string SubjectName;
        [SerializeField] string SegmentName;
        [SerializeField] Transform headset;

        bool IsScaled = true;
        uint NumberOfMarkers;
        bool MarkerEnabled = false;
        [SerializeField] bool isRoot = false;

        public ViconDataStreamClient Client;

        public Meta_Quest_Pro_5_markers_V00V01()
        {
        }

        void Start()
        {
            Application.targetFrameRate = 60;
            GameEvents.OnCalibrationInvoked += Calibrate;
        }

        void OnDestroy()
        {
            GameEvents.OnCalibrationInvoked -= Calibrate;
        }

        void Update()
        {
            if (!MarkerEnabled)
            {
                Client.EnableLabeledMarkerData();
                NumberOfMarkers = Client.GetNumberOfMarkers(SubjectName);
            }
            else {MarkerEnabled = true;}
            Output_GetSubjectRootSegmentName OGSRSN = Client.GetSubjectRootSegmentName(SubjectName);
            List<Output_GetMarkerName> OGMN = new List<Output_GetMarkerName>();
            Debug.Log("numberOfMarkers: " + NumberOfMarkers);

            // Use FindAndTransform instead of FindAndTransformMarker for tracking segments instead of markers
            // FindAndTransform(Root, OGSRSN.SegmentName);
            for (uint i = 0; i < NumberOfMarkers; i++)
            {
                FindAndTransformMarker(headset, strip(Client.GetMarkerNameFromIndex(SubjectName, i)));
            }
        }

        /*void FindAndTransform(Transform iTransform, string BoneName)
        {
            // Debug.Log(BoneName);
            if (BoneName == "HMD")
            {
                Debug.Log("Debug");
                Transform HeadsetTransform = headset;
                ApplyBoneTransform(HeadsetTransform);
                //TransformChildren(HeadsetTransform);
                // Debug.Log(Child.position);
            }
            else
            {
                int ChildCount = iTransform.childCount;
                for (int i = 0; i < ChildCount; ++i)
                {
                    Transform Child = iTransform.GetChild(i);
                    if (strip(Child.name) == BoneName)
                    {
                        ApplyBoneTransform(Child);
                        TransformChildren(Child);
                        break;
                    }
                    // if not finding root in this layer, try the children
                    FindAndTransform(Child, BoneName);
                }
            }
        }*/

        void Calibrate(Vector3 position, Quaternion rotation)
        {
            if (MarkerEnabled)
            {
                Debug.LogError("Calibrate!");
            }
        }

        void FindAndTransformMarker(Transform root, string MarkerName)
        {
            if (root.gameObject.name == MarkerName)
            {
                root.position = Client.GetMarkerGlobalTranslationVector3(SubjectName, MarkerName);
                double[] rot = Client.GetSegmentRotation(SubjectName, SegmentName).Rotation;
                root.rotation = new Quaternion((float)rot[0], (float)rot[1], (float)rot[2], (float)rot[3]);
                double[] scale = Client.GetSegmentScale(SubjectName, SegmentName).Scale;
                // root.localScale = new Vector3((float)scale[0], (float)scale[1], (float)scale[2]);
                return;
            }
            else
            {
                int childCount = root.childCount;
                Debug.Log("Recursive child count: " + childCount);
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
            Debug.Log("Result: " + ORot.Result + " SubjectName:" + SubjectName);
            Debug.Log("Segment Name:" + BoneName);
            Debug.Log("Debug:" + "," + (float)ORot.Rotation[0] + "," + (float)ORot.Rotation[1] + "," + (float)ORot.Rotation[2] + "," + (float)ORot.Rotation[3]);
            if (ORot.Result == Result.Success)
            {
                // mapping back to default data stream axis
                //Quaternion Rot = new Quaternion(-(float)ORot.Rotation[2], -(float)ORot.Rotation[0], (float)ORot.Rotation[1], (float)ORot.Rotation[3]);
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
                Bone.localPosition = new Vector3(-Translate.x, Translate.y, Translate.z);
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