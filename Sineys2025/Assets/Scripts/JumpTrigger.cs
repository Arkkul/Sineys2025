using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class JumpTrigger : MonoBehaviour
{
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _jumpDuration = 0.5f;
    [SerializeField] private float _forwardJumpForce = 5f;
    [SerializeField] private AnimationCurve _jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PigController>() != null)
        {
            print(other.name);
            StartCoroutine(SmoothJumpCoroutine(other.transform));
        }
    }

    private IEnumerator SmoothJumpCoroutine(Transform target)
    {
        Vector3 startPosition = target.position;
        Vector3 endPosition = startPosition + Vector3.up * _jumpForce +transform.forward* _forwardJumpForce;
        float elapsedTime = 0f;

        while (elapsedTime < _jumpDuration)
        {
            float progress = elapsedTime / _jumpDuration;
            float curveValue = _jumpCurve.Evaluate(progress);

            target.position = Vector3.Lerp(startPosition, endPosition, curveValue);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.position = endPosition;
    }
}
