using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterMovement))]
public class WorkerController : BaseStateMachine
{
	public CharacterMovement CharacterMovement;
	public enum WorkerState
	{
		Work,
		Store
	}
	public WorkerState CurrentState;
	Dictionary<WorkerState, BaseState> States = new Dictionary<WorkerState, BaseState>();

	private WorkerManager _manager;
	private bool _managed = false;

	private void Awake()
	{
		CharacterMovement = GetComponent<CharacterMovement>();
		_manager = GetComponentInParent<WorkerManager>();
		if (_manager != null)
		{
			_managed = true;
			_manager.AddHero(this.gameObject);
		}
		else
			_managed = false;
	}
	void Start()
	{
		InitiliazeStates();

		SetState(0);
	}
	public void ManagerUpdate()
	{
		if (_managed)
		{
			if (CurrentImplimentation != null)
				CurrentImplimentation.Update();
		}
	}
	private void Update()
	{
		if (!_managed)
		{
			if (CurrentImplimentation != null)
				CurrentImplimentation.Update();
		}
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