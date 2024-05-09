using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WorkerThreadController : MonoBehaviour
{
	Thread workerThread;

	bool workDone;
	float moveSpeed = 5f;
	float rotationSpeed = 5f;

	Transform computer; // the computers world position
	Transform storage; // the storage world position
	Transform retirement; // the storage world position

	bool workerDestroyed = false;

	private ConcurrentQueue<Action> taskQueue = new ConcurrentQueue<Action>();

	// used to let the thread know when we have reached a target destination
	private ManualResetEvent reachedTargetEvent = new ManualResetEvent(false);

	private static readonly object calculationLock = new object();

	private CharacterMovement characterMovement;
	// this method is run before Start() and is used for setup
	private void Awake()
	{
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
		workerThread = new Thread(() => DoWorkOnThread())
		{
			IsBackground = true // all threads I use in Unity for gameplay logic must be background threads, otherwise they will just keep running when the game stops.
		};
		workerThread.Start();
	}
	// this method runs multiple times a frame according to unitys internal clock
	private void Update()
	{
		// Execute all tasks queued by the worker threads on the main thread
		while (taskQueue.TryDequeue(out Action action))
		{
			action.Invoke();
		}
	}
	// the thread method which runs the workers logic
	private void DoWorkOnThread()
	{
		Debug.Log("Work starting.");

		if (workerDestroyed)
			return;

		// move back and forth from computer to storage 5 times
		for (int i = 0; i < 3; i++)
		{
			if (workerDestroyed)
				return;

			// enqueue the action to move to the computer
			taskQueue.Enqueue(() => MoveWorker(computer.position, 2));
			reachedTargetEvent.WaitOne(); // Wait until destination is reached
			reachedTargetEvent.Reset(); // Reset for the next event

			// perform calculation at the computer
			// lock (calculationLock)
			// {
			// 	taskQueue.Enqueue(() => MoveWorker(computer.position, 0));
			// 	reachedTargetEvent.WaitOne(); // Wait until destination is reached
			// 	reachedTargetEvent.Reset(); // Reset for the next event	
			// 	ComplexOutput();
			// }

			// try to access the computer for 1 minute
			if (Monitor.TryEnter(calculationLock, 60000))
			{
				try
				{
					taskQueue.Enqueue(() => MoveWorker(computer.position, 0));
					reachedTargetEvent.WaitOne(); // Wait until destination is reached
					reachedTargetEvent.Reset(); // Reset for the next event	
					ComplexOutput();
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

			// enqueue the action to move to the storage
			taskQueue.Enqueue(() => MoveWorker(storage.position, 1));
			reachedTargetEvent.WaitOne(); // Wait until destination is reached
			reachedTargetEvent.Reset(); // Reset for the next event
		}
		workDone = true;
		Debug.Log("Worker retiring.");
		taskQueue.Enqueue(() => MoveWorker(retirement.position, 0));
		reachedTargetEvent.WaitOne(); // Wait until destination is reached
		reachedTargetEvent.Reset(); // Reset for the next event

		Debug.Log("Worker has retired!");
		//TODO worker retirement animation (blowsup? flies up into the air? enters a darkened doorway?)
		taskQueue.Enqueue(() => RetireWorker());
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

	private void RetireWorker()
	{
		Destroy(this.gameObject);
	}

	int data = 0;
	private void ComplexOutput()
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
		data = finalRandom.Next(1, 6);
	}

	void OnDestroy()
	{
		// Stop the worker thread when this MonoBehaviour is destroyed
		workerDestroyed = true;
	}

}