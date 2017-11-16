using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//반드시 필요한 컴포넌트를 명시해 해당 컴포넌트가 삭제되는것을 방지하는 attribute
[RequireComponent(typeof(AudioSource))]

public class FireCtrl : MonoBehaviour {

    public GameObject bullet;
    public Transform firePos;
    //총알 발사 사운드
    public AudioClip fireSfx;
    //audiosource 컴포넌트를 저장할 변수
    private AudioSource source = null;
    //MuzzleFlash의 meshrenderer 컴포넌트 연결변수
    public MeshRenderer muzzleFlash;

    void Start()
    {
        //audiosource 컴포넌트를 추출한 후 변수에 할당
        source = GetComponent<AudioSource>();
        //최초에 MuzzleFlash MeshRenderer를 배활성화
        muzzleFlash.enabled = false;
    }

    // Update is called once per frame
    void Update () {
        //rAY를 표시
        Debug.DrawRay(firePos.position, firePos.forward * 10.0f, Color.green);

        if (Input.GetMouseButtonDown(0))
        {
            Fire();

            //Ray에 맞은 게임오브젝트의 정보를 받아올 변수
            RaycastHit hit;
            //Raycast 함수로 Ray를 발사해 맞은 게임오브젝트가 있을때 true를 반환
            if (Physics.Raycast(firePos.position, firePos.forward, out hit, 10.0f))
            {
                //Ray에 맞은 게임오브젝트의 tag 값을 비교해 몬스터 여부 체크
                if (hit.collider.tag == "MONSTER")
                {
                    //SendMessage를 이용해 전달한 인자를 배열에 담음
                    object[] _params = new object[2];
                    _params[0] = hit.point; //ray에 맞은 정확한 위치값
                    _params[1] = 20; //몬스터에 입힐 데미지값
                    //몬스터에 데미지 입히는 함수 호출
                    hit.collider.gameObject.SendMessage("OnDamage", _params, SendMessageOptions.DontRequireReceiver);
                }

                //Ray에 맞은 게임오브젝트가 Barrel인지 확인
                if (hit.collider.tag == "BARREL")
                {
                    //드럼통에 맞은 RAY의 입사각을 계산하기 위해 발사 원점과 맞은 지점 전달
                    object[] _params = new object[2];
                    _params[0] = firePos.position;
                    _params[1] = hit.point;
                    hit.collider.gameObject.SendMessage("OnDamege", _params, SendMessageOptions.DontRequireReceiver);
                }
            }
           
        }
	}
    void Fire()
    {
        //동적으로 총알을 생성하는 함수
        CreateBullet();

        //사운드 발생 함수
        //source.PlayOneShot(fireSfx, 0.9f);
        GameMgr.instance.PlaySfx(firePos.position, fireSfx);
        

        //잠시 기다리는 루틴을 위해 코루틴 함수로 호출
        StartCoroutine(this.ShowMuzzleFlash());
    }
    void CreateBullet()
    {
        //복사본을 만드는 함수...inistantiate(총알 프리팹, 총알 생성 위치, 총알 각도)
        Instantiate(bullet, firePos.position, firePos.rotation);
    }

    //muzzleflash 활성.비ㅗ할성화를 짧은 시간 동안 반복
    IEnumerator ShowMuzzleFlash()
    {
        //스케일을 불규칙하게 변경
        /*float scale = Random.Range(1.0f, 2.0f);
        muzzleFlash.transform.localScale = Vector3.one * scale;

        //z축을 기준으로 불규칙하게 회전시킴
        Quaternion rot = Quaternion.Euler(0, 0, Random.Range(0, 360));
        muzzleFlash.transform.localRotation = rot;

        muzzleFlash.enabled = true;

        yield return new WaitForSeconds(Random.Range(0.05f, 0.3f));

        muzzleFlash.enabled = false;
        */

        muzzleFlash.enabled = true;
        //불규칙적인 시간 동안 delay한 다음 meshrenderer를 비활성화
        yield return new WaitForSeconds(Random.Range(0.05f, 0.3f));
        //비활성화를 보이지 않게 함
        muzzleFlash.enabled = false;
    }

}
