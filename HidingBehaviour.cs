
using Unity.Netcode;
using UnityEngine;

public class HidingBehaviour : NetworkBehaviour
{
    // Number of hiding spots currently detecting the player.
    private static int activeHidingSpots;

    // True if the player is inside at least one hiding spot.
    public static bool isHiding => activeHidingSpots > 0;

    public float radius = 2f;
    public float offset = 0.2f;

    // This specific hiding spot's state.
    private bool playerInside;

    // Last state that was reported to the server.
    private bool lastHidingState;

    private void Update()
    {
        bool currentlyInside = Physics.CheckSphere(
            new Vector3(
                transform.position.x,
                transform.position.y - offset,
                transform.position.z),
            radius,
            LayerMask.GetMask("Player")
        );

        // Player entered THIS hiding spot.
        if (currentlyInside && !playerInside)
        {
            playerInside = true;
            activeHidingSpots++;
        }
        // Player left THIS hiding spot.
        else if (!currentlyInside && playerInside)
        {
            playerInside = false;
            activeHidingSpots = Mathf.Max(0, activeHidingSpots - 1);
        }

        Debug.Log("Player Hiding? " + isHiding);

        // Offline / single-player
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsListening)
        {
            return;
        }

        // Only the owning player reports their own hiding state.
        if (!IsOwner)
            return;

        // Only send when the GLOBAL hiding state changes.
        if (isHiding == lastHidingState)
            return;

        lastHidingState = isHiding;

        UpdateHidingStateServerRpc(isHiding);
    }

    [ServerRpc]
    private void UpdateHidingStateServerRpc(
        bool hiding,
        ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        MannequinAI.UpdateHidingState(
            clientId,
            hiding);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            new Vector3(
                transform.position.x,
                transform.position.y - offset,
                transform.position.z),
            radius);
    }
}

