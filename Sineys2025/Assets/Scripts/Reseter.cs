using UnityEngine;
using UnityEngine.SceneManagement;

public class Reseter : MonoBehaviour
{
    [SerializeField] private string _sceneName;

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PigController>() != null)
        {
            SceneManager.LoadScene(_sceneName);
        }
    }
}
