using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor;

#region Data Classes
[System.Serializable]
public class CharacterInformation
{
    [Header("Character Info")]
    [Tooltip("Name of the speaking character.")]
    public string name;

    [Tooltip("Character portrait sprite to display during dialogue.")]
    public Sprite sprite;

    [Space(10)]
    [Header("Dialogue")]
    [Tooltip("Dialogue text for this character.")]
    [TextArea]
    public string text;

    [Tooltip("If true, the character portrait will appear on the right side of the screen.")]
    public bool isRight = false;

    [Space(10)]
    [Header("Audio & Events")]
    [Tooltip("Optional reaction sound to play when this dialogue is active.")]
    public AudioClip reactionSFX;

    [Tooltip("Event triggered when THIS SPECIFIC LINE finishes playing.")]
    public UnityEvent finishEvent;
}

[System.Serializable]
public class KeyDuration
{
    [Header("Wait Key Settings")]
    [Tooltip("Special character or key used to trigger a typing pause.")]
    public string key;

    [Tooltip("Duration (in seconds) to wait when this key is encountered.")]
    public float duration = 0.1f;
}
#endregion

public class DialogueManager : MonoBehaviour
{
    #region Variables

    [Header("Singleton & Lifecycle")]
    [Tooltip("Global reference to the DialogueManager singleton instance.")]
    public static DialogueManager Instance;

    [Tooltip("If enabled, the dialogue system will start automatically on Awake.")]
    public bool playOnAwake;

    [Space(10)]
    [Header("Dialogue Settings")]
    [Tooltip("List of all characters and their dialogue lines.")]
    public List<CharacterInformation> allDialogues;

    [Tooltip("Typing speed for characters (seconds per character).")]
    public float textTypingSpeed = 0.01f;

    [Tooltip("Special wait keys and their durations to pause the typing.")]
    public KeyDuration[] allWaitKey;

    [Tooltip("Playback speed multiplier for dialogue-related animations.")]
    [Range(0.1f, 5f)]
    public float animationSpeed = 1f;

    [Space(10)]
    [Header("Input")]
    [Tooltip("Keyboard key used to skip or advance dialogue.")]
    public KeyCode skipDialoguesKey = KeyCode.Space;

    [Space(10)]
    [Header("Behavior")]
    [Tooltip("If the player presses skip before typing ends, complete the line instantly.")]
    public bool canSkipDialogue = true;

    [Tooltip("YENİ: Diyalog oynarken oyundaki zamanı (TimeScale) tamamen durdurur.")]
    public bool freezeTimeDuringDialogue = false;

    [Space(10)]
    [Header("Audio")]
    [Tooltip("AudioSource for character reaction SFX during typing.")]
    public AudioSource characterReactionSFX;

    [Tooltip("AudioSource for typing SFX while text is being written.")]
    public AudioSource typingSFX;

    [Space(10)]
    [Header("UI References")]
    [Tooltip("Dialogue box GameObject.")]
    public GameObject textBox;

    [Tooltip("Portrait of the speaking character on the right side.")]
    public Image rightImage;

    [Tooltip("Portrait of the speaking character on the left side.")]
    public Image leftImage;

    [Tooltip("TextMeshPro field where the dialogue line is written.")]
    public TextMeshProUGUI text;

    [Tooltip("TextMeshPro field for the speaker's name.")]
    public TextMeshProUGUI characterNameText;

    [Space(10)]
    [Header("Sequence Events")]
    [Tooltip("YENİ: TÜM diyalog sekansı tamamen bittiğinde tetiklenecek ana olay (Sahne geçişi vb. için).")]
    public UnityEvent onDialogueSequenceEnded;

    [Space(10)]
    [Header("Runtime State")]
    [ReadOnly, Tooltip("[ReadOnly] Is the dialogue box currently open?")]
    public bool textBoxIsOpen = false;

    [ReadOnly, Tooltip("[ReadOnly] Is the dialogue system currently playing?")]
    public bool isPlayDialogue = false;

    [Space(10)]
    [Header("Internals")]
    [Tooltip("Internal flag to track when the skip key is pressed.")]
    private bool pressSkipButton = false;

    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (playOnAwake)
        {
            StartDialogue();
        }
    }

    private void Update()
    {
        // --- DÜZELTME: Hem belirlediğin tuşla (Space) hem de Sol Tık ile geçiş yapılabilir ---
        if (Input.GetKeyDown(skipDialoguesKey) || Input.GetMouseButtonDown(0))
        {
            pressSkipButton = true;
        }
    }
    #endregion

    #region Dialogue Control
    public void StartDialogue()
    {
        if (isPlayDialogue == false)
        {
            isPlayDialogue = true;
            StartCoroutine(StartDialogueIE());
        }
    }

    public void AddDialogueAndStart(List<CharacterInformation> newDialogues)
    {
        if (isPlayDialogue == false)
        {
            isPlayDialogue = true;
            allDialogues.Clear();
            allDialogues.AddRange(newDialogues);
            StartCoroutine(StartDialogueIE());
        }
    }
    #endregion

    #region Image & Animation
    void ChangeImage(bool right, Sprite currentSprite, bool instant = false)
    {
        float tFade = instant ? 0.25f / animationSpeed : 0.5f / animationSpeed;
        float tScale = instant ? 0.5f / animationSpeed : 1f / animationSpeed;

        if (right)
        {
            if (rightImage.sprite != currentSprite)
                rightImage.sprite = currentSprite;

            rightImage.gameObject.SetActive(rightImage.sprite != null);
            leftImage.gameObject.SetActive(leftImage.sprite != null);

            leftImage.DOFade(0.5f, tFade).SetUpdate(true);
            leftImage.transform.DOScale(0.97f, tScale).SetUpdate(true);

            rightImage.DOFade(1f, tFade).SetUpdate(true);
            rightImage.transform.DOScale(1f, tScale).SetUpdate(true);
        }
        else
        {
            if (leftImage.sprite != currentSprite)
                leftImage.sprite = currentSprite;

            rightImage.gameObject.SetActive(rightImage.sprite != null);
            leftImage.gameObject.SetActive(leftImage.sprite != null);

            leftImage.DOFade(1f, tFade).SetUpdate(true);
            leftImage.transform.DOScale(1f, tScale).SetUpdate(true);

            rightImage.DOFade(0.5f, tFade).SetUpdate(true);
            rightImage.transform.DOScale(0.97f, tScale).SetUpdate(true);
        }
    }
    #endregion

    #region UI Animation
    IEnumerator OpenTextBox(bool isOpen)
    {
        var boxImg = textBox.GetComponent<Image>();
        boxImg.DOKill();
        textBox.transform.DOKill();

        if (isOpen)
        {
            textBox.SetActive(true);

            characterNameText.DOFade(1f, 0f).SetUpdate(true);

            leftImage.gameObject.SetActive(leftImage.sprite != null);
            rightImage.gameObject.SetActive(rightImage.sprite != null);

            boxImg.DOFade(1f, 0.25f / animationSpeed).From(0.85f).SetUpdate(true);
            textBox.transform.DOScale(1f, 0.25f / animationSpeed).From(0.85f).SetUpdate(true);

            yield return new WaitForSecondsRealtime(0.25f / animationSpeed);
        }
        else
        {
            isPlayDialogue = false;

            boxImg.DOFade(0f, 0.25f / animationSpeed).SetUpdate(true);
            textBox.transform.DOScale(0.85f, 0.25f / animationSpeed).From(1f).SetUpdate(true);

            leftImage.DOFade(0f, 0.25f / animationSpeed).SetUpdate(true);
            rightImage.DOFade(0f, 0.25f / animationSpeed).SetUpdate(true);

            yield return new WaitForSecondsRealtime(0.25f / animationSpeed);

            textBox.SetActive(false);
            text.text = string.Empty;
            characterNameText.text = string.Empty;

            rightImage.sprite = null;
            rightImage.gameObject.SetActive(false);
            leftImage.sprite = null;
            leftImage.gameObject.SetActive(false);
        }
        textBoxIsOpen = isOpen;
    }
    #endregion

    #region Dialogue Coroutine
    IEnumerator StartDialogueIE()
    {
        if (freezeTimeDuringDialogue)
        {
            Time.timeScale = 0f;
        }

        #region StartReset
        leftImage.DOKill(); rightImage.DOKill();
        leftImage.transform.DOKill(); rightImage.transform.DOKill();

        leftImage.sprite = null;
        rightImage.sprite = null;

        var lc = leftImage.color; lc.a = 0f; leftImage.color = lc;
        var rc = rightImage.color; rc.a = 0f; rightImage.color = rc;

        leftImage.gameObject.SetActive(false);
        rightImage.gameObject.SetActive(false);
        #endregion

        Sprite oldSpriteRight = null, oldSpriteLeft = null;
        text.text = "";

        for (int i = 0; i < allDialogues.Count; i++)
        {
            pressSkipButton = false;
            characterNameText.text = allDialogues[i].name;

            if (!textBoxIsOpen)
            {
                yield return OpenTextBox(true);
            }

            text.text = string.Empty;

            if (allDialogues[i].isRight)
            {
                if (oldSpriteRight != allDialogues[i].sprite)
                {
                    rightImage.DOFade(0f, 0.25f).SetUpdate(true);
                    rightImage.transform.DOScale(0.95f, 0.25f).SetUpdate(true);
                }
            }
            else
            {
                if (oldSpriteLeft != allDialogues[i].sprite)
                {
                    leftImage.DOFade(0f, 0.25f).SetUpdate(true);
                    leftImage.transform.DOScale(0.95f, 0.25f).SetUpdate(true);
                }
            }

            yield return new WaitForSecondsRealtime(0.25f);

            bool instant = true;
            ChangeImage(allDialogues[i].isRight, allDialogues[i].sprite, instant);

            if (allDialogues[i].isRight) oldSpriteRight = allDialogues[i].sprite;
            else oldSpriteLeft = allDialogues[i].sprite;

            if (allDialogues[i].reactionSFX != null)
            {
                characterReactionSFX.clip = allDialogues[i].reactionSFX;
                characterReactionSFX.Play();
            }

            #region WriteText
            string currentText = allDialogues[i].text;
            foreach (char letter in currentText)
            {
                text.text += letter;

                float waitTime = textTypingSpeed;

                for (int j = 0; j < allWaitKey.Length; j++)
                {
                    if (letter.ToString() == allWaitKey[j].key)
                    {
                        waitTime = allWaitKey[j].duration;
                        break;
                    }
                }

                if (pressSkipButton)
                {
                    text.text = allDialogues[i].text;
                    pressSkipButton = false;

                    // --- DÜZELTME: Metin atlandığında sese ANINDA sus emri veriliyor ---
                    if (typingSFX != null) typingSFX.Stop();

                    break;
                }

                if (typingSFX.clip != null)
                {
                    typingSFX.Play();
                }

                yield return new WaitForSecondsRealtime(waitTime);
            }

            // --- DÜZELTME: Döngü normal yolla (atlamadan) bitse bile sesi durduruyoruz ---
            if (typingSFX != null) typingSFX.Stop();
            #endregion

            // --- DÜZELTME: Sonraki satıra geçmek için hem Space hem de Sol Tık (Mouse 0) eklendi ---
            yield return new WaitUntil(() => Input.GetKeyDown(skipDialoguesKey) || Input.GetMouseButtonDown(0));

            // Metni geçtikten sonra bir sonraki cümleye başlarken yanlışlıkla basılmış sayılmasın diye temizlik
            pressSkipButton = false;

            if (allDialogues[i].finishEvent != null) allDialogues[i].finishEvent.Invoke();
        }

        yield return OpenTextBox(false);
        isPlayDialogue = false;

        if (freezeTimeDuringDialogue)
        {
            Time.timeScale = 1f;
        }

        if (onDialogueSequenceEnded != null)
        {
            onDialogueSequenceEnded.Invoke();
        }
    }
    #endregion
}

#region Custom Inspector
[CustomEditor(typeof(DialogueManager))]
public class DialogueInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(5);
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 14;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Created by Batu Özçamlık", titleStyle);

        GUILayout.Space(5);

        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        textStyle.wordWrap = true;
        textStyle.richText = true;

        GUIStyle signatureStyle = new GUIStyle(GUI.skin.label);
        signatureStyle.alignment = TextAnchor.MiddleRight;
        signatureStyle.fontStyle = FontStyle.Italic;
        EditorGUILayout.LabelField("www.batuozcamlik.com", signatureStyle);
    }
}
#endregion