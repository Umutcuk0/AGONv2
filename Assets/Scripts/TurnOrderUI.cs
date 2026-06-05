using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderUI : MonoBehaviour
{
    [Header("UI Elementleri")]
    [SerializeField] private GameObject iconPrefab; // TurnIconPrefab
    [SerializeField] private Transform iconContainer; // Horizontal Layout Group paneli

    [Header("Takým Renkleri")]
    [SerializeField] private Color playerColor = new Color(0f, 0.9f, 0.9f, 1f); // Parlak Dost Turkuaz
    [SerializeField] private Color enemyColor = new Color(0.9f, 0.1f, 0.1f, 1f);  // Kýrmýzý Düþman

    [Header("Görsel Ayarlar")]
    [SerializeField] private int maxVisibleIcons = 5;

    private List<GameObject> spawnedIcons = new List<GameObject>();

    /// <summary>
    /// TurnManager tarafýndan her sýra deðiþtiðinde çaðrýlýr.
    /// </summary>
    public void UpdateTimeline(List<Unit> fullTurnOrder, int currentTurnIndex)
    {
        // 1. Eski ikonlarý temizle
        foreach (var icon in spawnedIcons)
        {
            if (icon != null) Destroy(icon);
        }
        spawnedIcons.Clear();

        if (fullTurnOrder == null || fullTurnOrder.Count == 0 || currentTurnIndex < 0) return;

        // 2. Sonsuz Döngü Sýra Hesaplamasý
        List<Unit> displayOrder = new List<Unit>();

        int aliveUnitCount = 0;
        foreach (var u in fullTurnOrder) if (u != null && !u.IsDead) aliveUnitCount++;
        if (aliveUnitCount == 0) return;

        int iterations = 0;
        int indexOffset = 0;

        while (displayOrder.Count < maxVisibleIcons && iterations < fullTurnOrder.Count * 2)
        {
            int safeIndex = (currentTurnIndex + indexOffset) % fullTurnOrder.Count;
            Unit unit = fullTurnOrder[safeIndex];

            if (unit != null && !unit.IsDead)
            {
                displayOrder.Add(unit);
            }

            indexOffset++;
            iterations++;
        }

        // 3. Ýkonlarý Oluþturma, Renklendirme ve Týklama Entegrasyonu
        for (int i = 0; i < displayOrder.Count; i++)
        {
            Unit unit = displayOrder[i];

            GameObject newIcon = Instantiate(iconPrefab, iconContainer);
            spawnedIcons.Add(newIcon);

            // --- TIKLAMA VE KAMERA ODAKLAMA ÖZELLÝÐÝ ---
            Button iconButton = newIcon.GetComponent<Button>();
            if (iconButton == null) iconButton = newIcon.AddComponent<Button>();

            Unit targetUnit = unit;
            iconButton.onClick.RemoveAllListeners();
            iconButton.onClick.AddListener(() =>
            {
                if (targetUnit != null)
                {
                    IsoTacticalCamera tacticalCam = FindFirstObjectByType<IsoTacticalCamera>();
                    if (tacticalCam != null)
                    {
                        tacticalCam.FocusOnUnitFromUI(targetUnit);
                    }
                }
            });

            // Bileþenleri hiyerarþiden çek
            Image borderImage = newIcon.GetComponent<Image>();
            Image classImage = null;

            Transform childIconTransform = newIcon.transform.Find("ClassIcon");
            if (childIconTransform != null)
            {
                classImage = childIconTransform.GetComponent<Image>();
            }

            Color teamColor = unit.isPlayerUnit ? playerColor : enemyColor;

            // A) Çerçeve (Outline Kutusu) Kontrolü
            if (borderImage != null)
            {
                if (i == 0)
                {
                    borderImage.enabled = false;
                }
                else
                {
                    borderImage.enabled = true;
                    borderImage.color = teamColor;
                }
            }

            // B) Ýkon/Logo Kontrolü
            if (classImage != null)
            {
                if (unit.characterClass != null && unit.characterClass.classIcon != null)
                {
                    classImage.sprite = unit.characterClass.classIcon;
                    classImage.enabled = true;
                    classImage.color = new Color(teamColor.r, teamColor.g, teamColor.b, 1f);
                }
                else
                {
                    classImage.enabled = false;
                }

                // C) Yumuþak Vurgu/Gölge Kontrolü
                Shadow glowEffect = classImage.GetComponent<Shadow>();
                if (glowEffect != null)
                {
                    glowEffect.enabled = (i == 0);
                    if (i == 0)
                    {
                        glowEffect.effectColor = new Color(teamColor.r, teamColor.g, teamColor.b, 0.5f);
                        glowEffect.effectDistance = new Vector2(3f, -3f);
                    }
                }
            }

            // D) Ölçeklendirme
            if (i == 0)
            {
                newIcon.transform.localScale = Vector3.one * 1.3f;
            }
            else
            {
                newIcon.transform.localScale = Vector3.one * 0.9f;
            }
        }
    }

    // =================================================================
    // YENÝ EKLENEN SAVAÞ SONU TEMÝZLÝK FONKSÝYONLARI
    // =================================================================

    /// <summary>
    /// Savaþ bittiðinde (Win/Lose) dýþarýdan çaðrýlarak tüm arayüzü kapatýr ve ikonlarý siler.
    /// </summary>
    public void HideTimeline()
    {
        // Önce ekrandaki klon ikonlarý yok et ki arkada boþuna RAM yemesin
        foreach (var icon in spawnedIcons)
        {
            if (icon != null) Destroy(icon);
        }
        spawnedIcons.Clear();

        // Sonra UI Canvas'ýn veya panelin kendisini tamamen görünmez yap
        gameObject.SetActive(false);
        Debug.LogWarning("[TURN UI] Savaþ bitti, Timeline ekraný kapatýldý.");
    }

    /// <summary>
    /// Yeni bir savaþa baþlandýðýnda arayüzü tekrar görünür yapar.
    /// </summary>
    public void ShowTimeline()
    {
        gameObject.SetActive(true);
        Debug.LogWarning("[TURN UI] Timeline ekraný tekrar aktif edildi.");
    }
}