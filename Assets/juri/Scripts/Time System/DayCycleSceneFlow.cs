using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(DayCycleManager))]
public class DayCycleSceneFlow : MonoBehaviour
{
    [SerializeField] private string morningScene = "Shop";
    [SerializeField] private string nightScene = "NightHub";

    private DayCycleManager dayCycle;

    private void Awake()
    {
        dayCycle = GetComponent<DayCycleManager>();
    }

    private void OnEnable()
    {
        dayCycle.OnMorningStarted += GoToMorningScene;
        dayCycle.OnNightStarted += GoToNightScene;
    }

    private void OnDisable()
    {
        dayCycle.OnMorningStarted -= GoToMorningScene;
        dayCycle.OnNightStarted -= GoToNightScene;
    }

    private void GoToMorningScene(int day)
    {
        Debug.Log("Day " + day + " morning");
        LoadScene(morningScene);
    }

    private void GoToNightScene(int day)
    {
        Debug.Log("Day " + day + " night");
        LoadScene(nightScene);
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        if (SceneManager.GetActiveScene().name == sceneName) return;

        dayCycle.Pause();

        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneName);
        if (loading == null)
        {
            dayCycle.Resume();
            return;
        }
        loading.completed += OnSceneLoaded;
    }

    private void OnSceneLoaded(AsyncOperation loading)
    {
        dayCycle.Resume();
    }
}