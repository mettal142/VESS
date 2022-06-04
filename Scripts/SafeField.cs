using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeField : MonoBehaviour
{
    public GameObject Field;
    // Start is called before the first frame update
    void Start()
    {
        Place();
    }
    
    private void Place()
    {
        this.transform.position = new Vector3(Field.transform.position.x + Random.Range(-40f, 40f), Field.transform.position.y + 0.5f, Field.transform.position.z + Random.Range(-40f, 40f));
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.transform.CompareTag("safefield"))
            Place();
    }
}
