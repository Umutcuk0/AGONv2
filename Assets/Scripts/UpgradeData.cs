using UnityEngine;

public enum UpgradeType
{
    MaxHP,
    Damage,
    AP
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "TacticalRPG/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    [Header("Görsel Tasarým")]
    public GameObject cardPrefab;

    [Header("Arka Plan Etki Verisi")]
    public UpgradeType type;
    public int valueModifier;

    // --- YENÝ PROFESYONEL FÝLTRE ---
    [Header("Hedef Sýnýf (Ýsteðe Baðlý)")]
    [Tooltip("Sadece belirli bir sýnýfa (Örn: Sniper) gitmesini istiyorsanýz o sýnýfýn SO dosyasýný buraya sürükleyin. Boþ býrakýrsanýz kübe basan alýr.")]
    public CharacterClassSO targetClass;
}