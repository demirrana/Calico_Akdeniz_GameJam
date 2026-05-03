using System.Collections;
using UnityEngine;

public class ParryManager : MonoBehaviour
{
    public static ParryManager Instance { get; private set; }

    public bool IsParryActive = false;
    [SerializeField] private GameObject parryVisual;

    private bool isParrySuccessful;

    private float reactionTime = 1f;
    private float level2MinRythmPeriod = 1f; //change them if periods are long or short
    private float level2MaxRythmPeriod = 1.3f;

    private void Awake()
    {
        SetSingleton();
    }

    private void SetSingleton()
    {
        if (Instance != null && this != Instance)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    public IEnumerator TryParry(int level, System.Action onComplete)
    {
        Debug.Log($"6.5: TryParry Level {level}");

        //there is a 50% chance of getting parry opportunity in levels 1 and 2
        if (level < 3 && Random.value > 0.5f) 
        {
            onComplete?.Invoke();
            yield break;
        }

        IsParryActive = true;
        bool success = false;
        Vector3 originalScale = parryVisual.transform.localScale;
        SpriteRenderer spriteComp = parryVisual.GetComponent<SpriteRenderer>();

        //each level has its own method
        if (level == 1)
        {
            yield return StartCoroutine(Level1HoldRoutine(originalScale, spriteComp, (result) => success = result));
        }
        else if (level == 2)
        {
            yield return StartCoroutine(Level2RhythmicRoutine(originalScale, spriteComp, (result) => success = result));
        }
        else // Level 3 veya default
        {
            yield return StartCoroutine(Level3ClassicRoutine(originalScale, spriteComp, (result) => success = result));
        }

        parryVisual.SetActive(false);
        IsParryActive = false;
        isParrySuccessful = success;

        Debug.Log(success ? "PARRY BAŞARILI!" : "PARRY KAÇIRILDI!");
        onComplete?.Invoke();
    }

    private IEnumerator Level1HoldRoutine(Vector3 scale, SpriteRenderer sprite, System.Action<bool> callback)
    {
        parryVisual.SetActive(true);
        float timer = reactionTime;
        bool hasPressed = false;

        while (timer > 0) 
        {
            timer -= Time.deltaTime;
            ApplyGlowEffect(scale, sprite);
            if (Input.GetKeyDown(KeyCode.Space)) 
            { 
                hasPressed = true; 
                break; 
            }
            yield return null;
        }

        if (!hasPressed) { 
            callback(false); 
            yield break; 
        }

        float holdTime = 2.0f; //should press for 2 seconds
        while (holdTime > 0) {
            holdTime -= Time.deltaTime;
            ApplyGlowEffect(scale, sprite, true); //light brightness stay the same

            if (Input.GetKeyUp(KeyCode.Space)) //player lets go early
            { 
                callback(false); 
                yield break; 
            }

            yield return null;
        }

        timer = reactionTime;
        bool hasReleased = false;
        while (timer > 0) {
            timer -= Time.deltaTime;
            ApplyGlowEffect(scale, sprite); //
            if (Input.GetKeyUp(KeyCode.Space)) 
            { 
                hasReleased = true;
                break;
            }

            yield return null;
        }
        callback(hasReleased);
    }

    private IEnumerator Level2RhythmicRoutine(Vector3 scale, SpriteRenderer sprite, System.Action<bool> callback)
    {
        for (int i = 0; i < 3; i++) {
            parryVisual.SetActive(true);
            float timer = reactionTime * 0.8f; //a bit quicker than level 1
            bool hit = false;
            while (timer > 0) {
                timer -= Time.deltaTime;
                ApplyGlowEffect(scale, sprite);
                if (Input.GetKeyDown(KeyCode.Space)) 
                {
                    hit = true;
                    break;
                }

                yield return null;
            }
            parryVisual.SetActive(false);
            if (!hit) 
            { 
                callback(false); 
                yield break; 
            }

            yield return new WaitForSeconds(Random.Range(level2MinRythmPeriod, level2MaxRythmPeriod));
        }
        callback(true);
    }

    private IEnumerator Level3ClassicRoutine(Vector3 scale, SpriteRenderer sprite, System.Action<bool> callback)
    {
        parryVisual.SetActive(true);

        float timer = reactionTime;
        bool hit = false;

        while (timer > 0) {
            timer -= Time.deltaTime;
            ApplyGlowEffect(scale, sprite);

            if (Input.GetKeyDown(KeyCode.Space))
            { 
                hit = true;
                break;
            }

            yield return null;
        }
        
        callback(hit);
    }

    private void ApplyGlowEffect(Vector3 originalScale, SpriteRenderer spriteComp, bool isStatic = false)
    {
        //brightness stays the same when isStatic is true
        float intensity = isStatic ? reactionTime : Mathf.PingPong(Time.time * 15, reactionTime); 
        
        if (spriteComp != null) {
            Color c = spriteComp.color;
            c.a = intensity;
            spriteComp.color = c;
        }

        float scaleMultiplier = Mathf.Lerp(0.8f, 3.5f, intensity);
        parryVisual.transform.localScale = originalScale * scaleMultiplier;
    }

    public bool IsParrySuccessful()
    {
        return isParrySuccessful;
    }

    private bool CheckSuccessionParryLevel1()
    {
        return true;
    }

    private bool CheckSuccessionParryLevel2()
    {
        return true;
    }

    private bool CheckSuccessionParryLevel3()
    {
        return true;
    }

    private void SucceedParryLevel1()
    {
        
    }

    private void SucceedParryLevel2()
    {
        
    }

    private void SucceedParryLevel3()
    {
        
    }
}
