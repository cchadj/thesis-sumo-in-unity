using UnityEngine;

public class UpwardForce : MonoBehaviour
{
    // Force is an exposed property
    public float force;

    private Rigidbody _rb;

    // Start is called once at the start of the game simulation
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        _rb.AddForce(Vector3.up * force * Time.deltaTime);
    }
}
