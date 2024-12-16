using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour {

    public bool IsDead { get; private set; }

    public uint Health { get; private set; }

    [field: SerializeField]
    public uint MaxHealth { get; private set; }

    public uint Nutrition { get; private set; }

    [field: SerializeField]
    public uint MaxNutrition { get; private set; }

    [SerializeField]
    uint hungerRate; // In seconds

    int tick;
    const int TICKS_TO_SECONDS = 50;

    public void Awake() {
        Health = MaxHealth;
        Nutrition = MaxNutrition;

        IsDead = false;

        tick = 0;
    }

    public void FixedUpdate() {
        if (IsDead) return;

        tick += 1;

        if (tick >= TICKS_TO_SECONDS * hungerRate) {
            tick = 0;

            Nutrition = Nutrition == 0 ? 0 : Nutrition - 1;

            if (Nutrition == 0) {
                Damage(1);
            }
        }
    }

    public void Damage(uint amount) {
        Health = Health < amount ? 0 : Health - amount;

        if (Health == 0) IsDead = true;
    }

    public void Heal(uint amount) {
        if (IsDead) return;

        Health = Health + amount > MaxHealth ? MaxHealth : Health + amount;
    }

    public void Feed(Item item, uint quantity) {
        if (IsDead) return;

        FoodComponent fc = item.GetItemComponent(ItemTag.Food) as FoodComponent;
        uint amount = quantity * fc.NutritionalValue;

        Nutrition = Nutrition + amount > MaxNutrition ? MaxNutrition : Nutrition + amount;
    }


    public InfoBranch GetInfoBranch() {
        InfoBranch root = new InfoBranch("Health Information");

        String wellness;
        if (Health == 0) wellness = "Deceased";
        else if (Health <= MaxHealth / 5) wellness = "Critical";
        else if (Health <= 2 * MaxHealth / 5) wellness = "Poor";
        else if (Health <= 3 * MaxHealth / 5) wellness = "Average";
        else if (Health <= 4 * MaxHealth / 5) wellness = "Good";
        else wellness = "Excellent";

        InfoLeaf overviewProperty = new InfoLeaf("General wellness", wellness);
        root.AddChild(overviewProperty);

        if (IsDead == false) {
            InfoLeaf hitpointsProperty = new InfoLeaf("Health points", $"{Health} / {MaxHealth}");
            root.AddChild(hitpointsProperty);

            InfoLeaf hungerProperty = new InfoLeaf("Nutrition", $"{Nutrition} / {MaxNutrition}");
            root.AddChild(hungerProperty);
        }

        return root;
    }

}
