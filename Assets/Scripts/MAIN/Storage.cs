using TMPro;
using UnityEngine;
using UnityEngine.Events;

// workers pass a number to this component
// which is then written to IsolatedStorage
public class Storage : MonoBehaviour
{
	public static Storage Instance;

	private UnityIsolatedStorage isoStorage;
	[SerializeField] private int requiredNumbers = 10;
	private int totalNumbers = 0;
	[SerializeField] private TextMeshProUGUI progressNumber;
	[SerializeField] private HealthBar healthBar;
	public UnityEvent OnNumberReached;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this.gameObject); // Ensure that there's only one instance running
		}
		else
		{
			Instance = this;
		}

		isoStorage = GetComponent<UnityIsolatedStorage>();

		totalNumbers = 0;
		healthBar.SetMaxHealth(100);
		healthBar.SetHealth(0);
	}

	private void Start()
	{
		isoStorage.ReadDataFromIsolatedStorage();

		totalNumbers = 0;
		healthBar.SetMaxHealth(100);
		healthBar.SetHealth(0);
	}

	// called from a worker 
	public void AddNumberToStorageAndDisplay(string s)
	{
		// append the number passed by the worker
		isoStorage.AppendToStorageFile(s);

		totalNumbers++;
		float percent = ((float)totalNumbers / (float)requiredNumbers) * 100;
		int percentint = (int)percent;
		// update progress bar
		healthBar.SetHealth(percentint);

		// cancel all workers when the number of numbers is reached
		if (totalNumbers >= requiredNumbers)
			OnNumberReached?.Invoke();


		// read from isolated storage and display as text in game	
		string messageToDisplay = isoStorage.ReadDataFromIsolatedStorage();
		if (messageToDisplay != string.Empty)
			progressNumber.text = messageToDisplay;
		else
			Debug.LogWarning("IsolatedStorage file contents were empty.");
	}
}