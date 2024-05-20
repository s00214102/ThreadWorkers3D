using UnityEngine;

public class WorkerAnimationController : MonoBehaviour
{
	public Animator animator;
	private CharacterMovement movement;
	void Awake()
	{
		animator = GetComponent<Animator>();
		movement = GetComponent<CharacterMovement>();
	}
	private void Update()
	{
		// set IsMoving according to CharacterMovement
		if (movement.isMoving && !animator.GetBool("IsWorking"))
			SetMoving(true);
		else if (!movement.isMoving)
			SetMoving(false);
	}
	public void SetMoving(bool isMoving)
	{
		animator.SetBool("IsMoving", isMoving);
	}

	public void SetWorking(bool isWorking)
	{
		animator.SetBool("IsWorking", isWorking);
	}
}
