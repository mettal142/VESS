using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public GameObject Field;
    public float HP=100f;
    public Rigidbody rb;
    public void Dead()
    {
        HP = 100f;
        this.transform.position = new Vector3(Field.transform.position.x + Random.Range(-47f, 47f), Field.transform.position.y + 1, Field.transform.position.z + Random.Range(-47f, 47f));
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        this.transform.position = new Vector3(Field.transform.position.x + Random.Range(-47f, 47f), Field.transform.position.y + 1, Field.transform.position.z + Random.Range(-47f, 47f));
    }

    void Update()
    {
        if(HP<=0)
        {
            Dead();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("safefield"))
            Dead();
    }
}
