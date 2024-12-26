using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "ScriptableObjects/Constructable/Flower Constructable")]
public class Flower : TileEntity, IProducer {

    ReadOnlyCollection<Item> productionItemTypes;

    [SerializeField]
    protected bool isPollenProducer;

        [SerializeField]
        Item pollen;

        [SerializeField]
        int minPollenDelay, maxPollenDelay;
        
        [SerializeField]
        uint minPollenQuantity, maxPollenQuantity;

        /// <summary> Ready to harvest pollen? </summary>
        public const String POLLEN__IS_READY = "pollen__isReady";

        /// <summary> Number of ticks until the next pollen harvest.</summary>
        public const String POLLEN__TIME_LEFT = "pollen__timeLeft";

        /// <summary> Quantity of pollen for the upcoming harvest.</summary>
        public const String POLLEN__QUANTITY = "pollen__quantity";


    [SerializeField]
    protected bool isNectarProducer;

        [SerializeField]
        Item nectar;

        [SerializeField]
        int minNectarDelay, maxNectarDelay;
        
        [SerializeField]
        uint minNectarQuantity, maxNectarQuantity;

        /// <summary> Ready to harvest nectar? </summary>
        public const String NECTAR__IS_READY = "nectar__isReady";

        /// <summary> Number of ticks until the next nectar harvest.</summary>
        public const String NECTAR__TIME_LEFT = "nectar__timeLeft";

        /// <summary> Quantity of nectar for the upcoming harvest.</summary>
        public const String NECTAR__QUANTITY = "nectar__quantity";


    public override Dictionary<String, object> GenerateDefaultData() {
        Dictionary<String, object> data = new();

        ResetNectarData(data);
        ResetPollenData(data);

        return data;
    }

    void ResetPollenData(Dictionary<String, object> data) {
        if (isPollenProducer) {
            data[POLLEN__IS_READY] = false;
            data[POLLEN__TIME_LEFT] = Random.Range(minPollenDelay, maxPollenDelay);
            data[POLLEN__QUANTITY] = (uint) Random.Range(minPollenQuantity, maxPollenQuantity);
        }
    }
    void ResetNectarData(Dictionary<String, object> data) {
        if (isNectarProducer) {
            data[NECTAR__IS_READY] = false;
            data[NECTAR__TIME_LEFT] = Random.Range(minNectarDelay, maxNectarDelay);
            data[NECTAR__QUANTITY] = (uint) Random.Range(minNectarQuantity, maxNectarQuantity);
        }
    }

    public override void TickInstance(Vector2Int position, Dictionary<string, object> instance) {
        if (isPollenProducer) {
            int timeLeft = (int) instance[POLLEN__TIME_LEFT];
            timeLeft = timeLeft == 0 ? 0 : timeLeft - 1;
            instance[POLLEN__TIME_LEFT] = timeLeft;

            if (timeLeft == 0) instance[POLLEN__IS_READY] = true;
        }

        if (isNectarProducer) {
            int timeLeft = (int) instance[NECTAR__TIME_LEFT];
            timeLeft = timeLeft == 0 ? 0 : timeLeft - 1;
            instance[NECTAR__TIME_LEFT] = timeLeft;

            if (timeLeft == 0) instance[NECTAR__IS_READY] = true;
        }
    }

    public ReadOnlyCollection<Item> ProductionItemTypes() {
        if (productionItemTypes == null) {
            List<Item> list = new();

            if (isNectarProducer) list.Add(nectar);
            if (isPollenProducer) list.Add(pollen);

            productionItemTypes = list.AsReadOnly();
        }

        return productionItemTypes;
    }

    bool HasPollen(Dictionary<String, object> instance) {
        if (!isPollenProducer) return false;

        return (bool) instance[POLLEN__IS_READY];
    }

    (Item, uint) CollectPollen(Dictionary<String, object> instance) {
        if (!HasPollen(instance)) return (null, 0);

        uint quantity = (uint) instance[POLLEN__QUANTITY];

        ResetPollenData(instance);

        return (pollen, quantity);
    }

    bool HasNectar(Dictionary<String, object> instance) {
        if (!isNectarProducer) return false;

        return (bool) instance[NECTAR__IS_READY];
    }

    (Item, uint) CollectNectar(Dictionary<String, object> instance) {
        if (!HasNectar(instance)) return (null, 0);

        uint quantity = (uint) instance[NECTAR__QUANTITY];

        ResetNectarData(instance);

        return (nectar, quantity);
    }

    public List<Item> AvailableProductionItemTypes(Dictionary<String, object> instance) {
        List<Item> available = new();

        if (isNectarProducer && (bool) instance[NECTAR__IS_READY]) available.Add(nectar);
        if (isPollenProducer && (bool) instance[POLLEN__IS_READY]) available.Add(pollen);

        return available;
    }

    public List<(Item, uint)> CollectAll(Dictionary<string, object> instance) {
        List<(Item, uint)> harvest = new();

        if (isNectarProducer && (bool) instance[NECTAR__IS_READY]) harvest.Add(CollectNectar(instance));
        if (isPollenProducer && (bool) instance[POLLEN__IS_READY]) harvest.Add(CollectPollen(instance));

        return harvest;
    }


    public override InfoBranch GetTileEntityInfoTree(Dictionary<String, object> instance) {
        InfoBranch root = new InfoBranch(String.Empty);

        InfoBranch flowerCategory = new InfoBranch("Flower properties");
        root.AddChild(flowerCategory);

        InfoLeaf pollenProducerProperty = new InfoLeaf("Pollen producer", isPollenProducer.ToString());
        flowerCategory.AddChild(pollenProducerProperty);

        if (isPollenProducer) {
            if (HasPollen(instance)) {
                InfoLeaf hasPollenProperty = new InfoLeaf("Pollen is ready to harvest");
                flowerCategory.AddChild(hasPollenProperty);
            } else {
                int secondsLeft = (int) ((int) instance[POLLEN__TIME_LEFT] * TileManager.TICKS_TO_SECONDS);
                InfoLeaf pollenTimeProperty = new InfoLeaf("Time until pollen harvest", secondsLeft + "s");
                flowerCategory.AddChild(pollenTimeProperty);
            }
        }

        InfoLeaf nectarProducerProperty = new InfoLeaf("Nectar producer", isNectarProducer.ToString());
        flowerCategory.AddChild(nectarProducerProperty);

        if (isNectarProducer) {
            if (HasNectar(instance)) {
                InfoLeaf hasNectarProperty = new InfoLeaf("Nectar is ready to harvest");
                flowerCategory.AddChild(hasNectarProperty);
            } else {
                int secondsLeft = (int) ((int) instance[NECTAR__TIME_LEFT] * TileManager.TICKS_TO_SECONDS);
                InfoLeaf nectarTimeProperty = new InfoLeaf("Time until nectar harvest", secondsLeft + "s");
                flowerCategory.AddChild(nectarTimeProperty);
            }
        }
                
        return root;
    }
}

