using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

public struct BeesChunk
{
    public int beeCapacity;

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

    public NativeList<int> killBeesCommands; //Used for spawning and killing bees. Any bee that is killed is then respawned.

    public struct DeadBee
    {
        public float3 position;
        public float3 velocity;
        public float3 direction;
        public float aliveTime;
    }

    public NativeList<DeadBee> deadBees;
    public NativeList<int> despawnDeadBeesCommands;

    public BeesChunk(int beeCapacity, Allocator allocator)
    {
        this.beeCapacity = beeCapacity;

        beeAlive = new NativeArray<bool>(beeCapacity, allocator, NativeArrayOptions.UninitializedMemory);
        beeTargets = new NativeArray<int>(beeCapacity, allocator, NativeArrayOptions.UninitializedMemory);
        beeRandoms = new NativeArray<Random>(beeCapacity, allocator, NativeArrayOptions.UninitializedMemory);

        beePositionsX = new NativeArray<float>(beeCapacity, allocator, NativeArrayOptions.UninitializedMemory);
        beePositionsY = new NativeArray<float>(beeCapacity, allocator, NativeArrayOptions.UninitializedMemory);
        beePositionsZ = new NativeArray<float>(beeCapacity, allocator, NativeArrayOptions.UninitializedMemory);

        beeDirectionsX = new NativeArray<float>(beeCapacity, allocator, NativeArrayOptions.UninitializedMemory);
        beeDirectionsY = new NativeArray<float>(beeCapacity, allocator, NativeArrayOptions.UninitializedMemory);
        beeDirectionsZ = new NativeArray<float>(beeCapacity, allocator, NativeArrayOptions.UninitializedMemory);

        beeVelocitiesX = new NativeArray<float>(beeCapacity, allocator, NativeArrayOptions.UninitializedMemory);
        beeVelocitiesY = new NativeArray<float>(beeCapacity, allocator, NativeArrayOptions.UninitializedMemory);
        beeVelocitiesZ = new NativeArray<float>(beeCapacity, allocator, NativeArrayOptions.UninitializedMemory);

        killBeesCommands = new NativeList<int>(beeCapacity, allocator);
        deadBees = new NativeList<DeadBee>(beeCapacity, allocator);
        despawnDeadBeesCommands = new NativeList<int>(beeCapacity, allocator);
    }

    public void Dispose()
    {
        if (beeAlive.IsCreated) beeAlive.Dispose();
        if (beeTargets.IsCreated) beeTargets.Dispose();
        if (beeRandoms.IsCreated) beeRandoms.Dispose();

        if (beePositionsX.IsCreated) beePositionsX.Dispose();
        if (beePositionsY.IsCreated) beePositionsY.Dispose();
        if (beePositionsZ.IsCreated) beePositionsZ.Dispose();

        if (beeDirectionsX.IsCreated) beeDirectionsX.Dispose();
        if (beeDirectionsY.IsCreated) beeDirectionsY.Dispose();
        if (beeDirectionsZ.IsCreated) beeDirectionsZ.Dispose();

        if (beeVelocitiesX.IsCreated) beeVelocitiesX.Dispose();
        if (beeVelocitiesY.IsCreated) beeVelocitiesY.Dispose();
        if (beeVelocitiesZ.IsCreated) beeVelocitiesZ.Dispose();

        if (killBeesCommands.IsCreated) killBeesCommands.Dispose();

        if (deadBees.IsCreated) deadBees.Dispose();
        if (despawnDeadBeesCommands.IsCreated) despawnDeadBeesCommands.Dispose();
    }
}