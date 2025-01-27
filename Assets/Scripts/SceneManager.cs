using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    // ����ָ������
    public static SceneManagerScript instance;
    public string scene;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // ����Ѿ�����ʵ�������ٵ�ǰ����
            return;
        }

        instance = this;
    }
    public void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            Debug.Log($"Scene '{sceneName}' is loading...");
        }
        else
        {
            Debug.LogWarning("Scene name is empty or null!");
        }

       scene = sceneName;

        if (scene == "Cave")
        {

            StartCoroutine(GameManager.Instance.ChangeMusic(GameManager.Instance.BGM[2], 0.2f));
        }
        else
        {
            StartCoroutine(GameManager.Instance.ChangeMusic(GameManager.Instance.BGM[0], 0.05f));
        }
    }

    // ���ص�ǰ����
    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
        Debug.Log($"Reloading current scene: {currentSceneName}");
    }

    // ������һ����
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
            Debug.Log($"Loading next scene: {nextSceneIndex}");
        }
        else
        {
            Debug.LogWarning("This is the last scene, no next scene to load.");
        }
    }

    // ������һ����
    public void LoadPreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = currentSceneIndex - 1;

        if (previousSceneIndex >= 0)
        {
            SceneManager.LoadScene(previousSceneIndex);
            Debug.Log($"Loading previous scene: {previousSceneIndex}");
        }
        else
        {
            Debug.LogWarning("This is the first scene, no previous scene to load.");
        }
    }

    // �˳���Ϸ
    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // �ڱ༭�����˳�����ģʽ
#endif
    }
}
