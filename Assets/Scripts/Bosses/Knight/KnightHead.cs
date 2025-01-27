using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class KnightHead : Entity
{
    [SerializeField] public Transform Parent;
    float y, vy;
    Entity Target;
    public enum State
    {
        Idle,
        Chasing,
    }
    public State curState;
    protected override void Awake()
    {
        base.Awake();
        ChangeState(State.Idle);
    }
    public void Drop(float speedx,float speedy,float angularspeed,Transform Parent)
    {
        y = 3f;
        this.Parent = Parent;
        vy = speedy*Time.fixedDeltaTime;
        rb.angularVelocity = angularspeed;
        rb.AddForce(new Vector2(speedx,0));
        Target=GameManager.Instance.Player;
    }


    protected override void Update()
    {
        base.Update();//
        transform.GetChild(0).eulerAngles += new Vector3(0, 0, transform.eulerAngles.z);
        transform.eulerAngles = Vector3.zero;
    }

    protected override void FixedUpdate()
    {

        base.FixedUpdate();
        if (y > 0)
        {
            y += vy;
            vy -= 2*Time.fixedDeltaTime;
            rb.MovePosition(transform.position + new Vector3(0, vy, 0));
            if (y <= 0)
            {
                rb.MovePosition(transform.position - new Vector3(0, vy, 0));
                y = 0;
                vy = -vy / 2;
                if (vy <= 0.1f)
                {
                    vy=0;
                }
                else
                {
                    y = 0.1f;
                }
            }
        }
        
        if (!Parent)
        {
            DestroyBehavior();
            return;
        }

        FixedUpdateState(curState);
    }

    public void ChangeState(State newState)
    {
        if (curState != newState)
        {
            ExitState(curState);
            curState = newState;
            EnterState(curState);
        }

    }

    void EnterState(State state)
    {
        if (state == State.Chasing)
        {
            rb.AddForce((Target.transform.position - transform.position).normalized * speed*15, ForceMode2D.Impulse);
        }

    }

    void FixedUpdateState(State state)
    {
        if(state == State.Chasing)
        {
            Chasing(Target.transform.position);
            
        }
        if (rb.velocity.magnitude >= 5)
        {
            if (Vector2.Distance(transform.position, Target.transform.position) < 1f)
            {
                Target.GetHit(damage, (Target.transform.position - transform.position).normalized * 10, false);
            }
        }
    }
    public void AnimationTrigger(State state)
    {


    }
    void ExitState(State state)
    {
        if (state == State.Idle)
        {
        }
        if (state == State.Chasing)
        {
            Chasing(Target.transform.position);
        }


    }
    public override bool GetHit(float damage)
    {

        return GetHit(damage, Vector2.zero, false);
    }
    public override bool GetHit(float damage, Vector2 knockback)
    {

        return GetHit(damage, knockback, false);
    }
    public override bool GetHit(float damage, Vector2 knockback, bool musthit)
    {

        rb.AddForce(knockback * 2, ForceMode2D.Impulse);
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
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Wall")
        {
            vy = 0;

        }
    }
    public void Chasing(Vector3 t)
    {
        rb.AddForce((t-transform.position).normalized*speed*10);
        rb.angularVelocity -= rb.velocity.x*5;
    }
}
