using UnityEngine;

public class SniperBullet : MonoBehaviour
{
    public float speed = 50f;
    public bool hasHit = false;
    public int damage = 50;

    public float lifeTime = 3f;
    private float realTimeTimer = 0f;

    [HideInInspector] public bool isTimeout = false; // YENÝ: Zaman aþýmý bayraðý

    void Update()
    {
        // Ne çarptýysa ne de süresi dolduysa hareket et
        if (!hasHit && !isTimeout)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        // Zaman aþýmý kontrolü
        if (!hasHit && !isTimeout)
        {
            realTimeTimer += Time.unscaledDeltaTime;
            if (realTimeTimer >= lifeTime)
            {
                isTimeout = true;
                HideBullet(); // Silme, sadece gizle!
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit || isTimeout) return;
        hasHit = true;

        Unit enemy = other.GetComponent<Unit>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        HideBullet();
    }

    private void HideBullet()
    {
        // Mermiyi görünmez yap ki kamera asýlý kalýrken sahnede sýrýtmasýn
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;

        TrailRenderer tr = GetComponent<TrailRenderer>();
        if (tr != null) tr.enabled = false;
    }
}