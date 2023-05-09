using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //카테고리별로 변수정리
    [Header("----------[ Core ]")]
    public int score;
    //동글생성 최대레벨을 저장
    public int maxLevel;
    //게임오버가 지속적으로 호출되서 단 한번만 호출되도록 플래그생성
    public bool isOver;

    [Header("----------[ Object Pooling ]")]
    public GameObject donglePrefab;
    public Transform dongleGroup;
    //리스트를 쓰네 원래 뭐썻지
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

        //setint할때 키도 같이 생성하는건가
        if (!PlayerPrefs.HasKey("MaxScore"))
            PlayerPrefs.SetInt("MaxScore",0);

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }
    public void GameStart()
    {
        //오브젝트 활성화
        line.SetActive(true);
        bottom.SetActive(true);
        //텍스트는 게임오브젝트로 한번 접근해줘야됨
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlay(Sfx.Button);
        Invoke("NextDongle", 1.5f);
        //뭐지 인보크로 대체 되는거 맞나
        //NextDongle();
    }
     
    Dongle MakeDongle()
    {
        //이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        //네임이 하나 늘때마다 네임뒤에서 숫자가 증가하겠지 
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);



        //동글 생성
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Effect " + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        //동글을 생성하면서 manager, effect 변수를 같이 초기화
        instantDongle.gameManager = this;
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);
        //lastDongle = instantDongle;
        return instantDongle;

    }
    //반환값을 갑자기 동글로 왜하지
    Dongle GetDongle()
    {
        for (int index = 0; index < donglePool.Count; index++)
        {
            //풀커서가 0부터 시작하니까 나누고 난 나머지도 같음
            poolCursor = (poolCursor + 1) % donglePool.Count; 
            if(!donglePool[poolCursor].gameObject.activeSelf)
            {
                return donglePool[poolCursor];
            }
        }
        return MakeDongle();
    }   

    //한곳에서 처리하면 되지 왜 함수를 두개만든건지모르겠네
    //스크립트를 굳이 넘겼음
    //여기서 에니메이션처리하려고그러나
    public void NextDongle()
    {
        //코루틴에의해서 게임오버가 되도 생성한다고함
        if (isOver)
            return;

        //프리팹이라 스크립트를 가져오지못함 다른문젠가
        //생성한 동글의 스크립트를 변수에 담아서 동글타입으로 반환받은걸 변수에 저장해서 요리한다고함
        //왜 동글타입으로 만든거지 다른얘기했는데 스크립트대입용이라고 안했음
        lastDongle = GetDongle();
        //성장 시킬때마다 멕스레벨이 늘어남
        lastDongle.level = Random.Range(0, maxLevel);
        //갑자기 오브젝트활성화시킨다고함 onenable때문에
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);

        StartCoroutine(WaitNext());
    }

    //로직 제어를 유니티에 맡기는 함수
    IEnumerator WaitNext()
    {
        //while에 탈출 가능한 로직을 반드시 넣어줘야한다
        //생성된 프리팹이 착지하면 스크립트가 널이되니까 그때까지 무한루프돌리고 탈출시키고 코루틴 다음 프리팹 생성
        while(lastDongle != null)
        {
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);

        NextDongle();
         
        }
    public void TouchDown()
    {
        //이미 값을 넣었는데 왜 널이생길 가능성이있는거지
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
        //Debug.Log("게임 오버");
        StartCoroutine("GameOverRoutine");

    }
    IEnumerator GameOverRoutine()
    {
        //1. 장면 안에 활성화 되어있는 모든 동글 가져오기 - 모든 동글 가져와서 뭐하려는건지 동글의 공통적인 부분을 생각하고 태그가 동글이니까 태그로찾음
        //태그가 아니라 컴포넌트로 찾음
        Dongle[] dongles = FindObjectsOfType<Dongle>();
        //2. 지우기 전에 모든 동글의 물리효과 비활성화
        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].rigid.simulated = false;
        }
        //3. 1번의 목록을 하나씩 접근해서 지우기 
        for (int index=0; index<dongles.Length; index++)
        {
            //게임플레이하면서 절대 나올수 없는값을 넘겨서 숨긴다고함
            //왜냐면 넘기는 값이 원래 쓰던 hide에서는 타겟으로 이동하면서 사라지게 하는값인데 타겟값에 Vector3.up * 100 값은 없으니까
            //계산에 방해되지않을수있음
            dongles[index].Hide(Vector3.up * 100);
            //0.1초 간격으로 지우면서 합쳐지는 현상이 생길수있음 물리효과를 for문을 한번 더 사용해서 다 끈다고함
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1f);

        //최고 점수 갱신
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);
        //게임오버 UI 표시
        subScoreText.text = "점수 : " + scoreText.text;
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
        //swich에 숫자만 오던가
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
        //플레이어가 중복으로 쓰이면 무슨 오류가 난다고했음
        //그래서 한번 플레이됐으면 다음 플레이어를 재생하게
        //플레이어를 한개만 쓰게되면 효과음이 긴 오디오는 다 끝내지못하고 다음 효과음으로 넘어가기때문
        //그래서 동시에 여러가지 효과음이 플레이 되도록 3개를 만듬
        sfxPlayer[sfxCursor].Play();
        //플레이어는 0,1,2만 있으므로 3개만 반복해서 사용할수있는 식만듬
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }
    //모바일에서 나가는 기능을 위하여
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
