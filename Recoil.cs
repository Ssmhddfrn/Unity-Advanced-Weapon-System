using UnityEngine;

public class Recoil : MonoBehaviour
{
    // Kameranın Player içindeki orijinal pozisyonunu bozmamak için
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [Header("Geri Tepme Ayarları")]
    public float recoilX = -2f;
    public float recoilY = 1.5f;
    public float snappiness = 10f;
    public float returnSpeed = 5f;

    void Update()
    {
        // Hedef rotayı sıfıra döndür
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        // Yumuşak geçiş yap
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);

        // ÖNEMLİ: Kameranın sadece rotasyonuna ekleme yapıyoruz
        // Bu sayede Player'ın hareketini engellemiyoruz
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void FireRecoil()
    {
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilY, recoilY));
    }
}
