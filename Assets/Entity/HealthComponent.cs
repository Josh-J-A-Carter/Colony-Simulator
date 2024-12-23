using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour {

    public bool IsDead { get; private set; }

    public uint Health { get; private set; }

    [field: SerializeField]
    public uint MaxHealth { get; private set; }
    public bool LowHealth => Health <= MaxHealth / 5;


    public uint Nutrition { get; private set; }

    [field: SerializeField]
    public uint MaxNutrition { get; private set; }
    public bool LowNutrition => Nutrition <= MaxNutrition / 5;

    [SerializeField]
    uint hungerRate; // In seconds
    int tickHunger;
    const int TICKS_TO_SECONDS = 50;

    int tickHungerDamage;
    const int HUNGER_DAMAGE_RATE = 2; // In seconds

    public void Awake() {
        Health = MaxHealth;
        Nutrition = MaxNutrition;

        IsDead = false;

        tickHunger = 0;
        tickHungerDamage = 0;
    }

    public void FixedUpdate() {
        if (IsDead) return;

        if (hungerRate > 0) {
            tickHunger += 1;

            if (tickHunger >= TICKS_TO_SECONDS * hungerRate) {
                tickHunger = 0;

                Nutrition = Nutrition == 0 ? 0 : Nutrition - 1;
            }
        }

        tickHungerDamage += 1;

        if (tickHungerDamage >= HUNGER_DAMAGE_RATE * TICKS_TO_SECONDS) {
            tickHungerDamage = 0;

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

        Heal(fc.HealValue * quantity);
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
