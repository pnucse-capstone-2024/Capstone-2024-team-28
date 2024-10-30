using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;

    void Update()
    {
        // ī�޶� �̵� (WASD Ű��)
        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            movement += transform.forward; // W: ���� �̵�
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement -= transform.forward; // S: �Ĺ� �̵�
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement -= transform.right;   // A: ���� �̵�
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement += transform.right;   // D: ���� �̵�
        }

        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        // ���콺�� ī�޶� ȸ��
        if (Input.GetMouseButton(0)) // ���콺 ���� ��ư Ŭ��
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;
            transform.Rotate(-mouseY, mouseX, 0);
        }
    }
}


