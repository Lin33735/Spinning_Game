using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullets : Entity
{
    // Start is called before the first frame update
    [SerializeField] public Vector2 Direction;
    [SerializeField] protected int HitCount=10;
    protected float timer;
    public bool musthit = false;
    public bool isBullet = true;
    void Start()
    {
        
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
        timer += Time.fixedDeltaTime;
        if (timer >= 4)
        {
            DestroyBehavior();
        }
    }
    
    public virtual void SetProperty(float dam,float speed,Vector2 direction)
    {
        this.damage = dam;
        this.speed = speed;
        Direction = direction;
        rb.velocity += Direction * speed;
    }
    public virtual void SetProperty(float dam, float speed, Vector2 direction,bool musthit)
    {
        this.damage = dam;
        this.speed = speed;
        Direction = direction;
        rb.velocity += Direction * speed;
        this.musthit = musthit;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag=="Player")
        {
            collision.GetComponent<PlayerMovement>().GetHit(damage,Direction.normalized*10, musthit);
            HitCount -= 1;
            musthit = false;
            if (HitCount<=0)
            DestroyBehavior();
        }
        if((collision.tag == "Wall")&&isBullet)
        DestroyBehavior();
    }
}
