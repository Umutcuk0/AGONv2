using UnityEngine;

public class OutlineSelectionController : MonoBehaviour
{
    [Header("Raycast Ayarlarý")]
    [SerializeField] private LayerMask enemyLayer; // "Enemy" layer'ý
    [SerializeField] private float maxRayDistance = 100f;

    [Header("Görsel Ayarlar")]
    [SerializeField] private Color xrayColor = Color.red;

    // Burayý Inspector'dan 0.5 yapabilirsin. Ýnce çizgiler için hassas aralýk verdik.
    [SerializeField, Range(0f, 10f)] private float xrayWidth = 0.5f;

    private Outline currentOutline;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleOutlineAndXraySelection();
    }

    private void HandleOutlineAndXraySelection()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRayDistance, enemyLayer))
        {
            Outline newOutline = hit.collider.GetComponentInParent<Outline>();

            if (newOutline != null)
            {
                if (newOutline == currentOutline) return;

                if (currentOutline != null)
                {
                    currentOutline.enabled = false;
                }

                currentOutline = newOutline;

                // Mod ayarlarý ve senin belirlediðin ince geniþlik (0.5f) uygulanýyor
                currentOutline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
                currentOutline.OutlineColor = xrayColor;
                currentOutline.OutlineWidth = xrayWidth;

                currentOutline.enabled = true;
            }
            else
            {
                ClearCurrentOutline();
            }
        }
        else
        {
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