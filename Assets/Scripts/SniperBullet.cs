using UnityEngine;

public class SniperBullet : MonoBehaviour
{
    public float speed = 50f;
    public bool hasHit = false;
    public int damage = 50;

    void Update()
    {
        if (!hasHit)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        hasHit = true;

        // Enemy mi kontrol et
        Unit enemy = other.GetComponent<Unit>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // Ýstersen mermiyi yok et
        // Destroy(gameObject);
    }
}