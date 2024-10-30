using UnityEngine;
using TMPro;  // TMP ���̺귯�� ���

public class DistanceText : MonoBehaviour
{
    public TextMeshProUGUI positionText;  // Canvas ���� TMP Text ����
    private Vector3 initialPosition;

    void Start()
    {
        // �ʱ� ��ġ ����
        initialPosition = transform.position;
    }

    void Update()
    {
        // ���� ��ġ���� �ʱ� ��ġ������ �Ÿ� ��� (X�� Z ��ǥ�� �̿�)
        Vector3 currentPosition = transform.position;
        float distance = Vector2.Distance(new Vector2(initialPosition.x, initialPosition.z),
                                          new Vector2(currentPosition.x, currentPosition.z));

        // X, Z ��ǥ�� �Ÿ� TMP�� ǥ��
        positionText.text = string.Format("X: {0:F2}, Z: {1:F2}\nDistance: {2:F2}",
                                          currentPosition.x, currentPosition.z, distance);
    }
}

