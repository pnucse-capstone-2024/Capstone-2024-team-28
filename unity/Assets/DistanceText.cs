using UnityEngine;
using TMPro;  // TMP 라이브러리 사용

public class DistanceText : MonoBehaviour
{
    public TextMeshProUGUI positionText;  // Canvas 상의 TMP Text 연결
    private Vector3 initialPosition;

    void Start()
    {
        // 초기 위치 설정
        initialPosition = transform.position;
    }

    void Update()
    {
        // 현재 위치에서 초기 위치까지의 거리 계산 (X와 Z 좌표만 이용)
        Vector3 currentPosition = transform.position;
        float distance = Vector2.Distance(new Vector2(initialPosition.x, initialPosition.z),
                                          new Vector2(currentPosition.x, currentPosition.z));

        // X, Z 좌표와 거리 TMP에 표시
        positionText.text = string.Format("X: {0:F2}, Z: {1:F2}\nDistance: {2:F2}",
                                          currentPosition.x, currentPosition.z, distance);
    }
}

