using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectTool : Tool {

    [SerializeField]
    Constructable preview;

    TileManager TM => TileManager.Instance;
    InterfaceManager IM => InterfaceManager.Instance;

    enum SelectionType { Entity, Tile, None };
    SelectionType selectionType;

    // Entity preview data
    GameObject entityPreview;
    Informative entityPreviewInfo;

    // Tile preview data
    Vector2Int tilePreview;
    Constructable tilePreviewConstructable;
    Dictionary<String, object> tilePreviewData;

    public override void Run(HoverData data) {
        if (Input.GetKeyDown(KeyCode.Mouse0)) UpdateSelection(data);
    }
    
    void UpdateSelection(HoverData data) {
        HoverType type = data.GetHoverType();

        // Interacting with UI should not remove the selection
        if (type == HoverType.UI) return;


        /// Remove the old selection
        /// 
        SelectionType oldSelectionType = selectionType;

        if (selectionType == SelectionType.Entity) {
            // To be updated
            // 
            // 
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
            // Get informative component??? idk man

            // Show the info tree in the display panel
            // ...

            selectionType = SelectionType.Entity;
        }

        else if (type == HoverType.Tile) {
            tilePreview = data.GetGridPosition();

            // Careful - there may not even be a tile here
            (_, tilePreviewConstructable) = TM.GetConstructableAt(tilePreview);
            if (tilePreviewConstructable == null) {
                selectionType = SelectionType.None;
                IM.HideInfoContainer();
                return;
            }

            TM.SetPreview(tilePreview, preview);

            if (tilePreviewConstructable is TileEntity) tilePreviewData = TM.GetTileEntityData(tilePreview);
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
