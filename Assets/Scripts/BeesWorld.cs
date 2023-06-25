
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class BeesWorld : MonoBehaviour
{
    public const int BatchSize = 1024;

    public int beesPerTeam = 500000;

    [SerializeField]
    private Mesh beeMesh;
    [SerializeField]
    private Material beeMaterial;
    [SerializeField]
    private Color[] beeColors = new Color[3] { Color.blue, Color.yellow, Color.grey };

    [SerializeField]
    private float3 fieldSize = new float3(300);
    [SerializeField]
    private float3 fieldGravity = new float3(0.0f, -9.8f, 0.0f);

    [Header("Boids")]
    [SerializeField] private float flightJitter = 200;
    [SerializeField] private float damping = 0.9f;
    [SerializeField] private float teamAttraction = 5;
    [SerializeField] private float teamRepulsion = 4;
    [SerializeField] private float chaseForce = 50;
    [SerializeField] private float attackForce = 500;

    [Header("Misc")]
    [SerializeField] public float attackDistance = 4f;
    [SerializeField] public float hitDistance = 0.5f;
    [SerializeField] public float beeDeathTime = 30.0f;

    private BeesChunk beesChunk0;
    private BeesChunk beesChunk1;
    private JobHandle beesHandle = new JobHandle();
    private BeesRenderer beesRenderer;

    private void Start()
    {
        beesChunk0 = new BeesChunk(beesPerTeam, Allocator.Persistent);
        beesChunk1 = new BeesChunk(beesPerTeam, Allocator.Persistent);

        beesRenderer = new BeesRenderer(beeMesh, beeMaterial, beeColors, beesPerTeam * 2 + (int)(beesPerTeam * 0.25f));

        var InitializeBeesJob0 = new InitializeBeesJob
        {
            seed = (uint)UnityEngine.Random.Range(5000, 100000),
            halfFieldSize = fieldSize * 0.5f,
            beeAlive = beesChunk0.beeAlive,
            beeTargets = beesChunk0.beeTargets,
            beeRandoms = beesChunk0.beeRandoms,
            beePositionsX = beesChunk0.beePositionsX,
            beePositionsY = beesChunk0.beePositionsY,
            beePositionsZ = beesChunk0.beePositionsZ,
            beeDirectionsX = beesChunk0.beeDirectionsX,
            beeDirectionsY = beesChunk0.beeDirectionsY,
            beeDirectionsZ = beesChunk0.beeDirectionsZ,
            beeVelocitiesX = beesChunk0.beeVelocitiesX,
            beeVelocitiesY = beesChunk0.beeVelocitiesY,
            beeVelocitiesZ = beesChunk0.beeVelocitiesZ,
        };

        var InitializeBeesJob1 = new InitializeBeesJob
        {
            seed = (uint)UnityEngine.Random.Range(5000, 100000),
            halfFieldSize = fieldSize * 0.5f,
            beeAlive = beesChunk1.beeAlive,
            beeTargets = beesChunk1.beeTargets,
            beeRandoms = beesChunk1.beeRandoms,
            beePositionsX = beesChunk1.beePositionsX,
            beePositionsY = beesChunk1.beePositionsY,
            beePositionsZ = beesChunk1.beePositionsZ,
            beeDirectionsX = beesChunk1.beeDirectionsX,
            beeDirectionsY = beesChunk1.beeDirectionsY,
            beeDirectionsZ = beesChunk1.beeDirectionsZ,
            beeVelocitiesX = beesChunk1.beeVelocitiesX,
            beeVelocitiesY = beesChunk1.beeVelocitiesY,
            beeVelocitiesZ = beesChunk1.beeVelocitiesZ,
        };

        JobHandle.CombineDependencies(InitializeBeesJob0.ScheduleBatch(beesChunk0.beeCapacity, 128), InitializeBeesJob1.ScheduleBatch(beesChunk1.beeCapacity, 128)).Complete();
    }

    private void Update()
    {
        beesHandle = UpdateBeeChunk(ref beesChunk0, ref beesChunk1, beesHandle);
        beesHandle = UpdateBeeChunk(ref beesChunk1, ref beesChunk0, beesHandle);

        beesHandle = beesRenderer.CreateDrawCalls(ref beesChunk0, 0, beesHandle);
        beesHandle = beesRenderer.CreateDrawCalls(ref beesChunk1, 1, beesHandle);

        beesHandle.Complete();

        beesRenderer.SubmitDrawCalls();
    }

    private JobHandle UpdateBeeChunk(ref BeesChunk beesChunk, ref BeesChunk targetChunk, JobHandle dependency = new JobHandle())
    {
        // Kill any bees which were added to the kill commands.
        var KillBeeJob = new KillBeeJob
        {
            beeDeathTime = beeDeathTime,
            killBeesCommands = beesChunk.killBeesCommands.AsDeferredJobArray(),
            beePositionsX = beesChunk.beePositionsX,
            beePositionsY = beesChunk.beePositionsY,
            beePositionsZ = beesChunk.beePositionsZ,
            beeDirectionsX = beesChunk.beeDirectionsX,
            beeDirectionsY = beesChunk.beeDirectionsY,
            beeDirectionsZ = beesChunk.beeDirectionsZ,
            beeVelocitiesX = beesChunk.beeVelocitiesX,
            beeVelocitiesY = beesChunk.beeVelocitiesY,
            beeVelocitiesZ = beesChunk.beeVelocitiesZ,
            deadBees = beesChunk.deadBees.AsParallelWriter(),
        };

        // Update dead bees and create commands to despawn any.
        var DeadBeeJob = new DeadBeeJob
        {
            deltaTime = Time.deltaTime,
            fieldSize = fieldSize,
            fieldGravity = fieldGravity,
            deadBees = beesChunk.deadBees.AsDeferredJobArray(),
            despawnBees = beesChunk.despawnDeadBeesCommands.AsParallelWriter(),
        };

        // Spawn new bees to fill killed bees.
        var SpawnBeesJob = new SpawnBeesJob
        {
            killBeesCommands = beesChunk.killBeesCommands.AsDeferredJobArray(),
            halfFieldSize = fieldSize * 0.5f,
            beeAlive = beesChunk.beeAlive,
            beeTargets = beesChunk.beeTargets,
            beeRandoms = beesChunk.beeRandoms,
            beePositionsX = beesChunk.beePositionsX,
            beePositionsY = beesChunk.beePositionsY,
            beePositionsZ = beesChunk.beePositionsZ,
            beeDirectionsX = beesChunk.beeDirectionsX,
            beeDirectionsY = beesChunk.beeDirectionsY,
            beeDirectionsZ = beesChunk.beeDirectionsZ,
            beeVelocitiesX = beesChunk.beeVelocitiesX,
            beeVelocitiesY = beesChunk.beeVelocitiesY,
            beeVelocitiesZ = beesChunk.beeVelocitiesZ,
        };

        // Despawn our dead bees on the floor and clear any other commands. 
        var DespawnDeadBeesAndClearCommmandsJob = new DespawnDeadBeesAndClearCommmandsJob
        {
            deadBees = beesChunk.deadBees,
            despawnBees = beesChunk.despawnDeadBeesCommands,
            killBeesCommands = beesChunk.killBeesCommands,
        };

        // Now we run multiple jobs which update the bees.
        var BeeTargetAttackJob = new BeeTargetAttackJob
        {
            deltaTime = Time.deltaTime,
            attackDistance = attackDistance,
            attackForce = attackForce,
            chaseForce = chaseForce,
            hitDistance = hitDistance,
            beesAlive = beesChunk.beeAlive,
            beeTargets = beesChunk.beeTargets,
            beePositionsX = beesChunk.beePositionsX,
            beePositionsY = beesChunk.beePositionsY,
            beePositionsZ = beesChunk.beePositionsZ,
            targetPositionsX = targetChunk.beePositionsX,
            targetPositionsY = targetChunk.beePositionsY,
            targetPositionsZ = targetChunk.beePositionsZ,
            beeVelocitiesX = beesChunk.beeVelocitiesX,
            beeVelocitiesY = beesChunk.beeVelocitiesY,
            beeVelocitiesZ = beesChunk.beeVelocitiesZ,
            killBeeCommands = targetChunk.killBeesCommands.AsParallelWriter(),
            beeRandoms = beesChunk.beeRandoms,
            enemyBeeCount = targetChunk.beeCapacity,
        };

        var BeeBoidsJob = new BeeBoidsJob
        {
            deltaTime = Time.deltaTime,
            beeCapacity = beesChunk.beeCapacity,
            damping = damping,
            flightJitter = flightJitter,
            teamAttraction = teamAttraction,
            teamRepulsion = teamRepulsion,
            beeRandoms = beesChunk.beeRandoms,
            beePositionsX = beesChunk.beePositionsX,
            beePositionsY = beesChunk.beePositionsY,
            beePositionsZ = beesChunk.beePositionsZ,
            beeDirectionsX = beesChunk.beeDirectionsX,
            beeDirectionsY = beesChunk.beeDirectionsY,
            beeDirectionsZ = beesChunk.beeDirectionsZ,
            beeVelocitiesX = beesChunk.beeVelocitiesX,
            beeVelocitiesY = beesChunk.beeVelocitiesY,
            beeVelocitiesZ = beesChunk.beeVelocitiesZ,
        };

        var BeeMovementSystem = new BeeMovementSystem
        {
            deltaTime = Time.deltaTime,
            beeVelocitiesX = beesChunk.beeVelocitiesX,
            beeVelocitiesY = beesChunk.beeVelocitiesY,
            beeVelocitiesZ = beesChunk.beeVelocitiesZ,
            beePositionsX = beesChunk.beePositionsX,
            beePositionsY = beesChunk.beePositionsY,
            beePositionsZ = beesChunk.beePositionsZ,
        };

        var BeeWallCollisionJob = new BeeWallCollisionJob
        {
            fieldSize = fieldSize,
            beePositionsX = beesChunk.beePositionsX,
            beePositionsY = beesChunk.beePositionsY,
            beePositionsZ = beesChunk.beePositionsZ,
            beeVelocitiesX = beesChunk.beeVelocitiesX,
            beeVelocitiesY = beesChunk.beeVelocitiesY,
            beeVelocitiesZ = beesChunk.beeVelocitiesZ,
        };

        dependency = KillBeeJob.Schedule(beesChunk.killBeesCommands, 16, dependency);
        dependency = DeadBeeJob.Schedule(beesChunk.deadBees, 16, dependency);
        dependency = SpawnBeesJob.Schedule(beesChunk.killBeesCommands, 16, dependency);
        dependency = DespawnDeadBeesAndClearCommmandsJob.Schedule(dependency);
        dependency = BeeTargetAttackJob.ScheduleBatch(beesChunk.beeCapacity, BatchSize, dependency);
        dependency = BeeBoidsJob.ScheduleBatch(beesChunk.beeCapacity, BatchSize, dependency);
        dependency = BeeMovementSystem.ScheduleBatch(beesChunk.beeCapacity, BatchSize, dependency);
        dependency = BeeWallCollisionJob.ScheduleBatch(beesChunk.beeCapacity, BatchSize, dependency);
        return dependency;
    }

    private void OnDestroy()
    {
        beesHandle.Complete();
        beesChunk0.Dispose();
        beesChunk1.Dispose();
    }
}
