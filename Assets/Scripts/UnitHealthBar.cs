using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Unit targetUnit; // Karakterindeki Unit scripti
    [SerializeField] private Image fillImage; // Can barýnýn dolgu görseli (Image Type: Filled olmalý)

    private Transform mainCameraTransform;

    private void Start()
    {
        // Eðer hedef unit atanmadýysa, scriptin bulunduðu objede veya üst objelerde (parent) aramaya çalýþýr.
        if (targetUnit == null)
        {
            targetUnit = GetComponentInParent<Unit>();
        }

        // Kameranýn referansýný alýyoruz (sürekli Camera.main çaðýrmamak için)
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        UpdateHealthBar();
    }

    private void LateUpdate()
    {
        // UI'ýn her zaman kameraya bakmasýný saðlar (Billboard Effect)
        // LateUpdate içinde yapýyoruz ki kamera hareketini tamamladýktan sonra bar dönsün, titreme olmasýn.
        if (mainCameraTransform != null)
        {
            transform.LookAt(transform.position + mainCameraTransform.forward);
        }
    }

    private void UpdateHealthBar()
    {
        // Gerekli referanslar yoksa hata vermemesi için kontrol
        if (targetUnit == null || fillImage == null || targetUnit.characterClass == null) return;

        float currentHp = targetUnit.hp;
        float maxHp = targetUnit.characterClass.maxHP;

        // Can oranýný 0 ile 1 arasýnda bir deðere çevirip Image'in fillAmount özelliðine atýyoruz
        fillImage.fillAmount = currentHp / maxHp;
    }
}
