using System;
using System.Collections.Generic;
using UnityEngine;

public class Ferment : State {

    [SerializeField]
    State store, collect;

    [SerializeField]
    GetResources getResources;
    List<(Resource, uint)> resourceRequirements;

    public InventoryManager inventory;

    public override void OnSetup() {
        inventory = entity.GetComponent<InventoryManager>();

        resourceRequirements = new() { ( new(ItemTag.Fermentable), 1) };
    }
    
    public override void OnEntry() {
        // Stuff to collect?
        List<(Vector2Int, BroodComb, Dictionary<String, object>)> fermentableStorage = TileManager.Instance.QueryTileEntities<BroodComb>(
            tuple => tuple.Item2.FermentablesReady(tuple.Item3)
        );

        if (fermentableStorage.Count > 0) {
            stateMachine.SetChildState(collect);
            return;
        }

        // New stuff to ferment
        if (inventory.HasResources(resourceRequirements)) stateMachine.SetChildState(store);
        else {
            getResources.SetResourceRequirements(resourceRequirements.AsReadOnly());
            stateMachine.SetChildState(getResources);
        }
    }

    public override void OnChildExit(State exitingChild, bool success) {
        if (!success) {
            CompleteState(false);
            return;
        }

        // New stuff to ferment
        if (exitingChild == getResources) {
            if (inventory.HasResources(resourceRequirements)) stateMachine.SetChildState(store);
            else {
                getResources.SetResourceRequirements(resourceRequirements.AsReadOnly());
                stateMachine.SetChildState(getResources);
            }
        }
    
        else if (exitingChild == store) {
            // Stuff to collect?
            List<(Vector2Int, BroodComb, Dictionary<String, object>)> fermentableStorage = TileManager.Instance.QueryTileEntities<BroodComb>(
               tuple => tuple.Item2.FermentablesReady(tuple.Item3)
            );

            if (fermentableStorage.Count > 0) stateMachine.SetChildState(collect);
            // Else - done
            else CompleteState();
        }

        else CompleteState();
    }
}
