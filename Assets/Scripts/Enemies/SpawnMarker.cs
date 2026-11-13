/*
Marcajul care apare inainte de un inamic: pulseaza, ca jucatorul sa stie unde va aparea.
*/
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpawnMarker : MonoBehaviour
{
    [SerializeField] private float pulseSpeed = 12f;

    private SpriteRenderer spriteRenderer;
    private Color baseColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.color;
    }

    private void Update()
    {
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed);
        spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (0.35f + 0.65f * pulse));
    }
}
