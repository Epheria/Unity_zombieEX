using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum State
    {
        READY, // �߻� �غ��
        EMPTY, // źâ�� ��
        RELOADING // ������ ��
    }
    
    public State state { get; private set; } // ���� ���� ����

    public Transform fireTransform; // �Ѿ��� �߻�� ��ġ

    public ParticleSystem muzzleFlashEffect; // �ѱ� ȭ�� ȿ��
    public ParticleSystem shellEjectEffect; // ź�� ���� ȿ��

    private LineRenderer bulletLineRenderer; // �Ѿ� ������ �׸��� ���� ������

    private AudioSource gunAudioPlayer; // �� �Ҹ� �����
    public AudioClip shotClip; // �߻� �Ҹ�
    public AudioClip reloadClip; // ���� �Ҹ�
    public float damage = 25; // ���ݷ�
    private float fireDistance = 50f; // ���� �Ÿ�

    public int ammoRemain = 100; // ���� ��ü ź��
    public int magCapacity = 25; // źâ �뷮
    public int magAmmo; // ���� źâ�� �����ִ� ź��
    public float timeBetFire = 0.12f; // �Ѿ� �߻� ����
    public float reloadTime = 1.8f; // ������ �ҿ� �ð�
    private float lastFireTime; // ���� ���������� �߻��� ����

    // Start is called before the first frame update
    private void Awake()
    {
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();

        // ����� ���� �ΰ��� ����
        bulletLineRenderer.positionCount = 2;
        // ���� ������ ��Ȱ��ȭ
        bulletLineRenderer.enabled = false;
    }

    // Update is called once per frame
    private void OnEnable()
    {
        // ���� źâ ���� ä���
        magAmmo = magCapacity;
        // ���� ���� ���¸� ���� �� �غ� �� ���·� ����
        state = State.READY;
        // ���������� ���� �� ������ �ʱ�ȭ
        lastFireTime = 0;
    }

    private IEnumerator ShotEffect(Vector3 hitPosition)
    {
        // �ѱ� ȭ�� ȿ�� ���
        muzzleFlashEffect.Play();
        // ź�� ���� ȿ�� ���
        shellEjectEffect.Play();

        // �ѰݼҸ� ���
        gunAudioPlayer.PlayOneShot(shotClip);

        //���� �������� �ѱ��� ��ġ
        bulletLineRenderer.SetPosition(0, fireTransform.position);
        // ���� ������ �Է����� ���ƿ� �浹 ��ġ
        bulletLineRenderer.SetPosition(1, hitPosition);
        //���� �������� Ȱ��ȭ�Ͽ� �Ѿ� ������ �׸���.
        bulletLineRenderer.enabled = true;

        // 0.03�� ���� ��� ó���� ���
        yield return new WaitForSeconds(0.03f);

        // ���� �������� ��Ȱ���Ͽ� �Ѿ� ������ �����
        bulletLineRenderer.enabled = false;
    }

    public void Fire()
    {
        // ���� ���°� �߻� ������ ����?
        // �׸��� ������ �� �߻� �������� timeBetFire �̻��� �ð��� ���� ��
        if(state == State.READY && Time.time >= lastFireTime + timeBetFire)
        {
            // ������ �� �߻� ���� ����
            lastFireTime = Time.time;
            // ���� �߻� ó�� ����
            Shot();
        }
    }

    private void Shot()
    {
        // RayCast �� ���� �浹 ������ �����ϴ� �����̳�
        RaycastHit hit;

        //ź���� ���� ���� ������ ����
        Vector3 hitPosition = Vector3.zero;

        if(Physics.Raycast(fireTransform.position, fireTransform.forward, out hit, fireDistance))
        {
            // Ray �� � ��ü�� �浹�� ���
            // �浹�� �������� ���� IDmagaeable Component�� �����´�.
            IDamageable target = hit.collider.GetComponent<IDamageable>();

            if(target != null)
            {
                target.OnDamage(damage, hit.point, hit.normal);
            }

            // Ray �� �浹�� ��ġ ����
            hitPosition = hit.point;
        }
        else
        {
            // Ray �� �ٸ� ��ü�� �浹���� �ʾҴٸ�
            // ź���� �ִ� �����Ÿ����� ���ư��� ���� ��ġ�� �浹 ��ġ�� ���
            hitPosition = fireTransform.position + fireTransform.forward * fireDistance;
        }

        // �߻� ����Ʈ ��� ����
        StartCoroutine(ShotEffect(hitPosition));

        // ���� ź�� �� ����
        magAmmo--;

        if(magAmmo <= 0)
        {
            // źâ�� ���� ź���� ���ٸ� ���� ���� ���¸� Empty�� ����
            state = State.EMPTY;
        }
    }

    private IEnumerator ReloadRoutine()
    {
        // ���� ���¸� ������ �� ���·� ��ȯ
        state = State.RELOADING;

        // ������ �Ҹ� ���
        gunAudioPlayer.PlayOneShot(reloadClip);

        // ������ �ҿ� �ð� ��ŭ ó���� ����
        yield return new WaitForSeconds(reloadTime);

        int ammoToFill = magCapacity - magAmmo;

        // źâ�� ä������ ź���� ���� ź�˺��� ���ٸ�
        // ä���� �� ź�� ���� ���� ź�� ���� ���� ���δ�.
        if(ammoRemain < ammoToFill)
        {
            ammoToFill = ammoRemain;
        }

        //źâ�� ä��
        magAmmo += ammoToFill;
        //���� ź�˿��� źâ�� ä�ŭ ź���� ����
        ammoRemain -= ammoToFill;

        state = State.READY;
    }

    public bool Reload()
    {
        if(state == State.RELOADING || ammoRemain <= 0 || magAmmo > magCapacity)
        {
            // �̹� ������ ���̰ų� ���� ź���� ���ų�
            // źâ�� ź���� �̹� ������ ��� ������ �Ұ�
            return false;
        }

        // ������ ó�� ����
        StartCoroutine(ReloadRoutine());
        return true;
    }
}
