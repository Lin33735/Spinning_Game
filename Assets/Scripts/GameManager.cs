using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static GameManager Instance;
    public Camera MainCamera;
    public Transform PlayerTarget,Cursor,Cursor2;
    public PlayerMovement Player;
    public Vector2 MousePosition;
    public List<float> CameraSize;
    public Vector2 LockPosition;
    public Slider Slider,PlayerHealthBar,ChargeBar;
    public Entity Boss;
    public GameObject BossPrefrab;
    bool pausing;
    public TextMeshProUGUI textMeshProUGUI;
    public bool isCutscene,isPaused;
    public float DeathCount, Timer;
    public Animator CutScene;
    CameraTrigger Scene;
    public enum Tutorial { 
        Spin,
        Release,
        Plant,
        Bat,
    }
    public List<Tutorial> tutorials = new List<Tutorial>();
    public enum Buff
    {
        SnakeBoss,
        PlantBoss,
        BugBoss,
        BatBoss,
        Knight,
        SpiderBoss,
        FinalBoss,
    }

    public List<Buff> BuffList;
    public AudioSource audioSource;
    public AudioClip[] BGM;
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.MainCamera = MainCamera;
            Instance.textMeshProUGUI = textMeshProUGUI;
            Instance.Cursor = Cursor;
            Instance.Cursor2 = Cursor2;
            Instance.Player = Player;
            Instance.Slider = this.Slider;
            Instance.PlayerHealthBar = this.PlayerHealthBar;
            Instance.ChargeBar = this.ChargeBar;
            Instance.CutScene = CutScene;
            Instance.CameraSize.Clear();
            Instance.CameraSize.Add(7.0f);
            Destroy(gameObject); // 如果已经存在实例，销毁当前对象
            return;
        }

        BlackSceneFadeOut();
        isCutscene = true;
        Instance = this;
        DontDestroyOnLoad(gameObject); // 确保在场景切换时不会销毁


        MainCamera = Camera.main;
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        Player.GetComponent<Rigidbody2D>().MovePosition((Vector2)Player.transform.position+Vector2.up*8);
        audioSource = GetComponent<AudioSource>();
        Cursor2 = Cursor.GetChild(0);
        Application.targetFrameRate = 120;


    }
    public void StartScene()
    {
        Player.ChangeState(PlayerMovement.State.Enter);
    }
    void Start()
    {
        audioSource.volume = 0.2f;
        if (SceneManagerScript.instance.scene == "Cave")
            audioSource.clip = BGM[2];
        else
            audioSource.clip = BGM[0];
        audioSource.Play();
        CameraSize.Insert(0,MainCamera.orthographicSize);
        Slider.gameObject.SetActive(false);
        if (Player)
        {
            PlayerHealthBar.maxValue = Player.health;
            ChargeBar.maxValue = 5;
        }
        PlayerHealthBar.gameObject.SetActive(false);
        ChargeBar.gameObject.SetActive(false);
    }

    // Update is called once per frame
    
    void Update()
    {
        //Getting Mouse Position
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Pause(!isPaused);
        }
        if(!isCutscene)
        Timer += Time.deltaTime;
        if (!Cursor)
        {
            return;
        }
        if (isCutscene)
        {
            Cursor.gameObject.SetActive(false);
        }
        else
        {
            Cursor.gameObject.SetActive(true);
        }
        Vector3 mouseScreenPosition = Input.mousePosition;
        MousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, 0));
        if (PlayerTarget)
            Cursor.position = PlayerTarget.position;
        else if (Player.Target)
            Cursor.position = Player.Target.position;
        else
            Cursor.position = MousePosition;
        if (Player.Target)
        {
            Cursor2.transform.position = Player.transform.position;
            Cursor2.transform.position += (Vector3)(MousePosition- (Vector2)Player.transform.position) * (0.1f+(Player.ChargeLevel/6)*0.5f);
            Cursor2.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2((MousePosition - (Vector2)Player.transform.position).y, (MousePosition - (Vector2)Player.transform.position).x) * Mathf.Rad2Deg);
        }
        else
        {
            Cursor2.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Mouse0) &&!isCutscene)
        {
            if(!Player.Target && Player.currentState != PlayerMovement.State.Attacking && PlayerTarget)
            {
                Player.Target = PlayerTarget;
                Player.ChangeState(PlayerMovement.State.Attacking);
                Cursor2.gameObject.SetActive(true);
                Cursor2.transform.position = Player.transform.position;
                Cursor2.transform.position += (Vector3)(MousePosition - (Vector2)Player.transform.position) * (0.1f + (Player.ChargeLevel / 6) * 0.5f);
                Cursor2.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2((MousePosition - (Vector2)Player.transform.position).y, (MousePosition - (Vector2)Player.transform.position).x) * Mathf.Rad2Deg);
            }
            else if(!Player.isspinning&&BuffList.Contains(Buff.Knight))
            {
                Player.ChangeState(PlayerMovement.State.SwingSword);
            }

        }
        
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
           
            Player.Target = null;
        }
        

    }
    private void FixedUpdate()
    {
        if (isPaused)
        {
            Time.timeScale = 0;
            if (Input.GetKey(KeyCode.Space))
            {
                Pause(!isPaused);
            }
        }
        if (pausing||!Player)
        {
            return;
        }
        Vector2 camposition = Player.transform.position;
        if (Player)
        {

            if (Player.Target)
            {
                camposition += ((Vector2)Player.Target.transform.position - camposition) * 0.8f;
            }
        }
        if (CameraSize.Count == 1||Boss)
        {
            MainCamera.orthographicSize += (CameraSize[0] - 1 + (Player.isrunning ? 2 : 0) + (Player.currentState == PlayerMovement.State.Idle ? -1 : 0) - MainCamera.orthographicSize) * 0.1f;
        }
        else
        {
            MainCamera.orthographicSize += (CameraSize[0] - MainCamera.orthographicSize) * 0.1f;
        }
        if (LockPosition != Vector2.zero)
        {
            camposition = LockPosition;
        }
        MainCamera.transform.position += (new Vector3(camposition.x, camposition.y, MainCamera.transform.position.z) - MainCamera.transform.position) * 0.1f;
        if (Boss)
        {
           
            if (Slider.maxValue< Boss.health)
            {
                Slider.maxValue = Boss.health;
            }
            Slider.value += (Boss.health- Slider.value)*0.1f;
        }
        else
        {
            
        }
        if (Player)
        {
            if(PlayerHealthBar.value!= Player.health)
            {
                PlayerHealthBar.value += (Player.health - PlayerHealthBar.value) * 0.1f;
                if(PlayerHealthBar.maxValue < Player.health)
                {
                    PlayerHealthBar.maxValue = Player.health;
                }
            }
            if (ChargeBar.value != Player.ChargeLevel)
            {
                ChargeBar.value += (Player.ChargeLevel - ChargeBar.value) * 0.1f;
                if (ChargeBar.maxValue < 6)
                {
                    ChargeBar.maxValue = 6;
                }
            }

        }
    }
    public void SetBoss(Entity boss, CameraTrigger trigger,bool ismusic)
    {
        Scene = trigger;
        Boss = boss;
        if (boss != null)
        {
            BossPrefrab = Instantiate(Boss.gameObject);
            BossPrefrab.SetActive(false);
        }

        if (boss)
            Slider.maxValue = Boss.health;
        if (boss != null)
        {
            Slider.gameObject.SetActive(true);
            PlayerHealthBar.gameObject.SetActive(true);
            ChargeBar.gameObject.SetActive(true);
            if(!ismusic)
            if (SceneManagerScript.instance.scene == "Cave")
            {
                StartCoroutine(ChangeMusic(BGM[3], 0.03f));
            }
            else
            {
                StartCoroutine(ChangeMusic(BGM[1], 0.03f));
            }
            audioSource.Play();
        }
        else
        {
            Slider.gameObject.SetActive(false);
            PlayerHealthBar.gameObject.SetActive(false);
            ChargeBar.gameObject.SetActive(false);
            Player.ChargeLevel = 0;
            if (!ismusic)
                if (SceneManagerScript.instance.scene == "Cave")
            {
                StartCoroutine(ChangeMusic(BGM[2], 0.2f));
            }
            else
            {
                StartCoroutine(ChangeMusic(BGM[0], 0.2f));
            }

            audioSource.Play();
        }
    }
    public void SetBoss(Entity boss,CameraTrigger trigger)
    {
        SetBoss(boss,trigger,false);
    }
    public void PauseTime(float time, float charge)
    {
        if(time!=0)
        StartCoroutine(StopTime(time, charge));
    }
    public void ScreenShake(float time, float charge)
    {
        
        StartCoroutine(Shake(time, charge));
    }
    IEnumerator StopTime(float time,float charge)
    {
        float count=0;
        Time.timeScale = 0f;
        pausing = true;

        while (count < time)
        {
            if (!Player)
            {
                yield break;
            }
            count += Time.fixedUnscaledDeltaTime*5;
            Vector2 camposition = (Vector2)Player.transform.position+(new Vector2(Random.Range(1,-1), Random.Range(1, -1))* charge * (1-count/ time));
            MainCamera.transform.position += (new Vector3(camposition.x, camposition.y, MainCamera.transform.position.z) - MainCamera.transform.position) * 0.3f;
            MainCamera.orthographicSize += (8 - MainCamera.orthographicSize) * 0.3f;
            yield return null;
        }
        Time.timeScale = 1f;
        pausing=false;
        yield return null;
    }
    IEnumerator Shake(float time, float charge)
    {
        float count = 0;
        Vector2 campoint = MainCamera.transform.position;
        while (count < time)
        {
            if (!Player)
            {
                yield break;
            }
            count += Time.fixedUnscaledDeltaTime * 4;
            Vector2 camposition = (Vector2)campoint + (new Vector2(Random.Range(1, -1), Random.Range(1, -1)) * charge * (1 - count / time));
            MainCamera.transform.position += (new Vector3(camposition.x, camposition.y, MainCamera.transform.position.z) - MainCamera.transform.position) * 0.3f;
            yield return null;
        }
        yield return null;
    }
    public void addBuff(Buff buff)
    {
        if(!BuffList.Contains(buff))
        BuffList.Add(buff);
    }
    public void EnterCutScene()
    {
        CutScene.Play("CutScene");
    }
    public void ExitCutScene()
    {
        CutScene.Play("Null");
    }
    public void BlackSceneFadeOut()
    {
        CutScene.Play("Reverse");
    }
    public void EnterCutScene2()
    {
        CutScene.Play("CG", -1, 0f);
    }
    public void PlayCG(string n)
    {
        CutScene.Play(n, -1, 0f);
    }
    public void EndCG(string n)
    {
        CutScene.Play(n, -1, 1f);
    }
    public void ResetBoss()
    {
        Bullets[] bulletsScripts = FindObjectsOfType<Bullets>();
        foreach (Bullets g in bulletsScripts)
        {
            Destroy(g.gameObject);
        }
        if (Scene.CameraTarget == Boss.transform)
        {
            Scene.CameraTarget = BossPrefrab.transform;
        }
        print(Boss.name);
        Destroy(Boss.gameObject);
        BossPrefrab.SetActive(true);
        Scene.ActiveBoss = BossPrefrab.GetComponent<Entity>();
        Scene.OpenDoor();
        SetBoss(null,Scene);
        //SetBoss(BossPrefrab.GetComponent<Entity>(),Scene);
    }
    public void PlaySound(AudioClip clip)
    {
        StartCoroutine(ChangeMusic(clip, 0.05f));
    }
    public void PlaySound(AudioClip clip,float sound)
    {
        StartCoroutine(ChangeMusic(clip, sound));
    }
    public IEnumerator ChangeMusic(AudioClip clip,float volue)
    {
        
        float timer=0;
        float maxtime = audioSource.volume;
        while (timer <= audioSource.volume)
        {
            timer += maxtime / 30;
            audioSource.volume -= maxtime / 30;
            yield return null;
        }
        maxtime = volue*2;
        audioSource.volume = 0;
        timer = 0;
        audioSource.clip = clip;
        audioSource.Play();
        while (timer <= maxtime)
        {
            timer += maxtime / 30;
            audioSource.volume += maxtime / 30;
            yield return null;
        }
        audioSource.volume = maxtime*2;
        yield return null;
    }
    public void SendText(string t)
    {
        if (t == "")
        {
            Instance.textMeshProUGUI.transform.parent.GetComponent<Animator>().Play("Null");
            return;
        }
        Instance.textMeshProUGUI.text = t;
        Instance.textMeshProUGUI.fontSize = 20;
        Instance.textMeshProUGUI.transform.parent.GetComponent<Animator>().SetTrigger("Set");
        Instance.textMeshProUGUI.transform.parent.GetComponent<Animator>().speed = 1;
    }
    public void Pause(bool pause)
    {
        isPaused = pause;
        if (pause)
        {
            PlayCG("Pause");
            Time.timeScale = 0;
        }
        else
        {
            PlayCG("Null");
            Time.timeScale = 1;
        }
    }
}
