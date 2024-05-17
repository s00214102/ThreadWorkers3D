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
    [SerializeField] private Button cancelAllButton;
    [SerializeField] private GameObject workerPrefab;
    [SerializeField] private GameObject workerCardPrefab;
    [SerializeField] private GameObject workerCardUIContent;
    [SerializeField] private TextMeshProUGUI txtActiveWorkers;
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
        cancelAllButton.onClick.AddListener(CancelAllWorkers);
        //targetNumber.text = isolatedStorage.ReadJsonField("target");

        // listen to storage even for when the required numbers are reached
        GameObject.Find("storageBox").GetComponent<Storage>().OnNumberReached.AddListener(CancelAllWorkers);

        UpdateStatusText();
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
    public void SpawnWorker()
    {
        // create/spawn new worker prefab, position at spawn point, add to list
        GameObject newWorker = Instantiate(workerPrefab, this.transform);
        newWorker.transform.position = spawnPos.position;

        // set manager reference
        newWorker.GetComponent<WorkerThreadController>().workerManager = this;

        // get child gameobjects of the worker to change their colour
        Material workerMat = ChooseColour();
        newWorker.transform.Find("body").transform.Find("cap").gameObject.GetComponent<Renderer>().material = workerMat;

        // add worker to managers list of workers
        activeWorkers.Add(newWorker);

        // add a worker card to the UI
        GameObject newWorkerCard = Instantiate(workerCardPrefab, workerCardUIContent.transform);
        // change card colour to match the workers colour
        newWorkerCard.GetComponent<Image>().color = workerMat.color;

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

        UpdateStatusText();
    }

    private Material ChooseColour()
    {
        // resources folder contains a number of materials with different colours
        // choose one at random and pass it back
        string materialName = "Worker_Red_Mat";
        // pick random number
        int randomNumber = UnityEngine.Random.Range(0, 12);
        // switch on number to pick mat name
        switch (randomNumber)
        {
            case 0:
                materialName = "Worker_Red_Mat";
                break;
            case 1:
                materialName = "Worker_Blue_Mat";
                break;
            case 2:
                materialName = "Worker_Yellow_Mat";
                break;
            case 3:
                materialName = "Worker_Green_Mat";
                break;
            case 4:
                materialName = "Worker_Orange_Mat";
                break;
            case 5:
                materialName = "Worker_Purple_Mat";
                break;
            case 6:
                materialName = "Worker_Black_Mat";
                break;
            case 7:
                materialName = "Worker_White_Mat";
                break;
            case 8:
                materialName = "Worker_Gray_Mat";
                break;
            case 9:
                materialName = "Worker_Brown_Mat";
                break;
            case 10:
                materialName = "Worker_Pink_Mat";
                break;
            case 11:
                materialName = "Worker_Cyan_Mat";
                break;
            default:
                materialName = "Worker_Default_Mat";  // In case of an unexpected value
                break;
        }

        // Load the material from Resources folder
        Material newMat = Resources.Load<Material>(materialName);
        if (newMat == null)
        {
            Debug.LogError("Failed to load material: " + materialName);
            return null;
        }

        return newMat;
    }

    // when the Start All button on UI is clicked, call each workers StartWorker method
    private void StartAllWorkers()
    {
        foreach (var worker in activeWorkers)
        {
            worker.GetComponent<WorkerThreadController>().StartWorker();
        }
    }
    // called when the number of required numbers is reached
    private void CancelAllWorkers()
    {
        foreach (var worker in activeWorkers)
        {
            worker.GetComponent<WorkerThreadController>().CancelWorker();
        }
    }

    public void RemoveWorker(GameObject worker)
    {
        activeWorkers.Remove(worker);
        UpdateStatusText();
    }

    // Update the status text with the current number of active workers
    private void UpdateStatusText()
    {
        txtActiveWorkers.text = $"Workers: {activeWorkers.Count}";
    }
}
