using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    // 加载指定场景
    public static SceneManagerScript instance;
    public string scene;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // 如果已经存在实例，销毁当前对象
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

    // 加载当前场景
    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
        Debug.Log($"Reloading current scene: {currentSceneName}");
    }

    // 加载下一场景
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

    // 加载上一场景
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

    // 退出游戏
    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 在编辑器中退出播放模式
#endif
    }
}
