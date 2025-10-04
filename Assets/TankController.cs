using UnityEngine;

public interface
public class Tank : MonoBehaviour
{
    public float speed = 5.0f;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<RigidBody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {

    }
}
