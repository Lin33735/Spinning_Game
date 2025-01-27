using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    public float CameraSize;
    public Transform CameraTarget;
    public Entity ActiveBoss;
    public GameObject Door;
    bool trigger,hasboss, triggered;
    public string text;
    public bool hasText,changeScene;
    public float TextSpeed, TextSize;
    public string loadedScene;
    public bool lockposition = true;
    public AudioClip clip;
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
        
    }
    private void FixedUpdate()
    {
        if (hasboss && trigger == true)
        {
            if (!ActiveBoss)
            {
                GameManager.Instance.Player.SavePoint = transform.position;
                GameManager.Instance.CameraSize.Remove(CameraSize);
                GameManager.Instance.LockPosition = Vector2.zero;
                if (ActiveBoss)
                {
                    ActiveBoss.enabled = false;
                    
                }
                GameManager.Instance.SetBoss(null, this);
                if (Door)
                {
                    Door.SetActive(false);
                }
                trigger = false;
            }

        }
    }
    public void OpenDoor()
    {
        GameManager.Instance.CameraSize.Remove(CameraSize);
        GameManager.Instance.LockPosition = Vector2.zero;

        if (Door)
        {
            Door.SetActive(false);
        }
        trigger = false;
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
                if(GameManager.Instance.Boss!= ActiveBoss)
                GameManager.Instance.SetBoss(ActiveBoss,this, clip!=null);
                ActiveBoss.enabled = true;
            }
            if (Door)
            {
                Door.SetActive(true);
            }
            trigger=true;
            
            if (hasText && triggered == false)
            {
                GameManager.Instance.textMeshProUGUI.text = text;
                GameManager.Instance.textMeshProUGUI.fontSize = TextSize;
                GameManager.Instance.textMeshProUGUI.transform.parent.GetComponent<Animator>().SetTrigger("Set");
                GameManager.Instance.textMeshProUGUI.transform.parent.GetComponent<Animator>().speed = TextSpeed;
            }
            if (clip)
            {
                GameManager.Instance.PlaySound(clip,0.4f);
            }
            if (changeScene)
            {
                SceneManagerScript.instance.LoadScene(loadedScene);
            }
            triggered = true;
        }
       
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player" && trigger == true&& hasboss&&lockposition)
            if (CameraTarget)
            {
                GameManager.Instance.LockPosition = (CameraTarget.position+GameManager.Instance.Player.transform.position)/2;
            }
            else
            {
                GameManager.Instance.LockPosition = Vector2.zero;
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
                GameManager.Instance.SetBoss(null,this);
            }
            if (Door)
            {
                Door.SetActive(false);
            }
            trigger = false;
        }
    }

}
