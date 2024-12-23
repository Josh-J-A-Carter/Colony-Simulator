using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectTool : Tool {

    [SerializeField]
    Constructable preview;

    TileManager TM => TileManager.Instance;
    InterfaceManager IM => InterfaceManager.Instance;

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
            // Not a tile entity, so don't need to update the data constantly
            if (tilePreviewData == null) return;

            InfoToUI.DisplayInfoTree(tilePreviewConstructable.GetInfoTree(tilePreviewData));
        }

        else if (selectionType == SelectionType.Entity) {
            InfoToUI.DisplayInfoTree(entityPreviewInfo.GetInfoTree());
        }
    }
    
    void UpdateSelection(HoverData data) {
        HoverType type = data.GetHoverType();

        // Interacting with UI should not remove the selection
        if (type == HoverType.UI) return;


        /// Remove the old selection
        /// 
        SelectionType oldSelectionType = selectionType;

        if (selectionType == SelectionType.Entity) {
            entityPreview.GetComponent<IEntity>()?.ResetOutline();
        }

        else if (selectionType == SelectionType.Tile) {
            TM.RemovePreview(tilePreview);
        }

        /// Set the new selection type
        ///
        if (type == HoverType.None) {
            selectionType = SelectionType.None;
            IM.HideInfoContainer();
            return;
        }

        if (type == HoverType.Entity) {
            entityPreview = data.GetEntityData();
            entityPreviewInfo = entityPreview.GetComponent<IInformative>();

            // If it turns out that this entity does not have a component implementing Informative, reset selection
            if (entityPreviewInfo == null) {
                selectionType = SelectionType.None;
                IM.HideInfoContainer();
                return;
            }

            // Show the info tree in the display panel & outline
            entityPreview.GetComponent<IEntity>()?.SetOutline();
            InfoToUI.DisplayInfoTree(entityPreviewInfo.GetInfoTree());

            selectionType = SelectionType.Entity;
        }

        else if (type == HoverType.Tile) {
            tilePreview = data.GetGridPosition();

            // Careful - there may not even be a tile here
            (Vector2Int startPos, Constructable constructable) = TM.GetConstructableAt(tilePreview);
            tilePreviewConstructable = constructable;
            
            if (tilePreviewConstructable == null) {
                selectionType = SelectionType.None;
                IM.HideInfoContainer();
                return;
            }

            TM.SetPreview(tilePreview, preview);

            if (tilePreviewConstructable is TileEntity) tilePreviewData = TM.GetTileEntityData(startPos);
            else tilePreviewData = null;
            
            // Show the info tree in the display panel
            InfoBranch infoTree = tilePreviewConstructable.GetInfoTree(tilePreviewData);
            InfoToUI.DisplayInfoTree(infoTree);

            selectionType = SelectionType.Tile;
        }

        // New selection is not None, but the old one was, so we need to show the info container
        if (oldSelectionType == SelectionType.None) IM.ShowInfoContainer();
    }

    public override void OnDequip() {
        if (selectionType == SelectionType.None) return;

        if (selectionType == SelectionType.Entity) {
            // To be updated
            // 
            // 
        }

        else if (selectionType == SelectionType.Tile) {
            TM.RemovePreview(tilePreview);
        }

        selectionType = SelectionType.None;

        IM.HideInfoContainer();
    }

}
