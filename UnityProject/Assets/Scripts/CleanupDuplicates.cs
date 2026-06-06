using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CleanupDuplicates : EditorWindow
{
    [MenuItem("Tools/Cleanup Duplicate Objects")]
    public static void ShowWindow()
    {
        GetWindow<CleanupDuplicates>("Cleanup Duplicates");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Delete Duplicates at Same Position"))
        {
            RemoveDuplicates();
        }
    }

    void RemoveDuplicates()
    {
        // Select objects in Hierarchy before running
        Transform[] selectedObjects = Selection.GetTransforms(SelectionMode.TopLevel);
        
        Dictionary<Vector3, GameObject> uniquePositions = new Dictionary<Vector3, GameObject>();
        List<GameObject> toDelete = new List<GameObject>();

        foreach (Transform t in selectedObjects)
        {
            // If objects have slight float variances, use:
            // Vector3 pos = new Vector3(Mathf.Round(t.position.x * 100f)/100f, ...);
            Vector3 pos = t.position;

            if (uniquePositions.ContainsKey(pos))
            {
                toDelete.Add(t.gameObject);
            }
            else
            {
                uniquePositions.Add(pos, t.gameObject);
            }
        }

        foreach (GameObject obj in toDelete)
        {
            Undo.DestroyObjectImmediate(obj);
        }

        Debug.Log($"Cleanup complete. Deleted {toDelete.Count} duplicates.");
    }
}
