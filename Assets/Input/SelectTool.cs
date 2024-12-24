using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class SelectTool : Tool {

    [SerializeField]
    Constructable preview;

    int tick;
    const int MAX_TICKS = TileManager.TICK_RATE;

    enum SelectionType { Entity, Tile, None };
    SelectionType selectionType = SelectionType.None;

    // Entity preview data
    GameObject entityPreview;
    IInformative entityPreviewInfo;

    // Tile preview data
    Vector2Int tilePreview;
    Constructable tilePreviewConstructable;
    Dictionary<String, object> tilePreviewData;

    ReadOnlyCollection<Task> tilePreviewTasks;

    public override void OnEquip() {
        tick = TileManager.Instance.GetTileEntityTick();
    }

    public override void Run(HoverData data) {
        if (Input.GetKeyDown(KeyCode.Mouse0)) UpdateSelection(data);
    }

    public override void FixedRun() {
        tick += 1;

        if (tick >= MAX_TICKS) {
            tick = 0;

            if (selectionType != SelectionType.None) UpdateInfo();
        }
    }

    void UpdateInfo() {
        if (selectionType == SelectionType.Tile) {
            UpdateTileSelection();
        }

        else if (selectionType == SelectionType.Entity) {
            InfoToUI.DisplayInfoTree(entityPreviewInfo.GetInfoTree());
        }
    }
    
    void UpdateSelection(HoverData data) {
        HoverType type = data.GetHoverType();

        // Interacting with UI should not remove the selection
        if (type == HoverType.UI) return;

        // Remove previous selection
        if (selectionType == SelectionType.Entity) {
            entityPreview.GetComponent<IEntity>()?.ResetOutline();
        }

        else if (selectionType == SelectionType.Tile) {
            TileManager.Instance.RemovePreview(tilePreview);
        }


        ///////// Set the new selection type

        // Empty selection
        if (type == HoverType.None) {
            ResetSelection();
            return;
        }

        // Entity selection
        if (type == HoverType.Entity) {
            entityPreview = data.GetEntityData();
            entityPreviewInfo = entityPreview.GetComponent<IInformative>();

            // If it turns out that this entity does not have a component implementing Informative, reset selection
            if (entityPreviewInfo == null) {
                ResetSelection();
                return;
            }

            // Show the info tree in the display panel & outline
            selectionType = SelectionType.Entity;

            entityPreview.GetComponent<IEntity>()?.SetOutline();

            InfoToUI.DisplayInfoTree(entityPreviewInfo.GetInfoTree());
            InterfaceManager.Instance.ShowInfoContainer();
            InterfaceManager.Instance.HideTaskInfoContainer();
        }

        // Selection at a location (i.e. tiles OR tasks)
        else if (type == HoverType.Tile) {
            tilePreview = data.GetGridPosition();

            UpdateTileSelection();
        }
    }

    void UpdateTileSelection() {
        // Careful - there may not even be a tile here
        (Vector2Int startPos, Constructable constructable) = TileManager.Instance.GetConstructableAt(tilePreview);
        tilePreviewConstructable = constructable;

        tilePreviewTasks = TaskManager.Instance.GetTasksAt(tilePreview);

        if (tilePreviewConstructable == null && tilePreviewTasks.Count == 0) {
            ResetSelection();
            return;
        }
        
        selectionType = SelectionType.Tile;
        TileManager.Instance.SetPreview(tilePreview, preview);
    
        if (tilePreviewConstructable != null) {
            if (tilePreviewConstructable is TileEntity) tilePreviewData = TileManager.Instance.GetTileEntityData(startPos);
            else tilePreviewData = null;
            
            // Show the info tree in the display panel
            InfoBranch infoTree = tilePreviewConstructable.GetInfoTree(tilePreviewData);
            InfoToUI.DisplayInfoTree(infoTree);
            InterfaceManager.Instance.ShowInfoContainer();
        }

        else InterfaceManager.Instance.HideInfoContainer();

        if (tilePreviewTasks.Count > 0) {
            InfoBranch taskTree = new(String.Empty);

            foreach (Task task in tilePreviewTasks) taskTree.AddChild(task.GetInfoTree());

            InfoToUI.DisplayTaskTree(taskTree);
            InterfaceManager.Instance.ShowTaskInfoContainer();
        }

        else InterfaceManager.Instance.HideTaskInfoContainer();
    }

    void ResetSelection() {
        if (selectionType == SelectionType.Tile) TileManager.Instance.RemovePreview(tilePreview);

        selectionType = SelectionType.None;

        InterfaceManager.Instance.HideInfoContainer();
        InterfaceManager.Instance.HideTaskInfoContainer();
    }

    public override void OnDequip() {
        if (selectionType == SelectionType.None) return;

        if (selectionType == SelectionType.Entity) {
            entityPreview.GetComponent<IEntity>()?.ResetOutline();
        }

        else if (selectionType == SelectionType.Tile) {
            TileManager.Instance.RemovePreview(tilePreview);
        }

        selectionType = SelectionType.None;

        InterfaceManager.Instance.HideInfoContainer();
        InterfaceManager.Instance.HideTaskInfoContainer();
    }

}
