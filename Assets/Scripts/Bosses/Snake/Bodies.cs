using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Bodies : Entity
{
    [SerializeField] public Transform Parent;


    protected override void Awake()
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
        Vector2 direction = Parent.position - transform.position;
        float distance = direction.magnitude;

        if (distance > 0.3f && distance <= 4f)
        {
            rb.MovePosition((Vector2)transform.position + direction * 0.1f);
        }
        else if (distance > 4f)
        {
            rb.MovePosition((Vector2)transform.position + direction * 1f);
        }
        else if (distance < 0.2f)  // ����̫��ʱ������ object
        {
            rb.MovePosition((Vector2)transform.position - direction.normalized * 0.1f);
        }

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
        if(Parent)
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
