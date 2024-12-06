using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class EntityManager : MonoBehaviour {

    public static EntityManager Instance { get; private set; }

    [SerializeField]
    GameObject workerBeePrefab, queenBeePrefab;


    String[] awesomeWorkerBeeNames = new String[] {
        "Bella", "Brianna", "Brooke", "Blair", "Bethany", "Beatrice", "Bree", "Brynlee",
        "Bettie", "Bonnie", "Britney", "Bianca"
    };

    String[] awesomeQueenBeeNames = new String[] {
        "Artemisia", "Elizabeth", "Margaret", "Berenice", "Cleopatra", "Eleanor", "Catherine",
        "Maria", "Christina", "Victoria", "Mathilde", "Caterina", "Nefertiti", "Lady Jane"
    };

    int zIndex = -1;

    public void Awake() {
        // Instantiate singleton
        if (Instance != null) {
            Destroy(this);
            return;
        } else Instance = this;
    }

    public void Start() {
        InstantiateWorker(new Vector2Int(2, 2));
        InstantiateQueen(new Vector2Int(4, -4));
    }

    public GameObject InstantiateWorker(Vector2Int pos) {
        GameObject obj = Object.Instantiate(workerBeePrefab, new Vector3Int(pos.x, pos.y, zIndex), Quaternion.identity, transform);
        TaskManager.Instance.RegisterAgent(obj.GetComponent<WorkerBehaviour>());

        WorkerBehaviour worker = obj.GetComponent<WorkerBehaviour>();
        int index = Random.Range(0, awesomeWorkerBeeNames.Length);
        worker.SetName(awesomeWorkerBeeNames[index]);

        return obj;
    }

    public GameObject InstantiateQueen(Vector2Int pos) {
        GameObject obj = Object.Instantiate(queenBeePrefab, new Vector3Int(pos.x, pos.y, zIndex), Quaternion.identity, transform);
        TaskManager.Instance.RegisterAgent(obj.GetComponent<QueenBehaviour>());

        QueenBehaviour queen = obj.GetComponent<QueenBehaviour>();
        int index = Random.Range(0, awesomeQueenBeeNames.Length);
        String baseName = awesomeQueenBeeNames[index];

        int generation = Random.Range(1, 6);
        String genName = generation switch {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            _ => ""
        };

        String name = $"Queen {baseName} {genName}";
        queen.SetName(name);

        return obj;
    }
}
