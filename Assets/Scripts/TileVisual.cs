using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TileVisual : MonoBehaviour
{
    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private Color reachableColor = new Color(0.3f, 0.8f, 1f, 1f);
    [SerializeField] private Color hoverColor = Color.yellow;

    private Renderer rend;

    private bool isReachable;
    private bool isHovered;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        UpdateVisual();
    }

    public void SetReachable(bool on)
    {
        isReachable = on;
        UpdateVisual();
    }

    public void SetHovered(bool on)
    {
        isHovered = on;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (rend == null) rend = GetComponent<Renderer>();

        if (isHovered)
            rend.material.color = hoverColor;
        else if (isReachable)
            rend.material.color = reachableColor;
        else
            rend.material.color = baseColor;
    }
}