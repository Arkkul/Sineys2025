using UnityEngine;

public class BirdInput : MonoBehaviour
{
    private InputSystem_Actions _input;
    private BirdSinging _birdSinging;

    private void Start()
    {
        Debug.Log("dv");
    }

    private void Awake()
    {
        _input = new InputSystem_Actions();
        _birdSinging = GetComponent<BirdSinging>();
        Debug.Log(_input);
        
    }

    private void SendNote(string note)
    {
        _birdSinging.Sing(note);
    }

    private void D_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        print("qsdf");
        SendNote("D");
    }

    private void C_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        SendNote("C");
    }

    private void B_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        SendNote("B");
    }

    private void A_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        SendNote("A");
    }

    private void OnEnable()
    {
        _input.Enable();

        _input.Bird.A.performed += A_performed;
        _input.Bird.B.performed += B_performed;
        _input.Bird.C.performed += C_performed;
        _input.Bird.D.performed += D_performed;
    }

    private void OnDisable()
    {
        _input.Disable();

        _input.Bird.A.performed -= A_performed;
        _input.Bird.B.performed -= B_performed;
        _input.Bird.C.performed -= C_performed;
        _input.Bird.D.performed -= D_performed;
    }


}
