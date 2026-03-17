using UnityEngine;

public class TileHoverHighlighter : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask tileLayer;

    private Tile currentHoveredTile;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        UpdateHoveredTile();
    }

    private void UpdateHoveredTile()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        Tile newHoveredTile = null;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileLayer))
        {
            newHoveredTile = hit.collider.GetComponent<Tile>();

            if (newHoveredTile == null)
                newHoveredTile = hit.collider.GetComponentInParent<Tile>();
        }

        if (currentHoveredTile == newHoveredTile) return;

        // Eski hover kapat
        if (currentHoveredTile != null)
        {
            TileVisual oldVis = currentHoveredTile.GetComponent<TileVisual>();
            if (oldVis != null)
                oldVis.SetHovered(false);
        }

        currentHoveredTile = newHoveredTile;

        // Yeni hover aç
        if (currentHoveredTile != null)
        {
            TileVisual newVis = currentHoveredTile.GetComponent<TileVisual>();
            if (newVis != null)
                newVis.SetHovered(true);
        }
    }

    public Tile GetHoveredTile()
    {
        return currentHoveredTile;
    }

    public void ClearHover()
    {
        if (currentHoveredTile != null)
        {
            TileVisual vis = currentHoveredTile.GetComponent<TileVisual>();
            if (vis != null)
                vis.SetHovered(false);
        }

        currentHoveredTile = null;
    }
}