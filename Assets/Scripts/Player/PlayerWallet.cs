/*
Coin-urile jucatorului. Magazinul de upgrade-uri (alt task) va citi Coins si va apela TrySpend.
*/
using System;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public int Coins { get; private set; }
    public event Action<int> CoinsChanged;

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        CoinsChanged?.Invoke(Coins);
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || Coins < amount) return false;
        Coins -= amount;
        CoinsChanged?.Invoke(Coins);
        return true;
    }
}
