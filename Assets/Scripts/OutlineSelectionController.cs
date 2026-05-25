using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineSelectionController : MonoBehaviour
{
    [Header("Raycast Ayarlarý")]
    [SerializeField] private LayerMask enemyLayer; // Sadece düþmanlarý seçmek için
    [SerializeField] private float maxRayDistance = 100f;

    private Outline currentOutline;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleOutlineSelection();
    }

    private void HandleOutlineSelection()
    {
        // Mouse pozisyonundan ekrana doðru bir ýþýn gönderiyoruz
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRayDistance, enemyLayer))
        {
            // Iþýnýn çarptýðý objedeki veya üst/alt objelerindeki Outline bileþenini alýyoruz
            Outline newOutline = hit.collider.GetComponentInParent<Outline>();

            if (newOutline != null)
            {
                // Eðer imleç hala ayný düþmanýn üzerindeyse hiçbir þey yapma
                if (newOutline == currentOutline) return;

                // Eski düþmanýn outline efektini kapat
                if (currentOutline != null)
                {
                    currentOutline.enabled = false;
                }

                // Yeni düþmanýn outline efektini aç
                currentOutline = newOutline;
                currentOutline.enabled = true;
            }
            else
            {
                // Çarptýðý þey bir düþman ama üzerinde Outline componenti yoksa eskisini kapat
                ClearCurrentOutline();
            }
        }
        else
        {
            // Ýmleç boþluða veya baþka bir nesneye bakýyorsa efekti tamamen kapat
            ClearCurrentOutline();
        }
    }

    private void ClearCurrentOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }
    }
}
