using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

[BurstCompile]
public struct InitializeBeesJob : IJobParallelForBatch
{
    private const int BaseSeed = 420;
    [ReadOnly] public float3 halfFieldSize;
    [ReadOnly] public uint seed;

    public NativeArray<bool> beeAlive;
    public NativeArray<int> beeTargets;
    public NativeArray<Random> beeRandoms;
    public NativeArray<float> beePositionsX;
    public NativeArray<float> beePositionsY;
    public NativeArray<float> beePositionsZ;
    public NativeArray<float> beeDirectionsX;
    public NativeArray<float> beeDirectionsY;
    public NativeArray<float> beeDirectionsZ;
    public NativeArray<float> beeVelocitiesX;
    public NativeArray<float> beeVelocitiesY;
    public NativeArray<float> beeVelocitiesZ;

    public void Execute(int startIndex, int count)
    {
        Random random = new Random(math.asuint(startIndex + BaseSeed) + seed);

        int endIndex = startIndex + count;
        for (int index = startIndex; index < endIndex; index++)
        {
            beeAlive[index] = true;
            beeTargets[index] = -1;
            beeRandoms[index] = new Random(random.NextUInt(1, uint.MaxValue));

            var randomPosition = beeRandoms[index].NextFloat3(-halfFieldSize, halfFieldSize);
            beePositionsX[index] = randomPosition.x;
            beePositionsY[index] = randomPosition.y;
            beePositionsZ[index] = randomPosition.z;

            var randomDirection = beeRandoms[index].NextFloat3Direction();
            beeDirectionsX[index] = randomDirection.x;
            beeDirectionsY[index] = randomDirection.y;
            beeDirectionsZ[index] = randomDirection.z;

            var randomVelocity = beeRandoms[index].NextFloat3Direction();
            beeVelocitiesX[index] = randomVelocity.x;
            beeVelocitiesY[index] = randomVelocity.y;
            beeVelocitiesZ[index] = randomVelocity.z;
        }
    }
}


[BurstCompile]
public struct KillBeeJob : IJobParallelForDefer
{
    [ReadOnly] public float beeDeathTime;
    [ReadOnly] public NativeArray<int> killBeesCommands;

    [ReadOnly] public NativeArray<float> beePositionsX;
    [ReadOnly] public NativeArray<float> beePositionsY;
    [ReadOnly] public NativeArray<float> beePositionsZ;
    [ReadOnly] public NativeArray<float> beeDirectionsX;
    [ReadOnly] public NativeArray<float> beeDirectionsY;
    [ReadOnly] public NativeArray<float> beeDirectionsZ;
    [ReadOnly] public NativeArray<float> beeVelocitiesX;
    [ReadOnly] public NativeArray<float> beeVelocitiesY;
    [ReadOnly] public NativeArray<float> beeVelocitiesZ;

    [WriteOnly] public NativeList<BeesChunk.DeadBee>.ParallelWriter deadBees;

    public void Execute(int index)
    {
        int beeIndex = killBeesCommands[index];

        deadBees.AddNoResize(new BeesChunk.DeadBee
        {
            position = new float3(beePositionsX[beeIndex], beePositionsY[beeIndex], beePositionsZ[beeIndex]),
            direction = new float3(beeDirectionsX[beeIndex], beeDirectionsY[beeIndex], beeDirectionsZ[beeIndex]),
            velocity = new float3(0, 0, 0),
            aliveTime = beeDeathTime,
        });
    }
}

[BurstCompile]
public struct DeadBeeJob : IJobParallelForDefer
{
    public NativeArray<BeesChunk.DeadBee> deadBees;

    [ReadOnly] public float deltaTime;
    [ReadOnly] public float3 fieldSize;
    [ReadOnly] public float3 fieldGravity;

    public NativeList<int>.ParallelWriter despawnBees;

    public unsafe void Execute(int index)
    {
        //TODO: precalculate
        float3 fieldGravityValue = fieldGravity * deltaTime;
        float3 fieldMin = fieldSize * -0.5f;
        float3 fieldMax = fieldSize * 0.5f;

        ref var deadBee = ref UnsafeUtility.ArrayElementAsRef<BeesChunk.DeadBee>(deadBees.GetUnsafePtr(), index);

        deadBee.velocity += fieldGravityValue;
        deadBee.position += deadBee.velocity;
        deadBee.position = math.clamp(deadBee.position, fieldMin, fieldMax);

        deadBee.aliveTime -= deltaTime;

        if (deadBee.aliveTime < 0.0f)
        {
            despawnBees.AddNoResize(index);
        }
    }
}

// We just respawn any bees we previously killed.
[BurstCompile]
public struct SpawnBeesJob : IJobParallelForDefer
{
    [ReadOnly] public float3 halfFieldSize;
    [ReadOnly] public NativeArray<int> killBeesCommands;

    [NativeDisableParallelForRestriction, NoAlias]
    public NativeArray<bool> beeAlive;
    [NativeDisableParallelForRestriction, NoAlias]
    public NativeArray<int> beeTargets;
    [NativeDisableParallelForRestriction, NoAlias]
    public NativeArray<Random> beeRandoms;
    [NativeDisableParallelForRestriction, NoAlias]
    public NativeArray<float> beePositionsX;
    [NativeDisableParallelForRestriction, NoAlias]
    public NativeArray<float> beePositionsY;
    [NativeDisableParallelForRestriction, NoAlias]
    public NativeArray<float> beePositionsZ;
    [NativeDisableParallelForRestriction, NoAlias]
    public NativeArray<float> beeDirectionsX;
    [NativeDisableParallelForRestriction, NoAlias]
    public NativeArray<float> beeDirectionsY;
    [NativeDisableParallelForRestriction, NoAlias]
    public NativeArray<float> beeDirectionsZ;
    [NativeDisableParallelForRestriction, NoAlias]
    public NativeArray<float> beeVelocitiesX;
    [NativeDisableParallelForRestriction, NoAlias]
    public NativeArray<float> beeVelocitiesY;
    [NativeDisableParallelForRestriction, NoAlias]
    public NativeArray<float> beeVelocitiesZ;

    public void Execute(int index)
    {
        var beeIndex = killBeesCommands[index];

        var beeRandom = beeRandoms[beeIndex];

        beeAlive[beeIndex] = true;
        beeTargets[beeIndex] = -1;

        var randomPosition = beeRandom.NextFloat3(-halfFieldSize, halfFieldSize);
        beePositionsX[beeIndex] = randomPosition.x;
        beePositionsY[beeIndex] = randomPosition.y;
        beePositionsZ[beeIndex] = randomPosition.z;

        var randomDirection = beeRandom.NextFloat3Direction();
        beeDirectionsX[beeIndex] = randomDirection.x;
        beeDirectionsY[beeIndex] = randomDirection.y;
        beeDirectionsZ[beeIndex] = randomDirection.z;

        var randomVelocity = beeRandom.NextFloat3Direction();
        beeVelocitiesX[beeIndex] = randomVelocity.x;
        beeVelocitiesY[beeIndex] = randomVelocity.y;
        beeVelocitiesZ[beeIndex] = randomVelocity.z;

        beeRandoms[beeIndex] = beeRandom;
    }
}

[BurstCompile]
public struct DespawnDeadBeesAndClearCommmandsJob : IJob
{
    public NativeList<BeesChunk.DeadBee> deadBees;
    public NativeList<int> despawnBees;

    // We pass this just to clear it. Its better to avoid scheduling another job just to clear this.
    public NativeList<int> killBeesCommands;

    public void Execute()
    {
        //NOTE: It is important to sort and loop backwards because we are using RemoveAtSwapBack which changes the indicies of elements.
        despawnBees.Sort();

        for (int i = despawnBees.Length - 1; i >= 0; i--)
        {
            deadBees.RemoveAtSwapBack(despawnBees[i]);
        }

        despawnBees.Clear();
        killBeesCommands.Clear();
    }
}

// Equivalent to EnemyTargetSystem.cs and AttackSystem.cs
[BurstCompile]
public struct BeeTargetAttackJob : IJobParallelForBatch
{
    [ReadOnly] public float attackDistance;
    [ReadOnly] public float hitDistance;
    [ReadOnly] public float chaseForce;
    [ReadOnly] public float attackForce;
    [ReadOnly] public float deltaTime;

    [ReadOnly] public NativeArray<float> beePositionsX;
    [ReadOnly] public NativeArray<float> beePositionsY;
    [ReadOnly] public NativeArray<float> beePositionsZ;

    [ReadOnly] public int enemyBeeCount;
    [NativeDisableParallelForRestriction, NoAlias]
    [ReadOnly] public NativeArray<float> targetPositionsX;
    [NativeDisableParallelForRestriction, NoAlias]
    [ReadOnly] public NativeArray<float> targetPositionsY;
    [NativeDisableParallelForRestriction, NoAlias]
    [ReadOnly] public NativeArray<float> targetPositionsZ;

    [ReadOnly] public NativeArray<bool> beesAlive;
    public NativeArray<int> beeTargets;
    public NativeArray<Random> beeRandoms;
    public NativeArray<float> beeVelocitiesX;
    public NativeArray<float> beeVelocitiesY;
    public NativeArray<float> beeVelocitiesZ;

    [WriteOnly] public NativeList<int>.ParallelWriter killBeeCommands;

    public void Execute(int startIndex, int count)
    {
        int endIndex = startIndex + count;

        float attackForceValue = attackForce * deltaTime;
        float chaseForceValue = chaseForce * deltaTime;

        for (int index = startIndex; index < endIndex; index++)
        {
            var beeAlive = beesAlive[index];
            if (!beeAlive) continue;

            var beeTarget = beeTargets[index];
            if(beeTarget == -1)
            {
                var beeRandom = beeRandoms[index];
                beeTarget = beeRandom.NextInt(0, enemyBeeCount);
                beeRandoms[index] = beeRandom;
            }

            float3 targetPosition = new float3(targetPositionsX[beeTarget], targetPositionsY[beeTarget], targetPositionsZ[beeTarget]);
            float3 beePosition = new float3(beePositionsX[index], beePositionsY[index], beePositionsZ[index]);

            float3 beeDelta = targetPosition - beePosition;
            float distance = math.length(beeDelta.xyz);

            float3 beeVelocitity = new float3(beeVelocitiesX[index], beeVelocitiesY[index], beeVelocitiesZ[index]);
            beeVelocitity += beeDelta * math.select(chaseForceValue, attackForceValue, distance < attackDistance) / distance;

            beeVelocitiesX[index] = beeVelocitity.x;
            beeVelocitiesY[index] = beeVelocitity.y;
            beeVelocitiesZ[index] = beeVelocitity.z;

            if (distance < hitDistance)
            {
                killBeeCommands.AddNoResize(beeTarget);
                beeTarget = -1;
            }

            beeTargets[index] = beeTarget;
        }
    }
}

// Equivalent to BeeMovementSystem.cs
[BurstCompile]
public struct BeeBoidsJob : IJobParallelForBatch
{
    [ReadOnly] public float deltaTime;
    [ReadOnly] public float flightJitter;
    [ReadOnly] public float damping;
    [ReadOnly] public float teamAttraction;
    [ReadOnly] public float teamRepulsion;

    [ReadOnly] public int beeCapacity;                   //For sampling ally positions
    [ReadOnly] public NativeArray<float> beePositionsX;  //For sampling ally positions
    [ReadOnly] public NativeArray<float> beePositionsY;  //For sampling ally positions
    [ReadOnly] public NativeArray<float> beePositionsZ;  //For sampling ally positions

    public NativeArray<Random> beeRandoms;

    public NativeArray<float> beeVelocitiesX;
    public NativeArray<float> beeVelocitiesY;
    public NativeArray<float> beeVelocitiesZ;

    public NativeArray<float> beeDirectionsX;
    public NativeArray<float> beeDirectionsY;
    public NativeArray<float> beeDirectionsZ;

    public void Execute(int startIndex, int count)
    {
        int endIndex = startIndex + count;

        float flightJitterValue = flightJitter * deltaTime;
        float dampingValue = 1f - damping * deltaTime;
        float attractValue = teamAttraction * deltaTime;
        float repelValue = teamRepulsion * deltaTime;

        for (int index = startIndex; index < endIndex; index++)
        {
            var random = beeRandoms[index];
            int allyIndexAttract = random.NextInt(0, beeCapacity);
            int allyIndexRepel = random.NextInt(0, beeCapacity);
            float randomJitterVectorX = (random.NextFloat() * 2.0f - 1.0f) * flightJitterValue;
            float randomJitterVectorY = (random.NextFloat() * 2.0f - 1.0f) * flightJitterValue;
            float randomJitterVectorZ = (random.NextFloat() * 2.0f - 1.0f) * flightJitterValue;
            beeRandoms[index] = random;

            // Random velocity and damping.
            float beeVelocityX = beeVelocitiesX[index];
            float beeVelocityY = beeVelocitiesY[index];
            float beeVelocityZ = beeVelocitiesZ[index];

            beeVelocityX += randomJitterVectorX;
            beeVelocityY += randomJitterVectorY;
            beeVelocityZ += randomJitterVectorZ;

            beeVelocityX *= dampingValue;
            beeVelocityY *= dampingValue;
            beeVelocityZ *= dampingValue;

            // Move towards and away from random allies.
            var beePositionX = beePositionsX[index];
            var beePositionY = beePositionsY[index];
            var beePositionZ = beePositionsZ[index];

            var allyPositionAttractX = beePositionsX[allyIndexAttract];
            var allyPositionAttractY = beePositionsY[allyIndexAttract];
            var allyPositionAttractZ = beePositionsZ[allyIndexAttract];

            var allyPositionRepelX = beePositionsX[allyIndexRepel];
            var allyPositionRepelY = beePositionsY[allyIndexRepel];
            var allyPositionRepelZ = beePositionsZ[allyIndexRepel];

            // Repulsion
            var beeRepelDeltaX = allyPositionRepelX - beePositionX;
            var beeRepelDeltaY = allyPositionRepelY - beePositionY;
            var beeRepelDeltaZ = allyPositionRepelZ - beePositionZ;

            // Equivalent to: velocity -= delta * (Data.teamRepulsion * deltaTime / dist);
            float repelDistance = math.max(0.1f, math.sqrt(beeRepelDeltaX * beeRepelDeltaX + beeRepelDeltaY * beeRepelDeltaY + beeRepelDeltaZ * beeRepelDeltaZ));
            float repelMultiplier = math.rcp(repelDistance) * repelValue;

            beeVelocityX -= beeRepelDeltaX * repelMultiplier;
            beeVelocityY -= beeRepelDeltaY * repelMultiplier;
            beeVelocityZ -= beeRepelDeltaZ * repelMultiplier;

            // Attraction
            var beeAttractDeltaX = allyPositionAttractX - beePositionX;
            var beeAttractDeltaY = allyPositionAttractY - beePositionY;
            var beeAttractDeltaZ = allyPositionAttractZ - beePositionZ;

            // Equivalent to: velocity += delta * (Data.teamAttraction * deltaTime / dist);
            float attractDistance = math.max(0.1f, math.sqrt(beeAttractDeltaX * beeAttractDeltaX + beeAttractDeltaY * beeAttractDeltaY + beeAttractDeltaZ * beeAttractDeltaZ));
            float attractMultiplier = math.rcp(attractDistance) * attractValue;

            beeVelocityX += beeAttractDeltaX * attractMultiplier;
            beeVelocityY += beeAttractDeltaY * attractMultiplier;
            beeVelocityZ += beeAttractDeltaZ * attractMultiplier;

            beeVelocitiesX[index] = beeVelocityX;
            beeVelocitiesY[index] = beeVelocityY;
            beeVelocitiesZ[index] = beeVelocityZ;

            // Update directions
            var beeDirectionX = beeDirectionsX[index];
            var beeDirectionY = beeDirectionsY[index];
            var beeDirectionZ = beeDirectionsZ[index];

            var velocityInverseLength = math.rsqrt(beeVelocityX * beeVelocityX + beeVelocityY * beeVelocityY + beeVelocityZ * beeVelocityZ);
            beeDirectionX = math.lerp(beeDirectionX, beeVelocityX * velocityInverseLength, deltaTime * 4.0f);
            beeDirectionY = math.lerp(beeDirectionY, beeVelocityY * velocityInverseLength, deltaTime * 4.0f);
            beeDirectionZ = math.lerp(beeDirectionZ, beeVelocityZ * velocityInverseLength, deltaTime * 4.0f);

            beeDirectionsX[index] = beeDirectionX;
            beeDirectionsY[index] = beeDirectionY;
            beeDirectionsZ[index] = beeDirectionZ;
        }
    }
}

// Equivalent to BeePositionUpdateSystem.cs
[BurstCompile]
public struct BeeMovementSystem : IJobParallelForBatch
{
    [ReadOnly] public float deltaTime;

    [ReadOnly] public NativeArray<float> beeVelocitiesX;
    [ReadOnly] public NativeArray<float> beeVelocitiesY;
    [ReadOnly] public NativeArray<float> beeVelocitiesZ;

    public NativeArray<float> beePositionsX;
    public NativeArray<float> beePositionsY;
    public NativeArray<float> beePositionsZ;

    public void Execute(int startIndex, int count)
    {
        int endIndex = startIndex + count;
        for (int index = startIndex; index < endIndex; index++)
        {
            beePositionsX[index] += beeVelocitiesX[index] * deltaTime;
            beePositionsY[index] += beeVelocitiesY[index] * deltaTime;
            beePositionsZ[index] += beeVelocitiesZ[index] * deltaTime;
        }
    }
}

// Equivalent to BeeWallCollisionSystem.cs
[BurstCompile]
public struct BeeWallCollisionJob : IJobParallelForBatch
{
    [ReadOnly] public float3 fieldSize;

    public NativeArray<float> beePositionsX;
    public NativeArray<float> beePositionsY;
    public NativeArray<float> beePositionsZ;

    public NativeArray<float> beeVelocitiesX;
    public NativeArray<float> beeVelocitiesY;
    public NativeArray<float> beeVelocitiesZ;

    public void Execute(int startIndex, int count)
    {
        float fieldHalfSizeX = fieldSize.x * 0.5f;
        float fieldHalfSizeY = fieldSize.y * 0.5f;
        float fieldHalfSizeZ = fieldSize.z * 0.5f;

        int endIndex = startIndex + count;
        for (int index = startIndex; index < endIndex; index++)
        {
            float beeVelocityX = beeVelocitiesX[index];
            float beeVelocityY = beeVelocitiesY[index];
            float beeVelocityZ = beeVelocitiesZ[index];

            float beePositionX = beePositionsX[index];
            bool outsideX = math.abs(beePositionX) > fieldHalfSizeX;
            beePositionX = math.select(beePositionX, fieldHalfSizeX * math.sign(beePositionX), outsideX);
            beeVelocityX = math.select(beeVelocityX, beeVelocityX * -0.5f, outsideX);
            beeVelocityY = math.select(beeVelocityY, beeVelocityY * 0.8f, outsideX);
            beeVelocityZ = math.select(beeVelocityZ, beeVelocityZ * 0.8f, outsideX);
            beePositionsX[index] = beePositionX;

            float beePositionY = beePositionsY[index];
            bool outsideY = math.abs(beePositionY) > fieldHalfSizeY;
            beePositionY = math.select(beePositionY, fieldHalfSizeY * math.sign(beePositionY), outsideY);
            beeVelocityX = math.select(beeVelocityX, beeVelocityX * 0.8f, outsideY);
            beeVelocityY = math.select(beeVelocityY, beeVelocityY * -0.5f, outsideY);
            beeVelocityZ = math.select(beeVelocityZ, beeVelocityZ * 0.8f, outsideY);
            beePositionsY[index] = beePositionY;

            float beePositionZ = beePositionsZ[index];
            bool outsideZ = math.abs(beePositionZ) > fieldHalfSizeZ;
            beePositionZ = math.select(beePositionZ, fieldHalfSizeZ * math.sign(beePositionZ), outsideZ);
            beeVelocityX = math.select(beeVelocityX, beeVelocityX * 0.8f, outsideZ);
            beeVelocityY = math.select(beeVelocityY, beeVelocityY * 0.8f, outsideZ);
            beeVelocityZ = math.select(beeVelocityZ, beeVelocityZ * -0.5f, outsideZ);
            beePositionsZ[index] = beePositionZ;

            beeVelocitiesX[index] = beeVelocityX;
            beeVelocitiesY[index] = beeVelocityY;
            beeVelocitiesZ[index] = beeVelocityZ;
        }
    }
}

