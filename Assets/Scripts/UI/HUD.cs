using System;
using UnityEngine;
using UnityEngine.UI;       // Image, legacy Text
using TMPro;                // TMP_Text

public class HUD : MonoBehaviour
{
    [Header("Refs")]
    public Health playerHealth;      // drag PlayerRoot Health (auto-found if empty)
    public TMP_Text tmpText;         // optional
    public Text uiText;              // optional

    [Header("Bar (Filled Image)")]
    public Image barFill;            // set an Image whose Type = Filled (Horizontal)
    public bool smoothBar = true;
    public float barLerpSpeed = 10f;

    [Header("Formatting / Colors")]
    public string format = "Health: {0}/{1}";
    public Color normalColor = Color.white;              // text color when healthy
    public Color lowColor = new Color(1f, 0.35f, 0.35f); // text color when low
    [Range(0f, 1f)] public float lowThreshold = 0.25f;

    [Header("Optional Bar Gradient")]
    public bool useBarGradient = true;
    public Gradient barGradient = new Gradient
    {
        colorKeys = new[]
        {
            new GradientColorKey(new Color(0.9f, 0.2f, 0.2f), 0f),  // low = red
            new GradientColorKey(new Color(0.2f, 0.8f, 0.25f), 1f), // high = green
        },
        alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
    };

    float targetFill = 1f;

    void Awake()
    {
        // Auto-find player health if empty
        if (!playerHealth)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) playerHealth = p.GetComponent<Health>();
        }

        // If barFill not set, try to find an Image under this object
        if (!barFill) barFill = GetComponentInChildren<Image>();
    }

    void OnEnable()
    {
        if (playerHealth)
        {
            playerHealth.onHealthChanged.AddListener(OnHealthChanged);
            OnHealthChanged(playerHealth.currentHP, playerHealth.maxHP); // initial
        }
    }

    void OnDisable()
    {
        if (playerHealth)
            playerHealth.onHealthChanged.RemoveListener(OnHealthChanged);
    }

    void Update()
    {
        if (!barFill) return;

        if (smoothBar)
        {
            barFill.fillAmount = Mathf.Lerp(
                barFill.fillAmount, targetFill,
                1f - Mathf.Exp(-barLerpSpeed * Time.unscaledDeltaTime)
            );
        }
        else
        {
            barFill.fillAmount = targetFill;
        }

        if (useBarGradient)
            barFill.color = barGradient.Evaluate(barFill.fillAmount);
    }

    void OnHealthChanged(float current, float max)
    {
        int c = Mathf.CeilToInt(current);
        int m = Mathf.CeilToInt(max);

        // ---- text ----
        string s = FormatHP(format, c, m);
        if (tmpText) tmpText.text = s;
        if (uiText) uiText.text = s;

        float pct = (max > 0f) ? Mathf.Clamp01(current / max) : 0f;
        var textColor = (pct <= lowThreshold) ? lowColor : normalColor;
        if (tmpText) tmpText.color = textColor;
        if (uiText) uiText.color = textColor;

        // ---- bar ----
        targetFill = pct;
        if (!smoothBar && barFill) barFill.fillAmount = targetFill;
        if (useBarGradient && barFill) barFill.color = barGradient.Evaluate(targetFill);
    }

    // Handles both "HP {0}" and "HP {0}/{1}" (and avoids FormatException)
    static string FormatHP(string fmt, int current, int max)
    {
        // Fast path: if it references {1}, pass both; otherwise pass one.
        if (!string.IsNullOrEmpty(fmt) && fmt.IndexOf("{1}", StringComparison.Ordinal) >= 0)
            return string.Format(fmt, current, max);

        // Fallback to single-arg; if someone typed weird placeholders, catch it.
        try { return string.Format(fmt, current); }
        catch { return $"{current}/{max}"; }
    }
}
