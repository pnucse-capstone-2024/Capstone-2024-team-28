using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class PlayModeStateHandler
{
    static PlayModeStateHandler()
    {
        // Play ��� ��ȯ �� ȣ��Ǵ� �ݹ� ���
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            SceneSaver sceneSaver = Object.FindObjectOfType<SceneSaver>();
            if (sceneSaver != null)
            {
                sceneSaver.DeleteSavedSceneData(); // Play ��� ���� �� ���� JSON ���� ����
            }
            Debug.Log("Play ��尡 ���۵Ǿ����ϴ�.");
            // Play ��尡 ���۵� �� �ʿ��� �۾� ����
        }

        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("Play ��尡 ����� �����Դϴ�.");
            // Play ��� ���� ������ �� ���� ����
            GameObject sceneSaverObject = GameObject.Find("SceneSaver");
            if (sceneSaverObject != null)
            {
                SceneSaver sceneSaver = sceneSaverObject.GetComponent<SceneSaver>();
                sceneSaver.SaveSceneData();
            }
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            Debug.Log("Edit ���� ���ƿԽ��ϴ�.");
            // Edit ���� ���ƿ��� �� ���� ����
            GameObject sceneSaverObject = GameObject.Find("SceneSaver");
            if (sceneSaverObject != null)
            {
                SceneSaver sceneSaver = sceneSaverObject.GetComponent<SceneSaver>();
                sceneSaver.LoadSceneData();
            }
        }
    }
}