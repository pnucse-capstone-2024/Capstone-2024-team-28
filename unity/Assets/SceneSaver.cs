using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class PillarInfo
{
    public Vector3 position;
    public float height;
}

[System.Serializable]
public class WallInfo
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public float thickness;
    public float height;
}

[System.Serializable]
public class PlaneInfo
{
    public Vector3 position;
    public Vector3 scale;
}

[System.Serializable]
public class SceneData
{
    public List<PillarInfo> pillars;
    public List<WallInfo> walls;
    public PlaneInfo plane;
}

public class SceneSaver : MonoBehaviour
{
    private SceneData sceneData = new SceneData();

    // �� �����͸� JSON���� �����ϴ� �Լ�
    public void SaveSceneData()
    {
        // ��� �� �� �����͸� ����
        GameObject[] pillars = GameObject.FindGameObjectsWithTag("Pillar");
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        GameObject plane = GameObject.FindGameObjectWithTag("Plane");

        sceneData.pillars = new List<PillarInfo>();
        sceneData.walls = new List<WallInfo>();

        // ��� ���� ����
        foreach (GameObject pillar in pillars)
        {
            PillarInfo info = new PillarInfo
            {
                position = pillar.transform.position,
                height = pillar.transform.localScale.y // ����� ����
            };
            sceneData.pillars.Add(info);
        }

        // �� ���� ����
        foreach (GameObject wall in walls)
        {
            // ���� �� ���� ���� ��� (���� ����� ���̸� ���)
            Vector3 wallDirection = wall.transform.forward; // ���� ���ϴ� ����
            float wallLength = wall.transform.localScale.z;  // ���� ����
            Vector3 startPosition = wall.transform.position - (wallDirection * wallLength / 2); // ������
            Vector3 endPosition = wall.transform.position + (wallDirection * wallLength / 2);   // ����

            WallInfo info = new WallInfo
            {
                startPosition = startPosition,
                endPosition = endPosition,
                thickness = wall.transform.localScale.x,  // �� �β�
                height = wall.transform.localScale.y      // �� ���� �߰�
            };
            sceneData.walls.Add(info);
        }

        // Plane ���� ����
        if (plane != null)
        {
            sceneData.plane = new PlaneInfo
            {
                position = plane.transform.position,
                scale = plane.transform.localScale
            };
        }

        // JSON���� ��ȯ�Ͽ� ���Ϸ� ����
        string json = JsonUtility.ToJson(sceneData, true);
        string path = Path.Combine(Application.persistentDataPath, "sceneData.json");
        File.WriteAllText(path, json);

        PlayerPrefs.SetString("SavedSceneData", json);
        PlayerPrefs.Save();

        Debug.Log("�� ������ ���� �Ϸ�: " + path);
    }

    // ����� �� �����͸� �ҷ����� �Լ�
    public void LoadSceneData()
    {
        string path = Path.Combine(Application.persistentDataPath, "sceneData.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            sceneData = JsonUtility.FromJson<SceneData>(json);

            // ���� ���, �� �� Plane ����
            foreach (GameObject pillar in GameObject.FindGameObjectsWithTag("Pillar"))
            {
                DestroyImmediate(pillar); // �����Ϳ��� ��� �ı�
            }

            foreach (GameObject wall in GameObject.FindGameObjectsWithTag("Wall"))
            {
                DestroyImmediate(wall); // �����Ϳ��� ��� �ı�
            }

            GameObject plane = GameObject.FindGameObjectWithTag("Plane");
            if (plane != null)
            {
                DestroyImmediate(plane);
            }

            // ��� �����
            foreach (PillarInfo info in sceneData.pillars)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.tag = "Pillar";
                pillar.transform.position = info.position;
                pillar.transform.localScale = new Vector3(1, info.height, 1);
            }

            // �� �����
            foreach (WallInfo info in sceneData.walls)
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.tag = "Wall";

                Vector3 midPoint = (info.startPosition + info.endPosition) / 2;
                wall.transform.position = midPoint;

                // �� ũ�� �� ���� ���� (���̵� ����)
                float length = Vector3.Distance(info.startPosition, info.endPosition);
                wall.transform.localScale = new Vector3(info.thickness, info.height, length); // �� ���� �ݿ�
                wall.transform.LookAt(info.endPosition); // ���� �ùٸ� ������ �ٶ󺸵��� ����
            }

            // Plane �����
            if (sceneData.plane != null)
            {
                GameObject newPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                newPlane.tag = "Plane";
                newPlane.transform.position = sceneData.plane.position;
                newPlane.transform.localScale = sceneData.plane.scale;
            }

            Debug.Log("�� ������ �ε� �Ϸ�");
        }
    }

    // ���� JSON ������ �����ϴ� �Լ�
    public void DeleteSavedSceneData()
    {
        string path = Path.Combine(Application.persistentDataPath, "sceneData.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("���� �� ������ ���� ���� �Ϸ�: " + path);
        }
        else
        {
            Debug.LogWarning("������ JSON ������ �����ϴ�.");
        }
    }
}
