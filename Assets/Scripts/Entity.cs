using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Creature Property")]
    [SerializeField] protected float maxhealth;
    [SerializeField] protected float speed;
    public float health { get; set; }


    [Header("Private Componets")]
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Vector2 localScale;
    protected Animator animator;
    public virtual void Awake()
    {
        health = maxhealth;

        if (!GetComponent<Rigidbody2D>())
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        else
        {
            rb = GetComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        localScale = transform.localScale;
    }
    
    public virtual void GetHit(float damage)
    {
        health -= damage;
        StartCoroutine(GetHitEffect(0.5f));
        if (health <= 0)
        {
            DestroyBehavior();
        }
    }
    public virtual void GetHit(float damage,Vector2 knockback)
    {
        health -= damage;
        if(rb)rb.AddForce(knockback,ForceMode2D.Impulse);
        StartCoroutine(GetHitEffect(0.5f));
        if (health <= 0)
        {
            DestroyBehavior();
        }
    }
    public virtual void DestroyBehavior()
    {
        Destroy(gameObject);
    }
    public virtual IEnumerator GetHitEffect(float duration)
    {
        float counter=0;
        spriteRenderer.material.SetFloat("_HurtDuration",1);
        while (counter <= duration)
        {
            counter+=Time.fixedDeltaTime;
            spriteRenderer.material.SetFloat("_HurtDuration", 1*(1-counter/duration));
            yield return null;
        }
        spriteRenderer.material.SetFloat("_HurtDuration", 0);
        
    }
    public virtual void FaceTo(Vector2 position)
    {
        if (position.x > transform.position.x)
        {
            transform.localScale = new Vector2(localScale.x, localScale.y);
        }
        else
        {
            transform.localScale = new Vector2(-localScale.x, localScale.y);
        }
    }

}
