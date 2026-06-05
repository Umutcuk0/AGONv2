using UnityEngine;

// Enum yapýsýný sýnýfýn dýþýnda veya içinde tanýmlayabiliriz. 
// Sýnýfýn dýþýnda (en üstte) tanýmlamak diðer scriptlerin (UpgradePanelUI gibi) buna rahatça ulaþmasýný saðlar.
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
    public GameObject cardPrefab; // Kartýn tüm tasarýmý, yazýlarý ve butonu bu prefab'ýn içinde!

    [Header("Arka Plan Etki Verisi")]
    public UpgradeType type;       // ÖNEMLÝ: Ýþte az önce eksik olan "type" deðiþkeni tam olarak bu!
    public int valueModifier;
}