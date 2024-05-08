using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class WorkerWork : BaseState
{
    WorkerController controller;
    Transform computer;
    public WorkerWork(WorkerController controller) : base(controller)
    {
        this.controller = controller;
    }
    
    public override void Enter()
    {
        Debug.Log("Entered work state");

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
        // start a thread to beging the computation
        Debug.Log("Computing...");
        controller.SetState(1);
        Thread workThread = new Thread(new ThreadStart(ComplexOutput));
    }

    private int ComplexOutput(int inputNumber = 0)
    {
        // If no input number is given, generate a random three-digit number
        if (inputNumber == 0)
        {
            Random random = new Random();
            inputNumber = random.Next(100, 1000);
        }
        else if (inputNumber < 100 || inputNumber > 999)
        {
            throw new ArgumentException("Input number must be a three-digit number or zero.");
        }

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
        while ((DateTime.Now - start).TotalSeconds < 3)
        {
            // Simple busy-wait loop to simulate longer processing time
        }

        // Generate a final random number between 1 and 5
        Random finalRandom = new Random();
        return finalRandom.Next(1, 6);
    }
    public override void Exit()
    {
        controller.CharacterMovement.DestinationReached.RemoveListener(StartComputing);
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}
