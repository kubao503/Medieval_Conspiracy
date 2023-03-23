using System.Collections.Generic;
using PathCreation;
using UnityEngine;

[ExecuteInEditMode]
public class BuildingGenerator : PathCreation.Examples.PathSceneTool {

    [SerializeField] private Transform _holder;
    [SerializeField] private List<GameObject> _buildings;
    const float epsilon = .005f;
    const float minWidth = .1f;
    const float worldToPath = 10f / 14.95f; // World distance to path distance ratio
    [SerializeField] private int _seed = 0;


    public void NewSeed()
    {
        _seed = (int)System.DateTime.Now.Ticks;
        Generate();
    }


    void Generate ()
    {
        if (pathCreator == null || _buildings.Count == 0 || _holder == null) { Debug.Log("BuildingGenerator: Something is null");  return; }
        DestroyObjects();

        VertexPath path = pathCreator.path;
        var dst = 0f;

        // Get current vertex
        Vector3 currentVertex = path.GetPointAtDistance(dst);
        Random.InitState(_seed);

        while (true)
        {
            // Get random building
            var index = Random.Range(0, _buildings.Count);
            var building = _buildings[index];
            if (building == null) { Debug.Log("Building is null"); return; }

            var mainBox = building.transform.Find("Main Box");
            if (mainBox == null) { Debug.Log(building.name + " does not have Main Box"); return; }

            var positionOffset = mainBox.localScale / 2f;

            // Get building width
            var buildingWidth = Mathf.Max(mainBox.localScale.z, minWidth);

            // Get next vertex
            // If next vertex doesn't exist => break
            if (!FindNextVertexDistance(path, ref dst, buildingWidth)) break;

            var newBuilding = Instantiate(building, currentVertex + positionOffset, Quaternion.identity, _holder);

            // Calculate direction between two vertecies
            var nextVertex = path.GetPointAtDistance(dst);
            var direction = nextVertex - currentVertex;

            // Rotate building around vertex
            var angle = Quaternion.FromToRotation(newBuilding.transform.forward, direction).eulerAngles.y;
            newBuilding.transform.RotateAround(currentVertex, Vector3.up, angle);

            // Next vertex is now current
            currentVertex = nextVertex;
        }
    }


    private bool FindNextVertexDistance(VertexPath path, ref float pathDst, float buildingWidth)
    {
        var initPoint = path.GetPointAtDistance(pathDst);
        pathDst += buildingWidth * worldToPath;

        while (true)
        {
            // Run out of path
            if (pathDst > path.length) return false;

            // Get point at that distance
            var nextPoint = path.GetPointAtDistance(pathDst);

            // Measure real distance to that point
            var realDst = Vector3.Distance(initPoint, nextPoint);

            // Calculate how much is left
            var realDstLeft = buildingWidth - realDst;

            // Next vertex found
            if (realDstLeft < epsilon) break;

            // Increment dst
            pathDst += realDstLeft * worldToPath;
        }

        return true;
    }

    void DestroyObjects () {
        int numChildren = _holder.childCount;
        for (int i = numChildren - 1; i >= 0; i--) {
            DestroyImmediate (_holder.GetChild (i).gameObject, false);
        }
    }

    protected override void PathUpdated () {
        if (pathCreator != null) {
            Generate ();
        }
    }
}
