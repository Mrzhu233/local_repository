
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //public float rotate = 0;
    private int CameraMode = 1;              //第三人称，第一人称

    float m_Height = 4f;        //相机距离车高度
    float m_Distance = 10f;      //相机距离车距离
    float m_Speed = 4f;         //相机跟随速度
    Vector3 m_TargetPosition;       //目标位置
    Transform follow;           //要跟随的车


    void Start()
    {
        // 初始化要跟随的物体，获取方式是通过Tag
        follow = GameObject.FindWithTag("car").transform;
        // 由于后面是用LookAt函数，即使摄像头盯着车的TransForm中心看，但这样可能会有些违和，不信可以注释掉下面那句
        // 所以这里新设了需要跟随的物体，即车头前面的CameraToFollow子物体
        follow = follow.GetChild(0);        //根据index下标获取，因为好像没有通过名字获取，所以子物体一定放在第一位置，对应index0
                                            //可以通过循环获得下标，匹配名字来获取，暂时不写了
    }

    void FixedUpdate()
    {

        //ChangeCamera();           //更改相机跟随车辆
        //ModeChange();               //更改视角

        //得到这个目标位置
        m_TargetPosition = follow.position + Vector3.up * m_Height - follow.forward * m_Distance;
        //相机位置
        transform.position = Vector3.Lerp(transform.position, m_TargetPosition, m_Speed * Time.deltaTime);
        //相机时刻看着车
        transform.LookAt(follow);
    }

    void ChangeCamera()
    {
        if (Input.GetKey("2"))
        {
            follow = GameObject.FindWithTag("AI").transform;
        }
        if (Input.GetKey("1"))
        {
            follow = GameObject.FindWithTag("car").transform;
        }
    }
    void ModeChange()               //视角模式改变
    {
        if (Input.GetKey("t"))      //按“t”切换视角
        {
            if (CameraMode == 1)
                CameraMode--;
            else
                CameraMode++;

            if (CameraMode == 1)
            {
                m_Distance= 3f;
                m_Height = 7f;
            }
            else
            {
                m_Distance = 0f;
                m_Height = 0f;
            }

        }
    }
}
