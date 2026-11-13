/*
Baza pentru obiectele care se iau automat (coin, cheie): plutesc putin pe loc, iar cand
jucatorul trece pe langa ele sunt atrase spre el (magnet) si colectate.
Nu au nevoie de collider: comparam distanta fata de jucator.
*/
using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [SerializeField] private float magnetRadius = 2.5f;
    [SerializeField] private float magnetSpeed = 9f;
    [SerializeField] private float collectDistance = 0.4f;
    [SerializeField] private float activationDelay = 0.35f; // sa se vada drop-ul inainte sa zboare spre jucator
    [SerializeField] private float bobAmplitude = 0.08f;
    [SerializeField] private float bobSpeed = 4f;

    private float spawnTime;
    private Transform visual;
    private bool attracted;

    protected virtual void Awake()
    {
        spawnTime = Time.time;
        visual = transform.childCount > 0 ? transform.GetChild(0) : null;
    }

    private void Update()
    {
        // Mic efect de plutire (doar pe sprite-ul copil, daca exista).
        if (visual != null)
            visual.localPosition = new Vector3(0f, Mathf.Sin((Time.time - spawnTime) * bobSpeed) * bobAmplitude, 0f);

        PlayerHealth player = PlayerHealth.Current;
        if (player == null || player.IsDead) return;
        if (Time.time < spawnTime + activationDelay) return;

        Vector2 toPlayer = player.transform.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance <= collectDistance)
        {
            OnCollected(player);
            Destroy(gameObject);
            return;
        }

        if (attracted || distance <= magnetRadius)
        {
            attracted = true; // odata atras, nu mai scapa
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, magnetSpeed * Time.deltaTime);
        }
    }

    protected abstract void OnCollected(PlayerHealth player);
}
