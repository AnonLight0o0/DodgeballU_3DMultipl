using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerColor : MonoBehaviourPunCallbacks
{
    public enum PlayerColorID
    {
        Red = 0,
        Blue = 1,
        Green = 2,
        Yellow = 3,
        Purple = 4,
        Orange = 5
    }

    [Header("Player Color")]
    public Renderer[] playerRenderers;

    private static readonly Color[] colors =
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        new Color(0.6f, 0.2f, 1f),
        new Color(1f, 0.5f, 0f)
    };

    private const string ColorProperty = "PlayerColor";

    private void Start()
    {
        if (photonView.IsMine)
        {
            AssignColor();
        }

        ApplyColor();
    }

    public override void OnPlayerPropertiesUpdate(
        Player targetPlayer,
        Hashtable changedProps)
    {
        if (targetPlayer == photonView.Owner &&
            changedProps.ContainsKey(ColorProperty))
        {
            ApplyColor();
        }
    }

    private void AssignColor()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(ColorProperty))
            return;

        int colorIndex =
            (PhotonNetwork.LocalPlayer.ActorNumber - 1) % colors.Length;

        Hashtable properties = new Hashtable
        {
            { ColorProperty, colorIndex }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }

    private void ApplyColor()
    {
        if (photonView.Owner == null)
            return;

        if (!photonView.Owner.CustomProperties.ContainsKey(ColorProperty))
            return;

        int colorIndex =
            (int)photonView.Owner.CustomProperties[ColorProperty];

        if (colorIndex < 0 || colorIndex >= colors.Length)
            return;

        Color selectedColor = colors[colorIndex];

        foreach (Renderer renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.material.color = selectedColor;
            }
        }
    }

    public Color GetPlayerColor()
    {
        if (photonView.Owner != null &&
            photonView.Owner.CustomProperties.ContainsKey(ColorProperty))
        {
            int colorIndex =
                (int)photonView.Owner.CustomProperties[ColorProperty];

            if (colorIndex >= 0 && colorIndex < colors.Length)
                return colors[colorIndex];
        }

        return Color.white;
    }
}