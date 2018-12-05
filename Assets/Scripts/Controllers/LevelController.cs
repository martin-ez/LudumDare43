using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    [Header("Level Blueprints")]
    public TextAsset[] chapter1Blueprints;
    public TextAsset[] chapter2Blueprints;
    public TextAsset[] chapter3Blueprints;

    [Header("Character Prefabs")]
    public GameObject[] characterPrefabs;

    [Header("Characters")]
    private Character[] characters;
    private int currentChar = 0;

    [Header("Collectables")]
    private bool[] instruments;
    private int[] money;
    private int fans;

    private LevelBuilder builder;
    private bool needInstrument;
    private int allMembers;

    private int score;
    private int currentMembers;
    private int deadMembers;
    private bool[] deadCharacters;
    private bool[] finishCharacters;
    private bool needCharacterChange;
    private bool showingMap = false;
    private int currentScroll = 0;

    private float maxDistance;
    private float currentTime;

    private enum LevelState
    {
        Scrolls,
        ToStart,
        Playing,
        Pause,
        GameOver
    }

    private LevelState state;

    private MapController map;
    private AudioManager sound;
    private CameraController cam;
    private UIManager ui;

    private float nextInput;

    void Start()
    {
        map = FindObjectOfType<MapController>();
        sound = FindObjectOfType<AudioManager>();
        cam = FindObjectOfType<CameraController>();
        ui = FindObjectOfType<UIManager>();

        characters = new Character[3];
        instruments = new bool[3];
        money = new int[3];
        deadCharacters = new bool[] { true, true, true };
        finishCharacters = new bool[] { false, false, false };

        CreateLevel(Session.ChapterToLoad, Session.LevelToLoad);
        nextInput = Time.time;
    }

    void LateUpdate()
    {
        if (state == LevelState.Playing && characters[currentChar].IsPlayable())
        {
            needCharacterChange = false;
            Vector3 movement = Vector3.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                movement = Vector3.forward;
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                movement = Vector3.right;
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                movement = Vector3.back;
            }
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                movement = Vector3.left;
            }
            if (movement != Vector3.zero)
            {
                movement = Camera.main.transform.TransformDirection(movement);
                if (Mathf.Abs(movement.x) >= Mathf.Abs(movement.z))
                {
                    movement = new Vector3(Mathf.Sign(movement.x), 0, 0);
                }
                else
                {
                    movement = new Vector3(0, 0, Mathf.Sign(movement.z));
                }
                MapBlock toGo = map.GetBlock(characters[currentChar].GetCoord('x') + (int)movement.x, characters[currentChar].GetCoord('z') + (int)movement.z);
                if (toGo != null && toGo.Action(currentChar, false))
                {
                    characters[currentChar].Move(movement, toGo);
                }
                else if (toGo == null)
                {
                    FindObjectOfType<AudioManager>().PlaySound(AudioManager.Sound.No);
                }
            }
            if (needCharacterChange)
            {
                NextCharacter();
            }
        }
        if (state == LevelState.Playing && Time.time > nextInput)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                nextInput = Time.time + 1f;
                NextCharacter();
            }
            else if (Input.GetKey(KeyCode.Keypad1) || Input.GetKey(KeyCode.Alpha1))
            {
                nextInput = Time.time + 1f;
                ChangeCharacter(0);
            }
            else if (Input.GetKey(KeyCode.Keypad2) || Input.GetKey(KeyCode.Alpha2))
            {
                nextInput = Time.time + 1f;
                ChangeCharacter(1);
            }
            else if (Input.GetKey(KeyCode.Keypad3) || Input.GetKey(KeyCode.Alpha3))
            {
                nextInput = Time.time + 1f;
                ChangeCharacter(2);
            }
            else if (Input.GetKey(KeyCode.R))
            {
                nextInput = Time.time + 1f;
                Restart();
            }
        }
        if (state == LevelState.Playing)
        {
            currentTime += Time.deltaTime;
            if (builder.time > 0) ui.ChangeTimer(builder.time - (int)currentTime, builder.time);
            if (builder.time > 0 && currentTime > builder.time * 0.5f)
            {
                float i = currentTime / (builder.time * 0.5f);
                float threshold = Mathf.Lerp(maxDistance, 0f, Easing.Ease(i, Easing.Functions.Linear));
                map.Destroy(threshold);
                if (i > 1) GameOver();
            }
        }
        else if (state == LevelState.Scrolls)
        {
            if (Input.GetKey(KeyCode.Space) && Time.time > nextInput)
            {
                HandleScroll();
                nextInput = Time.time + 0.75f;
            }
        }
        else if (state == LevelState.ToStart)
        {
            if (Input.GetKey(KeyCode.Space) && Time.time > nextInput)
            {
                StartGame();
                nextInput = Time.time + 1f;
            }
        }
        if (Input.GetKey(KeyCode.Escape) && Time.time > nextInput)
        {
            TogglePause();
            nextInput = Time.time + 1f;
        }
    }

    private void CreateLevel(int chapter, int level)
    {
        transform.Find("Goal/Title/Canvas/Text").GetComponent<Text>().text = (chapter == 0 && level == 0) ? "Music Store" : (chapter == 2 || (chapter == 1 && level == 4)) ? "Venue" : "Rehearsal";
        string jsonBlocks = "";
        switch (chapter)
        {
            case 0:
                jsonBlocks = chapter1Blueprints[level].text;
                break;
            case 1:
                jsonBlocks = chapter2Blueprints[level].text;
                break;
            case 2:
                jsonBlocks = chapter3Blueprints[level].text;
                break;
        }
        builder = LevelBuilder.Build(jsonBlocks);
        needInstrument = !(builder.level == 0 && builder.chapter == 0);
        allMembers = 0;
        score = 0;
        currentMembers = 0;
        deadMembers = 0;
        maxDistance = map.CreateLevel(builder.blocks, builder.money > 0, builder.fans > 0);
        map.InitMap();
        if (builder.scrolls.Length == 0 || Session.LevelRestarted)
        {
            state = LevelState.ToStart;
            ui.LevelResume(builder.chapter, builder.level, (int)builder.time, builder.score, builder.money, builder.fans);
        }
        else
        {
            state = LevelState.Scrolls;
            currentScroll = 0;
            LevelBuilder.Scroll scroll = builder.scrolls[currentScroll];
            if (scroll.action.StartsWith("map"))
            {
                ShowMap();
            }
            else if (scroll.action.StartsWith("character"))
            {
                ChangeCharacter(int.Parse(scroll.action.Split(':')[1]));
            }
            ui.ShowScroll(scroll.text);
            if (currentScroll == builder.scrolls.Length)
            {
                state = LevelState.ToStart;
                ui.HideScroll();
                ui.LevelResume(builder.chapter, builder.level, (int)builder.time, builder.score, builder.money, builder.fans);
            }
            nextInput = Time.time + 0.5f;
        }
    }

    private void HandleScroll()
    {
        LevelBuilder.Scroll scroll = builder.scrolls[currentScroll];
        if (ui.OnScroll())
        {
            ui.SkipScroll(scroll.text);
        }
        else
        {
            currentScroll++;
            if (currentScroll == builder.scrolls.Length)
            {
                state = LevelState.ToStart;
                ui.HideScroll();
                ui.LevelResume(builder.chapter, builder.level, (int)builder.time, builder.score, builder.money, builder.fans);
            }
            else
            {
                scroll = builder.scrolls[currentScroll];
                if (scroll.action.StartsWith("map"))
                {
                    ShowMap();
                }
                else if (scroll.action.StartsWith("char"))
                {
                    ChangeCharacter(int.Parse(scroll.action.Split(':')[1]));
                }
                ui.ShowScroll(scroll.text);
            }
        }
    }

    private void ShowMap()
    {
        map.AppearMap();
        showingMap = true;
    }

    private void StartGame()
    {
        for (int i = 0; i < 3; i++)
        {
            if (i < allMembers)
            {
                sound.TurnFilter(i);
            }
            else
            {
                sound.TurnOff(i);
            }
        }
        if (!showingMap) ShowMap();
        ui.HideResume();
        string place = ((builder.chapter == 1 && builder.level == 4) || builder.chapter == 2) ? "Show starts in" : "Rehearsal starts in";
        ui.SetupMetrics(allMembers, needInstrument, builder.time, builder.money, builder.fans, place);
        ChangeCharacter(0);
        state = LevelState.Playing;
        currentTime = 0;
    }

    public void AddCharacter(int player, MapBlock block)
    {
        GameObject character = Instantiate(characterPrefabs[player]);
        Character member = character.GetComponent<Character>();
        member.Init(player, block);
        characters[player] = member;
        deadCharacters[player] = false;
        allMembers++;
    }

    public void OnCharacterDeath(int player)
    {
        deadMembers++;
        sound.TurnOff(player);
        deadCharacters[player] = true;
        ui.KillCharacter(player);
        if (deadMembers + currentMembers == allMembers)
        {
            GameOver();
        }
        else if (currentChar == player)
        {
            NextCharacter();
        }
    }

    private void NextCharacter()
    {
        int next = currentChar;
        int attempts = 0;
        while (attempts < 3)
        {
            next = next + 1;
            if (next == allMembers) next = 0;
            if (!deadCharacters[next])
            {
                if (finishCharacters[next])
                {
                    finishCharacters[next] = false;
                    characters[next].Show();
                    currentMembers--;
                }
                ChangeCharacter(next);
                return;
            }
            attempts++;
        }
    }

    private void ChangeCharacter(int character)
    {
        if (character < allMembers && !deadCharacters[character] && !finishCharacters[character])
        {
            currentChar = character;
            ui.SetActiveCharacter(character);
            cam.ChangeCharacter(characters[character]);
        }
    }

    public bool CharacterFinish(int player)
    {
        if (!(builder.money > 0 && money[player] < builder.money) && !(needInstrument && !instruments[player]))
        {
            currentMembers++;
            finishCharacters[player] = true;
            sound.PlaySound(AudioManager.Sound.Finish);
            if (!characters[player].AlreadyDeposit())
            {
                score += 100;
                score += money[player] - builder.money;
            }
            ui.ChangeScore(score);
            characters[player].Hide();
            if (deadMembers + currentMembers == allMembers)
            {
                GameOver();
            }
            else
            {
                needCharacterChange = true;
            }
            return true;
        }
        else
        {
            sound.PlaySound(AudioManager.Sound.No);
            return false;
        }
    }

    public Character GetCurrentCharacter()
    {
        return characters[currentChar];
    }

    public void CollectInstrument(int player)
    {
        instruments[player] = true;
        ui.AddInstrument(player);
        sound.TurnOn(player);
        sound.PlaySound(AudioManager.Sound.PickupInstruments);
    }

    public void CollectMoney(int player, int amount)
    {
        money[player] += amount;
        ui.AddMoney(player, money[player], builder.money);
        sound.PlaySound(AudioManager.Sound.PickupMoney);
    }

    public void CollectFans(int amount)
    {
        if (fans > builder.fans)
        {
            score += amount;
            ui.ChangeScore(score);
        }
        fans += amount;
        ui.AddFans(fans, builder.fans);
        sound.PlaySound(AudioManager.Sound.PickupFans);
    }

    public void TogglePause()
    {
        if (state == LevelState.Playing)
        {
            state = LevelState.Pause;
            ui.Pause(true);
        }
        else
        {
            state = LevelState.Playing;
            ui.Pause(false);
        }

    }

    public void GameOver()
    {
        state = LevelState.GameOver;
        bool success = score >= builder.score && !(builder.fans > 0 && fans < builder.fans);
        Session.LevelCompleted = success;
        ui.GameOver(success, score, builder.score, fans, builder.fans);
    }

    public void Resume()
    {
        TogglePause();
    }

    public void Restart()
    {
        Session.LevelRestarted = true;
        SceneManager.LoadScene("Level");
    }

    public void NewLevel()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void Quit()
    {
        Session.QuitGame();
    }
}
