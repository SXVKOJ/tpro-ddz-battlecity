using UnityEngine;
using UnityEngine.UIElements;

public class Tank : MonoBehaviour
{
    public float speed = 5.0f;
    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            float mv = Input.GetAxis("Vertical");
            Vector2 mov = new Vector2(0.0f, mv);
            rb.AddForce(mov * speed);

            Debug.Log("Mov W");
        }
    }

    void OnCollisionEnter(Collision collision)
    {

    }
}
