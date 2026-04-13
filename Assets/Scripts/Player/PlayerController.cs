using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float laneWidth = 1.5f;

    [Header("Прыжок")]
    [SerializeField] private float jumpForce = 7f;

    private int currentLane = 1;
    private Vector3 targetPosition;
    private Rigidbody rb;
    private bool isGrounded;
    private bool isInvincible = false;
    private float invincibleTime = 0f;

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

        Debug.DrawRay(transform.position, Vector3.down * 0.7f, isGrounded ? Color.green : Color.red);


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

        // Проверка земли (луч вниз)
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.4f);

        // Прыжок
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }

        // Плавное движение по X
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(transform.position.x, targetPosition.x, Time.deltaTime * 15f);
        transform.position = newPos;

        // Автоматическое движение вперёд
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // Обновление неуязвимости
        if (isInvincible)
        {
            invincibleTime -= Time.deltaTime;
            if (invincibleTime <= 0)
            {
                isInvincible = false;
            }
        }
    }

    void UpdateTargetPosition()
    {
        float xPos = (currentLane - 1) * laneWidth;
        targetPosition = new Vector3(xPos, transform.position.y, transform.position.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            // Проверяем большое или маленькое
            float obstacleHeight = collision.gameObject.transform.localScale.y;
            bool isBig = obstacleHeight > 1f;

            if (!isBig && !isGrounded)
            {
                // Маленькое препятствие + игрок в воздухе = успех
                Debug.Log("Перепрыгнул!");
                Destroy(collision.gameObject);
                return;
            }

            if (isInvincible) return;

            Debug.Log("Проигрыш!");
            Time.timeScale = 0;
        }
    }

    public void MakeInvincible(float duration)
    {
        isInvincible = true;
        invincibleTime = duration;
    }
}