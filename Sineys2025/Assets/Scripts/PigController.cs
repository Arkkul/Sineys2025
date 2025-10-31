using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PigController : MonoBehaviour
{
    [Header("Motion")]
    public float forwardSpeed = 5f;
    public float turnSpeedDegPerSec = 90f;

    [Header("Jump (trigger)")]
    [Tooltip("Тег триггера, при входе в Collider с которым выполняется прыжок.")]
    public string jumpTriggerTag = "JumpPad";
    public float jumpForce = 6f;

    [Header("Stun / Knockback")]
    [Tooltip("Тег препятствия. Только объекты с этим тегом будут вызывать стан/отталкивание.")]
    public string obstacleTag = "Obstacle";

    [Tooltip("Сила отбрасывания (импульс) при столкновении.")]
    public float knockbackForce = 8f;

    [Tooltip("Длительность оглушения в секундах; во время оглушения отключены постоянное движение и поворот.")]
    public float stunDuration = 1.0f;

    [Header("Gizmos")]
    public bool drawGizmo = true;
    public float gizmoLength = 2f;
    public float gizmoHeadSize = 0.3f;
    public Color stunGizmoColor = Color.yellow;

    Rigidbody rb;
    Collider col;

    float turnInput;
    bool isStunned = false;
    float stunTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // Чтение удерживаемого ввода поворота
        turnInput = 0f;
        if (Input.GetKey(KeyCode.A)) turnInput = -1f;
        if (Input.GetKey(KeyCode.D)) turnInput = 1f;
    }

    void FixedUpdate()
    {
        if (isStunned)
        {
            stunTimer -= Time.fixedDeltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                // удерживаемый ввод уже прочитан в Update, сработает сразу
            }
        }

        if (!isStunned && Mathf.Abs(turnInput) > 0f)
        {
            float deltaDeg = turnInput * turnSpeedDegPerSec * Time.fixedDeltaTime;
            Quaternion deltaRot = Quaternion.Euler(0f, deltaDeg, 0f);
            rb.MoveRotation(rb.rotation * deltaRot);
        }

        // Постоянное движение вперёд отключено во время стана.
        if (!isStunned)
        {
            Vector3 forwardVel = transform.forward * forwardSpeed;
            rb.linearVelocity = new Vector3(forwardVel.x, rb.linearVelocity.y, forwardVel.z);
        }
        // если isStunned == true, не перезаписываем rb.velocity чтобы сохранить эффект отбрасывания
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isStunned) return;

        if (string.IsNullOrEmpty(obstacleTag)) return;

        // Проверяем: если любой объект участвующий в столкновении имеет нужный тег -> стан
        if (CollisionHasTag(collision, obstacleTag))
        {
            StartStunWithKnockback(collision);
        }
    }

    bool CollisionHasTag(Collision collision, string tagToCheck)
    {
        // Проверяем непосредственно объект коллайдера (тот, с которым столкнулись)
        if (collision.collider != null && collision.collider.CompareTag(tagToCheck)) return true;
        // Проверяем весь GameObject
        if (collision.gameObject != null && collision.gameObject.CompareTag(tagToCheck)) return true;
        // Проверяем root (на случай, если тег висит на корне)
        if (collision.transform != null && collision.transform.root != null && collision.transform.root.CompareTag(tagToCheck)) return true;

        // Проверяем все контакты на предмет других коллайдеров с нужным тег (доп. надёжность)
        foreach (var contact in collision.contacts)
        {
            if (contact.otherCollider != null && contact.otherCollider.CompareTag(tagToCheck)) return true;
            if (contact.thisCollider != null && contact.thisCollider.CompareTag(tagToCheck)) return true;
        }

        return false;
    }

    void StartStunWithKnockback(Collision collision)
    {
        isStunned = true;
        stunTimer = Mathf.Max(0f, stunDuration);

        // Отбрасывание назад по локальной задней оси
        Vector3 knockDir = -transform.forward.normalized;
        rb.AddForce(knockDir * knockbackForce, ForceMode.Impulse);
    }

    // Прыжок срабатывает при входе в триггер с указанным тегом.
    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (!other.CompareTag(jumpTriggerTag)) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void OnDrawGizmos()
    {
        if (!drawGizmo) return;
        Transform t = transform;
        Vector3 start = t.position;
        Vector3 end = start + t.forward * gizmoLength;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(end, gizmoHeadSize * 0.4f);

        Vector3 dir = (end - start).normalized;
        Quaternion look = Quaternion.LookRotation(dir);
        Vector3 rightHead = look * Quaternion.Euler(0, 150f, 0) * Vector3.forward;
        Vector3 leftHead  = look * Quaternion.Euler(0, 210f, 0) * Vector3.forward;
        Gizmos.DrawLine(end, end + rightHead * gizmoHeadSize);
        Gizmos.DrawLine(end, end + leftHead * gizmoHeadSize);

        if (isStunned)
        {
            Gizmos.color = stunGizmoColor;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.35f);
        }
    }
}
