using TMPro;
using UnityEngine;

public class Storage : MonoBehaviour
{
	private UnityIsolatedStorageReader isoStorage;
	[SerializeField] private TextMeshProUGUI progressNumber;

	private void Awake()
	{
		isoStorage = GetComponent<UnityIsolatedStorageReader>();
	}

	private void Start()
	{

	}
}