using UnityEngine;

public class ObstacleMovement : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(4f, 0f, 0f);
    [SerializeField, Min(0f)] private float speed = 2f;

    private Vector3 startPosition;
    private Vector3 endPosition;

    private void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + offset;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            endPosition,
            speed * Time.deltaTime
        );

        if (transform.position == endPosition)
        {
            (startPosition, endPosition) = (endPosition, startPosition);
        }
    }
}
