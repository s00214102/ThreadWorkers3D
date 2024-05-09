using System.Threading;
using UnityEngine;

public class BalkingPattern : MonoBehaviour
{

	bool workDone;

	private void Start()
	{
		new Thread(() => DoWorkOnThread()).Start();
	}
	private void Update()
	{
		Debug.Log(workDone ? "Finished." : "Working...");
	}
	private void DoWorkOnThread()
	{
		Debug.Log("Work starting.");
		Thread.Sleep(2000);
		workDone = true;
		Debug.Log("Work done!");
	}

}