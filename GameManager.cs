using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //ī�װ����� ��������
    [Header("----------[ Core ]")]
    public int score;
    //���ۻ��� �ִ뷹���� ����
    public int maxLevel;
    //���ӿ����� ���������� ȣ��Ǽ� �� �ѹ��� ȣ��ǵ��� �÷��׻���
    public bool isOver;

    [Header("----------[ Object Pooling ]")]
    public GameObject donglePrefab;
    public Transform dongleGroup;
    //����Ʈ�� ���� ���� ������
    public List<Dongle> donglePool;
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;
    [Range(1, 30)]
    public int poolSIze;
    public int poolCursor;
    public Dongle lastDongle;

    [Header("----------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx { LevelUp, Next, Attach, Button, Over };
    int sfxCursor;

    [Header("----------[ UI ]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;

    [Header("----------[ ETC ]")]
    public GameObject line;
    public GameObject bottom;

    void Awake()
    {
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for(int index=0; index < poolSIze; index++)
            MakeDongle();

        //setint�Ҷ� Ű�� ���� �����ϴ°ǰ�
        if (!PlayerPrefs.HasKey("MaxScore"))
            PlayerPrefs.SetInt("MaxScore",0);

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }
    public void GameStart()
    {
        //������Ʈ Ȱ��ȭ
        line.SetActive(true);
        bottom.SetActive(true);
        //�ؽ�Ʈ�� ���ӿ�����Ʈ�� �ѹ� ��������ߵ�
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlay(Sfx.Button);
        Invoke("NextDongle", 1.5f);
        //���� �κ�ũ�� ��ü �Ǵ°� �³�
        //NextDongle();
    }
     
    Dongle MakeDongle()
    {
        //����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        //������ �ϳ� �ö����� ���ӵڿ��� ���ڰ� �����ϰ��� 
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);



        //���� ����
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Effect " + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        //������ �����ϸ鼭 manager, effect ������ ���� �ʱ�ȭ
        instantDongle.gameManager = this;
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);
        //lastDongle = instantDongle;
        return instantDongle;

    }
    //��ȯ���� ���ڱ� ���۷� ������
    Dongle GetDongle()
    {
        for (int index = 0; index < donglePool.Count; index++)
        {
            //ǮĿ���� 0���� �����ϴϱ� ������ �� �������� ����
            poolCursor = (poolCursor + 1) % donglePool.Count; 
            if(!donglePool[poolCursor].gameObject.activeSelf)
            {
                return donglePool[poolCursor];
            }
        }
        return MakeDongle();
    }   

    //�Ѱ����� ó���ϸ� ���� �� �Լ��� �ΰ���������𸣰ڳ�
    //��ũ��Ʈ�� ���� �Ѱ���
    //���⼭ ���ϸ��̼�ó���Ϸ���׷���
    public void NextDongle()
    {
        //�ڷ�ƾ�����ؼ� ���ӿ����� �ǵ� �����Ѵٰ���
        if (isOver)
            return;

        //�������̶� ��ũ��Ʈ�� ������������ �ٸ�������
        //������ ������ ��ũ��Ʈ�� ������ ��Ƽ� ����Ÿ������ ��ȯ������ ������ �����ؼ� �丮�Ѵٰ���
        //�� ����Ÿ������ ������� �ٸ�����ߴµ� ��ũ��Ʈ���Կ��̶�� ������
        lastDongle = GetDongle();
        //���� ��ų������ �߽������� �þ
        lastDongle.level = Random.Range(0, maxLevel);
        //���ڱ� ������ƮȰ��ȭ��Ų�ٰ��� onenable������
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);

        StartCoroutine(WaitNext());
    }

    //���� ��� ����Ƽ�� �ñ�� �Լ�
    IEnumerator WaitNext()
    {
        //while�� Ż�� ������ ������ �ݵ�� �־�����Ѵ�
        //������ �������� �����ϸ� ��ũ��Ʈ�� ���̵Ǵϱ� �׶����� ���ѷ��������� Ż���Ű�� �ڷ�ƾ ���� ������ ����
        while(lastDongle != null)
        {
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);

        NextDongle();
         
        }
    public void TouchDown()
    {
        //�̹� ���� �־��µ� �� ���̻��� ���ɼ����ִ°���
        if (lastDongle == null)
            return;
        
        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if (lastDongle == null)
            return;

        lastDongle.Drop();
        lastDongle = null;
    }
    public void GameOver()
    {
        if(isOver)
            return;

        isOver = true;
        //Debug.Log("���� ����");
        StartCoroutine("GameOverRoutine");

    }
    IEnumerator GameOverRoutine()
    {
        //1. ��� �ȿ� Ȱ��ȭ �Ǿ��ִ� ��� ���� �������� - ��� ���� �����ͼ� ���Ϸ��°��� ������ �������� �κ��� �����ϰ� �±װ� �����̴ϱ� �±׷�ã��
        //�±װ� �ƴ϶� ������Ʈ�� ã��
        Dongle[] dongles = FindObjectsOfType<Dongle>();
        //2. ����� ���� ��� ������ ����ȿ�� ��Ȱ��ȭ
        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].rigid.simulated = false;
        }
        //3. 1���� ����� �ϳ��� �����ؼ� ����� 
        for (int index=0; index<dongles.Length; index++)
        {
            //�����÷����ϸ鼭 ���� ���ü� ���°��� �Ѱܼ� ����ٰ���
            //�ֳĸ� �ѱ�� ���� ���� ���� hide������ Ÿ������ �̵��ϸ鼭 ������� �ϴ°��ε� Ÿ�ٰ��� Vector3.up * 100 ���� �����ϱ�
            //��꿡 ���ص�������������
            dongles[index].Hide(Vector3.up * 100);
            //0.1�� �������� ����鼭 �������� ������ ��������� ����ȿ���� for���� �ѹ� �� ����ؼ� �� ���ٰ���
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1f);

        //�ְ� ���� ����
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);
        //���ӿ��� UI ǥ��
        subScoreText.text = "���� : " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }
    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine("ResetCoroutine");
    }
    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(0);
    }
    public void SfxPlay(Sfx type)
    {
        //swich�� ���ڸ� ������
        switch(type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }
        //�÷��̾ �ߺ����� ���̸� ���� ������ ���ٰ�����
        //�׷��� �ѹ� �÷��̵����� ���� �÷��̾ ����ϰ�
        //�÷��̾ �Ѱ��� ���ԵǸ� ȿ������ �� ������� �� ���������ϰ� ���� ȿ�������� �Ѿ�⶧��
        //�׷��� ���ÿ� �������� ȿ������ �÷��� �ǵ��� 3���� ����
        sfxPlayer[sfxCursor].Play();
        //�÷��̾�� 0,1,2�� �����Ƿ� 3���� �ݺ��ؼ� ����Ҽ��ִ� �ĸ���
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }
    //����Ͽ��� ������ ����� ���Ͽ�
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            Application.Quit();
    }
    void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
