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

    [Header("Yetenek Bekleme Süresi (Cooldown)")]
    public int requiredTurns = 3; // Kaç turda dolacaðý
    [HideInInspector] public int currentTurns = 0; // Þu anki dolum seviyesi

    private bool isAiming = false;
    private float pitch = 0f;
    private float yaw = 0f;

    // Tur baþladýðýnda dolumu 1 artýracak fonksiyon
    public void IncreaseTurnCharge()
    {
        if (currentTurns < requiredTurns)
        {
            currentTurns++;
            Debug.Log($"[SNIPER] Yetenek Doluyor: {currentTurns}/{requiredTurns}");
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

        // Ateþ edildiðinde yetenek sayacýný sýfýrla!
        currentTurns = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(BulletCamRoutine());
    }

    private IEnumerator BulletCamRoutine()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, scopeCamera.transform.rotation);
        SniperBullet bulletScript = bullet.GetComponent<SniperBullet>();

        scopeCamera.gameObject.SetActive(false);
        bulletCamera.gameObject.SetActive(true);

        bulletCamera.transform.SetParent(bullet.transform);
        bulletCamera.transform.localPosition = new Vector3(0f, 1.5f, -2f);
        bulletCamera.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);

        Time.timeScale = 0.2f;

        yield return new WaitUntil(() => bulletScript.hasHit);

        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1f;

        bulletCamera.transform.SetParent(null);
        bulletCamera.gameObject.SetActive(false);
        mainIsoCamera.gameObject.SetActive(true);

        Destroy(bullet);
    }
}