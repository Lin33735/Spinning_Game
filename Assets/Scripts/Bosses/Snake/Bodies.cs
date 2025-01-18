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
        health = Parent.GetComponent<Entity>().health;
        float distance = direction.magnitude;

        if (distance > 0.3f && distance <= 4f)
        {
            rb.MovePosition((Vector2)transform.position + direction * 0.1f);
        }
        else if (distance > 4f)
        {
            rb.MovePosition((Vector2)transform.position + direction * 1f);
        }
        else if (distance < 0.2f)  // ¾àÀëÌ«½üÊ±£¬¼·¿ª object
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
    public override bool GetHit(float damage)
    {

        return GetHit(damage, Vector2.zero, false);
    }
    public override bool GetHit(float damage, Vector2 knockback)
    {

        return GetHit(damage,knockback,false);
    }
    public override bool GetHit(float damage, Vector2 knockback,bool musthit)
    {

        if (Parent)
            if (Parent.GetComponent<Bodies>())
            {
                Parent.GetComponent<Bodies>().GetHit(damage, knockback, musthit);
            }
            else
            {
                Parent.GetComponent<Entity>().GetHit(damage, knockback, musthit);
            }
        return true;
    }
}
