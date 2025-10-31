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

    [Tooltip("Минимальное расстояние до цели по YZ при котором движение прекращается.")]
    public float stopDistance = 0.01f;

    [Header("Gizmos")]
    public bool drawGizmos = true;
    public Color gizmoColor = Color.cyan;
    public float gizmoSphereSize = 0.05f;

    Rigidbody rb;
    Vector3 smoothVelocity;

    void Awake()
    {
        if (useRigidbody) rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (useRigidbody) return; // логика Rigidbody в FixedUpdate

        if (target == null) return;

        Vector3 desired = ProjectToYZ(transform.position, target.position);

        if (Vector2.SqrMagnitude(new Vector2(transform.position.z - desired.z, transform.position.y - desired.y)) <= stopDistance * stopDistance)
            return;

        if (smooth)
        {
            // SmoothDamp в world space. smoothTime=0 ведёт к мгновенному перемещению.
            if (smoothTime <= 0f)
                transform.position = desired;
            else
                transform.position = Vector3.SmoothDamp(transform.position, desired, ref smoothVelocity, smoothTime, Mathf.Infinity, Time.deltaTime);
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

        Vector3 desired = ProjectToYZ(rb.position, target.position);

        if (Vector2.SqrMagnitude(new Vector2(rb.position.z - desired.z, rb.position.y - desired.y)) <= stopDistance * stopDistance)
            return;

        // Для Rigidbody используем MovePosition. Smooth реализован через MoveTowards по FixedDeltaTime.
        if (smooth)
        {
            // Можно заменить на более сложный сглаживатель при необходимости.
            Vector3 newPos = Vector3.MoveTowards(rb.position, desired, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }
        else
        {
            Vector3 newPos = Vector3.MoveTowards(rb.position, desired, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }
    }

    // Проецирует цель на плоскость YZ сохраняя текущий X
    static Vector3 ProjectToYZ(Vector3 selfPos, Vector3 targetPos)
    {
        return new Vector3(selfPos.x, targetPos.y, targetPos.z);
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (target == null) return;

        Gizmos.color = gizmoColor;
        Vector3 from = transform.position;
        Vector3 to = ProjectToYZ(from, target.position);
        Gizmos.DrawLine(from, to);
        Gizmos.DrawSphere(to, gizmoSphereSize);
    }
}
