using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSkinTrail : MonoBehaviour
{
    public Transform SkidTrailPrefab;

    private Transform T_Cube;
    private Rigidbody R_Cube;
    private Transform m_SkidTrail;
    private static Transform skidTrailsDetachedParent;
    private float MoveSpeed = 10f;
    private bool skidding;
    // Start is called before the first frame update
    void Start()
    {
        T_Cube = GetComponent<Transform>();
        R_Cube = GetComponent<Rigidbody>();
        skidding = true;

        if(skidTrailsDetachedParent == null)
        {
            skidTrailsDetachedParent = new GameObject("Skid Trails - Detached").transform;
            
        }
        
        
        //m_SkidTrail.parent = transform;
        //m_SkidTrail.localPosition = -Vector3.up * 1f;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKey("up"))
        {
            T_Cube.Translate(Vector3.forward * Time.deltaTime * MoveSpeed);
        }
        if (Input.GetKey("1"))
        {
            skidding = true;
        }
        if(Input.GetKey("2"))
        {
            skidding = false;
        }
            


        if (skidding)                  
        {
            //StartSkidTrail();  
            TestStartSkid();
            //StartCoroutine(TestStartSkidTrail());           //开启漂移协程
        }
        else
        {
            TestEndSkid();
        }
        //m_SkidTrail.parent = skidTrailsDetachedParent;
    }

    public void StartSkidTrail()     //开始滑动轨迹
    {
        //skidTrailsDetachedParent = null;
        m_SkidTrail = Instantiate(SkidTrailPrefab);
        if (m_SkidTrail == null)
        {
            m_SkidTrail = Instantiate(SkidTrailPrefab);
        }

        //_BrakeMap = (GameObject)Instantiate(BrakeMap);
        //skidTrailsDetachedParent = m_SkidTrail;
        m_SkidTrail.transform.parent = skidTrailsDetachedParent.transform;
        m_SkidTrail.transform.position = skidTrailsDetachedParent.transform.position;
        m_SkidTrail.transform.rotation = skidTrailsDetachedParent.transform.rotation;


        //m_SkidTrail = transform;
        //m_SkidTrail.parent = transform;
        
        //Object.Instantiate(SkidTrailPrefab);
    }
    public void TestStartSkid()
    {
        //T_Cube.Translate(Vector3.forward * Time.deltaTime * MoveSpeed);
        //m_SkidTrail.position = transform.position;
        
        if(m_SkidTrail == null)
        {
            m_SkidTrail = Instantiate(SkidTrailPrefab);
        }

        
        m_SkidTrail.parent = transform;
        m_SkidTrail.localPosition = Vector3.up * 1.0f;
        
    }
    public void TestEndSkid()
    {
        //Destroy(m_SkidTrail.gameObject, 10);

        m_SkidTrail.parent = skidTrailsDetachedParent;
        Destroy(m_SkidTrail.gameObject, 10);        
    }
    public IEnumerator TestStartSkidTrail()     //开始滑动轨迹
    {
        skidding = true;

        if(m_SkidTrail == null)
        {
            m_SkidTrail = Instantiate(SkidTrailPrefab);   
        }
             
        while (m_SkidTrail == null)         
        {
            yield return null;              
                                            
        }
        m_SkidTrail.parent = transform;
        m_SkidTrail.localPosition = Vector3.up * 1.0f;
    }

}
