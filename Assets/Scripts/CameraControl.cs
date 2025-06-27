using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour
{
    public float rotationSpeed = 5f;
    
    private float yaw = 0f;
    private float pitch = 0f;
    private Vector2 mouseDelta;
    
    private void OnEnable()
    {
        if (Mouse.current != null)
        {
            Mouse.current.delta.ReadValue();
        }
    }

    void Update()
    {
        Vector2 mouseInput = Vector2.zero;
        
        if (Mouse.current != null)
        {
            mouseInput = Mouse.current.delta.ReadValue();
        }
        else
        {
            try
            {
                mouseInput.x = Input.GetAxis("Mouse X");
                mouseInput.y = Input.GetAxis("Mouse Y");
            }
            catch
            {
                return;
            }
        }

        yaw += mouseInput.x * rotationSpeed * Time.deltaTime * 60f;
        pitch -= mouseInput.y * rotationSpeed * Time.deltaTime * 60f;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }
}