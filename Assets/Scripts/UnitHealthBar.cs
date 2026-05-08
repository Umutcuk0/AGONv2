using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Unit targetUnit;
    [SerializeField] private Image fillImage;

    [Header("Ammo UI")]
    [SerializeField] private GameObject ammoLinePrefab; // Tek bir mermi çizgisinin prefab'ý
    [SerializeField] private Transform ammoContainer;   // Mermilerin dizileceði parent obje (HorizontalLayoutGroup)

    private Transform mainCameraTransform;
    private List<GameObject> ammoLines = new List<GameObject>(); // Mermi objelerini hafýzada tutmak için liste

    private void Start()
    {
        if (targetUnit == null)
        {
            targetUnit = GetComponentInParent<Unit>();
        }

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        UpdateHealthBar();
        UpdateAmmoBar(); // Update içine mermi kontrolünü ekliyoruz
    }

    private void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            transform.LookAt(transform.position + mainCameraTransform.forward);
        }
    }

    private void UpdateHealthBar()
    {
        if (targetUnit == null || fillImage == null || targetUnit.characterClass == null) return;

        float currentHp = targetUnit.hp;
        float maxHp = targetUnit.characterClass.maxHP;
        fillImage.fillAmount = currentHp / maxHp;
    }

    private void UpdateAmmoBar()
    {
        if (targetUnit == null || ammoLinePrefab == null || ammoContainer == null) return;

        int currentAmmo = targetUnit.ammo;
        int maxAmmo = targetUnit.characterClass.maxAmmo;

        while (ammoLines.Count < maxAmmo)
        {
            GameObject newAmmoLine = Instantiate(ammoLinePrefab, ammoContainer);
            ammoLines.Add(newAmmoLine);
        }

        for (int i = 0; i < ammoLines.Count; i++)
        {
            // Silah deðiþtirme vb. durumlar için max mermiden fazlasýný kökten kapatýyoruz
            if (i >= maxAmmo)
            {
                ammoLines[i].SetActive(false);
            }
            else
            {
                // Objenin KENDÝSÝ hep açýk kalsýn ki Layout Group düzeni kaydýrmasýn
                ammoLines[i].SetActive(true);

                // Objenin üzerindeki Image bileþenine ulaþýyoruz
                Image ammoImage = ammoLines[i].GetComponent<Image>();
                if (ammoImage != null)
                {
                    // Eðer i, mevcut mermiden küçükse görünür (true) yap, deðilse gizle (false)
                    // Mermi doluysa opak (Alpha: 1), boþsa yarý saydam (Alpha: 0.2f) yap
                    if (i < currentAmmo)
                    {
                        ammoImage.color = new Color(ammoImage.color.r, ammoImage.color.g, ammoImage.color.b, 1f);
                    }
                    else
                    {
                        ammoImage.color = new Color(ammoImage.color.r, ammoImage.color.g, ammoImage.color.b, 0.2f);
                    }
                }
            }
        }
    }
}