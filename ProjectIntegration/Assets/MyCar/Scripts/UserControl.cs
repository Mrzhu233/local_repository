using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.MyCar                  //设置包空间
{
    [RequireComponent(typeof (CarController))]      //需要的组件将会自动被添加到game object（游戏物体）
    public class UserControl : MonoBehaviour
    {
        private CarController m_Car;

        private void Awake()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            m_Car.Move();               //调用CarController的move函数
        }
    }
}


