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
    public bool isLimit = true;
    public bool attacking = true;
    public string EnemyTag = "Player";
    
    void Start()
    {
        
    }
    protected override void Awake()
    {
        base.Awake();
        localScale = transform.localScale;
        if (GetComponent<CircleCollider2D>())
        localScale = localScale*GetComponent<CircleCollider2D>().radius;
    }
    // Update is called once per frame
    protected override void Update()
    {
        
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        timer += Time.fixedDeltaTime;
        if (isLimit&&timer >= 8)
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
        transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg);
    }
    public virtual void SetProperty(float dam, float speed, Vector2 direction,bool musthit)
    {
        this.damage = dam;
        this.speed = speed;
        Direction = direction;
        rb.velocity += Direction * speed;
        this.musthit = musthit;
        transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(attacking)
        if (HitCount>=0&&collision.tag== EnemyTag && Vector2.Distance(collision.transform.position,transform.position)<= localScale.x+0.5f+Size)
        {
            collision.GetComponent<Entity>().GetHit(damage, Direction.normalized * 10, musthit);
            HitCount -= 1;
            musthit = false;
            if (HitCount <= 0)
            DestroyBehavior();
        }
        if((collision.tag == "Wall") && isBullet)
        {
            if (animator)
            {
                animator.SetTrigger("Set");
            }
            else
            {
                DestroyBehavior();
            }
        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    public override void DestroyBehavior()
    {
            base.DestroyBehavior();
    }
}
