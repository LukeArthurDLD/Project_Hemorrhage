using UnityEngine;

public class CameraController : MonoBehaviour
{
    //CameraMovement
    public float horizontalSensitivity, verticalSensitivity;

    public Transform orientation;

    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        MyInput();
    }
    void MyInput()
    {
        //mouseInput
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * horizontalSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * verticalSensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

    }
}
