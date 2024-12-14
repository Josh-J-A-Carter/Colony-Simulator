using UnityEngine;

public class ForageTool : Tool {
  
    public override void Run(HoverData hoverData) {
        if (Input.GetKeyDown(KeyCode.Mouse0)) UpdateSelection(hoverData);
    }

    void UpdateSelection(HoverData data) {
        
        HoverType type = data.GetHoverType();

        // Interacting with UI should not remove the selection
        if (type != HoverType.Tile) return;

        Vector2Int pos = data.GetGridPosition();

        // Careful - there may not even be a tile here
        (_, Constructable constructable) = TileManager.Instance.GetConstructableAt(pos);
        if (constructable is IProducer producer) {
            TaskManager.Instance.CreateTask(new ForageTask(pos, producer));
            Debug.Log("Task created :)");
        }

    }
}
