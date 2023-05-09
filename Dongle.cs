using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//���۷����� �ٽ� ���ߵ� �̰� �� ��ƴ�
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

    //�����ġ bool �� ���̾���
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
    //������ �ٽ� ���鼭 �ʱ�ȭ�� �������� ������
    void OnDisable()
    {
        //���� Ʈ������ �ʱ�ȭ
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;
        //���� ���� �ʱ�ȭ
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
            //��ǥ�������� �ε巴�� �̵�
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
        //enter�� ����ִµ��� ��� ȣ��Ǵµ� �÷��׷� �ᱸ�� false�Ǹ� �ٽ� true
        StartCoroutine("AttachRoutine");
    }

    IEnumerator AttachRoutine()
    {
        if (isAttach)
            yield break;

        isAttach = true;
        gameManager.SfxPlay(GameManager.Sfx.Attach);
        //�� �浹�ϰ��ִ��� üũ���� �����ͳ��� �浹�ϴ°� ���°ǰ�
        //0.2�� ������ �δ��� enter������ �׻��̿� ������ ����ɼ��־ �÷��׷� ��ų��� ���ѹ� �����ϰ�
        yield return new WaitForSeconds(0.2f);

        isAttach = false;
    }

    //enter���� ������ ���µ� stay���ߵǴ� ������ �ִ°ǰ�
    //�� ������ �ƴ϶� �Ű����� Ÿ���� ��������
    //����ҽ��� ���ؼ� �˾���
    void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            //��ġ�� ���� ��ġ�����϶� �������� �ʵ��� ���׸������� ��ݺ�������
            //������ ���� �Ѵ� ��ġ�� ���°��ƴϰ� ������ 7����
            if(level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                //���� ����� ��ġ ��������
                //other�� me�� �ٸ� �ǹ̾��� ���� �ֵ��ε� ��ġ�� �ٸ��ϱ� �װɷ� �Ǻ��Ҽ��ִ°�
                float meX = transform.position.x;
                float meY = transform.position.y;

                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;
                //other �� ��ġ�� ���ǿ��� �ɷ��� 
                //� ������ ���������� ��ġ�� ����
                //1. ���� �Ʒ��� ���� ��
                //2. ������ ���� �̰� ���� �����ʿ� ���� ��
                //�ε��� ����� �����ִٴ°� �����ִ� �ε�������� ������ ���� �����̱⶧���� ��ũ��Ʈ ������ transform�� �ѱ�� 
                if (meY < otherY ||( meY == otherY || meX > otherX))
                {
                    //������ �����
                    //�������µ� ����ġ�� �ֳ���
                    //�������� �ϰ��ִ� ������ ��ġ�� �ʿ��ϴٰ� ��
                    //�������°� �ƴ϶� �� ����� �ٵ� �ֻ����� ���̵���
                    //Ÿ���� ���� ������ ���̵� Ÿ�������� �̵� �� ��ũ��Ʈ
                    //�ε����� ���� �ε��� ���� �����߿��� ������ ������ �� �����ִ� ������
                    //�±״� ���� ����� ���� ���鳢�� �ε����� ���߿��� ����� ���� �����Ű� 
                    //������ �������� ���������
                    //����������̵� ���� ������ũ��Ʈ�� ���⶧���� 
                    //�ٸ��� ���� ��ġ�� �ٸ����̶� 
                    //�򰥸��� ������ ��ũ��Ʈ���� �ڱ⽺ũ��Ʈ������ �����ϴ���

                    //���ʿ� ��ũ��Ʈ ������ me���� transform�� ������������� �Ѱ��
                    //�׳� ��ġ�� �Ǻ��ϴ°ǰ� 
                    other.Hide(transform.position);
                    //���� ������
                    LevelUp();
                }
            }
        }
    }
    //�����̵� ���� ���Լ��� ȣ���ѳ��� �Լ��� �����Ѵ�
    public void Hide(Vector3 targetPos)
    {
        //������� other�� ����
        isMerge = true;

        //����Ǵϱ� ����ȿ�� �� ����
        rigid.simulated = false;
        circle.enabled = false;
        //�ؿ��� ȣ���ϸ� �������� ����οű� �ڷ�ƾ������
        if (targetPos == Vector3.up * 100)
            //����Ʈ�� ū ������ ȣ���Ҽ��� Ŀ����
            EffectPlay();

        //�̵��� ������ �ڷ�ƾ���� ���� �ִ°Ű�����
        StartCoroutine(HideRoutine(targetPos));
    }

    
    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;

        //�̵��� �����Ӹ��� �� ���ѷ��� ���������� �̵��� ����
        while(frameCount < 20)
        {
            frameCount++;
            if(targetPos != Vector3.up * 100)
            {
                //�����ϴ� ��뿡�� �̵��Ѵٰ���
                //������ ���Լ����ؼ� �̵��ϰ� ������� ��
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            }
            else if(targetPos == Vector3.up * 100)
            {
                //�̵������ʰ� ������Ը� �ϸ�� ���ӿ������� ȣ���� �����̱⶧����
                //ũ�Ⱑ �۾��������� ȿ���� ���ܾߵǼ� hide���� ȣ���ϴ°ǰ� ���⼭ �ϴϱ� ����Ʈ�� ��û ŭ
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
        //�������ϴ� ������ true�� �ٲ㼭 
        isMerge = true;
        
        //�������� �ӵ���,ȸ���ӵ��� ���صȴٴµ� ����
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());
    }

    //���� �Լ����Ҷ� ���ظ� �ȹ������� �����ġ�� ����Ŵϱ� �����Ű� 
    //�������ҋ� ���ϸ��̼��� �߻�
    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1);
        EffectPlay();
        gameManager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f);
        level++;

        //���� ���߿� �ִ밪�� ��ȯ�ϴ� �Լ��� �Ἥ ���� ������ ���Ӹ޴����� ������ ���ؼ� �ִ밪 ��ȯ
        //�߽������� �����ϴ°ǰ� ������ level�� ���ؼ�
        //�׷� �����Ǵ� ������ �������� �� �о���
        gameManager.maxLevel = Mathf.Max(level, gameManager.maxLevel);

        isMerge = false;
    }
    //Ʈ���� üũ�Ǿ��ֱ� ������ Ʈ���ŷ� ���
    void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime += Time.deltaTime;

            if(deadTime > 2)
            {
                //Color(r,g,b) ���� �׸� ��簡 ������ �÷�����
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }
            if(deadTime > 5)
            {
                //5�ʰ� ������ ���ӿ�����ų�ǵ� ���Ӹ޴������� ó���ϱ����
                gameManager.GameOver();
            }
        }
    }
    //���ο� ��Ҵٰ� ������������ deadTime�� ������ ������� ��������
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
