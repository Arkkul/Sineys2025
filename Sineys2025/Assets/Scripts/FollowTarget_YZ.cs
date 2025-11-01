using UnityEngine;

[AddComponentMenu("Movement/FollowTarget_YZ")]
public class FollowTarget_YZ : MonoBehaviour
{
    [Tooltip("Цель, за которой следует объект.")]
    public Transform target;

    [Tooltip("Скорость следования (ед./с).")]
    public float speed = 5f;

    [Tooltip("Если true — используем Rigidbody.MovePosition в FixedUpdate. Иначе — transform.position в Update.")]
    public bool useRigidbody = false;

    [Tooltip("Если true — движение плавное. Иначе мгновенное (MoveTowards).")]
    public bool smooth = true;

    [Tooltip("Время плавного сглаживания в секундах (только при smooth=true и useRigidbody=false).")]
    public float smoothTime = 0.12f;

    [Tooltip("Минимальное расстояние до цели по отслеживаемым осям при котором движение прекращается.")]
    public float stopDistance = 0.01f;

    [Header("Trigger Switch")]
    [Tooltip("Тег триггера, при входе в который переключается отслеживаемая ось Z <-> X (Y не меняется).")]
    public string switchTriggerTag = "SwitchZone";

    [Header("Gizmos")]
    public bool drawGizmos = true;
    public Color gizmoColor = Color.cyan;
    public float gizmoSphereSize = 0.05f;

    Rigidbody rb;
    Vector3 smoothVelocity;

    // internal: false = отслеживаем Z (по умолчанию), true = отслеживаем X
    bool useXInsteadOfZ = false;

    void Awake()
    {
        if (useRigidbody) rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (useRigidbody) return;
        if (target == null) return;

        Vector3 desired = useXInsteadOfZ ? ProjectToXY(transform.position, target.position) : ProjectToYZ(transform.position, target.position);

        if (IsWithinStopDistance(transform.position, desired)) return;

        if (smooth)
        {
            if (smoothTime <= 0f) transform.position = desired;
            else transform.position = Vector3.SmoothDamp(transform.position, desired, ref smoothVelocity, smoothTime, Mathf.Infinity, Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, desired, speed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (!useRigidbody) return;
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (target == null) return;

        Vector3 desired = useXInsteadOfZ ? ProjectToXY(rb.position, target.position) : ProjectToYZ(rb.position, target.position);

        if (IsWithinStopDistance(rb.position, desired)) return;

        Vector3 newPos = Vector3.MoveTowards(rb.position, desired, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    static Vector3 ProjectToYZ(Vector3 selfPos, Vector3 targetPos)
    {
        return new Vector3(selfPos.x, targetPos.y, targetPos.z);
    }

    static Vector3 ProjectToXY(Vector3 selfPos, Vector3 targetPos)
    {
        return new Vector3(targetPos.x, targetPos.y, selfPos.z);
    }

    bool IsWithinStopDistance(Vector3 current, Vector3 desired)
    {
        if (!useXInsteadOfZ)
        {
            float dy = current.y - desired.y;
            float dz = current.z - desired.z;
            return (dy * dy + dz * dz) <= stopDistance * stopDistance;
        }
        else
        {
            float dx = current.x - desired.x;
            float dy = current.y - desired.y;
            return (dx * dx + dy * dy) <= stopDistance * stopDistance;
        }
    }

    // Переключатель: каждый вход в триггер с нужным тегом инвертирует режим
    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (string.IsNullOrEmpty(switchTriggerTag)) return;
        if (other.CompareTag(switchTriggerTag))
        {
            useXInsteadOfZ = !useXInsteadOfZ;
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (target == null) return;

        Gizmos.color = gizmoColor;
        Vector3 from = transform.position;
        Vector3 to = useXInsteadOfZ ? ProjectToXY(from, target.position) : ProjectToYZ(from, target.position);
        Gizmos.DrawLine(from, to);
        Gizmos.DrawSphere(to, gizmoSphereSize);

        // Индикатор текущего режима в сцене
        GUIStyle style = new GUIStyle();
        // простая визуальная подсказка через цвет сферы
        Gizmos.color = useXInsteadOfZ ? Color.magenta : Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.06f);
    }
}
