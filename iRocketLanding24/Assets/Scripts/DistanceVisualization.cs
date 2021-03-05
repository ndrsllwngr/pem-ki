using System.Globalization;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class DistanceVisualization : MonoBehaviour
{
    public GameObject target;

    public float distanceBetweenObjects;


    private void Update()
    {
        var position = transform.position;
        var position1 = target.transform.position;
        distanceBetweenObjects = Vector3.Distance(position, position1);
        Debug.DrawLine(position, position1, Color.green);
    }

    private void OnDrawGizmos()
    {
        GUI.color = Color.black;
        var position = transform.position;
        Handles.Label(position - (position - 
                                  target.transform.position)/2, distanceBetweenObjects.ToString(CultureInfo.CurrentCulture));
    }
}