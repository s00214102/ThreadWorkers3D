using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkerCard : MonoBehaviour
{
	// priority buttons raise and lower the priority text (0-3)
	// this value then updates the workers thread priority (WorkerThreadController)   
	private Button lowerPriorityButton;
	private Button higherPriorityButton;
	private TextMeshProUGUI priorityValueText;

	private int priorityValue = 0;
	// Public event for notifying listeners about changes
	public event Action<int> OnPriorityValueChanged;

	// Public property to modify the priority value and trigger the event
	public int PriorityValue
	{
		get { return priorityValue; }
		set
		{
			if (priorityValue != value) // Ensure the value is changing
			{
				priorityValue = value;
				OnPriorityValueChanged?.Invoke(priorityValue); // Raise the event
			}
		}
	}

	private void Awake()
	{
		// btnLowerPriority
		GameObject lowerButtonGO = GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "btnLowerPriority")?.gameObject;
		lowerPriorityButton = lowerButtonGO.GetComponent<Button>();
		lowerPriorityButton.onClick.AddListener(LowerPriority);
		// btnHigherPriority
		GameObject higherButtonGO = GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "btnHigherPriority")?.gameObject;
		higherPriorityButton = higherButtonGO.GetComponent<Button>();
		higherPriorityButton.onClick.AddListener(HigherPriority);
		// txtPriorityValue
		GameObject PriorityValueGO = GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "txtPriorityValue")?.gameObject;
		priorityValueText = PriorityValueGO.GetComponent<TextMeshProUGUI>();
		priorityValueText.text = priorityValue.ToString();

	}
	private void LowerPriority()
	{
		priorityValue--;
		priorityValue = Mathf.Clamp(priorityValue, 0, 4);
		priorityValueText.text = priorityValue.ToString();
	}
	private void HigherPriority()
	{
		priorityValue++;
		priorityValue = Mathf.Clamp(priorityValue, 0, 4);
		priorityValueText.text = priorityValue.ToString();
	}
}