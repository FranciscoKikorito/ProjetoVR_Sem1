using UnityEngine;
using UnityEngine.SceneManagement;

public class VRMenuController : MonoBehaviour
{
    // The name of your actual game level
    [SerializeField] private string gameSceneName = "MainLevel";

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitApp()
    {
        Application.Quit();
        Debug.Log("Quit button pressed");
    }
}