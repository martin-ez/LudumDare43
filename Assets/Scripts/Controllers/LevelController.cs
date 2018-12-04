using UnityEngine;
using UnityEngine.SceneManagement;

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

    private BlockBuilder.MapBuilder builder;
    private bool needInstrument;
    private int allMembers;

    private int score;
    private int currentMembers;
    private int deadMembers;
    private bool[] deadCharacters;
    private bool[] finishCharacters;

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
            Vector3 movement = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                movement = Vector3.forward;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                movement = Vector3.right;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                movement = Vector3.back;
            }
            else if (Input.GetKey(KeyCode.A))
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
        }
        if (state == LevelState.Playing)
        {
            if (Input.GetKey(KeyCode.Space) && Time.time > nextInput)
            {
                NextCharacter();
                nextInput = Time.time + 1f;
            }
            else if ((Input.GetKey(KeyCode.Keypad1) || (Input.GetKey(KeyCode.Alpha1)) && Time.time > nextInput))
            {
                ChangeCharacter(0);
                nextInput = Time.time + 1f;
            }
            else if ((Input.GetKey(KeyCode.Keypad2) || (Input.GetKey(KeyCode.Alpha2)) && Time.time > nextInput))
            {
                ChangeCharacter(1);
                nextInput = Time.time + 1f;
            }
            else if ((Input.GetKey(KeyCode.Keypad3) || (Input.GetKey(KeyCode.Alpha3)) && Time.time > nextInput))
            {
                ChangeCharacter(2);
                nextInput = Time.time + 1f;
            }

            currentTime += Time.deltaTime;
            if (currentTime > builder.time * 0.2f && builder.time > 0)
            {
                float i = currentTime / builder.time;
                float threshold = Mathf.Lerp(maxDistance, 0f, Easing.Ease(i, Easing.Functions.Linear));
                map.Destroy(threshold);
                if (i > 1) GameOver();
            }
        }
        else if (state == LevelState.Scrolls)
        {
            if (Input.GetKey(KeyCode.Space) && Time.time > nextInput)
            {
                //TODO NextScroll
                nextInput = Time.time + 1f;
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
        else if (Input.GetKey(KeyCode.Escape) && Time.time > nextInput)
        {
            TogglePause();
            nextInput = Time.time + 1f;
        }
    }

    private void CreateLevel(int chapter, int level)
    {
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
        builder = BlockBuilder.Build(jsonBlocks);
        needInstrument = !(builder.level == 0 && builder.chapter == 0);
        allMembers = 0;
        score = 0;
        currentMembers = 0;
        deadMembers = 0;
        maxDistance = map.CreateLevel(builder.blocks, builder.money > 0, builder.fans > 0);
        map.InitMap();
        if (sound.GetChapter() != chapter)
        {
            sound.ChangeChapter(chapter, allMembers);
        }
        else
        {
            sound.ChangeLevel(allMembers);
        }
        if (builder.scrolls.Length == 0)
        {
            state = LevelState.ToStart;
            ui.LevelResume(chapter, level, (int)builder.time, builder.members, builder.money, builder.fans);
        }
        else
        {
            state = LevelState.Scrolls;
            //TODO ui.ShowScroll();
        }
    }

    private void StartGame()
    {
        map.AppearMap();
        ui.HideResume();
        ui.SetupMetrics(allMembers, needInstrument, builder.money, builder.fans);
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
        sound.KillPlayer(player);
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
            if (!deadCharacters[next] && !finishCharacters[next])
            {
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
            //TODO Calculate score
            if (deadMembers + currentMembers == allMembers)
            {
                GameOver();
            }
            else
            {
                NextCharacter();
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
        sound.TogglePlayer(player, 0f);
        sound.PlaySound(AudioManager.Sound.PickupInstruments);
    }

    public void CollectMoney(int player, int amount)
    {
        money[player] += amount;
        ui.AddMoney(player, money[player], builder.money);
        sound.PlaySound(AudioManager.Sound.PickupMoney);
    }

    public void CollectFans(int amaount)
    {
        fans += amaount;
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
        bool success = currentMembers >= builder.members && !(builder.fans > 0 && fans < builder.fans);
        Session.LevelCompleted = success;
        ui.GameOver(success);
    }

    public void Resume()
    {
        TogglePause();
    }

    public void Restart()
    {
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
