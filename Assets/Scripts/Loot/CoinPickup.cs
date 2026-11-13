using UnityEngine;

public class CoinPickup : Pickup
{
    [SerializeField] private int value = 1;

    protected override void OnCollected(PlayerHealth player)
    {
        PlayerWallet wallet = player.GetComponent<PlayerWallet>();
        if (wallet != null)
            wallet.AddCoins(value);
    }
}
