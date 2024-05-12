using System.IO;
using System.IO.IsolatedStorage;
using UnityEngine;

public class UnityIsolatedStorageReader : MonoBehaviour
{
	void Start()
	{
		ReadDataFromIsolatedStorage();
	}

	private void ReadDataFromIsolatedStorage()
	{
		using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly())
		{
			string filePath = "WorkerStorage\\settings.json";
			if (store.FileExists(filePath))
			{
				using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(filePath, FileMode.Open, FileAccess.Read, store))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						string contents = reader.ReadToEnd();
						Debug.Log("Read from isolated storage: " + contents);
						// Process contents as JSON or any other format as necessary
					}
				}
			}
			else
			{
				Debug.LogError("File not found in isolated storage.");
			}
		}
	}
}
