using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    // Inspector'da Canvas'ýn altýndaki Text objesini buraya sürükle
    [SerializeField] private TextMeshProUGUI textMesh;

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float lifeTime = 0.8f;

    public void Setup(int damageAmount)
    {
        if (textMesh != null)
            textMesh.text = damageAmount.ToString();

        Destroy(gameObject, lifeTime);
    }
    // ... Update fonksiyonu ayný kalýyor


private void Update()
    {
        // Sayýyý yukarý doðru hareket ettir
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Kameraya bakmasýný saðla (Billboard)
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}