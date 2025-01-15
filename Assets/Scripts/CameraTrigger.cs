using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    public float CameraSize;
    public Transform CameraTarget;
    public Entity ActiveBoss;
    public GameObject Door;
    bool trigger,hasboss;
    void Start()
    {
        if (ActiveBoss)
        {
            ActiveBoss.enabled = false;
            hasboss = true;
        }

        trigger=false;

    }

    // Update is called once per frame
    void Update()
    {
        if (hasboss && trigger == true)
        {
            if (!ActiveBoss)
            {
                GameManager.Instance.CameraSize.Remove(CameraSize);
                if (CameraTarget)
                    GameManager.Instance.LockPosition = Vector2.zero;
                if (ActiveBoss)
                {
                    ActiveBoss.enabled = false;
                    GameManager.Instance.SetBoss(null);
                }
                if (Door)
                {
                    Door.SetActive(false);
                }
                trigger = false;
            }

        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player"&& trigger==false)
        {
            if (hasboss&&!ActiveBoss)
            {
                return;
            }
            GameManager.Instance.CameraSize.Insert(0, CameraSize);
            if(CameraTarget)
            GameManager.Instance.LockPosition = CameraTarget.position;
            if (ActiveBoss)
            {
                ActiveBoss.enabled = true;
                GameManager.Instance.SetBoss(ActiveBoss);
            }
            if (Door)
            {
                Door.SetActive(true);
            }
            trigger=true;
        }
       
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (hasboss)
            {
                return;
            }
            GameManager.Instance.CameraSize.Remove(CameraSize);
            if (CameraTarget)
                GameManager.Instance.LockPosition = Vector2.zero;
            if (ActiveBoss)
            {
                ActiveBoss.enabled = false;
                GameManager.Instance.SetBoss(null);
            }
            if (Door)
            {
                Door.SetActive(false);
            }
            trigger = false;
        }
    }

}
