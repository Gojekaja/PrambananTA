using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>Satu titik/zona di peta kompleks Candi Prambanan.</summary>
    [Serializable]
    public class MapZone
    {
        public string id;
        public string displayName;
        [TextArea(2, 4)] public string description;
        [Tooltip("Posisi pin pada gambar peta (0..1). (0,0) = kiri bawah.")]
        public Vector2 mapPosition;
        [Header("Geofence (WAJIB dikalibrasi di lokasi)")]
        public double latitude;
        public double longitude;
        public float radiusMeters = 25f;
        [Tooltip("Kosongkan jika zona hanya titik informasi.")]
        public string characterId;
    }

    [CreateAssetMenu(menuName = "Prambanan AR/Zone Database", fileName = "ZoneDatabase")]
    public class ZoneDatabase : ScriptableObject
    {
        [Tooltip("Gambar peta kompleks Prambanan (Texture Type: Sprite 2D and UI).")]
        public Sprite mapSprite;

        [Header("Batas peta untuk konversi GPS -> posisi peta (perkiraan)")]
        public double northLatitude = -7.74108;
        public double southLatitude = -7.76252;
        public double westLongitude = 110.48406;
        public double eastLongitude = 110.50306;

        public List<MapZone> zones = new List<MapZone>();

        public MapZone Find(string id)
        {
            for (int i = 0; i < zones.Count; i++)
                if (zones[i].id == id) return zones[i];
            return null;
        }

        /// <summary>
        /// Konversi koordinat GPS ke posisi normal peta (0..1).
        /// Catatan: peta brosur bersifat skematis (tidak berskala), sehingga hasilnya perkiraan.
        /// </summary>
        public Vector2 GeoToMap(double lat, double lon)
        {
            float x = (float)((lon - westLongitude) / (eastLongitude - westLongitude));
            float y = (float)((lat - southLatitude) / (northLatitude - southLatitude));
            return new Vector2(Mathf.Clamp01(x), Mathf.Clamp01(y));
        }

        public void MapToGeo(Vector2 mapPos, out double lat, out double lon)
        {
            lat = southLatitude + (northLatitude - southLatitude) * mapPos.y;
            lon = westLongitude + (eastLongitude - westLongitude) * mapPos.x;
        }
    }
}
