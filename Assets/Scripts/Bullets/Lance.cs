using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lance : Bullets
{
    public float ScaleValue = 0.1f;
    public bool isattacking=false;
    Vector2 targetposition;
    bool stop;
    void Start()
    {
        stop = false;
    }
    protected override void Awake()
    {
        base.Awake();

    }
    // Update is called once per frame
    protected override void Update()
    {

    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Vector2.Distance(targetposition, transform.position) < 1&&!stop)
        {
            stop=true;
            transform.position = targetposition;
            if (attacking)
            {
                GameManager.Instance.ScreenShake(1, 1);
                attacking = false;
                

            }
            else
            {
                attacking = true;
                if (animator)
                {
                    animator.SetTrigger("Set");
                }
            }

        }
        else
        {
            rb.MovePosition(transform.position + ((Vector3)targetposition - transform.position).normalized * speed);
        }
    }
    public override void SetProperty(float dam, float speed, Vector2 direction)
    {
        this.damage = dam;
        this.speed = speed;

        targetposition = direction;
        
        transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(targetposition.y - transform.position.y, targetposition.x-transform.position.x) * Mathf.Rad2Deg);
    }
    public override void SetProperty(float dam, float speed, Vector2 direction, bool musthit)
    {
        this.damage = dam;
        this.speed = speed;
        targetposition = direction;
        this.musthit = musthit;

        transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(targetposition.y - transform.position.y, targetposition.x - transform.position.x) * Mathf.Rad2Deg);
    }
}
