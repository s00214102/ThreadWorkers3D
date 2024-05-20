using System.IO;
using System.IO.IsolatedStorage;
using UnityEngine;

public class UnityIsolatedStorage : MonoBehaviour
{
	string folderName = "WorkerStorage";
	string filePath = "WorkerStorage\\IsolatedStorage.txt";

	void Start()
	{
		InitializeStorage();
	}
	// Ensure storage directory and file exist
	private void InitializeStorage()
	{
		using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly())
		{
			if (!store.DirectoryExists(folderName))
			{
				store.CreateDirectory(folderName);
			}
			using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(filePath, FileMode.Create, FileAccess.Write, store))
			{
				using (StreamWriter writer = new StreamWriter(stream))
				{
					writer.Write("");
				}
			}
			Debug.Log("File created in isolated storage with default data.");
		}
	}

	// append text to the isolated storage file
	public void AppendToStorageFile(string dataToAppend)
	{

		using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly())
		{
			if (!store.DirectoryExists(folderName))
			{
				store.CreateDirectory(folderName);
			}
			// Using FileMode.Append will either create the file if it's not there or append to the end if it exists.
			using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(filePath, FileMode.Append, FileAccess.Write, store))
			{
				using (StreamWriter writer = new StreamWriter(stream))
				{
					writer.Write(dataToAppend); // Append the data
					Debug.Log("Data appended to isolated storage: " + dataToAppend);
				}
			}
		}
	}

	public string ReadDataFromIsolatedStorage()
	{
		using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly())
		{
			if (!store.DirectoryExists(folderName))
			{
				store.CreateDirectory(folderName);
			}

			if (store.FileExists(filePath))
			{
				using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(filePath, FileMode.Open, FileAccess.Read, store))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						string contents = reader.ReadToEnd();
						Debug.Log("Read from isolated storage: " + contents);
						return contents;
					}
				}
			}
			else
			{
				Debug.LogError("File not found in isolated storage.");
				return string.Empty;
			}
		}
	}
}
