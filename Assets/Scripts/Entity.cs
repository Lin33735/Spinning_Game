using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Creature Property")]
    [SerializeField] private float maxhealth;
    public float health { get; set; }
    public float speed { get; set; }


    [Header("Private Componets")]
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    private void Awake()
    {
        health = maxhealth;
        rb = rb.GetComponent<Rigidbody2D>();
        spriteRenderer = rb.GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
    public void GetHit(float damage)
    {
        health -= damage;
        StartCoroutine(GetHitEffect(0.5f));
        if (health <= 0)
        {
            DestroyBehavior();
        }
    }
    public void GetHit(float damage,Vector2 knockback)
    {
        health -= damage;
        if(rb)rb.AddForce(knockback,ForceMode2D.Impulse);
        StartCoroutine(GetHitEffect(0.5f));

    }
    public void DestroyBehavior()
    {
        Destroy(gameObject);
    }
    public IEnumerator GetHitEffect(float duration)
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
}
