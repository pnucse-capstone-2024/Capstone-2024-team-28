using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Connection
{
    public int to_pillar_index;
    public float wall_thickness;
}

[System.Serializable]
public class Pillar
{
    public float x;
    public float y;
    public float z;
    public float height;
}

[System.Serializable]
public class PillarData
{
    public Pillar pillar;
    public List<Connection> connections;
}

[System.Serializable]
public class PillarDataList
{
    public List<PillarData> pillars;
}

public class PillarWallSpawner : MonoBehaviour
{
    public GameObject pillarPrefab;
    public GameObject wallPrefab;
    public GameObject planePrefab;

    void Start()
    {
        DeleteExistingObjects();
        // 에디터에서만 동작하는 코드
#if UNITY_EDITOR

        // JSON 파일 경로 설정
        string filePath = "C:\\Users\\admin\\Desktop\\graduationproj\\lidardata\\pillars_with_walls.json";
        string destinationPath = Path.Combine(Application.dataPath, "Resources/pillars_with_walls.json");
        File.Copy(filePath, destinationPath, true);
        Debug.Log("파일이 Resources 폴더로 성공적으로 복사되었습니다: " + destinationPath);

        // Asset 데이터베이스 갱신 (파일이 에셋으로 인식되도록)
        AssetDatabase.Refresh();
#endif
        TextAsset resourcesPath = Resources.Load<TextAsset>("pillars_with_walls");

        // JSON 파일 읽기
        if (resourcesPath != null)
        {
            string jsonString = resourcesPath.text;
            PillarDataList pillarDataList = JsonUtility.FromJson<PillarDataList>(jsonString);

            // 기둥과 벽 생성
            Vector3 minPos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxPos = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = 0; i < pillarDataList.pillars.Count; i++)
            {
                PillarData pillarData = pillarDataList.pillars[i];

                // 기둥 생성
                Vector3 position = new Vector3(-pillarData.pillar.x, pillarData.pillar.y, pillarData.pillar.z);
                GameObject pillar = Instantiate(pillarPrefab, position, Quaternion.identity);
                pillar.transform.localScale = new Vector3(1, pillarData.pillar.height, 1);  // 기둥 높이 설정

                // 최소 및 최대 좌표 계산
                minPos = Vector3.Min(minPos, position);
                maxPos = Vector3.Max(maxPos, position);

                // 연결된 기둥 사이에 벽 생성
                foreach (Connection connection in pillarData.connections)
                {
                    PillarData nextPillar = pillarDataList.pillars[connection.to_pillar_index];
                    Vector3 nextPosition = new Vector3(-nextPillar.pillar.x, nextPillar.pillar.y, nextPillar.pillar.z);

                    // 두 기둥 사이의 중간 위치 계산
                    Vector3 midPoint = (position + nextPosition) / 2;

                    // 벽 생성
                    GameObject wall = Instantiate(wallPrefab, midPoint, Quaternion.identity);

                    // 두 기둥 중 더 큰 높이를 계산하여 벽의 높이로 설정
                    float maxPillarHeight = Mathf.Max(pillarData.pillar.height, nextPillar.pillar.height);

                    // 벽의 크기 및 회전 설정
                    float distance = Vector3.Distance(position, nextPosition);
                    wall.transform.localScale = new Vector3(connection.wall_thickness, maxPillarHeight * 2, distance); // 벽의 두께 및 길이 설정

                    // 벽이 두 기둥을 연결하는 방향을 보도록 회전
                    wall.transform.LookAt(nextPosition);

                    // 벽의 중심을 두 기둥 사이의 높이 중간 지점에 맞춰 배치
                    float pillarMidHeight = (pillarData.pillar.height + nextPillar.pillar.height) / 2;
                    wall.transform.position = new Vector3(wall.transform.position.x, pillarMidHeight / 2, wall.transform.position.z);

                    // 벽의 하단이 음수 Y축으로 내려가도록 설정
                    wall.transform.position -= new Vector3(0, maxPillarHeight / 2, 0);
                }
            }

            // 바닥(Plane) 생성
                // Plane의 위치를 기둥들 중 최소 y 좌표에 맞추고, 크기는 기둥들의 범위에 맞게 설정
                Vector3 planePosition = new Vector3(-9, -10, -11);
                GameObject plane = Instantiate(planePrefab, planePosition, Quaternion.identity);

                // Plane의 크기 설정
                plane.transform.localScale = new Vector3(100, 1, 100);
        }
        else
        {
            Debug.LogError("JSON 파일을 찾을 수 없습니다: " + resourcesPath);
        }
    }

    private void DeleteExistingObjects()
    {
        // 기둥 삭제
        foreach (GameObject pillar in GameObject.FindGameObjectsWithTag("Pillar"))
        {
            Destroy(pillar);
        }

        // 벽 삭제
        foreach (GameObject wall in GameObject.FindGameObjectsWithTag("Wall"))
        {
            Destroy(wall);
        }

        // 바닥(Plane) 삭제
        foreach (GameObject plane in GameObject.FindGameObjectsWithTag("Plane"))
        {
            Destroy(plane);
        }
    }
}
