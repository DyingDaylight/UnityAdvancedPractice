using UnityEngine;

namespace RTS.Visualization
{
    public class PathVisualizer : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        
        public void ShowPath(Vector3[] points)
        {
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }

        public void Clear()
        {
            lineRenderer.positionCount = 0;
        }
    }
}