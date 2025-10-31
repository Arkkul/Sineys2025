using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PigController : MonoBehaviour
{
    [Header("Motion")]
    [Tooltip("Постоянная скорость движения вперёд в единицах/сек.")]
    public float forwardSpeed = 5f;

    [Tooltip("Скорость поворота в градусах в секунду при удержании A/D.")]
    public float turnSpeedDegPerSec = 90f;

    [Header("Jump")]
    [Tooltip("Кнопка прыжка. Можно выбрать клавишу или Mouse0/Mouse1 и т.п.")]
    public KeyCode jumpKey = KeyCode.Space;

    [Tooltip("Сила прыжка (импульс), применяется по глобальной оси Y с ForceMode.Impulse.")]
    public float jumpForce = 6f;

    [Tooltip("Слой(ы), считающиеся 'землей' при проверке grounded. По умолчанию - все слои.")]
    public LayerMask groundLayerMask = ~0;

    [Tooltip("Дополнительное расстояние для проверки касания земли (в метрах).")]
    public float groundCheckExtra = 0.05f;

    [Header("Gizmos")]
    public bool drawGizmo = true;
    public float gizmoLength = 2f;
    public float gizmoHeadSize = 0.3f;
    public bool drawGroundCheckGizmo = true;
    public Color groundGizmoColor = Color.red;

    Rigidbody rb;
    Collider col;

    float turnInput;
    bool jumpRequested;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // Ввод поворота
        turnInput = 0f;
        if (Input.GetKey(KeyCode.A)) turnInput = -1f;
        if (Input.GetKey(KeyCode.D)) turnInput = 1f;

        // Ввод прыжка (фиксируем в Update)
        if (Input.GetKeyDown(jumpKey))
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        // Поворот через MoveRotation
        if (Mathf.Abs(turnInput) > 0f)
        {
            float deltaDeg = turnInput * turnSpeedDegPerSec * Time.fixedDeltaTime;
            Quaternion deltaRot = Quaternion.Euler(0f, deltaDeg, 0f);
            rb.MoveRotation(rb.rotation * deltaRot);
        }

        // Проверка "на земле" через Raycast вниз от центра коллайдера
        bool isGrounded = false;
        if (col != null)
        {
            float halfHeight = col.bounds.extents.y;
            float checkDistance = halfHeight + groundCheckExtra;
            Vector3 origin = transform.position;
            // Немного смещаем начало вниз чтобы корректно работать с Capsule/Box/Mesh
            origin.y += 0.01f;
            isGrounded = Physics.Raycast(origin, Vector3.down, checkDistance, groundLayerMask, QueryTriggerInteraction.Ignore);
        }

        // Прыжок: применяем импульс вверх в глобальной оси Y если запрошен и на земле
        if (jumpRequested && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        // Сброс флага (если прыжок не был применён — флаг всё равно сбрасываем, чтобы избежать повторов)
        jumpRequested = false;

        // Постоянная локальная скорость вперёд. Сохраняем вертикальную составляющую после прыжка.
        Vector3 forwardVel = transform.forward * forwardSpeed;
        rb.linearVelocity = new Vector3(forwardVel.x, rb.linearVelocity.y, forwardVel.z);
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

        // Рисуем линию ground check
        if (drawGroundCheckGizmo && col != null)
        {
            Gizmos.color = groundGizmoColor;
            float halfHeight = col.bounds.extents.y;
            float checkDistance = halfHeight + groundCheckExtra;
            Vector3 origin = transform.position;
            origin.y += 0.01f;
            Gizmos.DrawLine(origin, origin + Vector3.down * checkDistance);
            Gizmos.DrawSphere(origin + Vector3.down * checkDistance, 0.05f);
        }
    }
}
