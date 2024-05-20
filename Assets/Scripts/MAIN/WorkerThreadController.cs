using System;
using System.Collections;
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
	// Components
	private CharacterMovement characterMovement;
	private WorkerAnimationController animationController;
	[HideInInspector] public WorkerManager workerManager;

	// UI 
	[SerializeField] private TMP_Text dataText;
	public TMP_Text stateText;
	public Button startButton;
	public Button cancelButton;
	public GameObject workerCard;
	public GameObject smokeEffect;

	// world positions
	Transform computer; // the computers world position
	Transform storage; // the storage world position
	Transform retirement; // the storage world position

	// logic
	bool workerDestroyed = false;
	bool workerInitialized = false;
	private float rotationSpeed = 15;

	// threading
	Thread workerThread;
	[HideInInspector] public string workerName;
	private ConcurrentQueue<Action> taskQueue = new ConcurrentQueue<Action>();
	// used to let the thread know when we have reached a target destination
	private ManualResetEvent reachedTargetEvent = new ManualResetEvent(false);
	private ManualResetEvent facingTargetEvent = new ManualResetEvent(false);
	private static readonly object calculationLock = new object();
	private static readonly object storageLock = new object();

	// this method is run before Start() and is used for setup
	private void Awake()
	{
		dataText.text = "";
		// a component used to tell the robot where to move
		characterMovement = GetComponent<CharacterMovement>();
		animationController = GetComponent<WorkerAnimationController>();

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
		// delay worker initialization to give the worker manager time to finish its setup
		StartCoroutine(DelayedInitialization());
	}
	private IEnumerator DelayedInitialization()
	{
		yield return new WaitForSeconds(0.5f);

		CreateWorkerThread();

		stateText.text = GetReadableThreadState(workerThread.ThreadState);
		startButton.onClick.AddListener(StartWorker);
		cancelButton.onClick.AddListener(CancelWorker);
		workerCard.GetComponent<WorkerCard>().OnPriorityValueChanged += UpdatePriority;
		workerInitialized = true;
	}
	public void CreateWorkerThread()
	{
		if (workerThread == null || !workerThread.IsAlive)
		{
			workerThread = new Thread(() => RunWorker());
			workerThread.Name = workerName;
			workerThread.IsBackground = true; // Mark as background thread
		}
	}

	// this method runs multiple times a frame according to unitys internal clock
	private void Update()
	{
		if (workerInitialized)
			stateText.text = GetReadableThreadState(workerThread.ThreadState);

		// Execute all tasks queued by the worker threads on the main thread
		while (taskQueue.TryDequeue(out Action action))
		{
			action.Invoke();
		}
	}

	// called via UI button press
	public void StartWorker()
	{
		if (workerThread.ThreadState.HasFlag(ThreadState.Unstarted))
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
			for (int i = 0; i < 2; i++)
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

						taskQueue.Enqueue(() => StartCoroutine(RotateTowardsTarget(computer.position + new Vector3(0, 0, 1)))); // rotate to face computer
						facingTargetEvent.WaitOne();
						facingTargetEvent.Reset();

						//taskQueue.Enqueue(() => animationController.animator.Play("WorkerWorking"));
						taskQueue.Enqueue(() => animationController.SetWorking(true));
						ComplexOutput(taskQueue);
					}
					catch (Exception e)
					{
						Debug.LogWarning(e);
					}
					finally
					{
						Monitor.Pulse(calculationLock);
						Monitor.Exit(calculationLock);
					}
				}
				else
				{
					Debug.Log($"{workerThread.Name} can't access computer right now.");
				}
				if (workerDestroyed)
				{
					Debug.Log("Worker destroyed, exiting thread.");
					return;
				}
				// enqueue the action to move to the storage area
				taskQueue.Enqueue(() => MoveWorker(storage.position, 2));
				reachedTargetEvent.WaitOne(); // Wait until destination is reached
				reachedTargetEvent.Reset(); // Reset for the next event

				// worker reached the storage, try to access the storage for 1 minute
				if (Monitor.TryEnter(storageLock, 60000))
				{
					try
					{
						taskQueue.Enqueue(() => MoveWorker(storage.position, 0));
						reachedTargetEvent.WaitOne(); // Wait until destination is reached
						reachedTargetEvent.Reset(); // Reset for the next event	
						taskQueue.Enqueue(() => dataText.text = ""); // clear the workers text which displays the number
						taskQueue.Enqueue(() => Storage.Instance.AddNumberToStorageAndDisplay(data.ToString()));
						Debug.Log("Data: " + data);
					}
					catch (Exception e)
					{
						Debug.LogWarning(e);
					}
					finally
					{
						Monitor.Pulse(storageLock);
						Monitor.Exit(storageLock);
					}
				}
				else
				{
					Debug.Log($"{workerThread.Name} can't access storage right now.");
				}
			}
			RetireWorker();
		}
		catch (ThreadInterruptedException)
		{

		}
		catch (ThreadAbortException)
		{
			Debug.Log("Worker thread aborted");
		}
		finally
		{
			//do cleanup here 
		}
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

	private IEnumerator RotateTowardsTarget(Vector3 targetPosition)
	{
		// Calculate direction to the target, but ignore vertical displacement for rotation
		Vector3 directionToTarget = targetPosition - transform.position;
		directionToTarget.y = 0;  // Zero out the Y component

		// Calculate the target rotation based on the direction vector
		Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

		// Rotate towards the target rotation only on the Y axis
		while (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
		{
			// Calculate the rotation for this frame
			Quaternion currentRotation = transform.rotation;
			Quaternion newRotation = Quaternion.Lerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);

			// Apply the rotation only on the Y axis
			transform.rotation = Quaternion.Euler(0, newRotation.eulerAngles.y, 0);

			yield return null;  // Wait until next frame to continue execution
		}

		// Snap to the exact target rotation, ensuring it is only applied to the Y axis
		transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
		facingTargetEvent.Set();  // Signal that the target is now directly faced
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

	int data = 0; // the number which the worker carries from the computer to storage
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
		taskQueue.Enqueue(() => animationController.SetWorking(false));
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

	// called by UI cancel buttons, terminates worker immediately
	public void CancelWorker()
	{
		// only cancel if the worker has been started
		if (workerThread.ThreadState.HasFlag(ThreadState.Running))
		{
			Instantiate(smokeEffect, transform.position, Quaternion.identity);
			taskQueue.Enqueue(() => TerminateWorker());
		}
	}
	// worker moves to retirement home, then terminates
	private void RetireWorker()
	{
		Debug.Log("Worker retiring.");
		taskQueue.Enqueue(() => MoveWorker(retirement.position, 0));
		reachedTargetEvent.WaitOne(); // Wait until destination is reached
		reachedTargetEvent.Reset(); // Reset for the next event

		Debug.Log("Worker has retired!");
		taskQueue.Enqueue(() => TerminateWorker());
	}

	private void TerminateWorker()
	{
		workerManager.RemoveWorker(gameObject);

		if (workerCard != null)
			Destroy(workerCard);

		StopWorkerThread();
		DisableWorker();
	}
	// handle thread termination
	private void StopWorkerThread()
	{
		workerDestroyed = true;
		if (workerThread != null && workerThread.IsAlive)
		{
			workerThread.Abort(); // Use Abort as a last resort if the thread does not stop
		}
	}
	// first disable the gameobject, then after some time destroy the gameobject
	public void DisableWorker()
	{
		gameObject.SetActive(false);
	}
	void OnDisable()
	{
		//Debug.Log("Worker disabled");
	}
	void OnDestroy()
	{
		//Debug.Log("Worker destroyed");
		StopWorkerThread();
	}
}