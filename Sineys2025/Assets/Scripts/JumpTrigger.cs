using System.Collections;
using UnityEngine;

public class JumpTrigger : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _jumpDuration = 0.5f;
    [SerializeField] private float _forwardJumpForce = 5f;
    [SerializeField] private AnimationCurve _jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

  
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PigController>() != null)
        {
            print(other.name);
            StartCoroutine(SmoothJumpUpOnly(other.transform));
        }
    }

    private IEnumerator SmoothJumpUpOnly(Transform target)
    {
        Vector3 startPosition = target.position;
        Vector3 endPosition = startPosition + Vector3.up * _jumpForce + target.transform.forward*_forwardJumpForce;
        float elapsedTime = 0f;

        while (elapsedTime < _jumpDuration)
        {
            float progress = elapsedTime / _jumpDuration;
            // SmoothStep для плавного старта и остановки
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            target.position = Vector3.Lerp(startPosition, endPosition, smoothProgress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Фиксируем конечную позицию
        target.position = endPosition;
    }
}
