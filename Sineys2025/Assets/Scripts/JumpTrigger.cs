using UnityEngine;

public class JumpTrigger : MonoBehaviour
{
    [SerializeField] private float _jumpForce = 5f;

    private void OnTriggerEnter(Collider other)
    {
        
        if(other.tag == "Player")
        {
            print(other.name);
            other.GetComponent<Rigidbody>().AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }
}
