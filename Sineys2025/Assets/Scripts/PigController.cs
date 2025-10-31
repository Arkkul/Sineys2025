using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PigController : MonoBehaviour
{
    [Header("Motion")]
    public float forwardSpeed = 5f;
    public float turnSpeedDegPerSec = 90f;

    [Header("Input (New Input System)")]
    [Tooltip("Action типа Value -> Axis (float). Bind A = -1, D = +1, also gamepad stick X.")]
    public InputActionReference turnAction; // assign in inspector

    [Header("Jump (trigger)")]
    public string jumpTriggerTag = "JumpPad";
    public float jumpForce = 6f;

    [Header("Stun / Knockback")]
    public string obstacleTag = "Obstacle";
    public float knockbackForce = 8f;
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

    void OnEnable()
    {
        if (turnAction != null && turnAction.action != null)
        {
            // убеждаемся, что action активен; не подписываемся на события, читаем значение
            turnAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (turnAction != null && turnAction.action != null)
        {
            turnAction.action.Disable();
        }
    }

    void Update()
    {
        // читаем текущее удерживаемое значение поворота (float -1..1)
        if (turnAction != null && turnAction.action != null)
        {
            turnInput = turnAction.action.ReadValue<float>();
        }
        else
        {
            // fallback на клавиатуру, если action не назначен (необязательно)
            turnInput = 0f;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed) turnInput = -1f;
                if (kb.dKey.isPressed) turnInput = 1f;
            }
        }
    }

    void FixedUpdate()
    {
        if (isStunned)
        {
            stunTimer -= Time.fixedDeltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                // удерживаемый ввод уже читается в Update и применится сразу
            }
        }

        if (!isStunned && Mathf.Abs(turnInput) > 0.001f)
        {
            float deltaDeg = turnInput * turnSpeedDegPerSec * Time.fixedDeltaTime;
            Quaternion deltaRot = Quaternion.Euler(0f, deltaDeg, 0f);
            rb.MoveRotation(rb.rotation * deltaRot);
        }

        if (!isStunned)
        {
            Vector3 forwardVel = transform.forward * forwardSpeed;
            rb.linearVelocity = new Vector3(forwardVel.x, rb.linearVelocity.y, forwardVel.z);
        }
        // в стане не перезаписываем velocity, чтобы сохранить knockback эффект
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isStunned) return;
        if (string.IsNullOrEmpty(obstacleTag)) return;

        if (CollisionHasTag(collision, obstacleTag))
        {
            StartStunWithKnockback();
        }
    }

    bool CollisionHasTag(Collision collision, string tagToCheck)
    {
        if (collision.collider != null && collision.collider.CompareTag(tagToCheck)) return true;
        if (collision.gameObject != null && collision.gameObject.CompareTag(tagToCheck)) return true;
        if (collision.transform != null && collision.transform.root != null && collision.transform.root.CompareTag(tagToCheck)) return true;
        foreach (var contact in collision.contacts)
        {
            if (contact.otherCollider != null && contact.otherCollider.CompareTag(tagToCheck)) return true;
            if (contact.thisCollider != null && contact.thisCollider.CompareTag(tagToCheck)) return true;
        }
        return false;
    }

    void StartStunWithKnockback()
    {
        isStunned = true;
        stunTimer = Mathf.Max(0f, stunDuration);
        Vector3 knockDir = -transform.forward.normalized;
        rb.AddForce(knockDir * knockbackForce, ForceMode.Impulse);
    }

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
