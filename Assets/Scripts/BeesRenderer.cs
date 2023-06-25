using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public struct BeesRenderer : System.IDisposable
{
    private struct BeeRenderData
    {
        public float team;
        public float3 position;
    };

    private GraphicsBuffer beeRenderBuffer;
    private NativeList<BeeRenderData> beeRenderDatas;

    private readonly RenderParams renderParams;
    private GraphicsBuffer.IndirectDrawIndexedArgs indirectArguments;
    private GraphicsBuffer indirectArugmentsBuffer;

    private Mesh beeMesh;

    public BeesRenderer(Mesh beeMesh, Material beeMaterial, Color[] beeColors, int instanceCapacity)
    {
        this.beeMesh = beeMesh;
        renderParams = new RenderParams(beeMaterial);
        renderParams.worldBounds = new Bounds(float3.zero, new float3(10000));

        indirectArguments = new GraphicsBuffer.IndirectDrawIndexedArgs
        {
            startIndex = 0,
            startInstance = 0,
            baseVertexIndex = 0,
            indexCountPerInstance = beeMesh.GetIndexCount(0),
            instanceCount = 0,
        };
        indirectArugmentsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        
        beeRenderBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.LockBufferForWrite, instanceCapacity, UnsafeUtility.SizeOf<BeeRenderData>());
        beeRenderDatas = new NativeList<BeeRenderData>(instanceCapacity, Allocator.Persistent);

        beeMaterial.SetBuffer("matrixBuffer", beeRenderBuffer);
        beeMaterial.SetColorArray("colors", beeColors);
    }

    public JobHandle CreateDrawCalls(ref BeesChunk beesChunk, int team, JobHandle dependency)
    {
        var GenerateAliveBeeRenderDataJob = new GenerateAliveBeeRenderDataJob
        {
            team = team,
            beesAlive = beesChunk.beeAlive,
            beePositionsX = beesChunk.beePositionsX,
            beePositionsY = beesChunk.beePositionsY,
            beePositionsZ = beesChunk.beePositionsZ,
            beeDatas = beeRenderDatas.AsParallelWriter(),
        };

        var GenerateDeadBeeRenderDataJob = new GenerateDeadBeeRenderDataJob
        {
            deadBees = beesChunk.deadBees.AsDeferredJobArray(),
            beeDatas = beeRenderDatas.AsParallelWriter(),
        };

        dependency = GenerateAliveBeeRenderDataJob.ScheduleBatch(beesChunk.beeCapacity, BeesWorld.BatchSize, dependency);
        dependency = GenerateDeadBeeRenderDataJob.Schedule(beesChunk.deadBees, BeesWorld.BatchSize, dependency);

        return dependency;
    }

    [BurstCompile]
    private struct GenerateAliveBeeRenderDataJob : IJobParallelForBatch
    {
        [ReadOnly] public int team;
        [ReadOnly] public NativeArray<bool> beesAlive;
        [ReadOnly] public NativeArray<float> beePositionsX;
        [ReadOnly] public NativeArray<float> beePositionsY;
        [ReadOnly] public NativeArray<float> beePositionsZ;

        [WriteOnly] public NativeList<BeeRenderData>.ParallelWriter beeDatas;

        public unsafe void Execute(int startIndex, int count)
        {
            var localMatrixes = stackalloc BeeRenderData[BeesWorld.BatchSize];
            var localMatrixCount = 0;

            int endIndex = startIndex + count;
            for (int index = startIndex; index < endIndex; index++, localMatrixCount++)
            {
                localMatrixes[localMatrixCount] = new BeeRenderData
                {
                    team = team,
                    position = new float3(beePositionsX[index], beePositionsY[index], beePositionsZ[index]),
                };
            }

            beeDatas.AddRangeNoResize(localMatrixes, localMatrixCount);
        }
    }

    [BurstCompile]
    private struct GenerateDeadBeeRenderDataJob : IJobParallelForDefer
    {
        [ReadOnly] public NativeArray<BeesChunk.DeadBee> deadBees;

        [WriteOnly] public NativeList<BeeRenderData>.ParallelWriter beeDatas;

        public unsafe void Execute(int index)
        {
            var deadBee = deadBees[index];

            beeDatas.AddNoResize(new BeeRenderData
            {
                team = 3,
                position = deadBee.position,
            });
        }
    }

    public void SubmitDrawCalls()
    {
        var matrixArray = beeRenderBuffer.LockBufferForWrite<BeeRenderData>(0, beeRenderDatas.Length);
        matrixArray.CopyFrom(beeRenderDatas.AsArray());
        beeRenderBuffer.UnlockBufferAfterWrite<BeeRenderData>(beeRenderDatas.Length);

        indirectArguments.instanceCount = (uint)beeRenderDatas.Length;
        var indirectDrawIndexedArgs = new NativeArray<GraphicsBuffer.IndirectDrawIndexedArgs>(1, Allocator.Temp);
        indirectDrawIndexedArgs[0] = indirectArguments;
        indirectArugmentsBuffer.SetData(indirectDrawIndexedArgs);
        indirectDrawIndexedArgs.Dispose();

        Graphics.RenderMeshIndirect(renderParams, beeMesh, indirectArugmentsBuffer);

        beeRenderDatas.Clear();
    }

    public void Dispose()
    {
        if (beeRenderDatas.IsCreated) beeRenderDatas.Dispose();
        indirectArugmentsBuffer?.Dispose();
        beeRenderBuffer?.Dispose();
    }
}
