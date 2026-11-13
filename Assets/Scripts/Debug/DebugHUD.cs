/*
TEMPORAR, doar pentru testare: afiseaza viata, coin-urile si valul curent in coltul ecranului.
Se inlocuieste cu UI-ul real (health bar, contor de coin-uri) cand e gata.
*/
using UnityEngine;

public class DebugHUD : MonoBehaviour
{
    [SerializeField] private WaveSpawner waveSpawner;

    private bool keyCollected;
    private GUIStyle style;

    private void OnEnable() => KeyPickup.KeyCollected += OnKeyCollected;
    private void OnDisable() => KeyPickup.KeyCollected -= OnKeyCollected;
    private void OnKeyCollected() => keyCollected = true;

    private void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            style.normal.textColor = Color.white;
        }

        PlayerHealth player = PlayerHealth.Current;
        PlayerWallet wallet = player != null ? player.GetComponent<PlayerWallet>() : null;

        string text = "[DEBUG]\n";
        text += player != null ? $"Viata: {player.CurrentHealth}/{player.MaxHealth}\n" : "Viata: -\n";
        text += $"Coins: {(wallet != null ? wallet.Coins : 0)}\n";
        if (waveSpawner != null)
        {
            text += waveSpawner.Finished
                ? "Toate valurile terminate\n"
                : $"Val: {waveSpawner.CurrentWave}/{waveSpawner.TotalWaves}  Inamici: {waveSpawner.AliveCount}\n";
        }
        text += keyCollected ? "Cheie: DA" : "Cheie: nu";

        GUI.Label(new Rect(12, 8, 400, 140), text, style);
    }
}
