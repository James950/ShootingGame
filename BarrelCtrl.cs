using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelCtrl : MonoBehaviour {

    public GameObject expEffect;
    private Transform tr;
    public Texture[] textures;

    private int hitCount = 0;


	// Use this for initialization
	void Start () {
        tr = GetComponent<Transform>();

        int idx = Random.Range(0, textures.Length);
        GetComponentInChildren<MeshRenderer>().material.mainTexture = textures[idx];
	}

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.tag == "BULLET")
        {
            Destroy(coll.gameObject);

            if (++hitCount >= 3)
            {
                expBarrel();
            }
        }      
    }

    void expBarrel()
    {
        //폭파 파티클 형성
        Instantiate(expEffect, tr.position, Quaternion.identity);
        //지정한 원점을 중심으로 10.0f 반경 내에 들어와 있는 collider 객체 추출
        Collider[] colls = Physics.OverlapSphere(tr.position, 10.0f);
        //추출한 collider객체에 폭발력 전달
        foreach (Collider coll in colls)
        {
            Rigidbody rbody = coll.GetComponent<Rigidbody>();
            if (rbody != null)
            {
                rbody.mass = 1.0f;
                rbody.AddExplosionForce(1000.0f, tr.position, 10.0f, 300.0f);
                //                      폭발력,    원점,         반경, 위로 솟구치는 힘
            }
        }

        Destroy(gameObject, 5.0f);

    }

    //Raycast에 맞았을 때 호출할 함수
    void OnDamage(object[] _params)
    {
        Vector3 firePos = (Vector3) _params[0];
        Vector3 hitPos = (Vector3)_params[1];
        //입사벡터(Ray의 각도) =맞은좌표 - 발사 원점
        Vector3 incomeVector = hitPos - firePos;
        //입사각벡터를 정규화 벡터로 변경 <<< 크기x, only 방향
        incomeVector = incomeVector.normalized;
        //Ray의 hit좌표에 입사벡터의 각도로 힘을 생성
        GetComponent<Rigidbody>().AddForceAtPosition(incomeVector * 1000f, hitPos);
        //총알 맞은 횟수를 증가시키고 3회이상이면 폭발처리
        if (++hitCount >= 3)
        {
            expBarrel();
        }
    }


}
