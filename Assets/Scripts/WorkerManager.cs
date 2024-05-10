using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkerManager : MonoBehaviour
{
    [SerializeField] private Button spawnButton;
    [SerializeField] private GameObject workerPrefab;
    [SerializeField] private GameObject workerCardPrefab;
    [SerializeField] private GameObject workerCardUIContent;
    [SerializeField] private Text statusText;
    private Transform spawnPos;
    private int totalWorkersCreated = 0;
    private readonly List<GameObject> activeWorkers = new List<GameObject>();
    private readonly ConcurrentQueue<Action> taskQueue = new ConcurrentQueue<Action>();

    private void Awake()
    {
        spawnPos = GameObject.Find("spawnPos").transform;
    }
    void Start()
    {
        spawnButton.onClick.AddListener(SpawnWorker);

        // add any workers already in the scene

    }

    void Update()
    {
        // Execute tasks queued by worker threads
        while (taskQueue.TryDequeue(out Action action))
        {
            action.Invoke();
        }
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
        // cache reference to worker card on worker controlelr
        newWorker.GetComponent<WorkerThreadController>().workerCard = newWorkerCard;
        // name the worker
        GameObject workerCardName = newWorkerCard.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(t => t.name == "txtName")?.gameObject;
        workerCardName.GetComponent<TextMeshProUGUI>().text = $"Worker {totalWorkersCreated:D3}";

        totalWorkersCreated++;

        // Queue the worker logic on the ThreadPool
        // if (ThreadPool.QueueUserWorkItem(state => newWorker.GetComponent<WorkerThreadController>().RunWorker(taskQueue)))
        //     Debug.Log("Worker thread pooled correctly.");
        // else
        //     Debug.LogWarning("Worker thread not pooled correctly.");

        //UpdateStatusText();
    }

    // Update the status text with the current number of active workers
    private void UpdateStatusText()
    {
        statusText.text = $"Active Workers: {activeWorkers.Count}";
    }
}
