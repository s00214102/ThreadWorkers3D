using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SharedWorkerStorage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class WorkerManager : MonoBehaviour
{
    [SerializeField] private Button spawnButton;
    [SerializeField] private Button startAllButton;
    [SerializeField] private GameObject workerPrefab;
    [SerializeField] private GameObject workerCardPrefab;
    [SerializeField] private GameObject workerCardUIContent;
    [SerializeField] private TextMeshProUGUI statusText;
    //[SerializeField] private TextMeshProUGUI targetNumber;

    private Transform spawnPos;
    private int totalWorkersCreated = 0;
    private readonly List<GameObject> activeWorkers = new List<GameObject>();
    //private readonly ConcurrentQueue<Action> taskQueue = new ConcurrentQueue<Action>();
    //public static JsonStorageManager isolatedStorage;

    private void Awake()
    {
        spawnPos = GameObject.Find("spawnPos").transform;

        //isolatedStorage = new JsonStorageManager();
    }
    void Start()
    {
        spawnButton.onClick.AddListener(SpawnWorker);
        startAllButton.onClick.AddListener(StartAllWorkers);
        //targetNumber.text = isolatedStorage.ReadJsonField("target");
    }

    void Update()
    {
        // Execute tasks queued by worker threads
        // while (taskQueue.TryDequeue(out Action action))
        // {
        //     action.Invoke();
        // }
    }

    // Spawn a new worker and run its logic in a task
    private void SpawnWorker()
    {
        // create/spawn new worker prefab, position at spawn point, add to list
        GameObject newWorker = Instantiate(workerPrefab, this.transform);
        newWorker.transform.position = spawnPos.position;
        activeWorkers.Add(newWorker);

        // add a worker card to the UI
        GameObject newWorkerCard = Instantiate(workerCardPrefab, workerCardUIContent.transform);
        // pass the worker card start button reference to the newly created worker, btnStart
        GameObject workerStartButtonGO = newWorkerCard.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "btnStart")?.gameObject;
        Button workerStartButton = workerStartButtonGO.GetComponent<Button>();
        newWorker.GetComponent<WorkerThreadController>().startButton = workerStartButton;

        // cancel button
        GameObject workerCancelButtonGO = newWorkerCard.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "btnCancel")?.gameObject;
        Button workerCancelButton = workerCancelButtonGO.GetComponent<Button>();
        newWorker.GetComponent<WorkerThreadController>().cancelButton = workerCancelButton;

        // cache reference to worker card on worker controlelr
        newWorker.GetComponent<WorkerThreadController>().workerCard = newWorkerCard;
        // name the worker
        GameObject workerCardName = newWorkerCard.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(t => t.name == "txtName")?.gameObject;
        workerCardName.GetComponent<TextMeshProUGUI>().text = $"Worker {totalWorkersCreated:D3}";
        totalWorkersCreated++;
        // txtState
        GameObject workerStateTextGO = newWorkerCard.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(t => t.name == "txtState")?.gameObject;
        newWorker.GetComponent<WorkerThreadController>().stateText = workerStateTextGO.GetComponent<TextMeshProUGUI>();
        // listen to WorkerCard.OnPriorityValueChanged
        //newWorkerCard.GetComponent<WorkerCard>().OnPriorityValueChanged += newWorker.GetComponent<WorkerThreadController>().UpdatePriority;

        // Queue the worker logic on the ThreadPool
        // if (ThreadPool.QueueUserWorkItem(state => newWorker.GetComponent<WorkerThreadController>().RunWorker(taskQueue)))
        //     Debug.Log("Worker thread pooled correctly.");
        // else
        //     Debug.LogWarning("Worker thread not pooled correctly.");

        //UpdateStatusText();
    }

    // when the Start All button on UI is clicked, call each workers StartWorker method
    private void StartAllWorkers()
    {
        foreach (var worker in activeWorkers)
        {
            worker.GetComponent<WorkerThreadController>().StartWorker();
        }
    }
    // Update the status text with the current number of active workers
    private void UpdateStatusText()
    {
        statusText.text = $"Active Workers: {activeWorkers.Count}";
    }
}
