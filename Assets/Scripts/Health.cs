using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Bullets
{
    // Start is called before the first frame update
    public GameManager.Buff buff;
    public bool hasbuff=false;
    public override void DestroyBehavior()
    {
        if (hasbuff)
        {
            GameManager.Instance.addBuff(buff);
            switch(buff)
            {
                case GameManager.Buff.SnakeBoss:
                    GameManager.Instance.textMeshProUGUI.text = "You gain the ability to *RUN* While spinning";
                    break;
                case GameManager.Buff.PlantBoss:
                    GameManager.Instance.textMeshProUGUI.text = "You gain the ability to Damage Enemys while spinning";
                    break;
                case GameManager.Buff.BugBoss:
                    GameManager.Instance.textMeshProUGUI.text = "You gain the ability to Block *Most* of the Attack while spinning";
                    break;
                case GameManager.Buff.Knight:
                    GameManager.Instance.textMeshProUGUI.text = "You get the *Sword*, Left click when there is no targets. The damage of the sword is depends on your Chargelevel";
                    break;
                case GameManager.Buff.BatBoss:
                    GameManager.Instance.textMeshProUGUI.text = "You gain the ability to Regenerate HP while spinnning";
                    break;
                case GameManager.Buff.SpiderBoss:
                    GameManager.Instance.textMeshProUGUI.text = "Speed Boost";
                    break;
                default:
                    GameManager.Instance.textMeshProUGUI.text = "What the hell is this?";
                    break;
            }
            GameManager.Instance.textMeshProUGUI.fontSize = 30;
            GameManager.Instance.textMeshProUGUI.transform.parent.GetComponent<Animator>().SetTrigger("Set");
            GameManager.Instance.textMeshProUGUI.transform.parent.GetComponent<Animator>().speed = 1;

        }
        if (animator)
        animator.Play("Death");
        else
            _Destroy();
    }
    public void _Destroy()
    {
        
        Destroy(gameObject);
    }
}
