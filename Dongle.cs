using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//동글로직은 다시 봐야됨 이건 좀 어렵다
public class Dongle : MonoBehaviour
{
    public int level;
    public bool isDrag;
    public bool isMerge;
    public bool isAttach;
    public float deadTime;

    public GameManager gameManager;
    public ParticleSystem effect;

    public Rigidbody2D rigid;
    CircleCollider2D circle;
    Animator anim;
    SpriteRenderer spriteRenderer;

    //잠금장치 bool 이 많이쓰임
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        circle = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void OnEnable()
    {
        anim.SetInteger("Level", level);
    }
    //동글을 다시 쓰면서 초기화를 꺼졌을때 시켜줌
    void OnDisable()
    {
        //동글 트랜스폼 초기화
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;
        //동글 물리 초기화
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circle.enabled = true;
    }
    void Update()
    {
        if(isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float leftBorder = -4.2f + transform.localScale.x / 2;
            float rightBorder = 4.2f - transform.localScale.x / 2;
            if (mousePos.x > rightBorder)
                mousePos.x = rightBorder;
            else if(mousePos.x < leftBorder)
                mousePos.x = leftBorder;

            mousePos.y = 8;
            mousePos.z = 0;
            //목표지점으로 부드럽게 이동
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f); 
        }
    }
    public void Drag()
    {
        isDrag = true;
    }
    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //enter는 닿아있는동안 계속 호출되는데 플래그로 잠구고 false되면 다시 true
        StartCoroutine("AttachRoutine");
    }

    IEnumerator AttachRoutine()
    {
        if (isAttach)
            yield break;

        isAttach = true;
        gameManager.SfxPlay(GameManager.Sfx.Attach);
        //왜 충돌하고있는지 체크하지 같은것끼리 충돌하는거 막는건가
        //0.2초 간격을 두더라도 enter에서는 그사이에 여러번 실행될수있어서 플래그로 잠궈놓음 딱한번 실행하게
        yield return new WaitForSeconds(0.2f);

        isAttach = false;
    }

    //enter여서 오류가 났는데 stay여야되는 이유가 있는건가
    //그 문제가 아니라 매개변수 타입의 문제였고
    //영상소스랑 비교해서 알았음
    void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            //합치는 조건 합치는중일때 합쳐지지 않도록 버그막으려면 잠금변수생성
            //레벨이 같고 둘다 합치는 상태가아니고 레벨이 7이하
            if(level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                //나와 상대편 위치 가져오기
                //other과 me는 다른 의미없음 같은 애들인데 위치만 다르니까 그걸로 판별할수있는거
                float meX = transform.position.x;
                float meY = transform.position.y;

                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;
                //other 의 위치를 조건에서 걸렀고 
                //어떤 동글이 합쳐질건지 위치로 결정
                //1. 내가 아래에 있을 때
                //2. 동일한 높이 이고 내가 오른쪽에 있을 때
                //부딪힌 대상이 위에있다는거 위에있는 부딪힌대상은 무조건 위고 상대방이기때문에 스크립트 주인은 transform을 넘긴다 
                if (meY < otherY ||( meY == otherY || meX > otherX))
                {
                    //상대방은 숨기기
                    //상대방숨기는데 내위치를 왜넣지
                    //레벨업을 하고있는 동글의 위치가 필요하다고 함
                    //상대방숨기는게 아니라 날 숨기네 근데 왜상대방의 하이드지
                    //타겟은 상대방 상대방의 하이드 타겟쪽으로 이동 내 스크립트
                    //부딪힌건 상대방 부딪힌 같은 동글중에서 기준이 상대방은 더 위에있는 동글임
                    //태그는 딱히 상관이 없음 지들끼리 부딪히는 것중에서 상대방과 나를 나눈거고 
                    //상대방은 숨겨지고 합쳐질대상
                    //합쳐질대상이든 뭐든 같은스크립트를 쓰기때문에 
                    //다른건 같고 위치만 다를뿐이라서 
                    //헷갈린게 상대방의 스크립트인지 자기스크립트인지를 구분하느라

                    //애초에 스크립트 주인이 me여서 transform을 합쳐질대상으로 넘겼다
                    //그냥 위치만 판별하는건가 
                    other.Hide(transform.position);
                    //나는 레벨업
                    LevelUp();
                }
            }
        }
    }
    //주인이든 뭐든 이함수를 호출한놈이 함수를 적용한다
    public void Hide(Vector3 targetPos)
    {
        //여기부턴 other의 공간
        isMerge = true;

        //흡수되니까 물리효과 다 꺼줌
        rigid.simulated = false;
        circle.enabled = false;
        //밑에서 호출하면 오류나서 여기로옮김 코루틴오류가
        if (targetPos == Vector3.up * 100)
            //이펙트가 큰 이유가 호출할수록 커져서
            EffectPlay();

        //이동은 왜하지 코루틴으로 텀을 주는거같은데
        StartCoroutine(HideRoutine(targetPos));
    }

    
    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;

        //이동을 프레임마다 함 무한루프 빠져나가면 이동이 끝남
        while(frameCount < 20)
        {
            frameCount++;
            if(targetPos != Vector3.up * 100)
            {
                //성장하는 상대에게 이동한다고함
                //닿으면 이함수통해서 이동하고 사라지게 함
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            }
            else if(targetPos == Vector3.up * 100)
            {
                //이동하지않고 사라지게만 하면됨 게임오버에서 호출할 계산식이기때문에
                //크기가 작아지기전에 효과가 생겨야되서 hide에서 호출하는건가 여기서 하니까 이펙트가 엄청 큼
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
                
            }
            yield return null;
        }
        gameManager.score += (int)Mathf.Pow(3, level);

        isMerge = false;
        gameObject.SetActive(false);
    }
    void LevelUp()
    {
        //레벨업하는 순간에 true로 바꿔서 
        isMerge = true;
        
        //레벨업에 속도와,회전속도가 방해된다는데 왜지
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());
    }

    //무슨 함수를할때 방해를 안받으려고 잠금장치를 만든거니까 썻던거고 
    //레벨업할떈 에니메이션이 발생
    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1);
        EffectPlay();
        gameManager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f);
        level++;

        //인자 값중에 최대값을 반환하는 함수를 써서 현재 레벨과 게임메니저의 레벨을 비교해서 최대값 반환
        //멕스레벨을 갱신하는건가 증가한 level과 비교해서
        //그럼 생성되는 동글의 레벨폭이 더 넓어짐
        gameManager.maxLevel = Mathf.Max(level, gameManager.maxLevel);

        isMerge = false;
    }
    //트리거 체크되어있기 때문에 트리거로 사용
    void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime += Time.deltaTime;

            if(deadTime > 2)
            {
                //Color(r,g,b) 레드 그린 블루가 섞여서 컬러나옴
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }
            if(deadTime > 5)
            {
                //5초가 넘을때 게임오버시킬건데 게임메니저에서 처리하기로함
                gameManager.GameOver();
            }
        }
    }
    //라인에 닿았다가 빠져나왔을때 deadTime과 색깔을 원래대로 돌려놓음
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Finish")
        {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }
    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }

}
