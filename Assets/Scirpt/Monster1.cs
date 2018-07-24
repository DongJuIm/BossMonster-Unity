using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster1 : MonoBehaviour {

    Animator bossAnim;
   public Vector3 target_pos;

    public Transform target;
    public Transform firePos;
    Vector3 tmpPos;
    public GameObject bullet;
   // public Transform pos;
    public int bosshaert = 10;//보스체력
    
    public float rot_speed = 5;
    public float check_delayTime = 5f;
    public float ViewDistance = 50;
    public float ViewAngle = 0;

    float animDelay = 1f;

    float currentTime;
    bool isPlayer = false;
    bool isParallel = false;

    int n;

    public GameObject effect;

    enum Monster_State {Idle,TargetTrace,Attack,Demage,Die}
    Monster_State _ms;

    

    private void Awake()
    {

        //  bossAnim = gameObject.GetComponentInChildren<Animator>();
        bossAnim = gameObject.GetComponent<Animator>();
        Init();

        Debug.Log(_ms);

    }

    void Init() {

         bossAnim.SetBool("appear", true);
               
    }

    private void Start()
    {
        StartCoroutine(Check_Player());
    }

    void Update () {

        DrawView();
        M_State();

       // Angle_IsNan();
        /*
        currentTime2 += Time.deltaTime;
        

        if(currentTime2>animDelay)
        {

            isParallel = GetAngle();
            

            currentTime2 = 0f;
        } 
        
        */

    }
  

    void M_State() {

        switch (_ms) {

            case Monster_State.Idle:             
                Idle();
                
                break;

            case Monster_State.TargetTrace:
                TargetTrace();
                break;

            case Monster_State.Attack:
                Attack(n);
                break;

            case Monster_State.Demage:
                Demaged();
                break;

            case Monster_State.Die:

                break;

        }

    }
    void Idle() {


           bossAnim.SetBool("body_idle_001", true);
      
        // bossAnim.Play("body_idle_002");



        //idle anim
        //idle이 필요할까? 당연히 필요하다.

    }
    
    void TargetTrace()
    {
     
            target_pos = (target.position - transform.position);
            target_pos = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(target_pos, Vector3.up), rot_speed * Time.deltaTime);
       
  
    }



    void Attack(int attack_Num) {

        switch (attack_Num) {

            case 0:
                Normal_Attack();
                break;

            case 1:
                Range_Attack();
                break;

            case 2:
                All_Attack();
                break;
        
        }
       


    }

    void All_Attack() {
        if (DelayTime(10f))
        {
            Debug.Log("전체 공격!"); bossAnim.SetBool("roar", true);
            Instantiate(effect, transform.position, transform.rotation);
            
        } else{
            bossAnim.SetBool("roar", false); bossAnim.SetBool("body_idle_001", true);
        }
    }

    void Range_Attack() {
        if (DelayTime(2f))
        { Debug.Log("범위 공격!"); bossAnim.SetBool("attack_005", true);
        } else {
            bossAnim.SetBool("attack_005", false); bossAnim.SetBool("body_idle_001", true);
        }
    } 


    void Normal_Attack() {
        if (DelayTime(2f))
        {
            bossAnim.SetBool("attack_001", true);
            Debug.Log("일반 공격!");
        }
        else {
            bossAnim.SetBool("attack_001",false);
            bossAnim.SetBool("body_idle_001", true);
        }

        
    }

    
    void Demaged()  {
    
        
    }
    void Die()
    {

    }
    IEnumerator Check_Player() {

        while (true) {

            
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            //_transform.forward와 dirToTarget은 모두 단위벡터이므로 내적값은 두 벡터가 이루는 각의 Cos값과 같다.
            //내적값이 시야각/2의 Cos값보다 크면 시야에 들어온 것이다.

            if (Vector3.Dot(transform.forward, dirToTarget) > Mathf.Cos((ViewAngle / 2) * Mathf.Deg2Rad))
            //if (Vector3.Angle(_transform.forward, dirToTarget) < ViewAngle/2)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);

                tmpPos = target.position;

                if (Physics.Raycast(transform.position, dirToTarget, distToTarget))
                {
                    
                    Debug.DrawLine(transform.position, target.position, Color.red);

                    //Debug.Log("시야 내에 존재");

                    // Player가 시야 내에 있는 경우

                    // 시야 내에 있지만 서로 마주보고 있지않는 경우
                    // - 범위 공격

                    isParallel = Angle_IsNan();

                    if (isParallel)
                    {
                        // Debug.Log("집중 공격");
                        n = 0;
                        _ms = Monster_State.Attack;
                        isParallel = false;

                    }
                    else {

                        n = 1;
                        _ms = Monster_State.Attack;
                        //Debug.Log("범위 공격");
                    }
                    // 시야 내에 서로 마주보고 있는 경우
                    // - 집중 공격
                    
                   
                //    _ms = Monster_State.Attack;
                    
                    // isPlayer = true;

                   
                }

                yield return new WaitForSeconds(5f);
            }
            
            else {
                //Player가 시야 내에 없는 경우

                // 1. 플레이어 추적하기
                // 2. 전체 공격

                target_pos = (target.position - transform.position);

                float dot2 = Vector3.Dot(transform.forward, target_pos);

                Debug.Log("시야 내에 존재 하지않음");

               // Debug.Log(Mathf.Cos(dot2));

                int ran = Random.Range(0, 1);

                if (Mathf.Cos(dot2) < 0) {

                    

                    if (ran == 0) { n = 2; _ms = Monster_State.Attack; }
                    else { _ms = Monster_State.TargetTrace; }

                    // Debug.Log(" 뒤 : 전체 공격 or Trace");


                }

                else {
                    
                    if (ran == 0) { n = 1; _ms = Monster_State.Attack; }
                    else { _ms = Monster_State.TargetTrace; }
                  
                                 
                    //Debug.Log("Trace or 범위 공격");  

                }

                yield return new WaitForSeconds(5f);
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


    public bool Angle_IsNan() {

        target.forward = (transform.position - target.position).normalized;
        float dot = Vector3.Dot(transform.forward, target.forward);
        float acos = Mathf.Acos(dot);

        //Debug.Log("각도 : "+acos);

       // if (float.IsNaN(acos))
       if(acos>3f)
        {
            //이하면 범위 공격
            return true;
        }

            return false;
        
    }

     bool DelayTime(float delayTime) {

        
        currentTime += Time.deltaTime;
    
        if (currentTime>delayTime) {


            currentTime = 0;
            Debug.Log(true);

            return true;
            
        }

        return false;
    }
}
