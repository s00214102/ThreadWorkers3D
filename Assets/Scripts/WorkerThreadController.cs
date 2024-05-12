using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using SharedWorkerStorage;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WorkerThreadController : MonoBehaviour
{
	Thread workerThread;

	[SerializeField] private TMP_Text dataText;
	public TMP_Text stateText;
	public Button startButton;
	public Button cancelButton;
	public GameObject workerCard;

	Transform computer; // the computers world position
	Transform storage; // the storage world position
	Transform retirement; // the storage world position

	bool workerDestroyed = false;

	private ConcurrentQueue<Action> taskQueue = new ConcurrentQueue<Action>();

	// used to let the thread know when we have reached a target destination
	private ManualResetEvent reachedTargetEvent = new ManualResetEvent(false);

	private static readonly object calculationLock = new object();

	private CharacterMovement characterMovement;

	//private static JsonStorageManager isolatedStorage;

	// this method is run before Start() and is used for setup
	private void Awake()
	{
		dataText.text = "";
		// a component used to tell the robot where to move
		characterMovement = GetComponent<CharacterMovement>();

		// the worker should be able to find the storage and computer objects in the game world
		storage = GameObject.Find("storagePos").transform;
		computer = GameObject.Find("computerPos").transform;
		retirement = GameObject.Find("retirementPos").transform;
		if (storage == null || computer == null || retirement == null)
		{
			Debug.LogWarning("Worker couldnt find required storage or computer.");
			Destroy(gameObject);
		}
	}
	// this method is used to start a components logic
	private void Start()
	{
		CreateWorkerThread();

		stateText.text = GetReadableThreadState(workerThread.ThreadState);
		startButton.onClick.AddListener(StartWorker);
		cancelButton.onClick.AddListener(CancelWorker);
		workerCard.GetComponent<WorkerCard>().OnPriorityValueChanged += UpdatePriority;
	}

	// this method runs multiple times a frame according to unitys internal clock
	private void Update()
	{
		stateText.text = GetReadableThreadState(workerThread.ThreadState);

		// Execute all tasks queued by the worker threads on the main thread
		while (taskQueue.TryDequeue(out Action action))
		{
			action.Invoke();
		}
	}

	public void CreateWorkerThread()
	{
		if (workerThread == null || !workerThread.IsAlive)
		{
			workerThread = new Thread(() => RunWorker());
			workerThread.IsBackground = true; // Mark as background thread
		}
	}

	// called via UI button press
	public void StartWorker()
	{
		workerThread.Start();
	}

	// the thread method which runs the workers logic
	private void RunWorker()
	{
		Debug.Log("Work starting.");

		if (workerDestroyed)
		{
			Debug.Log("Worker destroyed, exiting thread.");
			return;
		}

		try
		{
			// move back and forth from computer to storage 5 times
			for (int i = 0; i < 4; i++)
			{
				if (workerDestroyed)
				{
					Debug.Log("Worker destroyed, exiting thread.");
					return;
				}

				// enqueue the action to move to the computer
				taskQueue.Enqueue(() => MoveWorker(computer.position, 2));
				reachedTargetEvent.WaitOne(); // Wait until destination is reached
				reachedTargetEvent.Reset(); // Reset for the next event

				// try to access the computer for 1 minute
				if (Monitor.TryEnter(calculationLock, 60000))
				{
					try
					{
						taskQueue.Enqueue(() => MoveWorker(computer.position, 0));
						reachedTargetEvent.WaitOne(); // Wait until destination is reached
						reachedTargetEvent.Reset(); // Reset for the next event	
						ComplexOutput(taskQueue);
					}
					catch (Exception e)
					{
						Debug.LogWarning(e);
					}
					finally
					{
						Monitor.Pulse(calculationLock);
						//TODO play a pulse animation from the anthenna of the worker
						Monitor.Exit(calculationLock);
					}
				}
				else
				{
					Debug.Log($"{workerThread.Name} can't access computer right now.");
				}

				// enqueue the action to move to the storage
				taskQueue.Enqueue(() => MoveWorker(storage.position, 1));
				reachedTargetEvent.WaitOne(); // Wait until destination is reached
				reachedTargetEvent.Reset(); // Reset for the next event
											// worker reached the storage
				taskQueue.Enqueue(() => dataText.text = ""); // clear the workers text which displays the number
				Debug.Log("Data: " + data);

				taskQueue.Enqueue(() => Storage.Instance.AddNumberToStorageAndDisplay(data.ToString()));

				// taskQueue.Enqueue(() =>
				// {
				// 	JsonFieldUpdate updateInfo = new JsonFieldUpdate("data", data);
				// 	WorkerManager.isolatedStorage.UpdateJsonField(updateInfo);  // add number to isolated storage
				// });

			}
			RetireWorker();
		}
		catch (ThreadInterruptedException)
		{

		}
		catch (ThreadAbortException)
		{
			RetireWorker();
		}
		finally
		{
			//do cleanup here 
		}
	}

	private void RetireWorker()
	{
		Debug.Log("Worker retiring.");
		taskQueue.Enqueue(() => MoveWorker(retirement.position, 0));
		reachedTargetEvent.WaitOne(); // Wait until destination is reached
		reachedTargetEvent.Reset(); // Reset for the next event

		Debug.Log("Worker has retired!");
		//TODO worker retirement animation (blowsup? flies up into the air? enters a darkened doorway?)
		taskQueue.Enqueue(() => DestroyWorker());
	}
	// Move the worker GameObject to the specified target position
	private void MoveWorker(Vector3 targetPosition, float stopRange)
	{
		// uses the Unity NavMesh system to move the worker to the target position
		characterMovement.MoveTo(targetPosition, stopRange);
		characterMovement.DestinationReached.RemoveAllListeners();
		// an event will signal that the worker has reached their destination, this is used to continue the code within the thread
		characterMovement.DestinationReached.AddListener(() => reachedTargetEvent.Set());
	}

	public void UpdatePriority(int value)
	{
		switch (value)
		{
			case 0:
				workerThread.Priority = System.Threading.ThreadPriority.Lowest;
				break;
			case 1:
				workerThread.Priority = System.Threading.ThreadPriority.BelowNormal;
				break;
			case 2:
				workerThread.Priority = System.Threading.ThreadPriority.Normal;
				break;
			case 3:
				workerThread.Priority = System.Threading.ThreadPriority.AboveNormal;
				break;
			case 4:
				workerThread.Priority = System.Threading.ThreadPriority.Highest;
				break;
		}
	}

	private void CancelWorker()
	{
		workerThread.Abort();
	}

	private void DestroyWorker()
	{
		Destroy(this.gameObject);
	}

	int data = 0;
	private void ComplexOutput(ConcurrentQueue<Action> taskQueue)
	{
		// Simulate a delay using inefficient computations
		DateTime start = DateTime.Now;

		// Inefficient prime number computation
		List<int> primes = new List<int>();
		for (int num = 2; num < 10000; num++)
		{
			bool prime = true;
			for (int i = 2; i <= Math.Sqrt(num); i++)
			{
				if (num % i == 0)
				{
					prime = false;
					break;
				}
			}
			if (prime) primes.Add(num);
		}

		// Fibonacci computation (up to a large arbitrary index)
		List<long> fib = new List<long> { 0, 1 };
		for (int i = 2; i < 10000; i++)
		{
			fib.Add(fib[i - 1] + fib[i - 2]);
		}

		// Waste time with repetitive and non-optimal calculations
		long result = 0;
		for (long i = 0; i < 1000000; i++)
		{
			result += (i * i) % 10;
		}

		// Make sure execution takes a few seconds
		Thread.Sleep(1000);

		// Generate a final random number between 1 and 5
		System.Random finalRandom = new System.Random();
		data = finalRandom.Next(0, 9);
		taskQueue.Enqueue(() => dataText.text = data.ToString());
	}

	// Decode the ThreadState into a more human-readable message
	private string GetReadableThreadState(ThreadState state)
	{
		if (state.HasFlag(ThreadState.Unstarted))
			return "Unstarted";
		if (state.HasFlag(ThreadState.Running))
			return "Running";
		if (state.HasFlag(ThreadState.WaitSleepJoin))
			return "Blocked (Wait/Sleep/Join)";
		if (state.HasFlag(ThreadState.Stopped))
			return "Stopped";
		if (state.HasFlag(ThreadState.SuspendRequested))
			return "Suspend Requested";
		if (state.HasFlag(ThreadState.Suspended))
			return "Suspended";
		if (state.HasFlag(ThreadState.AbortRequested))
			return "Abort Requested";
		if (state.HasFlag(ThreadState.Aborted))
			return "Aborted";
		// if (state.HasFlag(ThreadState.Background))
		//     return "Running (Background)";

		return "Unknown";
	}

	void OnDestroy()
	{
		// Stop the worker thread when this MonoBehaviour is destroyed
		workerDestroyed = true;

		if (workerThread != null && workerThread.IsAlive)
			workerThread.Join();

		Destroy(workerCard);
	}

}