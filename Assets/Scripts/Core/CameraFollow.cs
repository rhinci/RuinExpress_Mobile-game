using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 3, -5);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float lookAheadDistance = 5f; // Смотрим вперёд

    void LateUpdate()
    {
        if (target == null) return;

        // Позиция камеры за игроком
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Точка, куда смотреть - немного впереди игрока по оси Z
        Vector3 lookTarget = target.position + Vector3.forward * lookAheadDistance;
        transform.LookAt(lookTarget);
    }
}