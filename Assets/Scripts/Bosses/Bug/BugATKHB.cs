using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugATKHB : Entity
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<Entity>().GetHit(10);
        }
    }
}
