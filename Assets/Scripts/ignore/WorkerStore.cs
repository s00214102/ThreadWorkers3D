using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerStore : BaseState
{
    WorkerController controller;
    Transform storage;
    public WorkerStore(WorkerController controller) : base(controller)
    {
        this.controller = controller;
    }
    
    public override void Enter()
    {
        Debug.Log("Entered store state");

        storage = GameObject.Find("storagePos").transform;

        if (storage != null){
            controller.CharacterMovement.MoveTo(storage.position, 1);
            controller.CharacterMovement.DestinationReached.AddListener(StoreData);
            }
        else
            Debug.LogWarning("Worker couldnt find storage while in store state.");
    }
    private void StoreData(){
        // give the held data to the background thread to add to isolated storage
        Debug.Log("Storing data...");
        controller.SetState(0);
    }
    public override void Exit()
    {
        controller.CharacterMovement.DestinationReached.RemoveListener(StoreData);
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}
