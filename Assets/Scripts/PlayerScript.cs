using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    //SerializeField face o variabila privata sa apara in editor

    private Rigidbody2D rb;
    private Vector2 movement;
    private float knockbackUntil; // pana cand miscarea e controlata de knockback, nu de input

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;
    }
    //Folosim GetAxisRaw in loc de GetAxis sa primim valori fixe (-1, 1 sau 0) in loc de nr
    //cu virgula (nu folosim controller si chiar daca am folosi, caracterul nostru vrem
    //sa se miste la o viteza neinfluentata de cat de mult e impins joystick-ul)
    //GetAxisRaw primeste valorile -1, 1 sau 0. De exemplu pe orizontala, daca apesi
    //A, GetAxisRaw("Horizontal") returneaza -1, daca apesi D returneaza 1 daca nu apesi
    //ceva returneaza 0. Acelasi concept si pentru Vertical
    //Dupa normalizam vectorul de miscare sa nu ne miscam mai repede pe diagonala

    void FixedUpdate(){
        //In timpul knockback-ului nu suprascriem viteza, altfel impingerea ar disparea instant
        if (Time.time < knockbackUntil) return;
        rb.linearVelocity = movement * speed;
    }
    //FixedUpdate deoarece operatia aceasta tine de fizica

    // Impinge jucatorul (ex: cand e lovit de un inamic) pentru o durata scurta.
    public void ApplyKnockback(Vector2 velocity, float duration){
        knockbackUntil = Time.time + duration;
        rb.linearVelocity = velocity;
    }

    private void OnDisable(){
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
}
