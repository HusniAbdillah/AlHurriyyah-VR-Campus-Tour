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
        // Subscribe to input events if using new Input System
        if (Mouse.current != null)
        {
            Mouse.current.delta.ReadValue();
        }
    }

    void Update()
    {
        // New Input System compatible
        Vector2 mouseInput = Vector2.zero;
        
        if (Mouse.current != null)
        {
            mouseInput = Mouse.current.delta.ReadValue();
        }
        else
        {
            // Fallback to legacy input
            try
            {
                mouseInput.x = Input.GetAxis("Mouse X");
                mouseInput.y = Input.GetAxis("Mouse Y");
            }
            catch
            {
                // Input system not available
                return;
            }
        }

        yaw += mouseInput.x * rotationSpeed * Time.deltaTime * 60f; // Frame rate independent
        pitch -= mouseInput.y * rotationSpeed * Time.deltaTime * 60f;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }
}