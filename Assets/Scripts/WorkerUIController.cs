using UnityEngine;
using UnityEngine.SceneManagement;

public class WorkerUIController : MonoBehaviour
{
	private void Start()
	{

	}
	public void ResetGame()
	{
		// Get the current active scene
		Scene currentScene = SceneManager.GetActiveScene();

		// Reload the current scene using its build index
		SceneManager.LoadScene(currentScene.buildIndex);
	}
	public void ExitGame()
	{
		Application.Quit();
	}
}