using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float fireInterval = 0.25f;
    [SerializeField, Min(0.01f)] private float range = 100f;
    [SerializeField, Min(1)] private int damage = 1;
    [SerializeField, Min(0.01f)] private float tracerDuration = 0.25f;
    [SerializeField, Min(0.001f)] private float tracerWidth = 0.18f;
    [SerializeField, Min(0.01f)] private float muzzleFlashSize = 0.45f;
    [SerializeField, Min(1f)] private float reticleSize = 20f;
    [SerializeField, Min(1f)] private float reticleThickness = 2f;
    [SerializeField] private Color reticleColor = Color.white;
    [SerializeField] private AudioClip shotSound;
    [SerializeField, Range(0f, 1f)] private float shotVolume = 1f;
    [SerializeField] private ParticleSystem shotParticles;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private Texture2D eliminatedMarkTexture;
    [SerializeField] private RawImage hunterIcon;
    [SerializeField] private RawImage peleteroIcon;
    [SerializeField] private RawImage butcherIcon;
    private Camera aimCamera;
    private Animator animator;
    private AudioSource shotAudioSource;
    private StrawberryCollector strawberryCollector;
    private MissionProgress missionProgress;
    private LineRenderer tracer;
    private LineRenderer muzzleFlash;
    private Material tracerMaterial;
    private Coroutine hideTracerCoroutine;
    private RawImage hunterEliminatedMark;
    private RawImage peleteroEliminatedMark;
    private RawImage butcherEliminatedMark;
    private float nextFireTime;
    private bool hasAttackParameter;
    private bool gamepadAimActive;

    public bool IsAiming
    {
        get
        {
            Mouse mouse = Mouse.current;
            return gamepadAimActive || (mouse != null && mouse.rightButton.isPressed);
        }
    }

    public Vector3 AimDirection
    {
        get { return aimCamera.ScreenPointToRay(GetAimScreenPosition()).direction; }
    }

    private void Awake()
    {
        aimCamera = Camera.main;
        animator = GetComponentInChildren<Animator>();
        strawberryCollector = GetComponent<StrawberryCollector>();
        missionProgress = GetComponent<MissionProgress>();
        shotAudioSource = GetComponent<AudioSource>();
        if (shotAudioSource == null)
        {
            shotAudioSource = gameObject.AddComponent<AudioSource>();
        }

        shotAudioSource.playOnAwake = false;
        hasAttackParameter = animator != null && Array.Exists(
            animator.parameters,
            parameter => parameter.name == "Attack" &&
                         parameter.type == AnimatorControllerParameterType.Bool
        );

        if (aimCamera == null)
        {
            Debug.LogError("PlayerShooter requires a camera tagged MainCamera.", this);
            enabled = false;
            return;
        }

        if (strawberryCollector == null || missionProgress == null)
        {
            Debug.LogError(
                "PlayerShooter requires StrawberryCollector and MissionProgress components.",
                this
            );
            enabled = false;
            return;
        }

        CreateTracer();
        CreateEliminationMarks();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Gamepad gamepad = Gamepad.current;
        Mouse mouse = Mouse.current;
        bool gamepadAttacking = gamepad != null && gamepad.leftTrigger.isPressed;
        bool gamepadShot = gamepad != null &&
                           gamepad.leftTrigger.wasPressedThisFrame;
        bool keyboardCollect = keyboard != null && keyboard.xKey.wasPressedThisFrame;
        bool gamepadCollect = gamepad != null && gamepad.buttonWest.wasPressedThisFrame;
        Vector2 gamepadLookInput = gamepad != null
            ? gamepad.rightStick.ReadValue()
            : Vector2.zero;
        bool gamepadLooking = gamepadLookInput.sqrMagnitude > 0.01f;
        bool mouseMoved = mouse != null && mouse.delta.ReadValue().sqrMagnitude > 0.01f;
        bool mouseShot = mouse != null && mouse.leftButton.wasPressedThisFrame;
        if (gamepadAttacking || gamepadLooking)
        {
            gamepadAimActive = true;
        }
        else if (mouseMoved || mouseShot ||
                 (mouse != null && mouse.rightButton.isPressed))
        {
            gamepadAimActive = false;
        }

        bool keyboardAttacking = keyboard != null && keyboard.fKey.isPressed;
        bool mouseAttacking = mouse != null && mouse.leftButton.isPressed;
        bool hasEnemyInAim = (keyboardAttacking || gamepadAttacking || mouseAttacking) &&
                             TryGetAimedEnemy(out _, out _);

        if (hasAttackParameter)
        {
            animator.SetBool("Attack", hasEnemyInAim);
        }
        bool keyboardShot = keyboard != null && keyboard.fKey.wasPressedThisFrame;
        if (keyboardCollect || gamepadCollect)
        {
            Ray collectionRay = aimCamera.ScreenPointToRay(GetAimScreenPosition());
            strawberryCollector.TryCollect(collectionRay);
        }

        if (keyboardShot || gamepadShot || mouseShot)
        {
            Shoot();
        }
    }

    private void OnGUI()
    {
        Vector2 aimScreenPosition = GetAimScreenPosition();
        float left = aimScreenPosition.x - reticleSize * 0.5f;
        float top = Screen.height - aimScreenPosition.y - reticleSize * 0.5f;
        Color previousColor = GUI.color;
        GUI.color = reticleColor;

        GUI.DrawTexture(
            new Rect(left, top, reticleSize, reticleThickness),
            Texture2D.whiteTexture
        );
        GUI.DrawTexture(
            new Rect(left, top + reticleSize - reticleThickness, reticleSize, reticleThickness),
            Texture2D.whiteTexture
        );
        GUI.DrawTexture(
            new Rect(left, top, reticleThickness, reticleSize),
            Texture2D.whiteTexture
        );
        GUI.DrawTexture(
            new Rect(left + reticleSize - reticleThickness, top, reticleThickness, reticleSize),
            Texture2D.whiteTexture
        );

        GUI.color = previousColor;
    }

    private Vector2 GetAimScreenPosition()
    {
        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    private void Shoot()
    {
        if (Time.time < nextFireTime ||
            !TryGetAimedEnemy(out EnemyHealth enemy, out RaycastHit hit))
        {
            return;
        }

        nextFireTime = Time.time + fireInterval;
        if (shotSound != null)
        {
            shotAudioSource.PlayOneShot(shotSound, shotVolume);
        }

        Ray ray = aimCamera.ScreenPointToRay(GetAimScreenPosition());
        bool isHunter = enemy.GetComponent<HunterAim>() != null;
        bool isPeletero = enemy.GetComponent<PeleteroPatrol>() != null;
        bool isButcher = enemy.GetComponent<ButcherChase>() != null;
        if (enemy.TakeDamage(damage))
        {
            ShowEliminationMark(enemy);
            if (isHunter)
            {
                missionProgress.CompleteMission(MissionProgress.Objective.HunterDefeated);
            }
            else if (isPeletero)
            {
                missionProgress.CompleteMission(MissionProgress.Objective.PeleteroDefeated);
            }
            else if (isButcher)
            {
                missionProgress.CompleteMission(MissionProgress.Objective.ButcherDefeated);
            }
        }

        if (shotParticles != null)
        {
            Vector3 position = muzzlePoint != null ? muzzlePoint.position : transform.position;
            Quaternion rotation = muzzlePoint != null ? muzzlePoint.rotation : transform.rotation;

            ParticleSystem particles = Instantiate(shotParticles, position, rotation);
            particles.Play();

            ParticleSystem.MainModule main = particles.main;
            Destroy(particles.gameObject, main.duration + main.startLifetime.constantMax);
        }

        ShowTracer(ray.origin, hit.point);
    }

    private bool TryGetAimedEnemy(out EnemyHealth aimedEnemy, out RaycastHit enemyHit)
    {
        aimedEnemy = null;
        enemyHit = default;

        Ray ray = aimCamera.ScreenPointToRay(GetAimScreenPosition());
        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            range,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide
        );
        Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            if (hit.collider.GetComponentInParent<StrawberryCollectible>() != null)
            {
                continue;
            }

            aimedEnemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (aimedEnemy == null)
            {
                return false;
            }

            enemyHit = hit;
            return true;
        }

        return false;
    }

    private void CreateEliminationMarks()
    {
        if (eliminatedMarkTexture == null)
        {
            Debug.LogError("PlayerShooter requires the eliminated enemy mark texture.", this);
            return;
        }

        hunterEliminatedMark = CreateEliminationMark(hunterIcon, "Hunter Eliminated Mark");
        peleteroEliminatedMark = CreateEliminationMark(peleteroIcon, "Peletero Eliminated Mark");
        butcherEliminatedMark = CreateEliminationMark(butcherIcon, "Butcher Eliminated Mark");
    }

    private RawImage CreateEliminationMark(RawImage enemyIcon, string markName)
    {
        if (enemyIcon == null)
        {
            Debug.LogError($"PlayerShooter requires the {markName} target icon.", this);
            return null;
        }

        GameObject markObject = new GameObject(
            markName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage)
        );
        markObject.transform.SetParent(enemyIcon.transform, false);
        markObject.transform.SetAsLastSibling();

        RectTransform markRect = (RectTransform)markObject.transform;
        markRect.anchorMin = Vector2.zero;
        markRect.anchorMax = Vector2.one;
        markRect.offsetMin = Vector2.zero;
        markRect.offsetMax = Vector2.zero;
        markRect.pivot = new Vector2(0.5f, 0.5f);

        RawImage markImage = markObject.GetComponent<RawImage>();
        markImage.texture = eliminatedMarkTexture;
        markImage.color = Color.white;
        markImage.raycastTarget = false;
        markImage.enabled = false;
        return markImage;
    }

    private void ShowEliminationMark(EnemyHealth enemy)
    {
        RawImage mark = null;
        if (enemy.GetComponent<HunterAim>() != null)
        {
            mark = hunterEliminatedMark;
        }
        else if (enemy.GetComponent<PeleteroPatrol>() != null)
        {
            mark = peleteroEliminatedMark;
        }
        else if (enemy.GetComponent<ButcherChase>() != null)
        {
            mark = butcherEliminatedMark;
        }

        if (mark != null)
        {
            mark.enabled = true;
        }
    }

    private void CreateTracer()
    {
        GameObject tracerObject = new GameObject("Shot Tracer");
        tracerObject.transform.SetParent(transform, false);
        tracer = tracerObject.AddComponent<LineRenderer>();
        tracer.useWorldSpace = true;
        tracer.positionCount = 2;
        tracer.startWidth = tracerWidth;
        tracer.endWidth = tracerWidth * 0.2f;
        tracer.startColor = new Color(1f, 0.9f, 0.35f, 1f);
        tracer.endColor = new Color(1f, 0.45f, 0.1f, 0.15f);
        tracer.numCapVertices = 4;
        tracer.alignment = LineAlignment.View;
        tracer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        tracer.receiveShadows = false;
        tracer.enabled = false;

        GameObject flashObject = new GameObject("Muzzle Flash");
        flashObject.transform.SetParent(transform, false);
        muzzleFlash = flashObject.AddComponent<LineRenderer>();
        muzzleFlash.useWorldSpace = true;
        muzzleFlash.positionCount = 17;
        muzzleFlash.startWidth = tracerWidth * 0.8f;
        muzzleFlash.endWidth = tracerWidth * 0.8f;
        muzzleFlash.startColor = new Color(1f, 1f, 0.8f, 1f);
        muzzleFlash.endColor = new Color(1f, 0.55f, 0.05f, 1f);
        muzzleFlash.numCapVertices = 4;
        muzzleFlash.alignment = LineAlignment.View;
        muzzleFlash.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        muzzleFlash.receiveShadows = false;
        muzzleFlash.enabled = false;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (shader == null)
        {
            Debug.LogError("PlayerShooter could not find a shader for the shot tracer.", this);
            Destroy(tracerObject);
            tracer = null;
            return;
        }

        tracerMaterial = new Material(shader);
        tracer.material = tracerMaterial;
        muzzleFlash.material = tracerMaterial;
    }

    private void ShowTracer(Vector3 start, Vector3 end)
    {
        if (tracer == null)
        {
            return;
        }

        tracer.SetPosition(0, start);
        tracer.SetPosition(1, end);
        tracer.enabled = true;

        Vector3 direction = (end - start).normalized;
        Vector3 center = start + direction * muzzleFlashSize;
        Vector3 right = aimCamera.transform.right * muzzleFlashSize;
        Vector3 up = aimCamera.transform.up * muzzleFlashSize;
        Vector3[] flashPoints =
        {
            center - right, center, center + right, center,
            center - up, center, center + up, center,
            center - (right + up) * 0.7f, center, center + (right + up) * 0.7f, center,
            center + (right - up) * 0.7f, center, center - (right - up) * 0.7f, center,
            center - right
        };
        muzzleFlash.SetPositions(flashPoints);
        muzzleFlash.enabled = true;

        if (hideTracerCoroutine != null)
        {
            StopCoroutine(hideTracerCoroutine);
        }

        hideTracerCoroutine = StartCoroutine(HideTracerAfterDelay());
    }

    private IEnumerator HideTracerAfterDelay()
    {
        yield return new WaitForSeconds(tracerDuration);
        if (tracer != null)
        {
            tracer.enabled = false;
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.enabled = false;
        }

        hideTracerCoroutine = null;
    }

    private void OnDestroy()
    {
        if (tracerMaterial != null)
        {
            Destroy(tracerMaterial);
        }
    }
}
