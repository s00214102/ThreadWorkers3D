using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    [SerializeField] private List<WorkerController> Workers = new();
    public void AddHero(GameObject worker)
    {
        Workers.Add(worker.GetComponent<WorkerController>());
    }
    void Update()
    {
        // simplest implementation, simply call each update function in one go
        foreach (var worker in Workers)
        {
            worker.ManagerUpdate();
        }
    }
}
