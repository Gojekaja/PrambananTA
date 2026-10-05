using UnityEngine;
using System.Collections;

public class EfekPemberianBunga : MonoBehaviour
{
    [Header("Pengaturan Objek")]
    public Transform objekBunga;      // Tarik objek bunga ke sini
    public Transform objekTangan;     // Tarik objek "Hand" Roro Jonggrang ke sini
    
    [Header("Pengaturan Animasi")]
    public float durasiAnimasi = 1.2f; 
    
    public Vector3 posisiAwalVirtual = new Vector3(0f, 1.5f, -3f);
    
    // Skala saat masih di depan pemain (Zoom In)
    public Vector3 skalaBesar = new Vector3(3f, 3f, 3f); 
    
    // Skala normal saat sudah di tangan karakter (Zoom Out)
    public Vector3 skalaNormal = new Vector3(1f, 1f, 1f);

    public void MulaiBerikanBunga()
    {
        if (objekBunga != null)
        {
            objekBunga.gameObject.SetActive(true);
            StartCoroutine(AnimasiPemberian());
        }
    }

    // Dipakai layar Kamera AR: sprite benda yang dipilih pemain ditampilkan di tangan tokoh
    public void MulaiBerikan(Sprite spriteBenda)
    {
        if (objekBunga == null) return;
        var sr = objekBunga.GetComponent<SpriteRenderer>();
        if (sr != null && spriteBenda != null) sr.sprite = spriteBenda;
        MulaiBerikanBunga();
    }

    public void SembunyikanBunga()
    {
        StopAllCoroutines();
        if (objekBunga != null) objekBunga.gameObject.SetActive(false);
    }

    IEnumerator AnimasiPemberian()
    {
        // Pindah parent ke "Hand" agar bunga ikut bergerak jika karakter bernapas (Idle)
        objekBunga.SetParent(objekTangan);
        SpriteRenderer spriteBunga = objekBunga.GetComponent<SpriteRenderer>();
        
        // Pengecekan agar tidak error
        if (spriteBunga == null) 
        {
            Debug.LogError("Objek bunga tidak memiliki komponen SpriteRenderer!");
            yield break;
        }

        Color warnaBunga = spriteBunga.color;

        // kondisi AWAL
        objekBunga.localPosition = posisiAwalVirtual;
        objekBunga.localScale = skalaBesar;
        warnaBunga.a = 0f; 
        spriteBunga.color = warnaBunga;
        
        objekBunga.gameObject.SetActive(true);

        float waktu = 0f;

        // Proses animasi 
        while (waktu < durasiAnimasi)
        {
            waktu += Time.deltaTime;
            float progress = waktu / durasiAnimasi;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            objekBunga.localPosition = Vector3.Lerp(posisiAwalVirtual, Vector3.zero, smoothProgress);
            objekBunga.localScale = Vector3.Lerp(skalaBesar, skalaNormal, smoothProgress);
            warnaBunga.a = Mathf.Lerp(0f, 1f, smoothProgress);
            spriteBunga.color = warnaBunga;

            yield return null;
        }
        objekBunga.localPosition = Vector3.zero;
        objekBunga.localScale = skalaNormal;
        warnaBunga.a = 1f;
        spriteBunga.color = warnaBunga;
    }

    void LateUpdate()
    {
        if (objekBunga != null && objekBunga.parent == objekTangan)
        {
            // Kunci rotasi global bunga agar selalu tegak (0,0,0)
            objekBunga.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}