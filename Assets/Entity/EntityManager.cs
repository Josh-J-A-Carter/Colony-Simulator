using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Random = UnityEngine.Random;

public class EntityManager : MonoBehaviour {

    public static EntityManager Instance { get; private set; }

    [SerializeField]
    GameObject workerBeePrefab, queenBeePrefab, itemEntityPrefab, droneBeePrefab;

    List<ItemEntity> itemEntities;


    String[] awesomeWorkerBeeNames = new String[] {
        "Bella", "Brianna", "Brooke", "Blair", "Bethany", "Beatrice", "Bree", "Brynlee",
        "Bettie", "Bonnie", "Britney", "Bianca"
    };

    String[] awesomeQueenBeeNames = new String[] {
        "Artemisia", "Elizabeth", "Margaret", "Berenice", "Cleopatra", "Eleanor", "Catherine",
        "Maria", "Christina", "Victoria", "Mathilde", "Caterina", "Nefertiti", "Lady Jane"
    };

    String[] awesomeDroneBeeNames = new String[] {
        "Barry", "Bartholomew", "Ben", "Behemoth", "Bill"
    };

    int zIndex = -2;
    int zIndexItem = -1;

    public void Awake() {
        // Instantiate singleton
        if (Instance != null) {
            Destroy(this);
            return;
        } else Instance = this;

        itemEntities = new();
    }

    public void Start() {
        InstantiateWorker(new Vector2Int(2, 2));
        InstantiateWorker(new Vector2Int(3, 2));
        InstantiateWorker(new Vector2Int(-10, 4));
        InstantiateQueen(new Vector2Int(10, 10));
    }

    public GameObject InstantiateWorker(Vector2 pos) {
        GameObject obj = Instantiate(workerBeePrefab, new Vector3(pos.x, pos.y, zIndex), Quaternion.identity, transform);
        TaskManager.Instance.RegisterAgent(obj.GetComponent<WorkerBehaviour>());

        WorkerBehaviour worker = obj.GetComponent<WorkerBehaviour>();
        int index = Random.Range(0, awesomeWorkerBeeNames.Length);
        worker.SetName(awesomeWorkerBeeNames[index]);

        return obj;
    }

    public GameObject InstantiateDrone(Vector2 pos) {
        GameObject obj = Instantiate(droneBeePrefab, new Vector3(pos.x, pos.y, zIndex), Quaternion.identity, transform);

        DroneBehaviour drone = obj.GetComponent<DroneBehaviour>();
        int index = Random.Range(0, awesomeDroneBeeNames.Length);
        drone.SetName(awesomeDroneBeeNames[index]);

        return obj;
    }

    public GameObject InstantiateQueen(Vector2 pos) {
        GameObject obj = Instantiate(queenBeePrefab, new Vector3(pos.x, pos.y, zIndex), Quaternion.identity, transform);
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

    public GameObject InstantiateItemEntity(Vector2 pos, Item item, uint quantity) {
        GameObject obj = Instantiate(itemEntityPrefab, new Vector3(pos.x, pos.y, zIndexItem), Quaternion.identity, transform);

        ItemEntity itemEntity = obj.GetComponent<ItemEntity>();
        itemEntity.Setup(item, quantity);

        itemEntities.Add(itemEntity);

        return obj;
    }

    public void DestroyEntity(Entity entity) {
        if (entity is ItemEntity itemEntity) {
            itemEntities.Remove(itemEntity);
        }

        Destroy(entity.GetGameObject());
    }

    public bool FindItemEntities(Item item, uint quantity, out List<ItemEntity> result) {
        result = new();
        int target = (int) quantity;
        
        foreach (ItemEntity entity in itemEntities) {
            if (entity.item != item) continue;

            result.Add(entity);
            target -= (int) entity.quantity;

            // Return early if we already reach the target
            if (target <= 0) return true;
        }

        return false;
    }

    public ReadOnlyCollection<ItemEntity> GetItemEntities() {
        return itemEntities.AsReadOnly();
    }
}
