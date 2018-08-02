using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{

    Animator bossAnim;

    const int CREATE_TO_POS_WEAPON = 0;
    const int CREATE_TO_POS_JAW = 1;
    const int CREATE_TO_POS_TARGET = 2;

    const string FALSE_DAMAGE = "Damage";
    const string FALSE_ATTATCK = "Attack";


    public Transform jawPos;
    public Transform target;
    public Transform firePos;

    public Vector3 target_pos;
    public static Vector3 tmpPos;

    int ran;
    int mask;
    public int bosshaert = 10;//보스체력


    public float[] eTime;
    public float rot_speed = 1;
    public float check_delayTime = 5f;
    public float anim_delayTime = 7f;
    public float ViewDistance = 50;
    public float ViewAngle = 60;

    float currentTime;

    bool isParallel = false;
    bool isAlive = true;

    public GameObject[] effect;
    public static string tagName;




    public enum Monster_State { Idle, TargetTrace, Attack, Demage, Die }
    public static Monster_State _ms;




    void Init()
    {

        bossAnim = gameObject.GetComponent<Animator>();
        bossAnim.SetBool("appear", true);

        mask = 1 << LayerMask.NameToLayer("Player");
        // tmpPos = target.position;

    }


    private void Awake() { Init(); }

 
    private void Start() { StartCoroutine(Check_Player()); }

    void Update() { DrawView(); M_State(); if (bosshaert == 0) { _ms = Monster_State.Die; } }


    void M_State()
    {
        if (isAlive) {


            switch (_ms)
            {

                case Monster_State.Idle:
                    Idle();

                    break;

                case Monster_State.TargetTrace:
                    TargetTrace();
                    break;

                case Monster_State.Attack:
                    //  Attack(n);
                    Normal_Attack();
                    break;


                case Monster_State.Demage:
                    Demaged(tagName);

                    break;

                case Monster_State.Die:

                    isAlive = false;
                    Die();
                    break;

            }

        }
      

    }

    void Idle(){bossAnim.SetBool("body_idle_001", true);}

    void TargetTrace()
    {

        target_pos = (target.position - transform.position);
        target_pos = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(target_pos, Vector3.up), rot_speed * Time.deltaTime);
        // bossAnim.SetBool("body_idle_001", true);
    }

    void Normal_Attack()
    {


        if (DelayTime(anim_delayTime))
        {

            // bossAnim.SetBool("body_idle_001", false);

            ran = Random.Range(0, effect.Length + 1);


            switch (ran)
            {


                case 0:

                    bossAnim.SetBool("attack_002", true); Debug.Log("일반 공격 1");


                    StartCoroutine(CreateEffect(effect[1], eTime[0], CREATE_TO_POS_WEAPON));

                    break;

                case 1:

                    bossAnim.SetBool("attack_003", true); Debug.Log("일반 공격 2");
                    StartCoroutine(CreateEffect(effect[1], eTime[1], CREATE_TO_POS_WEAPON));


                    break;

                case 2:


                    bossAnim.SetBool("roar", true); Debug.Log("메테오 !");
                    StartCoroutine(CreateEffect(effect[0], 0, CREATE_TO_POS_TARGET));


                    break;

                case 3:

                    bossAnim.SetBool("attack_006", true); Debug.Log("용암 뿜기");
                    StartCoroutine(CreateEffect(effect[2], eTime[1], CREATE_TO_POS_JAW));

                    break;
                case 4:

                    bossAnim.SetBool("attack_005", true); Debug.Log("불벽!");
                    StartCoroutine(CreateEffect(effect[3], eTime[0], CREATE_TO_POS_TARGET));

                    break;

            }

        } else { FalseAnim(FALSE_ATTATCK); }

    }


    public void Demaged(string damageTag)
    {

        if (DamageNotify.isDamage)
        {
            Debug.Log("Damage Func : " + damageTag);

            bosshaert = (bosshaert - 5);

            switch (damageTag)
            {

                case "BODY":
                case "HEAD":

                    bossAnim.SetBool("damage_001", true);

                    break;


                case "R_Arm":

                    bossAnim.SetBool("damage_002", true);

                    break;

                case "L_Arm":

                    bossAnim.SetBool("damage_003", true);


                    break;

            }
            DamageNotify.isDamage = false;
        }
        else
        {

            FalseAnim(FALSE_DAMAGE);

        }


    }


    void Die()
    {

        bossAnim.SetBool("disappear", true);
;
        Debug.Log("Die");

    }



    IEnumerator Check_Player()
    {

        while (isAlive)
        {

            Vector3 dirToTarget = (target.position - transform.position).normalized;

            //_transform.forward와 dirToTarget은 모두 단위벡터이므로 내적값은 두 벡터가 이루는 각의 Cos값과 같다.
            //내적값이 시야각/2의 Cos값보다 크면 시야에 들어온 것이다.

            if (Vector3.Dot(transform.forward, dirToTarget) > Mathf.Cos((ViewAngle / 2) * Mathf.Deg2Rad))
            //if (Vector3.Angle(_transform.forward, dirToTarget) < ViewAngle/2)
            {

                float distToTarget = Vector3.Distance(transform.position, target.position);

                Debug.Log("거리 : " + distToTarget);

                tmpPos = target.position;

                if (Physics.Raycast(transform.position, dirToTarget, distToTarget, mask))
                {

                    Debug.DrawLine(transform.position, target.position, Color.red);

                    // Player가 시야 내에 있는 경우

                    isParallel = Angle_IsNan();

                    if (isParallel) { _ms = Monster_State.Attack; isParallel = false;} // 마주 보고 있으면 공격
                    else{_ms = Monster_State.TargetTrace; } // 그렇지 않으면 추적

                }

                yield return new WaitForSeconds(check_delayTime);
            }

            else
            {
                //Player가 시야 내에 없는 경우

                Debug.DrawLine(transform.position, target.position, Color.red);
                _ms = Monster_State.TargetTrace;

                yield return new WaitForSeconds(check_delayTime);

            }


        }

    }

    public Vector3 DirFromAngle(float angleInDegrees)
    {
        //탱크의 좌우 회전값 갱신
        angleInDegrees += transform.eulerAngles.y;
        //경계 벡터값 반환
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public void DrawView()
    {
        Vector3 leftBoundary = DirFromAngle(-ViewAngle / 2);
        Vector3 rightBoundary = DirFromAngle(ViewAngle / 2);

        Debug.DrawLine(transform.position, transform.position + leftBoundary * ViewDistance, Color.blue);
        Debug.DrawLine(transform.position, transform.position + rightBoundary * ViewDistance, Color.blue);

    }

    void FalseAnim(string what_false)
    {

        switch (what_false)
        {

            case FALSE_ATTATCK:

                bossAnim.SetBool("attack_002", false);
                bossAnim.SetBool("attack_003", false);
                bossAnim.SetBool("attack_005", false);
                bossAnim.SetBool("attack_006", false);
                bossAnim.SetBool("roar", false);
                bossAnim.SetBool("body_idle_001", true);


                break;

            case FALSE_DAMAGE:


                bossAnim.SetBool("damage_001", false);
                bossAnim.SetBool("damage_002", false);
                bossAnim.SetBool("damage_003", false);

                break;

        }


    }

    public bool Angle_IsNan()
    {

        target.forward = (transform.position - target.position).normalized;
        float dot = Vector3.Dot(transform.forward, target.forward);
        float acos = Mathf.Acos(dot);

        //Debug.Log("각도 : "+acos);

        // if (float.IsNaN(acos))
        if (acos > 3f)
        {
            //이하면 범위 공격
            return true;
        }

        return false;

    }

    bool DelayTime(float delayTime)
    {


        currentTime += Time.deltaTime;



        if (currentTime > delayTime)
        {


            currentTime = 0;


            return true;

        }

        return false;
    }

    IEnumerator CreateEffect(GameObject e_Obj, float delayTime, int posNum)
    {

        yield return new WaitForSeconds(delayTime);

        switch (posNum)
        {

            case 0:

                Instantiate(e_Obj, firePos.position, firePos.rotation);

                break;


            case 1:

                GameObject tmpObj = Instantiate(e_Obj, jawPos.position, jawPos.rotation);
                tmpObj.transform.parent = jawPos.transform;

                break;

            case 2:

                Instantiate(e_Obj, target.position, target.rotation);

                break;

        }

    }
}

