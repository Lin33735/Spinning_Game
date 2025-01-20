using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Bullets
{
    // Start is called before the first frame update
    public override void DestroyBehavior()
    {
        animator.Play("Death");
    }
    public void _Destroy()
    {
        Destroy(gameObject);
    }
}
