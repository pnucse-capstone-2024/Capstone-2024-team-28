using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class PlayModeStateHandler
{
    static PlayModeStateHandler()
    {
        // Play 모드 전환 시 호출되는 콜백 등록
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            SceneSaver sceneSaver = Object.FindObjectOfType<SceneSaver>();
            if (sceneSaver != null)
            {
                sceneSaver.DeleteSavedSceneData(); // Play 모드 시작 시 기존 JSON 파일 삭제
            }
            Debug.Log("Play 모드가 시작되었습니다.");
            // Play 모드가 시작될 때 필요한 작업 수행
        }

        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("Play 모드가 종료될 예정입니다.");
            // Play 모드 종료 직전에 씬 상태 저장
            GameObject sceneSaverObject = GameObject.Find("SceneSaver");
            if (sceneSaverObject != null)
            {
                SceneSaver sceneSaver = sceneSaverObject.GetComponent<SceneSaver>();
                sceneSaver.SaveSceneData();
            }
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            Debug.Log("Edit 모드로 돌아왔습니다.");
            // Edit 모드로 돌아오면 씬 상태 복원
            GameObject sceneSaverObject = GameObject.Find("SceneSaver");
            if (sceneSaverObject != null)
            {
                SceneSaver sceneSaver = sceneSaverObject.GetComponent<SceneSaver>();
                sceneSaver.LoadSceneData();
            }
        }
    }
}