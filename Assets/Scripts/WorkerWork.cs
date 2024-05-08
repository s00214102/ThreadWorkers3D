using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using UnityEngine.Events;

public class WorkerWork : BaseState
{
    Thread workThread;
    WorkerController controller;
    Transform computer;
    public WorkerWork(WorkerController controller) : base(controller)
    {
        this.controller = controller;
    }
    
    int data = 0;
    public override void Enter()
    {
        Debug.Log("Entered work state");
        
        data=0;

        computer = GameObject.Find("computerPos").transform;

        if (computer != null)
            {
                controller.CharacterMovement.MoveTo(computer.position, 1);
                controller.CharacterMovement.DestinationReached.AddListener(StartComputing);
            }
        else
            Debug.LogWarning("Worker couldnt find computer while in work state.");
    }

    private void StartComputing(){
        controller.CharacterMovement.DestinationReached.RemoveListener(StartComputing);
        // start a thread to beging the computation
        Debug.Log("Computing...");
        controller.SetState(1);

        workThread = new Thread(new ThreadStart(ComplexOutput));
        workThread.Start();
    }

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
        data= finalRandom.Next(1, 6);

        ComputationComplete();
    }
    private void ComputationComplete(){
        Debug.Log("Computation complete!");
        controller.SetState(1);
    }
    public override void Exit()
    {
        
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}
