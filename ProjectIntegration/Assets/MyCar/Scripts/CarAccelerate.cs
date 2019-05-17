using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.MyCar
{
    public class CarAccelerate : MonoBehaviour
    {
        private ParticleSystem Nitrogen = new ParticleSystem();         //粒子特效
        private AudioSource m_AudioSource = new AudioSource();          //加速的引擎声，或者说是氮气声，随便叫

        public bool PlayingAudio { get; private set; }      //声音是否播放
        public bool Acceleration;                           //是否在加速

        private float AccelerateLife = 0;                     //自创建的声音调节函数
        


        // Start is called before the first frame update
        //初始化氮气特效
        void Start()
        {
            Nitrogen = this.GetComponent<ParticleSystem>();

            if (Nitrogen == null)          //判断氮气特效是否获取成功
            {
                Debug.LogWarning(" no particle system found on car to generate Nitrogen particles", gameObject);
            }
            else
            {
                Nitrogen.Stop();
            }

            m_AudioSource = GetComponent<AudioSource>();
            PlayingAudio = false;
            Acceleration = false;
        }

        public void NitrogenPlay()
        {
            Nitrogen.Play();                         //这里是不断试出的结果，
            //Nitrogen.Emit(10);
        }
        public void PlayAudio()
        {
            m_AudioSource.Play();
            PlayingAudio = true;
            StartCoroutine(AudioAccelerateUp());    //加速时开启协程，因为音效过程是一个渐变的过程
        }
        public void StopAudio()
        {
            m_AudioSource.Stop();
            PlayingAudio = false;

            Acceleration = false;
            AccelerateLife = 1.0f;
            
        }
        public IEnumerator AudioAccelerateUp()                    //加速过程对应调节声音,这里可能写成协程好一些，因为这个函数调用一遍就够了
        {
            Acceleration = true;                                  //有点类似于锁              
            while(Acceleration)                                   //一直循环跑，直到Acceleration被降速置为false
            {
                m_AudioSource.pitch = AccelerateLife;
                AccelerateLife += 0.1f;
                yield return null;                              //暂停协程，等待下一帧继续执行，一帧提高一次AccelerateLife
                                                                  //但是返回到调用协程的函数那，即StartCoroutine（）处
            }
            //return null;
        }
        
    }

}
