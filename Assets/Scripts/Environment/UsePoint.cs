using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsePoint : MonoBehaviour
{
    private Environment parent;

    private void Start()
    {
        parent = transform.parent.GetComponent<Environment>();
    }

    private void OnTriggerEnter(Collider player)
    {
        parent.UsePointTriggered(player);
    }
    private void OnTriggerExit(Collider player)
    {
        parent.UsePointUnTriggered(player);
    }
}
