using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PlayerPositionGauge : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform gaugeRect;

    [SerializeField] private RectTransform markerPrefab;

    [Header("World Y range")]
    [SerializeField] private float minWorldY = -5f;
    [SerializeField] private float maxWorldY = 15f;

    [Header("Marker")]
    [SerializeField] private int fontSize = 18;
    [SerializeField] private float markerOffsetX = -5f;

    private readonly Dictionary<ulong, RectTransform> markers =
        new Dictionary<ulong, RectTransform>();

    private readonly Dictionary<ulong, TextMeshProUGUI> markerTexts =
        new Dictionary<ulong, TextMeshProUGUI>();

    private void Awake()
    {
        if (gaugeRect == null)
            gaugeRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (gaugeRect == null || markerPrefab == null)
            return;

        PlayerMove[] players = FindObjectsOfType<PlayerMove>();
        HashSet<ulong> existingPlayers = new HashSet<ulong>();

        foreach (PlayerMove player in players)
        {
            if (!player.IsSpawned)
                continue;

            ulong clientId = player.OwnerClientId;
            existingPlayers.Add(clientId);

            if (!markers.ContainsKey(clientId))
                CreateMarker(clientId);

            UpdateMarker(player, clientId);
        }

        List<ulong> removeIds = new List<ulong>();

        foreach (ulong id in markers.Keys)
        {
            if (!existingPlayers.Contains(id))
                removeIds.Add(id);
        }

        foreach (ulong id in removeIds)
        {
            Destroy(markers[id].gameObject);
            markers.Remove(id);
            markerTexts.Remove(id);
        }
    }

    private void CreateMarker(ulong clientId)
    {
        RectTransform markerRect =
            Instantiate(markerPrefab, gaugeRect);

        markerRect.gameObject.SetActive(true);

        TextMeshProUGUI markerText =
            markerRect.GetComponent<TextMeshProUGUI>();

        if (markerText == null)
        {
            Debug.LogError(
                "PlayerMarkerにTextMeshProUGUIが付いていません。"
            );

            Destroy(markerRect.gameObject);
            return;
        }

        markerText.text = "▶     P" + (clientId + 1);
        markerText.fontSize = fontSize;
        markerText.alignment = TextAlignmentOptions.Left;
        markerText.color = GetPlayerColor(clientId);

        markerText.outlineColor = new Color(0f, 0f, 0f, 0.8f);
        markerText.outlineWidth = 0.15f;

        markers.Add(clientId, markerRect);
        markerTexts.Add(clientId, markerText);
    }

    private Color GetPlayerColor(ulong clientId)
    {
        switch (clientId)
        {
            case 0:
                return new Color(1f, 0.35f, 0.25f);
            case 1:
                return new Color(0.3f, 0.7f, 1f);
            case 2:
                return new Color(1f, 0.85f, 0.25f);
            default:
                return Color.white;
        }
    }

    private void UpdateMarker(PlayerMove player, ulong clientId)
    {
        RectTransform marker = markers[clientId];

        float normalizedY = Mathf.InverseLerp(
            minWorldY,
            maxWorldY,
            player.transform.position.y
        );

        normalizedY = Mathf.Clamp01(normalizedY);

        float markerY =
            (normalizedY - 0.5f) * gaugeRect.rect.height;

        marker.anchoredPosition =
            new Vector2(markerOffsetX, markerY);
    }
}