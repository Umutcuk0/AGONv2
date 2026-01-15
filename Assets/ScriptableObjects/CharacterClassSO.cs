using UnityEngine;

[CreateAssetMenu(menuName = "ProjectVisitors/Character Class", fileName = "CC_NewClass")]
public class CharacterClassSO : ScriptableObject
{
    [Header("Identity")]
    public string className = "Rifle";

    [Header("Stats")]
    public int maxHP = 10;
    public int maxAP = 2;
    public int moveRange = 6;
    public int aim = 70;

    [Header("Combat")]
    public int maxAmmo = 6;
    public int damage = 3;

    [Header("Abilities")]
    public bool canOverwatch = true;
    public bool canTakeCover = true;

    [Header("Costs")]
    public int moveAPCost = 1;
}
