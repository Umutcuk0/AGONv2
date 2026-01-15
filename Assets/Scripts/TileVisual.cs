using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TileVisual : MonoBehaviour
{
    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private Color reachableColor = new Color(0.3f, 0.8f, 1f, 1f);

    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = baseColor;
    }

    public void SetReachable(bool on)
    {
        if (rend == null) rend = GetComponent<Renderer>();
        rend.material.color = on ? reachableColor : baseColor;
    }
}
