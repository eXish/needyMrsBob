using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class needyMrsBobScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMNeedyModule needyModule;
    public KMAudio Audio;

    [TextArea] public String[] potentialMrsBobMessages;
    public Texture[] potentialMrsBobEmojis;

    public TextMesh mrsBobsMessage;
    public Renderer mrsBobsEmoji;
    public GameObject unreadMessage;

    public GameObject simonMessage;
    public GameObject responses;
    [TextArea] public String[] simonMessageOptions;
    public TextMesh simonText;

    public KMSelectable[] responseButtons;
    public Texture[] responseOptions;
    public string[] emojiNameOptions;
    public ResponseEmojiName[] emojiNames;
    private List<int> chosenIndices = new List<int>();
    public GameObject responseEmojiObject;
    public Renderer responseEmoji;
    private Texture selectedTexture;
    private string selectedResponse = "";

    public string[] answerOptions;
    private string correctAnswer = "";
    int bankIndex = 0;
    int index1 = 0;
    int index2 = 0;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        needyModule = GetComponent<KMNeedyModule>();
        needyModule.OnNeedyActivation += OnNeedyActivation;
        needyModule.OnNeedyDeactivation += OnNeedyDeactivation;
        needyModule.OnTimerExpired += OnTimerExpired;
        foreach (KMSelectable button in responseButtons)
        {
            KMSelectable pressedResponse = button;
            button.OnInteract += delegate () { ResponsePress(pressedResponse); return false; };
        }
    }


    void Start()
    {
        int simonIndex = UnityEngine.Random.Range(0,20);
        simonText.text = simonMessageOptions[simonIndex];
        responseEmojiObject.SetActive(false);
        unreadMessage.SetActive(false);
        responses.SetActive(false);
        mrsBobsEmoji.material.mainTexture = potentialMrsBobEmojis[4];
    }

    void OnNeedyActivation()
    {
        StartCoroutine(ReceiveMessage());
    }

    IEnumerator ReceiveMessage()
    {
        yield return new WaitForSeconds(1f);
        Audio.PlaySoundAtTransform("receive", transform);
        ContinueActivation();
    }

    void ContinueActivation()
    {
        responseEmojiObject.SetActive(false);
        bankIndex = UnityEngine.Random.Range(0,6);
        index1 = UnityEngine.Random.Range(0,4);
        index2 = UnityEngine.Random.Range(0,5);
        mrsBobsMessage.text = potentialMrsBobMessages[index1 + (bankIndex * 4)];
        mrsBobsEmoji.material.mainTexture = potentialMrsBobEmojis[index2 + (bankIndex * 5)];
        Debug.LogFormat("[Needy Mrs Bob #{0}] Message received: '{1}'.", moduleId, mrsBobsMessage.text.Replace("\n", " "));
        Debug.LogFormat("[Needy Mrs Bob #{0}] Emoji: {1}.", moduleId, mrsBobsEmoji.material.mainTexture.name);
        unreadMessage.SetActive(true);
        simonMessage.SetActive(false);
        responses.SetActive(true);
        SetResponses();
    }

    void SetResponses()
    {
        for(int i = 0; i <= 23; i++)
        {
            int emojiIndex = UnityEngine.Random.Range(0,24);
            while(chosenIndices.Contains(emojiIndex))
            {
                emojiIndex = UnityEngine.Random.Range(0,24);
            }
            chosenIndices.Add(emojiIndex);
            responseButtons[i].GetComponentInChildren<Renderer>().material.mainTexture = responseOptions[emojiIndex];
            emojiNames[i].emojiName = emojiNameOptions[emojiIndex];
        }
        chosenIndices.Clear();
        CalculateAnswer();
    }

    void CalculateAnswer()
    {
        correctAnswer = answerOptions[(bankIndex * 20) + (index1 * 5) + index2];
        Debug.LogFormat("[Needy Mrs Bob #{0}] Correct response: {1}.", moduleId, correctAnswer);
    }

    void OnNeedyDeactivation()
    {
        GetComponent<KMNeedyModule>().HandlePass();
        responses.SetActive(false);
        simonMessage.SetActive(true);
        mrsBobsEmoji.material.mainTexture = potentialMrsBobEmojis[4];
    }

    void OnTimerExpired()
    {
        Debug.LogFormat("[Needy Mrs Bob #{0}] Strike! You ran out of time. Needy deactivated", moduleId);
        GetComponent<KMNeedyModule>().HandleStrike();
        OnNeedyDeactivation();
    }

    void ResponsePress(KMSelectable pressedResponse)
    {
        if(moduleSolved)
        {
            return;
        }
        pressedResponse.AddInteractionPunch(.5f);
        Audio.PlaySoundAtTransform("send", transform);
        selectedResponse = pressedResponse.GetComponentInChildren<ResponseEmojiName>().emojiName;
        mrsBobsMessage.text = "";
        responseEmojiObject.SetActive(true);
        selectedTexture = pressedResponse.GetComponentInChildren<Renderer>().material.mainTexture;
        responseEmoji.material.mainTexture = selectedTexture;
        if(selectedResponse == correctAnswer)
        {
            unreadMessage.SetActive(false);
            OnNeedyDeactivation();
            Debug.LogFormat("[Needy Mrs Bob #{0}] Message sent: {1}. That is correct. Needy deactivated.", moduleId, selectedResponse);
        }
        else
        {
            unreadMessage.SetActive(false);
            GetComponent<KMNeedyModule>().HandleStrike();
            OnNeedyDeactivation();
            Debug.LogFormat("[Needy Mrs Bob #{0}] Strike! Message sent: {1}. That is not correct. Needy deactivated.", moduleId, selectedResponse);
        }
    }
}
