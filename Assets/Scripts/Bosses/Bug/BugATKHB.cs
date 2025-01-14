using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class BugATKHB : Entity
{
    [SerializeField] private Vector2 targetPos, thisPos;
    [SerializeField] private GameObject Bug;
    [SerializeField] private Vector3 changeScale;
    [SerializeField] private int counter;

    [SerializeField] private GameObject AcidHB;

    private void Start()
    {
        changeScale = new Vector3 (0.01f, 0.01f, 0f);
    }

    protected override void FixedUpdate()
    {
        counter++;
        //Debug.Log(counter);

        if (AcidHB.gameObject.activeSelf)
        {
            targetPos = Bug.GetComponent<Bug>().targetPos;
            AcidAttack();
        }
    }

    void AcidAttack()
    {
        thisPos = this.transform.position;
        transform.position = Vector2.MoveTowards(this.transform.position, targetPos, speed * Time.deltaTime);
        if (thisPos == targetPos)
        {
            AcidHB.transform.localScale += changeScale;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {   
        if (other.gameObject.tag == "Player")
        {
            if (counter >= 30)
            {
                counter = 0;
                other.gameObject.GetComponent<Entity>().GetHit(10);
            }
        }
    }
}
