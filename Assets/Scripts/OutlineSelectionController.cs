using UnityEngine;

public class OutlineSelectionController : MonoBehaviour
{
    [Header("Raycast Ayarlarý")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float maxRayDistance = 100f;

    [Header("Görsel Ayarlar")]
    [SerializeField] private Color xrayColor = Color.red;

    // Dýþ çizginin görünür olmasý için 3.0f - 5.0f idealdir
    [SerializeField, Range(0f, 10f)] private float xrayWidth = 3.0f;

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

                // --- HATALI SÝLÜET MODU ÝPTAL EDÝLDÝ ---
                // Sadece dýþ hattý çizen en kararlý mod olan OutlineAll'a geçildi
                currentOutline.OutlineMode = Outline.Mode.OutlineAll;
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