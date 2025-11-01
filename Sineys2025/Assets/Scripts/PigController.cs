using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PigController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [Header("Motion")]
    public float forwardSpeed = 5f;
    public float turnSpeedDegPerSec = 90f;

    [Header("Input (New Input System)")]
    [Tooltip("Action типа Value -> Axis (float). Bind A = -1, D = +1, also gamepad stick X.")]
    public InputActionReference turnAction; // assign in inspector

    [Header("Jump (trigger)")]
    public string jumpTriggerTag = "JumpPad";
    public float jumpForce = 6f;

    [Header("Stun / Knockback (front check)")]
    [Tooltip("Тег препятствия. Только объекты с этим тегом будут вызывать стан/отталкивание.")]
    public string obstacleTag = "Obstacle";

    [Tooltip("Сила отбрасывания (импульс) при столкновении.")]
    public float knockbackForce = 8f;

    [Tooltip("Длительность оглушения в секундах.")]
    public float stunDuration = 1.0f;

    [Tooltip("Длина фронтальной зоны проверки (вперед от центра).")]
    public float frontCheckLength = 1.2f;

    [Tooltip("Ширина фронтальной зоны (по локальной X).")]
    public float frontCheckWidth = 1.0f;

    [Tooltip("Вертикальная высота зоны (по локальной Y). Обычно равна высоте коллайдера).")]
    public float frontCheckHeight = 1.0f;

    [Tooltip("Вертикальный отступ начала зоны от позиции объекта (вверх положительное, вниз отрицательное).\n" +
             "Нижняя грань зоны будет расположена в transform.position + transform.up * frontCheckYOffset.")]
    public float frontCheckYOffset = 0f;

    [Tooltip("Смещение зоны вперёд от позиции объекта (чтобы зона начиналась перед носом).")]
    public float frontOffset = 0.2f;

    [Header("Gizmos")]
    public bool drawGizmo = true;
    public Color gizmoColor = new Color(1f, 0.5f, 0f, 0.7f); // оранжевый прозрачный
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

        // Если height не задан явно, взять из коллайдера
        if (frontCheckHeight <= 0f && col != null)
            frontCheckHeight = Mathf.Max(0.1f, col.bounds.size.y);
    }

    public void Jump()
    {
       // _animator.SetTrigger("Jump");
        _animator.SetBool("Jump2", true);
    }

    public void UnJump()
    {
        //_animator.SetTrigger("Jump");
        _animator.SetBool("Jump2", false);
    }

    void OnEnable()
    {
        if (turnAction != null && turnAction.action != null)
            turnAction.action.Enable();
    }

    void OnDisable()
    {
        if (turnAction != null && turnAction.action != null)
            turnAction.action.Disable();
    }

    void Update()
    {
        if (turnAction != null && turnAction.action != null)
        {
            turnInput = turnAction.action.ReadValue<float>();
        }
        else
        {
            // fallback на клавиатуру
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
            _animator.SetBool("Jiggle", true);
            stunTimer -= Time.fixedDeltaTime;
            if (stunTimer <= 0f)
                isStunned = false;
        }

        // Проверяем фронтальную зону только если не в стане
        if (!isStunned)
        {
            _animator.SetBool("Jiggle", false);
            if (CheckFrontObstacle())
                StartStunWithKnockback();
        }

        // Поворот (только вне стана)
        if (!isStunned && Mathf.Abs(turnInput) > 0.001f)
        {
            float deltaDeg = turnInput * turnSpeedDegPerSec * Time.fixedDeltaTime;
            Quaternion deltaRot = Quaternion.Euler(0f, deltaDeg, 0f);
            rb.MoveRotation(rb.rotation * deltaRot);
        }

        // Постоянное движение вперёд (выключено в стане)
        if (!isStunned)
        {
            Vector3 forwardVel = transform.forward * forwardSpeed;
            rb.linearVelocity = new Vector3(forwardVel.x, rb.linearVelocity.y, forwardVel.z);
        }
        // если в стане — rb.velocity не перезаписываем, чтобы сохранить knockback эффект
    }

    bool CheckFrontObstacle()
    {
        if (string.IsNullOrEmpty(obstacleTag)) return false;
        if (col == null) return false;

        // Нижняя грань зоны: позиция + vertical offset
        Vector3 bottomPoint = transform.position + transform.up * frontCheckYOffset;

        // Центр зоны = нижняя грань + половина высоты + смещение вперёд (half depth)
        Vector3 center = bottomPoint +
                         transform.up * (frontCheckHeight * 0.5f) +
                         transform.forward * (frontOffset + frontCheckLength * 0.5f);

        Vector3 halfExtents = new Vector3(frontCheckWidth * 0.5f, frontCheckHeight * 0.5f, frontCheckLength * 0.5f);

        // Собираем все коллайдеры в зоне. QueryTriggerInteraction.Ignore чтобы не срабатывать на триггеры.
        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation, ~0, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return false;

        foreach (var h in hits)
        {
            if (h == null) continue;
            if (h.gameObject == this.gameObject) continue;
            if (h.CompareTag(obstacleTag) || h.gameObject.CompareTag(obstacleTag) || h.transform.root.CompareTag(obstacleTag))
                return true;
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
        if (transform == null) return;

        // Нижняя грань зоны
        Vector3 bottomPoint = transform.position + transform.up * frontCheckYOffset;

        // центр зоны для отрисовки
        Vector3 center = bottomPoint +
                         transform.up * (frontCheckHeight * 0.5f) +
                         transform.forward * (frontOffset + frontCheckLength * 0.5f);
        Vector3 halfExtents = new Vector3(frontCheckWidth * 0.5f, frontCheckHeight * 0.5f, frontCheckLength * 0.5f);

        Gizmos.color = gizmoColor;
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
        Gizmos.matrix = old;

        // рисуем точку начала зоны
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(bottomPoint, 0.03f);

        // состояние стана
        if (isStunned)
        {
            Gizmos.color = stunGizmoColor;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.35f);
        }
    }
}
