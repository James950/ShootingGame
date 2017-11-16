using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Anim
{
    public AnimationClip idle;
    public AnimationClip runForward;
    public AnimationClip runBackward;
    public AnimationClip runLeft;
    public AnimationClip runRight;
}



public class PlayerCtrl : MonoBehaviour {
    private float h = 0.0f;
    private float v = 0.0f;

    private Transform tr;

    public float moveSpeed = 10.0f;

    public float rotSpeed = 100.0f;

    public Anim anim;

    public Animation _animation;

    public int hp = 100;
    //생명초깃값
    private int initHp;
    //hpbar이미지
    public Image imgHpbar;

    //델게이트 및 이벤트 선언
    public delegate void PlayerDiedHandler();
    public static event PlayerDiedHandler OnPlayerDie;

    private GameMgr gameMgr;

    // Use this for initialization
    void Start () {
        //생명초깃값 설정
        initHp = hp;

        tr = GetComponent<Transform>();

        _animation = GetComponentInChildren<Animation>();
        _animation.clip = anim.idle;
        _animation.Play();

        //GameMgr 스크립트 할당
        gameMgr = GameObject.Find("GameManager").GetComponent<GameMgr>();
    }
	
	// Update is called once per frame
	void Update () {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        Debug.Log("H=" + h.ToString());


        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

        tr.Translate(moveDir.normalized * Time.deltaTime * moveSpeed, Space.Self);

        tr.Rotate(Vector3.up * Time.deltaTime * rotSpeed * Input.GetAxis("Mouse X"));

        //애니메이션 블렌딩(부드러운 움직임변화)
        if (v >= 0.1f)
        {
            _animation.CrossFade(anim.runForward.name, 0.3f);
        }
        else if (v <= -0.1f)
        {
            _animation.CrossFade(anim.runBackward.name, 0.3f);
        }
        else if (h >= 0.1f)
        {
            _animation.CrossFade(anim.runRight.name, 0.3f);
        }
        else if (h <= -0.1f)
        {
            _animation.CrossFade(anim.runLeft.name, 0.3f);
        }
        else
        {
            _animation.CrossFade(anim.idle.name, 0.3f);
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "PUNCH")
        {
            hp -= 10;
            //Image UI 항목의 fillAmount 속성을 조절해 생명 게이지 값 조절
            imgHpbar.fillAmount = (float)hp / (float)initHp;
            Debug.Log("Player HP =" + hp.ToString());

            if (hp <= 0)
            {
                PlayerDie();
            }
        }
    }
    void PlayerDie()
    {
        Debug.Log("Player Die !!");
        /*
        //MONSTER라는 Tag를 가진 모든 게임오브젝느를 찾음
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //모든 몬스터의 onplayerdie 함수를 순차적으로 호출
        foreach (GameObject monster in monsters)
        {
            monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
        }
        */
        OnPlayerDie();
        //몬스텉 출현 중지
        //gameMgr.isGameOver = true;

        //GameMgr의 싱글턴 인스턴스를 접근해 isGameOver 변숫값을 변경
        GameMgr.instance.isGameOver = true;
    }
}
