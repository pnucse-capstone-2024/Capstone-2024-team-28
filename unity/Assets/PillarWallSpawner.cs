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
        // �����Ϳ����� �����ϴ� �ڵ�
#if UNITY_EDITOR

        // JSON ���� ��� ����
        string filePath = "C:\\Users\\admin\\Desktop\\graduationproj\\lidardata\\pillars_with_walls.json";
        string destinationPath = Path.Combine(Application.dataPath, "Resources/pillars_with_walls.json");
        File.Copy(filePath, destinationPath, true);
        Debug.Log("������ Resources ������ ���������� ����Ǿ����ϴ�: " + destinationPath);

        // Asset �����ͺ��̽� ���� (������ �������� �νĵǵ���)
        AssetDatabase.Refresh();
#endif
        TextAsset resourcesPath = Resources.Load<TextAsset>("pillars_with_walls");

        // JSON ���� �б�
        if (resourcesPath != null)
        {
            string jsonString = resourcesPath.text;
            PillarDataList pillarDataList = JsonUtility.FromJson<PillarDataList>(jsonString);

            // ��հ� �� ����
            Vector3 minPos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxPos = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = 0; i < pillarDataList.pillars.Count; i++)
            {
                PillarData pillarData = pillarDataList.pillars[i];

                // ��� ����
                Vector3 position = new Vector3(-pillarData.pillar.x, pillarData.pillar.y, pillarData.pillar.z);
                GameObject pillar = Instantiate(pillarPrefab, position, Quaternion.identity);
                pillar.transform.localScale = new Vector3(1, pillarData.pillar.height, 1);  // ��� ���� ����

                // �ּ� �� �ִ� ��ǥ ���
                minPos = Vector3.Min(minPos, position);
                maxPos = Vector3.Max(maxPos, position);

                // ����� ��� ���̿� �� ����
                foreach (Connection connection in pillarData.connections)
                {
                    PillarData nextPillar = pillarDataList.pillars[connection.to_pillar_index];
                    Vector3 nextPosition = new Vector3(-nextPillar.pillar.x, nextPillar.pillar.y, nextPillar.pillar.z);

                    // �� ��� ������ �߰� ��ġ ���
                    Vector3 midPoint = (position + nextPosition) / 2;

                    // �� ����
                    GameObject wall = Instantiate(wallPrefab, midPoint, Quaternion.identity);

                    // �� ��� �� �� ū ���̸� ����Ͽ� ���� ���̷� ����
                    float maxPillarHeight = Mathf.Max(pillarData.pillar.height, nextPillar.pillar.height);

                    // ���� ũ�� �� ȸ�� ����
                    float distance = Vector3.Distance(position, nextPosition);
                    wall.transform.localScale = new Vector3(connection.wall_thickness, maxPillarHeight * 2, distance); // ���� �β� �� ���� ����

                    // ���� �� ����� �����ϴ� ������ ������ ȸ��
                    wall.transform.LookAt(nextPosition);

                    // ���� �߽��� �� ��� ������ ���� �߰� ������ ���� ��ġ
                    float pillarMidHeight = (pillarData.pillar.height + nextPillar.pillar.height) / 2;
                    wall.transform.position = new Vector3(wall.transform.position.x, pillarMidHeight / 2, wall.transform.position.z);

                    // ���� �ϴ��� ���� Y������ ���������� ����
                    wall.transform.position -= new Vector3(0, maxPillarHeight / 2, 0);
                }
            }

            // �ٴ�(Plane) ����
                // Plane�� ��ġ�� ��յ� �� �ּ� y ��ǥ�� ���߰�, ũ��� ��յ��� ������ �°� ����
                Vector3 planePosition = new Vector3(-9, -10, -11);
                GameObject plane = Instantiate(planePrefab, planePosition, Quaternion.identity);

                // Plane�� ũ�� ����
                plane.transform.localScale = new Vector3(100, 1, 100);
        }
        else
        {
            Debug.LogError("JSON ������ ã�� �� �����ϴ�: " + resourcesPath);
        }
    }

    private void DeleteExistingObjects()
    {
        // ��� ����
        foreach (GameObject pillar in GameObject.FindGameObjectsWithTag("Pillar"))
        {
            Destroy(pillar);
        }

        // �� ����
        foreach (GameObject wall in GameObject.FindGameObjectsWithTag("Wall"))
        {
            Destroy(wall);
        }

        // �ٴ�(Plane) ����
        foreach (GameObject plane in GameObject.FindGameObjectsWithTag("Plane"))
        {
            Destroy(plane);
        }
    }
}
