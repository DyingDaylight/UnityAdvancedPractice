using UnityEngine;

namespace RTS.Visualization
{
    public class SelectionBox : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RectTransform canvasRect;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void UpdateBounds(Vector3 start, Vector3 end)
        {
            Vector2 localStart = ScreenToLocalPoint(start);
            Vector2 localEnd = ScreenToLocalPoint(end);
        
            Vector2 min = Vector2.Min(localStart, localEnd);
            Vector2 max = Vector2.Max(localStart, localEnd);

            rectTransform.sizeDelta = max - min;
            rectTransform.anchoredPosition = (max + min) / 2f;
        }
    
        private Vector2 ScreenToLocalPoint(Vector2 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint);

            return localPoint;
        }
    }
}
