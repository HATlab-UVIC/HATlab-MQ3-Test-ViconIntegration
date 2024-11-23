using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVicon;

public class WalkwayCube : MonoBehaviour
{
    [SerializeField] private GameObject walkway;
    [SerializeField] private Meta_Quest_Markers_Manager metaQuestMarkersManager;
    [SerializeField] private Material walkwayMaterial;
    [SerializeField] float height = 0.05f;
    public bool IsInstantiated = false;
    private List<Transform> anchors;
    private Transform instantiatedWalkway;
    private float time = 0f;

    private void Start()
    {
        anchors = new List<Transform>();
    }

    private void Update() {
        if (IsInstantiated && time > 1)
        {
            UpdateWalkway();
            time = 0f;
        }
        else
        {
            time += Time.deltaTime;
        }
    }

    public void SetISInstantiatedWalkwayTrue() {
        InstantiateWalkway();
        IsInstantiated = true;
    }

    private void UpdateWalkway() {
        List<Meta_Quest_Markers> trackedSubjects;
        trackedSubjects = metaQuestMarkersManager.GetTrackedMetaQuestSubjects();
        // DebugConsole.Log($"There are {trackedSubjects.Count} tracked subjects.");

        foreach (var subject in trackedSubjects)
        {
            if (!subject.IsRoot)
            {
                anchors.Add(subject.transform.GetChild(0));
                Debug.Log($"Subject's child's position: {subject.transform.GetChild(0).position}");
                // DebugConsole.Log($"Subject {subject.gameObject.name} was added to the Walkway anchors.");
            }
        }

        var walkwayMesh = new Mesh
        {
            name = "WalkwayCube"
        };
        walkwayMesh.vertices = new[]
        {
            // down side
            anchors[0].position,
            anchors[1].position,
            anchors[2].position,
            anchors[3].position,

            // left
            anchors[0].position,
            anchors[1].position,
            anchors[1].position + new Vector3(0, height, 0),
            anchors[0].position + new Vector3(0, height, 0),

            // back
            anchors[1].position,
            anchors[2].position,
            anchors[2].position + new Vector3(0, height, 0),
            anchors[1].position + new Vector3(0, height, 0),

            // right
            anchors[2].position,
            anchors[3].position,
            anchors[3].position + new Vector3(0, height, 0),
            anchors[2].position + new Vector3(0, height, 0),

            // front
            anchors[3].position,
            anchors[0].position,
            anchors[0].position + new Vector3(0, height, 0),
            anchors[3].position + new Vector3(0, height, 0),

            // top
            anchors[0].position + new Vector3(0, height, 0),
            anchors[1].position + new Vector3(0, height, 0),
            anchors[2].position + new Vector3(0, height, 0),
            anchors[3].position + new Vector3(0, height, 0)
        };

        walkwayMesh.triangles = new int[]
        {
            // down
            0, 1, 2,
            2, 3, 0,

            // left
            4, 5, 6,
            6, 7, 4,

            // back
            8, 9, 10,
            10, 11, 8,

            // right
            12, 13, 14,
            14, 15, 12,

            // front
            16, 17, 18,
            18, 19, 16,

            // top
            20, 21, 22,
            22, 23, 20
        };

        walkwayMesh.normals = new Vector3[]
        {
            Vector3.down, Vector3.down, Vector3.down, Vector3.down,
            Vector3.left, Vector3.left, Vector3.left, Vector3.left,
            Vector3.back, Vector3.back, Vector3.back, Vector3.back,
            Vector3.right, Vector3.right, Vector3.right, Vector3.right,
            Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
            Vector3.up, Vector3.up, Vector3.up, Vector3.up
        };
        for (int i = 0; i < walkwayMesh.vertices.Length; i++)
        {
            Debug.Log("Vertices: " + walkwayMesh.vertices[i]);
        }

        walkwayMesh.Optimize();
        walkwayMesh.RecalculateNormals();

        instantiatedWalkway.GetComponent<MeshFilter>().mesh = walkwayMesh;
        instantiatedWalkway.GetComponent<MeshRenderer>().material = walkwayMaterial;
    }

    private void InstantiateWalkway()
    {
        instantiatedWalkway = Instantiate(walkway, Vector3.zero, Quaternion.identity).transform;
        UpdateWalkway();
    }
}
