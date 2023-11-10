using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
            Invoke(nameof(Sleep), 0.5f);
    }
    void Sleep()
    {
        GetComponent<Rigidbody>().Sleep();
    }
}
