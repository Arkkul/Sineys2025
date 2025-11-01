using Unity.VisualScripting;
using UnityEngine;

public class JumpTrigger : MonoBehaviour
{
    [SerializeField] private float _jumpForce = 5f;

    private void OnTriggerEnter(Collider other)
    {
        
        if(other.GetComponent<PigController>() != null)
        {
            print(other.name);
            //other.GetComponent<Rigidbody>().AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y + _jumpForce, other.transform.position.z);
        }
    }
}
