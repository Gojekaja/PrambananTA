using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Memunculkan tokoh 2D (rig PSB) sebagai billboard di depan kamera AR,
/// lalu memainkan reaksi tokoh (animasi Animator) saat pemain memberikan benda.
/// Dipanggil oleh layar Kamera AR (ARCameraScreen).
/// </summary>
public class TampilKarakterAR : MonoBehaviour
{
    [Serializable]
    public class KarakterAR
    {
        [Tooltip("Sama dengan CharacterData.id, mis. roro_jonggrang")]
        public string idKarakter;
        public GameObject objek;
    }

    [Header("Pengaturan Karakter")]
    public List<KarakterAR> daftarKarakter = new List<KarakterAR>();
    public float jarakKeDepan = 5f;
    public float jarakKeBawah = 1.3f;

    [Header("Parameter Animator")]
    public string trigBerhasil = "trigTerimaBerhasil";
    public string trigGagal = "trigTerimaGagal";
    public string trigTanpaBenda = "trigNoItem";
    public string boolBicara = "isTalking";

    private GameObject karakterAktif;

    public GameObject KarakterAktif => karakterAktif;

    void Awake()
    {
        // Tokoh baru tampil saat bertemu di layar kamera AR
        SembunyikanSemua();
    }

    public bool PunyaModel(string idKarakter) => Cari(idKarakter) != null;

    public GameObject MunculkanDiDepanPemain(string idKarakter)
    {
        var karakter = Cari(idKarakter);
        if (karakter == null) return null;

        SembunyikanSemua();
        Transform kameraAR = Camera.main != null ? Camera.main.transform : null;
        if (kameraAR != null)
        {
            Vector3 posisiBaru = kameraAR.position + (kameraAR.forward * jarakKeDepan);
            posisiBaru.y -= jarakKeBawah;
            karakter.transform.position = posisiBaru;
        }

        karakter.SetActive(true);
        karakterAktif = karakter;
        return karakter;
    }

    // Kompatibel dengan versi lama: munculkan karakter pertama di daftar
    public void MunculkanDiDepanPemain()
    {
        if (daftarKarakter.Count > 0) MunculkanDiDepanPemain(daftarKarakter[0].idKarakter);
    }

    public void SembunyikanSemua()
    {
        foreach (var k in daftarKarakter)
        {
            if (k.objek == null) continue;
            var efek = k.objek.GetComponent<EfekPemberianBunga>();
            if (efek != null) efek.SembunyikanBunga();
            k.objek.SetActive(false);
        }
        karakterAktif = null;
    }

    /// <summary>Reaksi tokoh setelah menerima benda (benar = senang + benda pindah ke tangan).</summary>
    public void ReaksiBenda(bool berhasil, Sprite spriteBenda)
    {
        var animator = AnimatorAktif();
        if (animator != null) animator.SetTrigger(berhasil ? trigBerhasil : trigGagal);
        if (!berhasil || karakterAktif == null) return;

        var efek = karakterAktif.GetComponent<EfekPemberianBunga>();
        if (efek != null) efek.MulaiBerikan(spriteBenda);
    }

    /// <summary>Pemain mengusap tanpa memilih benda.</summary>
    public void ReaksiTanpaBenda()
    {
        var animator = AnimatorAktif();
        if (animator != null) animator.SetTrigger(trigTanpaBenda);
    }

    public void SetBicara(bool bicara)
    {
        var animator = AnimatorAktif();
        if (animator != null) animator.SetBool(boolBicara, bicara);
    }

    void Update()
    {
        if (karakterAktif == null || !karakterAktif.activeInHierarchy || Camera.main == null) return;

        Transform kameraAR = Camera.main.transform;
        Vector3 arahKamera = new Vector3(kameraAR.position.x, karakterAktif.transform.position.y, kameraAR.position.z);
        karakterAktif.transform.LookAt(arahKamera);
    }

    private GameObject Cari(string idKarakter)
    {
        foreach (var k in daftarKarakter)
            if (k.objek != null && k.idKarakter == idKarakter) return k.objek;
        return null;
    }

    private Animator AnimatorAktif() => karakterAktif != null ? karakterAktif.GetComponent<Animator>() : null;
}
