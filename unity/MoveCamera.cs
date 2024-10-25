using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;

    void Update()
    {
        // 카메라 이동 (WASD 키만)
        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            movement += transform.forward; // W: 전방 이동
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement -= transform.forward; // S: 후방 이동
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement -= transform.right;   // A: 좌측 이동
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement += transform.right;   // D: 우측 이동
        }

        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        // 마우스로 카메라 회전
        if (Input.GetMouseButton(0)) // 마우스 왼쪽 버튼 클릭
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;
            transform.Rotate(-mouseY, mouseX, 0);
        }
    }
}


