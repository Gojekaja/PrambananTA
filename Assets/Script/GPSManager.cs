using System;
using System.Collections;
using PrambananAR.UI;
using TMPro;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

/// <summary>
/// Membaca GPS perangkat lalu menghitung jarak ke setiap zona pada ZoneDatabase (geofence).
/// UI cukup berlangganan OnLokasiBerubah (peta) atau memanggil ZonaDimasuki() (kamera AR).
/// </summary>
public class GPSManager : MonoBehaviour
{
    public static GPSManager Instance { get; private set; }

    [Header("Data Zona (geofence)")]
    public ZoneDatabase zoneDatabase;

    [Header("UI Teks (opsional, untuk debug di lapangan)")]
    public TextMeshProUGUI koordinatText;

    [Header("Titik Target cadangan (dipakai bila ZoneDatabase kosong)")]
    public float targetLat = -7.752020f;
    public float targetLon = 110.491467f;
    public float batasJarakGeofence = 10f;

    [Header("Pengaturan GPS")]
    public float akurasiMeter = 5f;
    public float jarakUpdateMeter = 1f;
    [Tooltip("Nyalakan GPS setelah orang tua menekan SETUJU pada popup persetujuan.")]
    public bool tungguPersetujuan = true;

    [Header("Simulasi di Editor (tanpa GPS)")]
    [Tooltip("Hanya berlaku di Unity Editor: pakai koordinat di bawah sebagai posisi pemain.")]
    public bool gunakanPosisiSimulasi = false;
    public double simulasiLintang = -7.752020;
    public double simulasiBujur = 110.491467;

    public bool LokasiAktif { get; private set; }
    public double Lintang { get; private set; }
    public double Bujur { get; private set; }
    public string Status { get; private set; } = "GPS belum aktif";

    /// <summary>Dipanggil setiap ada data lokasi baru (lintang, bujur).</summary>
    public event Action<double, double> OnLokasiBerubah;

    private double timestampTerakhir = -1;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    IEnumerator Start()
    {
        if (tungguPersetujuan)
        {
            while (!PlayerProgressService.Data.consentGiven)
            {
                SetStatus("Menunggu persetujuan orang tua...");
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

#if UNITY_EDITOR
        if (gunakanPosisiSimulasi)
        {
            SetStatus("Simulasi GPS (Editor)");
            TerimaLokasi(simulasiLintang, simulasiBujur);
            yield break;
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            // Beri waktu dialog izin Android tampil & dijawab
            float tunggu = 0f;
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) && tunggu < 15f)
            {
                tunggu += 0.5f;
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }
#endif
        yield return StartLocationService();
    }

    IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            SetStatus("GPS HP belum dinyalakan!");
            yield break;
        }

        SetStatus("Mencari sinyal GPS...");
        Input.location.Start(akurasiMeter, jarakUpdateMeter);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSecondsRealtime(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            SetStatus("Gagal mendapat sinyal GPS.");
            yield break;
        }
        SetStatus("GPS aktif");
    }

    void Update()
    {
        if (Input.location.status != LocationServiceStatus.Running) return;

        // Hanya proses bila sensor memberi data baru (hemat baterai & CPU)
        var data = Input.location.lastData;
        if (data.timestamp == timestampTerakhir) return;
        timestampTerakhir = data.timestamp;
        TerimaLokasi(data.latitude, data.longitude);
    }

    void OnApplicationQuit()
    {
        if (Input.location.status == LocationServiceStatus.Running) Input.location.Stop();
    }

    private void TerimaLokasi(double lintang, double bujur)
    {
        Lintang = lintang;
        Bujur = bujur;
        LokasiAktif = true;
        TulisDebug();
        OnLokasiBerubah?.Invoke(lintang, bujur);
    }

    // ------------------------------------------------------------------
    // Geofence
    // ------------------------------------------------------------------

    /// <summary>Jarak pemain ke zona (meter). -1 bila lokasi belum tersedia.</summary>
    public float JarakKeZona(MapZone zona)
    {
        if (!LokasiAktif || zona == null) return -1f;
        return HitungJarakMeters((float)Lintang, (float)Bujur, (float)zona.latitude, (float)zona.longitude);
    }

    /// <summary>Zona terdekat yang lolos filter (mis. hanya zona bertokoh). Null bila tidak ada.</summary>
    public MapZone ZonaTerdekat(out float jarak, Func<MapZone, bool> filter = null)
    {
        jarak = float.MaxValue;
        MapZone terdekat = null;
        if (!LokasiAktif || zoneDatabase == null) return null;

        foreach (var zona in zoneDatabase.zones)
        {
            if (filter != null && !filter(zona)) continue;
            float d = JarakKeZona(zona);
            if (d < jarak)
            {
                jarak = d;
                terdekat = zona;
            }
        }
        return terdekat;
    }

    /// <summary>Zona yang radiusnya sedang dimasuki pemain (terdekat bila tumpang tindih).</summary>
    public MapZone ZonaDimasuki(Func<MapZone, bool> filter = null)
    {
        var zona = ZonaTerdekat(out float jarak, filter);
        return zona != null && jarak <= zona.radiusMeters ? zona : null;
    }

    public float HitungJarakMeters(float lat1, float lon1, float lat2, float lon2)
    {
        float R = 6371000f;
        float dLat = (lat2 - lat1) * Mathf.Deg2Rad;
        float dLon = (lon2 - lon1) * Mathf.Deg2Rad;

        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(lat1 * Mathf.Deg2Rad) * Mathf.Cos(lat2 * Mathf.Deg2Rad) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return R * c;
    }

    // ------------------------------------------------------------------
    // Teks debug
    // ------------------------------------------------------------------
    private void SetStatus(string status)
    {
        Status = status;
        if (koordinatText != null && !LokasiAktif) koordinatText.text = status;
    }

    private void TulisDebug()
    {
        if (koordinatText == null) return;

        var zona = ZonaTerdekat(out float jarak);
        if (zona == null)
            jarak = HitungJarakMeters((float)Lintang, (float)Bujur, targetLat, targetLon);

        koordinatText.text = "Lat: " + Lintang.ToString("F6") + "\nLon: " + Bujur.ToString("F6") +
                             "\n\nJarak ke " + (zona != null ? zona.displayName : "Target") + ":\n" +
                             jarak.ToString("F2") + " Meter";

        float batas = zona != null ? zona.radiusMeters : batasJarakGeofence;
        if (jarak <= batas) koordinatText.text += "\n\nAnda di dalam area geofence!";
    }
}
