using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Channel;
using GameGrid;
using GameState;
using JetBrains.Annotations;
using UnityEngine;

namespace EnemyBehaviour
{
    public class EnemyPlayState : MonoBehaviour, IGameObjectState, IObserver<GameActivityState>
    {
        public bool isExploding;
        public bool isBoss;

        public GameObject EMPExplosion;
        public GameObject BossEMPExplision;
        private Enemy enemy;
        
        // private EnergyCounter energyCounterScript;
        // private Motherboard motherboardScript;

        private EnemyInvasionEventChannel invasionEventChannel;
        private EnemyDeathEventChannel deathEventChannel;
        
        
        private bool notMoving;

        private IEnumerator<IEnemyPathNode> path;

        private EnemyStateMachine stateMachine;
        private Vector3 targetedPosition;
        private const float yOffset = -0.5f;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            stateMachine = GetComponent<EnemyStateMachine>();
            stateMachine.StateChannel.Subscribe(this);
            enabled = false;
        }

        // Update is called once per frame
        private void Update()
        {
            // if we have no path, wait until we can get one.
            if (path == null) return;
            // transform.position = Vector3.MoveTowards(transform.position, nextNode.transform.position, speed * Time.deltaTime);

            if (IsReachedCurrentNode())
                // if there are no more nodes to go to
                if (!AdvanceToNextNode())
                {
                    invasionEventChannel.Broadcast(new EnemyInvasionEvent(enemy));
                    // destroy the object.
                    // motherboardScript.hp -= enemy.DamageValue;
                    Destroy(gameObject);
                    return;
                }

            transform.position =
                Vector3.MoveTowards(transform.position, targetedPosition, enemy.Speed * Time.deltaTime);

            if (enemy.Health <= 0)
            {
                //chance for spawning a chip
                // energyCounterScript.energy += enemy.EnergyDrop;
                
                if (isExploding)
                {
                    var spawnScript = GameObject.Find("EnemySpawnRoot").GetComponent<SpawnManager>();
                    //this explodes the enemy
                    if (!notMoving)
                    {
                        var spawnExplosionEffectLoco = new Vector3(transform.position.x, transform.position.y + 2,
                            transform.position.z);
                        var spawnExplosionRotation = new Quaternion(180, 0, 0, 180);
                        if(isBoss)
                        {
                            Instantiate(BossEMPExplision, spawnExplosionEffectLoco, spawnExplosionRotation);
                            notMoving = true;
                            StartCoroutine(DisableTimer(7.0f, 6 + (spawnScript.wave / 10)));
                        }
                        else
                        {
                            Instantiate(EMPExplosion, spawnExplosionEffectLoco, spawnExplosionRotation);
                            notMoving = true;
                            StartCoroutine(DisableTimer(2.5f, (spawnScript.wave / 10)));
                        }
                    }
                }
                else
                {
                    deathEventChannel.Broadcast(new EnemyDeathEvent(enemy));
                    Destroy(gameObject);
                }
            }
        }
        
        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void Init([NotNull] IEnumerable<IEnemyPathNode> path, EnemyDeathEventChannel deathEventChannel, EnemyInvasionEventChannel invasionEventChannel)
        {
            this.path = path.GetEnumerator();
            this.deathEventChannel = deathEventChannel;
            this.invasionEventChannel = invasionEventChannel;
            
            JumpToStartOfPath();
        }

        private void JumpToStartOfPath()
        {
            path.MoveNext();
            var startingPathNode = path.Current;
            if (startingPathNode == null)
                throw new NullReferenceException("The passed path has a null starting point. This is forbidden.");
            transform.position = GridSpaceGlobalSpaceConverter.FromLocation(startingPathNode.Location, yOffset);
            AdvanceToNextNode();
        }

        private bool AdvanceToNextNode()
        {
            if (path == null) return true;
            var isMoved = path.MoveNext();
            if (isMoved && path.Current != null)
            {
                targetedPosition = GridSpaceGlobalSpaceConverter.FromLocation(path.Current.Location, yOffset);
                transform.LookAt(targetedPosition);
            }

            return isMoved;
        }

        private bool IsReachedCurrentNode()
        {
            var position = transform.position;
            return Enumerable.Range(0, 3).All(i => Math.Abs(position[i] - targetedPosition[i]) < 1e-6);
        }

        private IEnumerator DisableTimer(float radius, float DisableTimeLength)
        {
            yield return new WaitForSeconds(0.3f);
            deathEventChannel.Broadcast(new EnemyDeathEvent(enemy));
            DisableTurretsInRange(transform.position, radius, DisableTimeLength);
            Destroy(gameObject);
        }

        private void DisableTurretsInRange(Vector3 center, float radius, float DisabletimeLength)
        {
            var hitColliders = Physics.OverlapSphere(center, radius);

            foreach (var hitCollider in hitColliders)
                if (hitCollider.gameObject.TryGetComponent(out TileTurretBehavior behavior))
                {
                    var turret = behavior.GetTurret();
                    if (turret != null) turret.Disable(DisabletimeLength);
                }

            Destroy(gameObject);
        }

        #region GamePlayingState

        public void OnStateStart()
        {
            if (this == null) return;
            enabled = true;
        }

        public void OnStateEnd()
        {
            if (this == null) return;
            enabled = false;
        }

        #endregion

        #region GamePlayingStateChangeListener

        public void OnCompleted()
        {
            // do nothing.
        }

        public void OnError(Exception error)
        {
            // do nothing.
        }

        public void OnNext(GameActivityState value)
        {
            if (value != GameActivityState.Playing) return;
            stateMachine.ActivateState(this);
        }

        #endregion
    }
}