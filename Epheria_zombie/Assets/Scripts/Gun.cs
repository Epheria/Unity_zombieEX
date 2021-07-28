using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum State
    {
        READY, // 발사 준비됨
        EMPTY, // 탄창이 빔
        RELOADING // 재장전 중
    }
    
    public State state { get; private set; } // 현재 총의 상태

    public Transform fireTransform; // 총알이 발사될 위치

    public ParticleSystem muzzleFlashEffect; // 총구 화염 효과
    public ParticleSystem shellEjectEffect; // 탄피 배출 효과

    private LineRenderer bulletLineRenderer; // 총알 궤적을 그리기 위한 렌더러

    private AudioSource gunAudioPlayer; // 총 소리 재생기
    public AudioClip shotClip; // 발사 소리
    public AudioClip reloadClip; // 장전 소리
    public float damage = 25; // 공격력
    private float fireDistance = 50f; // 사정 거리

    public int ammoRemain = 100; // 남은 전체 탄약
    public int magCapacity = 25; // 탄창 용량
    public int magAmmo; // 현재 탄창에 남아있는 탄약
    public float timeBetFire = 0.12f; // 총알 발사 간격
    public float reloadTime = 1.8f; // 재장전 소요 시간
    private float lastFireTime; // 총을 마지막으로 발사한 시점

    // Start is called before the first frame update
    private void Awake()
    {
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();

        // 사용할 점을 두개로 변경
        bulletLineRenderer.positionCount = 2;
        // 라인 렌더러 비활성화
        bulletLineRenderer.enabled = false;
    }

    // Update is called once per frame
    private void OnEnable()
    {
        // 현재 탄창 가득 채우기
        magAmmo = magCapacity;
        // 총의 현재 상태를 총을 쏠 준비가 된 상태로 변경
        state = State.READY;
        // 마지막으로 총을 쏜 시점을 초기화
        lastFireTime = 0;
    }

    private IEnumerator ShotEffect(Vector3 hitPosition)
    {
        // 총구 화연 효과 재생
        muzzleFlashEffect.Play();
        // 탄피 배출 효과 재생
        shellEjectEffect.Play();

        // 총격소리 재생
        gunAudioPlayer.PlayOneShot(shotClip);

        //선의 시작점은 총구의 위치
        bulletLineRenderer.SetPosition(0, fireTransform.position);
        // 선의 끝점은 입력으로 돌아온 충돌 위치
        bulletLineRenderer.SetPosition(1, hitPosition);
        //라인 렌더러를 활상화하여 총알 궤적을 그린다.
        bulletLineRenderer.enabled = true;

        // 0.03초 동안 잠시 처리를 대기
        yield return new WaitForSeconds(0.03f);

        // 라인 렌더러를 비활성하여 총알 궤적을 지운다
        bulletLineRenderer.enabled = false;
    }

    public void Fire()
    {
        // 현재 상태가 발사 가능한 상태?
        // 그리고 마지막 총 발사 시점에서 timeBetFire 이상의 시간이 지날 때
        if(state == State.READY && Time.time >= lastFireTime + timeBetFire)
        {
            // 마지막 총 발사 시점 갱신
            lastFireTime = Time.time;
            // 실제 발사 처리 실행
            Shot();
        }
    }

    private void Shot()
    {
        // RayCast 에 의한 충돌 정보를 저장하는 컨테이너
        RaycastHit hit;

        //탄알이 맞은 곳을 저장할 변수
        Vector3 hitPosition = Vector3.zero;

        if(Physics.Raycast(fireTransform.position, fireTransform.forward, out hit, fireDistance))
        {
            // Ray 가 어떤 물체와 충돌한 경우
            // 충돌한 상대방으로 부터 IDmagaeable Component를 가져온다.
            IDamageable target = hit.collider.GetComponent<IDamageable>();

            if(target != null)
            {
                target.OnDamage(damage, hit.point, hit.normal);
            }

            // Ray 가 충돌한 위치 저장
            hitPosition = hit.point;
        }
        else
        {
            // Ray 가 다른 물체와 충돌하지 않았다면
            // 탄알이 최대 사정거리까지 날아갔을 때의 위치를 충돌 위치로 사용
            hitPosition = fireTransform.position + fireTransform.forward * fireDistance;
        }

        // 발사 이펙트 재생 시작
        StartCoroutine(ShotEffect(hitPosition));

        // 남은 탄알 수 차감
        magAmmo--;

        if(magAmmo <= 0)
        {
            // 탄창에 남은 탄알이 없다면 총의 현재 상태를 Empty로 갱신
            state = State.EMPTY;
        }
    }

    private IEnumerator ReloadRoutine()
    {
        // 현재 상태를 재장전 중 상태로 전환
        state = State.RELOADING;

        // 재장전 소리 재생
        gunAudioPlayer.PlayOneShot(reloadClip);

        // 재장전 소요 시간 만큼 처리를 쉬기
        yield return new WaitForSeconds(reloadTime);

        int ammoToFill = magCapacity - magAmmo;

        // 탄창에 채워야할 탄알이 남은 탄알보다 많다면
        // 채워야 할 탄알 수를 남은 탄알 수에 맞춰 줄인다.
        if(ammoRemain < ammoToFill)
        {
            ammoToFill = ammoRemain;
        }

        //탄창을 채움
        magAmmo += ammoToFill;
        //남은 탄알에서 탄창에 채운만큼 탄알을 뺀다
        ammoRemain -= ammoToFill;

        state = State.READY;
    }

    public bool Reload()
    {
        if(state == State.RELOADING || ammoRemain <= 0 || magAmmo > magCapacity)
        {
            // 이미 재장전 중이거나 남은 탄알이 없거나
            // 탄창에 탄알이 이미 가득한 경우 재장전 불가
            return false;
        }

        // 재장전 처리 시작
        StartCoroutine(ReloadRoutine());
        return true;
    }
}
