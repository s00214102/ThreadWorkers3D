using TMPro;
using UnityEngine;

// workers pass a number to this component
// which is then written to IsolatedStorage
public class Storage : MonoBehaviour
{
	public static Storage Instance;

	private UnityIsolatedStorage isoStorage;
	[SerializeField] private TextMeshProUGUI progressNumber;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this.gameObject); // Ensure that there's only one instance running
		}
		else
		{
			Instance = this;
			DontDestroyOnLoad(this.gameObject); // Optional: Keep instance alive across scenes
		}

		isoStorage = GetComponent<UnityIsolatedStorage>();
	}

	private void Start()
	{
		isoStorage.ReadDataFromIsolatedStorage();
	}
	// called from a worker 
	public void AddNumberToStorageAndDisplay(string s)
	{
		// append the number passed by the worker
		isoStorage.AppendToStorageFile(s);

		// read from isolated storage and display as text in game	
		string messageToDisplay = isoStorage.ReadDataFromIsolatedStorage();
		if (messageToDisplay != string.Empty)
			progressNumber.text = messageToDisplay;
		else
			Debug.LogWarning("IsolatedStorage file contents were empty.");
	}
}