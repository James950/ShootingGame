using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterCtrl : MonoBehaviour {

    //몬스터의 상태 정복 ㅏ있는 Enumerable 변수 선언 << 열거형 변수 설정
    public enum MonsterState { idle, trace, attack, die };
    //몬스터의 현재 상태 정보 저장할 Enum 변수
    public MonsterState monsterState = MonsterState.idle;

    private Transform monsterTr;
    private Transform playerTr;
    private UnityEngine.AI.NavMeshAgent nvAgent;
    private Animator animator;

    //추적 사정거리
    public float traceDist = 10.0f;
    //공격 사정거리
    public float attackDist = 2.0f;
    //몬스터의 사망 여부
    private bool isDie = false;
    //혈흔 효과 프리팹
    public GameObject bloodEffect;
    //혈흔 데칼 효과 프리팹
    public GameObject bloodDecal;

    private int hp = 100;

    private GameUI gameUI;


	// Use this for initialization
	void Awake () {
        //몬스터의 transform할당
        monsterTr = this.gameObject.GetComponent<Transform>();
        //추적 대상인 player의 transform 할당
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        //navvmeshagent 컴포넌트 할당
        nvAgent = this.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        //Animator 컴포넌트 할당
        animator = this.gameObject.GetComponent<Animator>();
        //추적대사의 위치를 설정하면 바로 추적 시작
        //nvAgent.destination = playerTr.position;

        /*
        //일정한 간격으로 몬스터의 행동 상태를 체크하는 코루틴 함수 실행
        StartCoroutine(this.CheckMonsterState());
        //몬스터의 상태에 따라 동작하는 루틴을 실행하는 코루틴 함수 실행
        StartCoroutine(this.MonsterAction());
        */

        //GameUI 게임오브젝트의 GameUI 스크립트 할당
        gameUI = GameObject.Find("GameUI").GetComponent<GameUI>();
        
	}

    //이벤트 발생 시 수행할 함수 연결  << 스크립트 or 게임오브젝트가 비활성화된 상태에서 다시 활성화된때 마다 발생하는 콜백 함수
    //순위가 높기때문에 start함수를 awake함수로 바꿈...
    void OnEnable()
    {
        PlayerCtrl.OnPlayerDie += this.OnPlayerDie;
        //일정한 간격으로 몬스터의 행동 상태를 체크하는 코루틴 함수 실행  << 재사용을 위해 콜백함수에
        StartCoroutine(this.CheckMonsterState());
        //몬스터의 상태에 따라 동작하는 루틴을 실행하는 코루틴 함수 실행  << "
        StartCoroutine(this.MonsterAction());
 
    }
    //이벤트 발생 시 연결된 함수 해제
    void OnDisable()
    {
        PlayerCtrl.OnPlayerDie -= this.OnPlayerDie;
    }

    //일정한 간격으로 몬스터의 행동 상태를 체크하고 monsterstate 값 변경
    IEnumerator CheckMonsterState()
    {
        while (!isDie)
        {
            //0.2초 동안 기다렸다가 다음으로 넘어감 << delay...일정한 시간 간격에 맞춰 발생시킬 로직을 구현할때
            yield return new WaitForSeconds(0.2f);
            //몬스터와 플레이어 사이의 거리 측정
            float dist = Vector3.Distance(playerTr.position, monsterTr.position);

            if (dist <= attackDist)
            {
                monsterState = MonsterState.attack;
            }
            else if (dist >= traceDist)
            {
                monsterState = MonsterState.trace;
            }
            else
            {
                monsterState = MonsterState.idle;
            }
        }
    }
    //몬스터의 상태값에 따라 적절한 동작을 수행하는 함수
    IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            switch (monsterState)
            {
                //idle 상태
                case MonsterState.idle:
                    //추격중지
                    animator.SetBool("IsTrace", false);
                    nvAgent.Stop();
                    break;
                case MonsterState.trace:
                    nvAgent.destination = playerTr.position;
                    nvAgent.Resume();
                    animator.SetBool("IsAttack", false);
                    animator.SetBool("IsTrace", true);
                    break;
                case MonsterState.attack:
                    nvAgent.Stop();
                    animator.SetBool("IsAttack", true);
                    break;
            }
            yield return null;
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.tag == "BULLET")
        {
            //혈흔 효과 함수 호출
            animator.SetTrigger("IsHit");
            Destroy(coll.gameObject);
            CreateBloodEffect(coll.transform.position);
            //Damage 발생
            hp -= coll.gameObject.GetComponent<BulletCtrl>().damage;
            if (hp <= 0)
            {
                MonsterDie();
            }
        }
    }
    void MonsterDie()
    {
        //사망하면 태그를 Untagged로 변경
        gameObject.tag = "Untagged";

        //모든 코루틴 정지
        StopAllCoroutines();

        isDie = true;
        monsterState = MonsterState.die;
        nvAgent.Stop();
        animator.SetTrigger("IsDie");

        //몬스터에 추가된 collider를 비활성ㅇ화
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;

        foreach (Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = false;
        }

        gameUI.DispScore(50);

        //몬스터 오브젝트 풀로 환원시키는 코루틴 함수 호출
        StartCoroutine(this.PushObjectPool());
    }

    IEnumerator PushObjectPool()
    {
        yield return new WaitForSeconds(3.0f);

        //각종 변수 초기화
        isDie = false;
        hp = 100;
        gameObject.tag = "MONSTER";
        monsterState = MonsterState.idle;

        //몬스터에 추가된 collider을 다시 활성화
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = true;

        foreach (Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = true;
        }

        //몬스터를 비활성화
        gameObject.SetActive(false);
    }

    void CreateBloodEffect(Vector3 pos)
    {
        //혈흔 효과 생성
        GameObject blood1 = (GameObject)Instantiate(bloodEffect, pos, Quaternion.identity);
        Destroy(blood1, 2.0f);

        //데칼 생성 위치 -바닥에서 조금 위
        Vector3 decalPos = monsterTr.position + (Vector3.up * 0.05f);
        //데칼 회전값 무작위로 설정
        Quaternion decalRot = Quaternion.Euler(90, 0, Random.Range(0, 360));

        //데칼 프리팹 생성
        GameObject blood2 = (GameObject)Instantiate(bloodDecal, decalPos, decalRot);
        //데칼 크기도 불규칙적으로 나타나게끔 스케일 조정
        float scale = Random.Range(1.5f, 3.5f);
        blood2.transform.localScale = Vector3.one * scale;

        //5초후 삭제
        Destroy(blood2, 5.0f);
    }
    
    void OnPlayerDie()
    {
        //몬스터의 상태를 체크하는 코루틴 함수를 모두 정지시킴
        StopAllCoroutines();
        //추적을 정지하고 애니메이션을 실행
        nvAgent.Stop();
        animator.SetTrigger("IsPlayerDie");
    }

    //몬스터가 ray에 맞았을때 호출되는 함수
    void OnDamage(object[] _params)
    {
        Debug.Log(string.Format("Hit ray {0} : {1}", _params[0], _params[1]));

        //혈흔효과 함수 호출
        CreateBloodEffect((Vector3)_params[0]);
        //맞은 총알의 damage를 추출해 몬스터 hp차감
        hp -= (int)_params[1];
        if (hp <= 0)
        {
            MonsterDie();
        }

        //IsHit Trigger를 발생시키면 Any State에서 gothit로 전이됨
        animator.SetTrigger("IsHit");
    }
}
