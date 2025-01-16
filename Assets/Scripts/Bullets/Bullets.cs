using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullets : Entity
{
    // Start is called before the first frame update
    [SerializeField] public Vector2 Direction;
    float timer;

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
        if (timer >= 10)
        {
            DestroyBehavior();
        }
    }
    
    public void SetProperty(float dam,float speed,Vector2 direction)
    {
        this.damage = dam;
        this.speed = speed;
        Direction = direction;
        rb.velocity += Direction * speed;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag=="Player")
        {
            collision.GetComponent<PlayerMovement>().GetHit(damage,Direction.normalized*10,false);
            DestroyBehavior();
        }
        if((collision.tag == "Wall"))
        DestroyBehavior();
    }
}
