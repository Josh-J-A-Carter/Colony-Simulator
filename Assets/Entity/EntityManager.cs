using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour {

    public static EntityManager Instance { get; private set; }

    [SerializeField]
    GameObject workerBeePrefab, queenBeePrefab;

    int zIndex = -1;

    void Awake() {
        // Instantiate singleton
        if (Instance != null) {
            Destroy(this);
            return;
        } else Instance = this;
    }

    void Start() {
        InstantiateWorker(new Vector2Int(2, 2));
        InstantiateQueen(new Vector2Int(4, -4));
    }

    void Update() {
        
    }

    public GameObject InstantiateWorker(Vector2Int pos) {
        GameObject obj = Object.Instantiate(workerBeePrefab, new Vector3Int(pos.x, pos.y, zIndex), Quaternion.identity, transform);
        TaskManager.Instance.RegisterAgent(obj.GetComponent<WorkerBehaviour>());

        return obj;
    }

    public GameObject InstantiateQueen(Vector2Int pos) {
        GameObject obj = Object.Instantiate(queenBeePrefab, new Vector3Int(pos.x, pos.y, zIndex), Quaternion.identity, transform);
        TaskManager.Instance.RegisterAgent(obj.GetComponent<QueenBehaviour>());

        return obj;
    }


}
