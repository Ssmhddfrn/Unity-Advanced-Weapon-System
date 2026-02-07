using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class GunSystem : MonoBehaviour
{
    [Header("Silah Ayarları")]
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.1f;

    [Header("Mermi Görseli")]
    public GameObject bulletPrefab;
    public Transform barrelTip;
    public float bulletSpeed = 100f;

    [Header("Mermi Ayarları")]
    public int magSize = 30;
    public int totalAmmo = 90;
    private int currentAmmo;
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    [Header("Ses Efektleri")]
    public AudioSource audioSource; // Silahın üzerindeki AudioSource
    public AudioClip gunshotSound;  
    public AudioClip reloadSound;    

    [Header("Referanslar")]
    public Camera fpsCam;
    public GameObject impactEffect;
    public TextMeshProUGUI ammoText;
    public Recoil recoilScript;

    private float nextTimeToFire = 0f;

    void Start() { currentAmmo = magSize; UpdateAmmoUI(); }

    void Update()
    {
        if (isReloading) return;

        if ((currentAmmo <= 0 || Keyboard.current.rKey.wasPressedThisFrame) && currentAmmo < magSize && totalAmmo > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Mouse.current.leftButton.isPressed && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        currentAmmo--;
        UpdateAmmoUI();

        // SES ÇAL: Her mermi çıktığında ateş sesini bir kez oynat.
        if (audioSource != null && gunshotSound != null)
        {
            audioSource.PlayOneShot(gunshotSound);
        }

        if (recoilScript != null) recoilScript.FireRecoil();

        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, range))
        {
            targetPoint = hit.point;
            EnemyTarget target = hit.transform.GetComponent<EnemyTarget>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 1f);
            }
        }
        else
        {
            targetPoint = ray.GetPoint(range);
        }

        Vector3 direction = (targetPoint - barrelTip.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, barrelTip.position, Quaternion.LookRotation(direction));

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null) rb = bullet.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.linearVelocity = direction * bulletSpeed;

        Destroy(bullet, 2f);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        if (ammoText != null) ammoText.text = "Dolduruluyor...";

        // Şarjör değiştirme sesini çal
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);
        int ammoToFill = magSize - currentAmmo;
        if (totalAmmo >= ammoToFill) { currentAmmo += ammoToFill; totalAmmo -= ammoToFill; }
        else { currentAmmo += totalAmmo; totalAmmo = 0; }
        isReloading = false;
        UpdateAmmoUI();
    }

    void UpdateAmmoUI() { if (ammoText != null) ammoText.text = currentAmmo + " / " + totalAmmo; }
}
