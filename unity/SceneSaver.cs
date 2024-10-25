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

    // 씬 데이터를 JSON으로 저장하는 함수
    public void SaveSceneData()
    {
        // 기둥 및 벽 데이터를 수집
        GameObject[] pillars = GameObject.FindGameObjectsWithTag("Pillar");
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        GameObject plane = GameObject.FindGameObjectWithTag("Plane");

        sceneData.pillars = new List<PillarInfo>();
        sceneData.walls = new List<WallInfo>();

        // 기둥 정보 수집
        foreach (GameObject pillar in pillars)
        {
            PillarInfo info = new PillarInfo
            {
                position = pillar.transform.position,
                height = pillar.transform.localScale.y // 기둥의 높이
            };
            sceneData.pillars.Add(info);
        }

        // 벽 정보 수집
        foreach (GameObject wall in walls)
        {
            // 벽의 두 점을 직접 계산 (벽의 방향과 길이를 사용)
            Vector3 wallDirection = wall.transform.forward; // 벽이 향하는 방향
            float wallLength = wall.transform.localScale.z;  // 벽의 길이
            Vector3 startPosition = wall.transform.position - (wallDirection * wallLength / 2); // 시작점
            Vector3 endPosition = wall.transform.position + (wallDirection * wallLength / 2);   // 끝점

            WallInfo info = new WallInfo
            {
                startPosition = startPosition,
                endPosition = endPosition,
                thickness = wall.transform.localScale.x,  // 벽 두께
                height = wall.transform.localScale.y      // 벽 높이 추가
            };
            sceneData.walls.Add(info);
        }

        // Plane 정보 수집
        if (plane != null)
        {
            sceneData.plane = new PlaneInfo
            {
                position = plane.transform.position,
                scale = plane.transform.localScale
            };
        }

        // JSON으로 변환하여 파일로 저장
        string json = JsonUtility.ToJson(sceneData, true);
        string path = Path.Combine(Application.persistentDataPath, "sceneData.json");
        File.WriteAllText(path, json);

        PlayerPrefs.SetString("SavedSceneData", json);
        PlayerPrefs.Save();

        Debug.Log("씬 데이터 저장 완료: " + path);
    }

    // 저장된 씬 데이터를 불러오는 함수
    public void LoadSceneData()
    {
        string path = Path.Combine(Application.persistentDataPath, "sceneData.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            sceneData = JsonUtility.FromJson<SceneData>(json);

            // 기존 기둥, 벽 및 Plane 삭제
            foreach (GameObject pillar in GameObject.FindGameObjectsWithTag("Pillar"))
            {
                DestroyImmediate(pillar); // 에디터에서 즉시 파괴
            }

            foreach (GameObject wall in GameObject.FindGameObjectsWithTag("Wall"))
            {
                DestroyImmediate(wall); // 에디터에서 즉시 파괴
            }

            GameObject plane = GameObject.FindGameObjectWithTag("Plane");
            if (plane != null)
            {
                DestroyImmediate(plane);
            }

            // 기둥 재생성
            foreach (PillarInfo info in sceneData.pillars)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.tag = "Pillar";
                pillar.transform.position = info.position;
                pillar.transform.localScale = new Vector3(1, info.height, 1);
            }

            // 벽 재생성
            foreach (WallInfo info in sceneData.walls)
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.tag = "Wall";

                Vector3 midPoint = (info.startPosition + info.endPosition) / 2;
                wall.transform.position = midPoint;

                // 벽 크기 및 방향 설정 (높이도 포함)
                float length = Vector3.Distance(info.startPosition, info.endPosition);
                wall.transform.localScale = new Vector3(info.thickness, info.height, length); // 벽 높이 반영
                wall.transform.LookAt(info.endPosition); // 벽이 올바른 방향을 바라보도록 설정
            }

            // Plane 재생성
            if (sceneData.plane != null)
            {
                GameObject newPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                newPlane.tag = "Plane";
                newPlane.transform.position = sceneData.plane.position;
                newPlane.transform.localScale = sceneData.plane.scale;
            }

            Debug.Log("씬 데이터 로드 완료");
        }
    }

    // 기존 JSON 파일을 삭제하는 함수
    public void DeleteSavedSceneData()
    {
        string path = Path.Combine(Application.persistentDataPath, "sceneData.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("기존 씬 데이터 파일 삭제 완료: " + path);
        }
        else
        {
            Debug.LogWarning("삭제할 JSON 파일이 없습니다.");
        }
    }
}
