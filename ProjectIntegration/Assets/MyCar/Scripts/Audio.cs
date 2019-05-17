using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.MyCar
{
    [RequireComponent(typeof (CarController))]
    public class NewBehaviourScript : MonoBehaviour
    {
        public AudioClip lowAccelClip;                                              // Audio clip for low acceleration
        public AudioClip lowDecelClip;                                              // Audio clip for low deceleration
        public AudioClip highAccelClip;                                             // Audio clip for high acceleration
        public AudioClip highDecelClip;                                             // Audio clip for high deceleration

        public float pitchMultiplier = 1f;                                          // Used for altering the pitch of audio clips   调试声音曲线
        public float highPitchMultiplier = 0.25f;                                   // Used for altering the pitch of high sounds
        //public float lowPitchMin = 1f;                                              // The lowest possible pitch for the low sounds
        //public float lowPitchMax = 6f;                                              // The highest possible pitch for the low sounds

        public float maxRolloffDistance = 500;                                      // 声音开始衰减的距离


        private AudioSource m_LowAccel;         // 低加速音源
        private AudioSource m_LowDecel;         
        private AudioSource m_HighAccel;        
        private AudioSource m_HighDecel;        
        private bool m_StartedSound;            // 是否开声音
        private CarController m_CarController;  // 效果返回至我们控制的车

        private void StartSound()       //打开声音
        {
            // get the carcontroller ( this will not be null as we have require component)
            //即，获取这个组件（当我们获取使，组件不会为空）
            m_CarController = GetComponent<CarController>();

            // setup the simple audio source
            //建立单一音源
            m_HighAccel = SetUpEngineAudioSource(highAccelClip);

            m_LowAccel = SetUpEngineAudioSource(lowAccelClip);
            m_LowDecel = SetUpEngineAudioSource(lowDecelClip);
            m_HighDecel = SetUpEngineAudioSource(highDecelClip);

            //播放声音标志
            m_StartedSound = true;

        }

        private void StopSound()            //关闭声音
        {
            //Destroy all audio sources on this object:
            foreach (var source in GetComponents<AudioSource>())
            {
                Destroy(source);
            }

            m_StartedSound = false;         
        }
        //上面的两个函数可以发现每次打开声音都需要重新初始化一遍4个audiosource组件，而关闭声音则释放它们
        //减少内存使用，增加cpu机算
        //安全性得到保障，减少bug出现

        
        void Update()
        {
            // get the distance to main camera
            // 获取摄像机到车的位置
            float camDist = (Camera.main.transform.position - transform.position).sqrMagnitude;

            // stop sound if the object is beyond the maximum roll off distance
            //如果播放声音为ture 并且 大于最大播放距离，则调用关闭播放函数
            if (m_StartedSound && camDist > maxRolloffDistance * maxRolloffDistance)
            {
                StopSound();
            }

            //如果播放声音为false 并且 小于最大播放距离，则调用打开播放函数
            //这里只是相当于提前初始化好了，但不播放声音
            if (!m_StartedSound && camDist < maxRolloffDistance * maxRolloffDistance)
            {
                StartSound();
            }

            if (m_StartedSound)
            {
                // The pitch is interpolated between the min and max values, according to the car's revs.
                //float pitch = ULerp(lowPitchMin, lowPitchMax, m_CarController.Revs);
                float pitch = 1.0f;
                // adjust the pitches based on the multipliers
                m_LowAccel.pitch = pitch * pitchMultiplier;
                m_LowDecel.pitch = pitch * pitchMultiplier;
                m_HighAccel.pitch = pitch * highPitchMultiplier * pitchMultiplier;
                m_HighDecel.pitch = pitch * highPitchMultiplier * pitchMultiplier;

            }
        }

        // sets up and adds new audio source to the game object
        //类似于将AudioClip转为AudioSource类型
        private AudioSource SetUpEngineAudioSource(AudioClip clip)
        {
            // create the new audio source component on the game object and set up its properties
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = 0;
            source.loop = true;

            // start the clip from a random point
            source.time = Random.Range(0f, clip.length);
            source.Play();
            source.minDistance = 5;
            source.maxDistance = maxRolloffDistance;
            source.dopplerLevel = 0;

            return source;
        }
        // unclamped versions of Lerp and Inverse Lerp, to allow value to exceed the from-to range
        // 
        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }
    }
}

