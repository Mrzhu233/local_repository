using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.MyCar
{
    public class CarController : MonoBehaviour
    {

        private Rigidbody m_Rigidbody;
        private Transform m_Transform;

        //public WheelCollider fl;  //左前轮
        //public WheelCollider fr;  //右前轮
        public float force = 3000f;     //氮气喷力
        
        [SerializeField] private WheelCollider[] WheelColliders = new WheelCollider[4];     //车轮对应的WheelCollider
        [SerializeField] private GameObject[] WheelMeshes = new GameObject[4];              //车轮的Mesh
        [SerializeField] private WheelEffects[] m_WheelEffects = new WheelEffects[4];       //车轮漂移后期的脚本，声音，滑痕，冒烟。
        [SerializeField] private CarAccelerate[] m_Accelerate = new CarAccelerate[2];       //两个喷气桶，对应加速后期脚本，氮气，声音。
        [SerializeField] private float max_degree = 30;     //最大转角
        [SerializeField] private float degree = 5;          //转动角度
        [SerializeField] private float Max_Speed = 0f;      //最大速度，为0即不受限制
        [SerializeField] private static int NoOfGears = 5;  //档位数
        [SerializeField] private float m_RevRangeBoundary = 1f;//最大滑动距离
        [SerializeField] private float m_SlipLimit = 0.6f;         //滑动限制，用于轮胎是否打滑检测

        private int NitrogenParticleLife = 0;                       //（自定义的）氮气特效粒子生命周期,后面不是很清楚为什么相反的


        private float m_GearFactor;     //挡位因子
        private int m_GearNum;          //档位数
        private bool skidding;          //是否漂移

        //对外接口
        public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * 2.23693629f; } }//当前速度
        public float MaxSpeed { get { return Max_Speed; } } //最大速度
        public float Revs { get; private set; }

        //驱动力公式：驱动力=扭矩×变速箱齿比×主减速器速比×机械效率÷轮胎半径

        // Use this for initialization
        private void Awake()
        {

        }

        void Start()
        {
            m_Rigidbody = this.GetComponent<Rigidbody>();   //给变量赋值变量
            m_Transform = this.GetComponent<Transform>();   //获取当前对象的位置
        }



        public void Move()
        {
            CarMove();              //wasd移动
            CarNitrogen();          //氮气加速，并控制粒子特效
            CapSpeed();             //捕获速度,并使车速小于最大速度
            CalculateRevs();        //计算转速，供外部调用转速属性Revs,来播放引擎声音
            CarWheelMeshTurn();     //使车轮mesh贴图跟着WheelCollider转动
            CarReset();             //按'r'赛车复位
            CheckForWheelSpin();    //检查车轮是否漂移
        }

        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor) * (1 - factor);
        }

        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }

        private void CalculateGearFactor()
        {
            float f = (1 / (float)NoOfGears);           //档位数的倒数
            // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
            // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
            var targetGearFactor = Mathf.InverseLerp(f * m_GearNum, f * (m_GearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
            m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime * 5f);
        }

        void CalculateRevs()
        {
            CalculateGearFactor();
            var gearNumFactor = m_GearNum / (float)NoOfGears;
            var revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
            var revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
            Revs = ULerp(revsRangeMin, revsRangeMax, m_GearFactor);
        }
        void CarMove()              //引擎设置
        {
            if (Input.GetKey(KeyCode.I))//按W前进
            {
                WheelColliders[0].motorTorque = 300;//设置马达扭矩
                WheelColliders[1].motorTorque = 300;
            }
            else if (Input.GetKey(KeyCode.K))//按S后退或减速
            {

                WheelColliders[0].motorTorque = -300;
                WheelColliders[1].motorTorque = -300;
            }
            else
            {
                WheelColliders[0].motorTorque = 0;
                WheelColliders[1].motorTorque = 0;
            }
            if (Input.GetKey(KeyCode.Space))//按空格刹车
            {
                WheelColliders[2].brakeTorque = 10000;//设置制动扭矩
                WheelColliders[3].brakeTorque = 10000;
            }
            else
            {
                WheelColliders[2].brakeTorque = 0;
                WheelColliders[3].brakeTorque = 0;
            }
            if (Input.GetKey(KeyCode.J) && WheelColliders[0].steerAngle >= -max_degree)//按A左转
            {
                WheelColliders[0].steerAngle += -degree;//设置转向角
                WheelColliders[1].steerAngle += -degree;
            }
            else if (Input.GetKey(KeyCode.L) && WheelColliders[0].steerAngle <= max_degree)//按D右转
            {
                WheelColliders[0].steerAngle += degree;
                WheelColliders[1].steerAngle += degree;
            }
            else if (WheelColliders[0].steerAngle != 0)//车轮自动摆正
            {
                if (WheelColliders[1].steerAngle > 0)
                {
                    WheelColliders[0].steerAngle += -degree;
                    WheelColliders[1].steerAngle += -degree;
                }
                if (WheelColliders[0].steerAngle < 0)
                {
                    WheelColliders[0].steerAngle += degree;
                    WheelColliders[1].steerAngle += degree;
                }
            }

        }
        void CapSpeed()
        {
            if (Max_Speed == 0)                                 //为0即无顶速限制
                return;
            float speed = m_Rigidbody.velocity.magnitude;       //速度是矢量，获取它的模
            speed *= 2.23693629f;
            if (speed > Max_Speed)
            {
                m_Rigidbody.velocity = (Max_Speed / 2.23693629f) * m_Rigidbody.velocity.normalized;     //最大速度 * 单位矢量
            }
        }
        void CarNitrogen()
        {
            float speed = m_Rigidbody.velocity.magnitude;
            if (Input.GetKey(KeyCode.LeftShift))
            {

                Vector3 Car_vector = transform.forward;       //获取自身的x轴的方向
                //Debug.Log(Car_vector);
                //Car_vector = Car_vector.normalized;         //单元化
                //Debug.Log(Car_vector);
                //Debug.Log(transform.InverseTransformPoint(Car_vector));
                //Vector3 Car_vector = m_Rigidbody.velocity.normalized;        //当前速度矢量，因为.velocity只读不能改
                //Car_vector.x = Mathf.Abs(Car_vector.x);                      //将新创速度矢量的x轴值取绝对值，防止倒加速
                m_Rigidbody.AddForce(force * Car_vector);     //添加喷力

                NitrogenParticleLife = 20;              //给与氮气粒子20帧的生命

            }

            NitrogenParticleLife--;                     //每一帧会减一，因为这个函数在move里，所以会每帧都被调用
            if (NitrogenParticleLife > 0 )            
            {                                           
                if(!(m_Accelerate[0].PlayingAudio || m_Accelerate[1].PlayingAudio)) //两者都false时，即都没发出声音时，使一个发出声音
                {
                    m_Accelerate[0].PlayAudio();
                    m_Accelerate[1].PlayAudio();
                }
            }
            else                                         //我也不太清除为什么这里是相反情况满足，猜测是因为只要一直调用ParticleSystem.Play函数，就会产生没有
            {                                           //氮气的效果，而停下来调用时就会产生氮气效果，因为粒子是一直处于Play效果的
                m_Accelerate[0].NitrogenPlay();
                m_Accelerate[1].NitrogenPlay();         //氮气特效
                m_Accelerate[0].StopAudio();
                m_Accelerate[1].StopAudio();            //关闭加速音效
            }
            


        }
        void CarWheelMeshTurn()
        {
            for (int i = 0; i < 4; i++)         //使车轮mesh获得wheelcollder转动效果
            {
                Quaternion quat;                //转动矢量
                Vector3 position;               //位置矢量
                WheelColliders[i].GetWorldPose(out position, out quat);     //获取WheelCollider的位置和转动位置
                WheelMeshes[i].transform.position = position;       //对车轮Mesh赋值
                WheelMeshes[i].transform.rotation = quat;
            }
        }
        void CarReset()
        {
            if (Input.GetKey(KeyCode.R))
            {
                Vector3 current_Vector = m_Transform.position;          //修改赛车的位置
                current_Vector.y = current_Vector.y + 1;                                   //将高度改成比当前位置高5
                m_Transform.position = current_Vector;
                Vector3 current_Quaternion = m_Transform.eulerAngles;   //修改赛车方向
                current_Quaternion.z = 0;                               //将x轴方向改为0，即将车身翻正
                m_Transform.eulerAngles = current_Quaternion;

                Vector3 SpeedClear = new Vector3(0, 0, 0);              //清除惯性速度状态，对应两个速度
                m_Rigidbody.velocity = SpeedClear;                      //速度置0，
                m_Rigidbody.angularVelocity = SpeedClear;               //角速度置0
                //Debug.Log(m_Rigidbody.velocity);
                //下面需要再弄：
                //1，摄像机跟踪修改，避免看的晕
                //2，设置线程睡眠，避免一直按r使得出现飞天不下来情况
            }
        }
        private void CheckForWheelSpin()
        {
            for (int i = 0; i < 4; i++)         //轮询4个轮子
            {
                WheelHit wheelHit;              //车轮的摩擦模型
                WheelColliders[i].GetGroundHit(out wheelHit);       //对地摩擦

                if (Mathf.Abs(wheelHit.forwardSlip) >= m_SlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= m_SlipLimit)
                {
                    m_WheelEffects[i].EmitTyreSmoke();
                    if (!AnySkidSoundPlaying())     //如果没有一个轮子播放声音
                    {
                        m_WheelEffects[i].PlayAudio();
                    }
                    continue;                       

                }

                // if it wasnt slipping stop all the audio
                if (m_WheelEffects[i].PlayingAudio)
                {
                    m_WheelEffects[i].StopAudio();
                }
                // end the trail generation
                m_WheelEffects[i].EndSkidTrail();


            }
        }
        private bool AnySkidSoundPlaying()      //只有4个轮子都没播放声音，才会为假
        {
            for (int i = 0; i < 4; i++)
            {
                if (m_WheelEffects[i].PlayingAudio)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

