using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerEnableThenDisable : MonoBehaviour
{
    [Tooltip("Тег входящего коллайдера (объект без isTrigger).")]
    public string requiredTag = "Player";

    [Tooltip("Сначала включить этот GameObject (SetActive(true)).")]
    public GameObject enableObject;

    [Tooltip("Затем выключить этот GameObject (SetActive(false)).")]
    public GameObject disableObject;

    void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }

    void Awake()
    {
        var c = GetComponent<Collider>();
        if (c == null || !c.isTrigger)
            Debug.LogWarning($"{name}: Компонент Collider отсутствует или Is Trigger=false. Этот скрипт предназначен для триггер-коллайдера.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // Требуем, чтобы входящий коллайдер был НЕ триггером.
        if (other.isTrigger) return;

        // Требуем совпадение тега.
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        // Сначала включаем первый объект.
        if (enableObject != null)
            enableObject.SetActive(true);
        else
            Debug.LogWarning($"{name}: enableObject не задан.");

        // Затем выключаем второй объект.
        if (disableObject != null)
            disableObject.SetActive(false);
        else
            Debug.LogWarning($"{name}: disableObject не задан.");
    }
}
