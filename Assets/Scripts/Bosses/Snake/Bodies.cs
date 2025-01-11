using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Bodies : Entity
{
    [SerializeField] public Transform Parent;

    
    public override void Awake()
    {
        base.Awake();

    }



    protected override void Update()
    {
        base.Update();//
        
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!Parent)
        {
            DestroyBehavior();
            return;
        }
        if(Vector2.Distance(transform.position,Parent.position)>0.5f)
            rb.MovePosition(transform.position + (Parent.position-transform.position)*0.08f);
        else if(Vector2.Distance(transform.position, Parent.position) > 4f)
            rb.MovePosition(transform.position + (Parent.position - transform.position) * 1f);

        Vector3 direction = Parent.position - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        if (transform.rotation.z > 90 && transform.rotation.z < 270)
        {
            transform.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
    public override void GetHit(float damage)
    {
        
        if (Parent.GetComponent<Bodies>())
        {
            Parent.GetComponent<Bodies>().GetHit(damage);
        }
        else
        {
            Parent.GetComponent<Entity>().GetHit(damage);
        }
        
    }
    public override void GetHit(float damage, Vector2 knockback)
    {

        GetHit(damage);
    }
}
