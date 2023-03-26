using UnityEngine;
using PathCreation;


[RequireComponent(typeof(PathCreator))]
public class MainPath : MonoBehaviour
{
    public  static VertexPath Path => _path;

    private static VertexPath _path;

    private void Awake()
    {
        if (_path == null)
            _path = GetComponent<PathCreator>().path;
        else
        {
            Debug.LogError("More than one MainPath on scene. Deleting one of them");
            Destroy(this);
        }
    }
}
