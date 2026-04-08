using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float laneWidth = 2f;

    [Header("Прыжок")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;

    private int currentLane = 1;
    private Vector3 targetPosition;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        targetPosition = transform.position;
    }

    void Update()
    {
        // Движение влево
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentLane = Mathf.Max(0, currentLane - 1);
            UpdateTargetPosition();
        }

        // Движение вправо
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentLane = Mathf.Min(2, currentLane + 1);
            UpdateTargetPosition();
        }

        // Прыжок
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && IsGrounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }

        // Плавное движение по X
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(transform.position.x, targetPosition.x, Time.deltaTime * 15f);
        transform.position = newPos;
    }

    void UpdateTargetPosition()
    {
        float xPos = (currentLane - 1) * laneWidth;
        targetPosition = new Vector3(xPos, transform.position.y, transform.position.z);
    }

    bool IsGrounded()
    {
        // Немного расширяем луч вниз
        return Physics.Raycast(transform.position, Vector3.down, 0.6f, groundLayer);
    }
}