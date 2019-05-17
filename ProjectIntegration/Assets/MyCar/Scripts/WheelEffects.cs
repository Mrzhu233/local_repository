using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.MyCar
{
    //该脚本用于控制漂移时的轮胎轨迹和声音特效
    //每个轮胎一个

    [RequireComponent(typeof (AudioSource))]
    public class WheelEffects : MonoBehaviour
    {
        public Transform SkidTrailPrefab;                   //预设的漂移
        public ParticleSystem skidParticles;                //烟雾特效
        public static Transform skidTrailsDetachedParent;   //滑痕对应的销毁时的父物体

        public bool skidding { get; private set; }          //是否产生滑动，即漂移
        public bool PlayingAudio { get; private set; }      //声音是否播放

        private AudioSource m_AudioSource;
        private Transform m_SkidTrail;
        private WheelCollider m_WheelCollider;


        
        void Start()                    //初始化   //特效，声音，碰撞器
        {

            skidParticles = transform.root.GetComponentInChildren<ParticleSystem>();        //根部的烟雾特效

            if (skidParticles == null)          //判断轨迹特效是否获取成功
            {
                Debug.LogWarning(" no particle system found on car to generate smoke particles", gameObject);
            }
            else
            {
                skidParticles.Stop();
            }

            m_WheelCollider = GetComponent<WheelCollider>();    
            m_AudioSource = GetComponent<AudioSource>();
            PlayingAudio = false;

            if (skidTrailsDetachedParent == null)
            {
                skidTrailsDetachedParent = new GameObject("Skid Trails - Detached").transform;
            }
        }

        public void EmitTyreSmoke()         //发出轮胎烟雾
        {
            skidParticles.transform.position = transform.position - transform.up * m_WheelCollider.radius;  //烟雾位置为轮胎中心减轮胎半径，即轮胎与地接触的地方
            skidParticles.Emit(1);          //特效发射器为1
            Debug.Log(skidding);
            if (!skidding)                  //如果未滑动,相对于这个部件，是外部，即车的控制器来调控是否滑动，而滑动结束是由这里内部调控的
            {
                StartCoroutine(StartSkidTrail());       //开启协同程序，即漂移痕迹生成器,并不会影响主进程
            }
        }

        public void PlayAudio()         //播放漂移声音
        {
            m_AudioSource.Play();       
            PlayingAudio = true;        
        }

        public void StopAudio()
        {
            m_AudioSource.Stop();
            PlayingAudio = false;
        }

        public IEnumerator StartSkidTrail()     //开始滑动轨迹
        {
            skidding = true;
            //m_SkidTrail = Instantiate(SkidTrailPrefab);         //将预设好的滑痕object赋给前者
            //if (m_SkidTrail == null)
            {
                m_SkidTrail = Instantiate(SkidTrailPrefab);
            }

            while (m_SkidTrail == null)         //这里可能为null吗？
            {
                yield return null;              //下一帧继续执行这段代码，这里并不是很明白什么意思
                                                //一个协同程序在执行过程中,可以在任意位置使用yield语句。yield的返回值控制何时恢复协同程序向下执行
            }
            m_SkidTrail.parent = transform;
            m_SkidTrail.localPosition = -Vector3.up * m_WheelCollider.radius;
        }

        public void EndSkidTrail()          //结束滑动轨迹
        {
            if (!skidding)              
            {
                return;
            }
            skidding = false;
            m_SkidTrail.parent = skidTrailsDetachedParent;
            Destroy(m_SkidTrail.gameObject, 10);        //对应的滑痕脚本中的滑痕也会被destroy
        }
    }
}

