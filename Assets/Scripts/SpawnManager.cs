using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Channel;
using EnemyBehaviour;
using GameGrid;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    [Header("ScoreStuffs")]
    [SerializeField] private ScoreTracker ScoreTrackerScript;
    [SerializeField] private TextMeshProUGUI ScoreNotifierText;

    [FormerlySerializedAs("enemies")] public List<Enemy> enemyPrefabs;
    public float wave;
    public float statGain; //scaling variable for enemy health
    [SerializeField] private TextMeshProUGUI waveDisplayText;
    [SerializeField] private GameObject tutorialDisplayText;
    public GameObject enemySpawn;

    [Header("Audio")] public AudioClip spawnSound;

    public AudioSource audioSource;

    [Header("Cooldown")] [Range(0, 1)] [SerializeField]
    private float cooldownOpacity;

    public float callCooldown;


    [Header("Associated Scripts")] [SerializeField]
    private EnergyCounter energyCounter;

    [SerializeField] private Motherboard motherboard;
    [SerializeField] private EnemyPathManager pathManager;
    private float callCooldownBase;
    private EnemySpawnExclusionZone exclusionZone;
    private bool isOnCooldown;

    private SpriteRenderer spriteRenderer;

    [Header("Channels")]
    [SerializeField] private EnemyInvasionEventChannel invasionEventChannel;
    [SerializeField] private EnemyDeathEventChannel deathEventChannel;

    // Start is called before the first frame update

    protected virtual void Awake()
    {
        spriteRenderer = enemySpawn.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        statGain = 1;
        // for test exploder enemy. (not needed anymore)
        //FindObjectOfType<Enemy>().Init(pathManager.GetActivePath().GetExtrapolator().GetMinimalRepresentation(), deathEventChannel, invasionEventChannel);
        callCooldownBase = 15;
        CalculateCorrectTransform();
    }

    // Update is called once per frame
    private void Update()
    {
        waveDisplayText.text = "Wave " + wave;

        if(DataSaverLoader.Gd.IsArcadeBuild)
        {
            if ((Input.GetKeyDown(KeyCode.Alpha1)) && isOnCooldown == false)
            {
                audioSource.clip = spawnSound;
                audioSource.Play();
                wave += 1;
                if (wave % 10 != 0)
                {
                    if (wave > 10)
                    {
                        SpawnWave(2);
                    }
                    else SpawnWave(1);
                }
                else
                {
                    //if a boss wave
                    SpawnWave(wave / 10);
                    //spawn boss here
                    SpawnBoss();
                }
                
                isOnCooldown = true;
                SetOpacity(cooldownOpacity);
                StartCoroutine(Cooldown());
            }
        }
        else if(Input.GetKeyDown(DataSaverLoader.Gd.CallNextWave) && isOnCooldown == false)
        {
            audioSource.clip = spawnSound;
            audioSource.Play();
            wave += 1;
            if (wave % 10 != 0)
            {
                //if a normal wave
                if (wave > 10)
                {
                    SpawnWave(2);
                }
                else SpawnWave(1);
            }
            else
            { 
                //if a boss wave
                SpawnWave(wave / 10);
                //spawn boss here
                SpawnBoss();
            }

            isOnCooldown = true;
            SetOpacity(cooldownOpacity);
            StartCoroutine(Cooldown());
        }
    }


    private void SpawnBoss()
    {
        var enemyPathNodes = GetEnemyPathNodes();

        var enemyPrefab = enemyPrefabs[4];
        var enemy = Instantiate(enemyPrefab);

        // determine how much heath that enemy gets.
        enemy.Health += enemy.HealthGain * (wave - 1) * statGain;
        enemy.EnergyDrop += enemy.EnergyGain * (wave - 1);

        // send the enemy the data it wants
        enemy.Init(enemyPathNodes, deathEventChannel, invasionEventChannel);

        statGain += 0.7f;
    }


    public void UpdateExclusionZone()
    {
        if (exclusionZone == null) exclusionZone = GetComponent<EnemySpawnExclusionZone>();

        var firstEnemyPathNode = pathManager.GetActivePath().GetExtrapolator().GetMinimalRepresentation().First();
        Debug.Assert(firstEnemyPathNode.OutgoingDirection != null, "firstEnemyPathNode.OutgoingDirection != null");
        exclusionZone.SetDirection(firstEnemyPathNode.Location, firstEnemyPathNode.OutgoingDirection.Value);
    }

    /// <summary>
    ///     Calculates the correct transform from
    ///     the path, so that way this is always
    ///     correctly positioned at the start at
    ///     the path.
    /// </summary>
    public void CalculateCorrectTransform()
    {
        var firstEnemyPathNode = pathManager.GetActivePath().GetExtrapolator().GetMinimalRepresentation().First();
        gameObject.transform.position = GridSpaceGlobalSpaceConverter.FromLocation(firstEnemyPathNode.Location, 3);
        // make sure that this thing isn't null.
        Debug.Assert(firstEnemyPathNode.OutgoingDirection != null, "firstEnemyPathNode.OutgoingDirection != null");
        gameObject.transform.rotation =
            Quaternion.Euler(CardinalDirectionToEuler(firstEnemyPathNode.OutgoingDirection.Value));
    }

    private Vector3 CardinalDirectionToEuler(CardinalDirection direction)
    {
        return direction switch
        {
            CardinalDirection.North => new Vector3(0, 180, 0),
            CardinalDirection.South => new Vector3(0, 0, 0),
            CardinalDirection.East => new Vector3(0, 270, 0),
            CardinalDirection.West => new Vector3(0, 90, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    public void SetOpacity(float value)
    {
        spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, value);
    }

    /// <summary>
    ///     Spawns in a new wave of enemies.
    ///     Each wave consists of a certain number of sub waves, each
    ///     Sub Wave contains only one enemy.
    /// </summary>
    /// <param name="numberOfSubWaves"> the number of sub waves to spawn</param>
    public void SpawnWave(float numberOfSubWaves)
    {
        // iterate for the number of sub waves.
        for (var subWaveIndex = 0; subWaveIndex < numberOfSubWaves; subWaveIndex++)
        {
            // pick a random enemy prefab
            var enemyPrefab = enemyPrefabs[1];
            int randSelection = Random.Range(0, 11);
            switch (randSelection)
            {
                case 0:
                case 1:
                case 2:
                    enemyPrefab = enemyPrefabs[0];
                    break;
                case 3:
                case 4:
                case 5:
                case 6:
                    enemyPrefab = enemyPrefabs[1];
                    break;
                case 7:
                case 8:
                case 9:
                    enemyPrefab = enemyPrefabs[2];
                    break;
                case 10:
                    if(wave >= 11)
                    {
                        enemyPrefab = enemyPrefabs[3];
                    }
                    else
                    {
                        enemyPrefab = enemyPrefabs[1];
                    }
                    break;
                default:
                    enemyPrefab = enemyPrefabs[1];
                    break;
            }

            // determine how many we want to spawn
            var armySize = (int) (enemyPrefab.ArmySize + enemyPrefab.ArmySizeGain * (wave - 1));

            // determine how fast we want to spawn them.
            var spawnRate = CurveSpawnRate(subWaveIndex, enemyPrefab.SpawnRate);

            // spawn the enemy sub wave.
            StartCoroutine(SpawnSubWave(enemyPrefab, armySize, spawnRate));
        }
    }

    /// <summary>
    ///     Curves the enemy spawn rate depending on how many
    ///     "sub waves" (waves of enemies within a wave) have
    ///     already happened.
    /// </summary>
    /// <param name="enemySubWaveIndex"> how many "sub waves" have already spawned.</param>
    /// <param name="enemyPrefabSpawnRate"> how fast that enemy wants to spawn</param>
    /// <returns>the curved spawn wave value</returns>
    private float CurveSpawnRate(int enemySubWaveIndex, float enemyPrefabSpawnRate)
    {
        return enemySubWaveIndex >= 1 ? enemyPrefabSpawnRate + 0.5f : enemyPrefabSpawnRate;
    }

    private IEnumerator SpawnSubWave(Enemy enemyPrefab, int armySize, float spawnRate)
    {
        // Get a list of path nodes that we can pass through.
        var enemyPathNodes = GetEnemyPathNodes();

        for (var armyIndex = 0; armyIndex < armySize; armyIndex++)
        {
            var enemy = Instantiate(enemyPrefab);

            // determine how much heath that enemy gets.
            enemy.Health += enemy.HealthGain * (wave - 1) * statGain;
            enemy.EnergyDrop += enemy.EnergyGain * (wave - 1);

            // send the enemy the data it wants
            enemy.Init(enemyPathNodes, deathEventChannel, invasionEventChannel);

            // delay so that not all enemies spawn at the same time.
            yield return new WaitForSeconds(spawnRate);
        }
    }

    /// <summary>
    ///     Get the path that the enemies will have to take
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    private List<IEnemyPathNode> GetEnemyPathNodes()
    {
        var locationEnumerator = pathManager.GetActivePath().GetExtrapolator().GetMinimalRepresentation();
        var enemyPathNodes = locationEnumerator?.ToList();

        // make sure that we are able to iterate over that list.
        if (enemyPathNodes == null)
            throw new NullReferenceException(
                "Enemy Path Enumerator is null, when it shouldn't be, check the stacktrace and provide a valid path.");
        return enemyPathNodes;
    }

    private IEnumerator Cooldown()
    {
        ScoreTrackerScript.RushMod = 3;
        ScoreNotifierText.text = "Score X2";
        Color og_color = ScoreNotifierText.color;
        ScoreNotifierText.color = Color.red;
        while (callCooldown > 0)
        {
            yield return new WaitForSeconds(0.5f);
            if(callCooldown%2 == 1)
            {
                ScoreNotifierText.color = og_color;
            }
            else
            {
                ScoreNotifierText.color = Color.red;
            }
            callCooldown -= 1f;
        }

        tutorialDisplayText.SetActive(false);
        isOnCooldown = false;
        callCooldown = callCooldownBase;
        ScoreTrackerScript.RushMod = 1;
        ScoreNotifierText.text = "Score";
        ScoreNotifierText.color = og_color;
        SetOpacity(1.0f);
    }
}