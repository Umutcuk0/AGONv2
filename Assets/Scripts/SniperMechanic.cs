using System.Collections;
using UnityEngine;

public class SniperMechanic : MonoBehaviour
{
    [Header("Kameralar")]
    public Camera mainIsoCamera;
    public Camera scopeCamera;
    public Camera bulletCamera;

    [Header("Ateþleme Ayarlarý")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float mouseSensitivity = 2f;

    [Tooltip("Ateþ edildiðinde harcanacak AP miktarý")]
    public int apCost = 1;

    [Header("Yetenek Bekleme Süresi (Cooldown)")]
    public int requiredTurns = 3;
    [HideInInspector] public int currentTurns = 0;

    private bool isAiming = false;
    private float pitch = 0f;
    private float yaw = 0f;

    public void IncreaseTurnCharge()
    {
        if (currentTurns < requiredTurns)
        {
            currentTurns++;
        }
    }

    public void EnterScopeView()
    {
        mainIsoCamera.gameObject.SetActive(false);
        scopeCamera.gameObject.SetActive(true);
        isAiming = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 rot = scopeCamera.transform.localRotation.eulerAngles;
        yaw = rot.y;
        pitch = rot.x;
    }

    void Update()
    {
        if (isAiming)
        {
            AimTarget();

            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }
    }

    private void AimTarget()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        scopeCamera.transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }

    private void Shoot()
    {
        isAiming = false;
        currentTurns = 0; // Cooldown sýfýrla

        // --- AP HARCAMA MANTIÐI ---
        // Karakterin anlýk Unit scriptini bulup içindeki "ap" (anlýk aksiyon puaný) deðerini düþürüyoruz.
        Unit myUnit = GetComponentInParent<Unit>();
        if (myUnit != null)
        {
            myUnit.ap -= apCost;
            if (myUnit.ap < 0) myUnit.ap = 0; // Eksiye düþmesini engelle

            Debug.LogWarning($"[SNIPER] Ateþ edildi! {apCost} AP harcandý. Kalan AP: {myUnit.ap}");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Kamerayý ve tur sýrasýný yönetmesi için karakterin verisini Coroutine'e yolluyoruz
        StartCoroutine(BulletCamRoutine(myUnit));
    }

    private IEnumerator BulletCamRoutine(Unit shooterUnit)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, scopeCamera.transform.rotation);
        SniperBullet bulletScript = bullet.GetComponent<SniperBullet>();

        scopeCamera.gameObject.SetActive(false);
        bulletCamera.gameObject.SetActive(true);

        bulletCamera.transform.SetParent(bullet.transform);
        bulletCamera.transform.localPosition = new Vector3(0f, 1.5f, -2f);
        bulletCamera.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);

        Time.timeScale = 0.2f;

        // Mermi hedefe çarpana veya zaman aþýmýna (Timeout) uðrayana kadar bekle
        yield return new WaitUntil(() => bulletScript == null || bulletScript.hasHit || bulletScript.isTimeout);

        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1f;

        // Kamerayý kurtar ve eski haline dön
        bulletCamera.transform.SetParent(null);
        bulletCamera.gameObject.SetActive(false);
        mainIsoCamera.gameObject.SetActive(true);

        if (bullet != null) Destroy(bullet);

        // --- SÝNEMATÝK BÝTTÝ, ÞÝMDÝ TURU KONTROL ET ---
        // Eðer karakterin AP'si sýfýrlandýysa sýrayý burada düþmana devrediyoruz.
        // Bu sayede kamera aksiyonu izlerken arkada baþka karakterler hareket etmez.
        if (shooterUnit != null && shooterUnit.ap <= 0)
        {
            if (TurnManager.Instance != null && TurnManager.Instance.currentUnit == shooterUnit)
            {
                Debug.LogWarning("[SNIPER] AP sýfýrlandý, sinematik bitti. Tur devrediliyor...");
                TurnManager.Instance.EndCurrentUnitTurn();
            }
        }
    }
}