using System;
using System.Collections;
using GameState;
using ProjectileBehaviour;
using UnityEngine;

namespace TurretBehaviour
{
    public class TurretPlayState : MonoBehaviour, IGameObjectState, IObserver<GameActivityState>
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject turretDisplay;
        [SerializeField] private GameObject turretAnimdisplay;
        [SerializeField] private GameObject hackedAnimDisplay;
        [SerializeField] private Transform turretNeck;
        [SerializeField] private float energyCost;
        [SerializeField] private float level;
        [SerializeField] private float maxLevel;
        [SerializeField] private float sellAmount; // current = level * base sellAmount;
        [SerializeField] private float upgradeCost; // current = level * base upgradeCost;
        [SerializeField] private float damagePerShot;
        [SerializeField] private float damageUp; //Damage increase per level
        [SerializeField] private float rateOfFire;
        [SerializeField] private float animStopTime;
        [SerializeField] private float fireCooldownTime;
        [SerializeField] private float range;
        [SerializeField] private bool isTeslaTurret;
        [SerializeField] private bool isLaserTurret;
        [SerializeField] private bool isFiringLaser;
        public bool isDisabled;

        public AudioClip fireSound;
        public AudioSource audioSource;
        public float baseVolume;

        private Quaternion angleToFireLaser;
        private Vector3 placeToFireLaser;

        private float rotationSpeed = 10;
        private TurretStateMachine stateMachine;

        private GameObject target;

        public float Range => range;
        public float EnergyCost => energyCost;
        public float SellAmount => sellAmount;

        protected virtual void Awake()
        {
            stateMachine = GetComponent<TurretStateMachine>();
            stateMachine.StateChannel.Subscribe(this);
            enabled = false;
        }

        // Start is called before the first frame update
        private void Start()
        {
        
            InvokeRepeating("UpdateTarget", 0, 0.25f);
            if (isLaserTurret)
            {
                rotationSpeed = 20;
                InvokeRepeating("FindNewTarget", 0, 0.05f);
                fireCooldownTime = 0.3f;
            }
            else
            {
                fireCooldownTime = 1 / rateOfFire;
            }

            isDisabled = false;
        }

        // Update is called once per frame
        private void Update()
        {
            if (target != null && isDisabled == false && !isFiringLaser)
            {
                var directionToPoint = target.transform.position - transform.position;
                var rotateToFaceTarget = Quaternion.LookRotation(directionToPoint);

                var rotation = Quaternion.Lerp(turretNeck.rotation, rotateToFaceTarget, Time.deltaTime * rotationSpeed)
                    .eulerAngles;
                turretNeck.rotation = Quaternion.Euler(0f, rotation.y, 0f);

                if (fireCooldownTime <= 0)
                {
                    Fire();
                    fireCooldownTime = 1 / rateOfFire;
                }

                if (!isLaserTurret || fireCooldownTime <= 0.3f)
                {
                    fireCooldownTime -= Time.deltaTime;
                }
            }

            if (isLaserTurret && !isFiringLaser && fireCooldownTime > 0.3f)
            {
                fireCooldownTime -= Time.deltaTime;

            }

            ReduceSound(); //reduces sound based on number of other objects of the same type in the scene
        }

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

        public void OnCompleted()
        {
            // just do nothing.
        }

        public void OnError(Exception error)
        {
            // just do nothing.
        }

        public void OnNext(GameActivityState value)
        {
            if (value != GameActivityState.Playing) return;
            stateMachine.ActivateState(this);
        }

        private void Fire()
        {
            //do animation here
            turretDisplay.SetActive(false);
            turretAnimdisplay.SetActive(true);
            //temp comment out below. Once a laser sound is in use that too and don't use if statement
            //audioSource.clip = fireSound;
            //audioSource.Play();
            if (!isLaserTurret)
            {
                audioSource.clip = fireSound;
                audioSource.Play();
            }

            if (!isLaserTurret)
            {
                StartCoroutine(makeProjectile());
            }
            else
            {
                isFiringLaser = true;
                StartCoroutine(makeLaser());
            }
        }

        private IEnumerator makeLaser()
        {

                var projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                projectileGO.GetComponent<ProjectilePlayState>().damage = damagePerShot;

                audioSource.clip = fireSound;
                audioSource.Play();

            //make the laser
            //assign damage value

            yield return new WaitForSeconds(animStopTime / 2);

            StartCoroutine(animStop());
        }


        private IEnumerator animStop()
        {
            yield return new WaitForSeconds(animStopTime / 2);
            if (isLaserTurret)
            {
                //end laser
                //resume movement
                isFiringLaser = false;
                audioSource.Stop();
            }

            turretAnimdisplay.SetActive(false);
            turretDisplay.SetActive(true);
        }


        private IEnumerator makeProjectile()
        {
            yield return new WaitForSeconds(animStopTime / 2);
            var projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            projectileGO.GetComponent<ProjectilePlayState>().ChaseThisEnemy(target, damagePerShot);
            StartCoroutine(animStop());
        }


        private void UpdateTarget()
        {
            //if target is null, continue
            //if target not null, but out of range, but is out of range, continue
            if (target == null)
                FindNewTarget();
            else if (Vector3.Distance(transform.position, target.transform.position) > range || isTeslaTurret)
                FindNewTarget();
        }

        private void FindNewTarget()
        {
            var allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            var shortestDistance = Mathf.Infinity;
            GameObject closestEnemy = null;

            foreach (var enemy in allEnemies)
            {
                var distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
                if (shortestDistance > distanceToEnemy)
                {
                    shortestDistance = distanceToEnemy;
                    closestEnemy = enemy;
                }
            }


            if (closestEnemy != null && shortestDistance <= range)
                target = closestEnemy;
            else
                target = null;
        }

        public void disableThisTurret(float DTime)
        {
            hackedAnimDisplay.SetActive(true);
            isDisabled = true;
            StartCoroutine(enableTurretTimer(DTime));
        }

        private IEnumerator enableTurretTimer(float DTime)
        {
            yield return new WaitForSeconds(DTime);
            hackedAnimDisplay.SetActive(false);
            isDisabled = false;
        }

        private void ReduceSound()
        {
            float totalCount = 0;
            var turrets = GameObject.FindGameObjectsWithTag("Turret");
            for (int i = 0; i < turrets.Length; i++)
            {
                if (turrets[i].name == gameObject.name) //if the turret is the same as this one
                {
                    totalCount += 1;
                }
            }
            audioSource.volume = baseVolume / totalCount; //reduce sound by number of turrets in the scene
        }
    }
}