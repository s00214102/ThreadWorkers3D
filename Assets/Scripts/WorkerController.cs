using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterMovement))]
public class WorkerController : BaseStateMachine
{
	public bool test = false;
	[ContextMenu("Set Bool")]
	public void SetBool() => test = true;

	public CharacterMovement CharacterMovement;
	public enum WorkerState
	{
		Work,
		Store
	}
	public WorkerState CurrentState;
	Dictionary<WorkerState, BaseState> States = new Dictionary<WorkerState, BaseState>();



	private void Awake()
	{
		CharacterMovement = GetComponent<CharacterMovement>();

	}
	void Start()
	{
		InitiliazeStates();

		SetState(0);
	}
	public void ManagerUpdate()
	{

		if (CurrentImplimentation != null)
			CurrentImplimentation.Update();

	}
	private void Update()
	{

		if (CurrentImplimentation != null)
			CurrentImplimentation.Update();

	}
	private void InitiliazeStates()
	{
		States.Add(WorkerState.Work, new WorkerWork(this));
		States.Add(WorkerState.Store, new WorkerStore(this));
	}
	public override void SetState(int newState)
	{
		CurrentState = (WorkerState)newState;

		if (States.ContainsKey(CurrentState))
		{
			// if another state is running, exit before switching to new state
			if (CurrentImplimentation != null)
				CurrentImplimentation.Exit();

			CurrentImplimentation = States[CurrentState];
			CurrentImplimentation.Enter();
		}

		base.SetState(newState);
	}
}
