using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.MyCar
{
    public class SkidTrail : MonoBehaviour
    {
        [SerializeField] private float m_PersistTime;       //延迟摧毁时间


        private IEnumerator Start()
        {
            while (true)
            {
                yield return null;

                if (transform.parent.parent == null)        //这里是当滑痕父物体m_SkidTrail换了绑定的对象时，即漂移结束时换成了skidTrailsDetachedParent
                {                                           //这样，skidTrailsDetachedParent的父物体一定是空，自然会执行下面函数
                    Destroy(gameObject, m_PersistTime);
                }
            }
        }
    }
}

