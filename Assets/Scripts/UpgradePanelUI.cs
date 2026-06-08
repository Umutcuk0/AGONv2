using System.Collections.Generic;
using UnityEngine;

public class UpgradePanelUI : MonoBehaviour
{
    public static UpgradePanelUI Instance { get; private set; }

    [Header("Ana Konteynerlar")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Transform leftCardParent;  // Sol kartýn doðacaðý boþ UI nesnesi (Grid/Horizontal Layout)
    [SerializeField] private Transform rightCardParent; // Sað kartýn doðacaðý boþ UI nesnesi

    [Header("Mevcut Geliþtirme Havuzu")]
    [SerializeField] private List<UpgradeData> allAvailableUpgrades;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (mainPanel != null) mainPanel.SetActive(false);
    }

    public void OpenUpgradePanel()
    {
        Debug.LogWarning($"[UI AÇILIYOR] OpenUpgradePanel tetiklendi! Havuzdaki kart sayýsý: {(allAvailableUpgrades != null ? allAvailableUpgrades.Count : 0)}");

        if (allAvailableUpgrades == null || allAvailableUpgrades.Count < 2)
        {
            Debug.LogError("[UI HATA] Kart listeniz BOÞ veya 2'den az! Lütfen Inspector'dan UpgradeData kartlarýný ekleyin.");
            return;
        }

        // 1. ADIM: Eski turlardan kalan kart klonlarý varsa temizle
        CleanParent(leftCardParent);
        CleanParent(rightCardParent);

        // 2. ADIM: Rastgele iki veri seç
        int firstIndex = Random.Range(0, allAvailableUpgrades.Count);
        int secondIndex = Random.Range(0, allAvailableUpgrades.Count);
        while (secondIndex == firstIndex) secondIndex = Random.Range(0, allAvailableUpgrades.Count);

        UpgradeData leftData = allAvailableUpgrades[firstIndex];
        UpgradeData rightData = allAvailableUpgrades[secondIndex];

        // 3. ADIM: Prefab'larý ekranda üret (Spawn)
        SpawnCard(leftData, leftCardParent);
        SpawnCard(rightData, rightCardParent);

        // --- KESÝN GÖRÜNÜRLÜK ÇÖZÜMÜ ---
        // Eðer scriptin takýlý olduðu Ana Canvas kapalýysa önce onu zorla açýyoruz!
        this.gameObject.SetActive(true);

        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
            Debug.LogWarning("[UI BAÞARILI] Hem Ana Canvas hem de Main Panel ekrana çizildi!");
        }
    }

    private void SpawnCard(UpgradeData data, Transform parent)
    {
        if (data == null || data.cardPrefab == null || parent == null) return;

        // Prefab'ý hiyerarþide oluþturuyoruz
        GameObject cardGo = Instantiate(data.cardPrefab, parent);

        // Üzerindeki köprü scriptini bulup veriyi enjekte ediyoruz
        if (cardGo.TryGetComponent(out UpgradeCardSlot slot))
        {
            slot.SetupSlot(data);
        }
    }

    private void CleanParent(Transform parent)
    {
        if (parent == null) return;
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    // --- DEÐÝÞÝKLÝK: 'public' yapýlarak dýþarýdan eriþim saðlandý, hatalý switch-case temizlendi ---
    public void ApplyUpgradeAndClose(UpgradeData upgrade)
    {
        if (upgrade == null) return;

        Debug.LogWarning($"[HUB] Geliþtirme uygulanýyor: {upgrade.name} (+{upgrade.valueModifier})");

        // 1. KONTROL: Kartýn üzerinde doðrudan bir hedef sýnýf dosyasý var mý?
        if (upgrade.targetClass != null)
        {
            // Sahnede OLSUN VEYA OLMASIN, veriyi doðrudan sýnýf dosyasýna yazýyoruz!
            switch (upgrade.type)
            {
                case UpgradeType.MaxHP:
                    upgrade.targetClass.maxHP += upgrade.valueModifier;
                    break;
                case UpgradeType.Damage:
                    upgrade.targetClass.damage += upgrade.valueModifier;
                    break;
                case UpgradeType.AP:
                    upgrade.targetClass.maxAP += upgrade.valueModifier;
                    break;
            }
            Debug.LogWarning($"[HUB BAÞARILI] Geliþtirme uzaktan telsizle doðrudan {upgrade.targetClass.name} sýnýfýna iþlendi!");
        }
        // 2. KONTROL: Hedef sýnýf yoksa (boþ býrakýldýysa), kübe basan karaktere ver
        else if (TurnManager.Instance != null && TurnManager.Instance.currentUnit != null)
        {
            Unit activeUnit = TurnManager.Instance.currentUnit;
            activeUnit.HandleUpgrade(upgrade.type, upgrade.valueModifier);
            Debug.LogWarning($"[HUB BAÞARILI] Geliþtirme küpteki karaktere uygulandý: {activeUnit.name}");
        }
        else
        {
            Debug.LogError("[HUB HATA] Geliþtirme uygulanacak hedef bulunamadý!");
        }

        // --- Kapanýþ ve Temizlik ---
        if (mainPanel != null) mainPanel.SetActive(false);
        if (HubManager.Instance != null) HubManager.Instance.SelfDestroyAndClearGrid();
        if (TurnManager.Instance != null) TurnManager.Instance.EndCurrentUnitTurn();
    }
}