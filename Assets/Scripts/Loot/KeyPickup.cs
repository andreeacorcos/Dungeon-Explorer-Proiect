/*
Cheia lasata de ultimul inamic. Doar anunta ca a fost luata;
trecerea la nivelul urmator (usa, incarcarea scenei) e partea de nivele.
*/
using System;
using UnityEngine;

public class KeyPickup : Pickup
{
    public static event Action KeyCollected;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => KeyCollected = null;

    protected override void OnCollected(PlayerHealth player)
    {
        Debug.Log("Cheia a fost colectata! Nivelul poate continua.");
        KeyCollected?.Invoke();
    }
}
