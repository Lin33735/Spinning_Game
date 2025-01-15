using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Creature Property")]
    [SerializeField] protected float maxhealth;
    [SerializeField] protected float speed;
    [SerializeField] protected float damage;
    protected float gethitcd, maxgethitcd;
    public float health { get; set; }


    [Header("Private Componets")]
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Vector2 localScale;
    protected Animator animator;
    protected virtual void Awake()
    {
        health = maxhealth;
        gethitcd = maxgethitcd;
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
    protected virtual void Update()
    {
        
    }
    protected virtual void FixedUpdate()
    {
        if (tag == "Enemy")
        {
            SetSelfAsTarget(2f);
        }
        if (gethitcd > 0)
        {
            gethitcd -= Time.fixedDeltaTime;
        }
    }
    /// <summary>
    /// If distance to cursor is less than certain distance, set itself as the player's target
    /// </summary>
    public void SetSelfAsTarget(float distance)
    {
        if (Vector2.Distance(GameManager.Instance.MousePosition, transform.position) < distance)
        {
            if (GameManager.Instance.PlayerTarget == null)
            {
                GameManager.Instance.PlayerTarget = transform;
            }
        }
        else if (GameManager.Instance.PlayerTarget == transform)
        {
            GameManager.Instance.PlayerTarget = null;
        }
    }
    /// <summary>
    /// Do the get hit behavior, if health less than 0 call the die behavior
    /// </summary>
    public virtual void GetHit(float damage)
    {
        GetHit(damage, Vector2.zero);
    }
    /// <summary>
    /// Do the get hit behavior, if health less than 0 call the die behavior
    /// </summary>
    public virtual void GetHit(float damage,Vector2 knockback)
    {
        if (gethitcd <= 0)
        {
            gethitcd = maxgethitcd;
            health -= damage;
            if (rb) rb.AddForce(knockback);
            StartCoroutine(GetHitEffect(0.2f));
            if (health <= 0)
            {
                DestroyBehavior();
            }
        }
        
    }
    /// <summary>
    /// Die behavior
    /// </summary>
    public virtual void DestroyBehavior()
    {
        Destroy(gameObject);
    }
    /// <summary>
    /// When get hit, set the sprite to white 
    /// </summary>
    public virtual IEnumerator GetHitEffect(float duration)
    {
        float counter=0;
        spriteRenderer.material.SetFloat("_HurtDuration",1);
        while (counter <= duration)
        {
            counter+=Time.deltaTime;
            spriteRenderer.material.SetFloat("_HurtDuration", 1*(1-counter/duration));
            yield return null;
        }
        spriteRenderer.material.SetFloat("_HurtDuration", 0);
        
    }
    /// <summary>
    /// Face to direction, change the scale of x
    /// </summary>
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
