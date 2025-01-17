using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Burst;
using Rukhanka;
using Unity.Transforms;

namespace Unity.Template.CompetitiveActionMultiplayer
{
    public struct SwapSkinnedMesh : IComponentData
    {
        public E_CharacterEditorCategory Category;
        public int Index;
    }

    public struct BodyPartMeshKey : IEquatable<BodyPartMeshKey>
    {
        public E_CharacterEditorCategory Category;
        public int Index;

        public bool Equals(BodyPartMeshKey other)
        {
            return Category == other.Category && Index == other.Index;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)Category;
                hash += Index;
                return hash;
            }
        }
    }

    public struct BodyPartData
    {
        public E_CharacterEditorCategory Category;
        public int Index;
    }

    // Server to test for server owned stuff ?
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct CharacterEditorSystem : ISystem
    {
        private ComponentLookup<AnimatedSkinnedMeshComponent> m_AnimatedSkinnedMeshLookUp;
        private ComponentLookup<CharacterBodyParts> m_CharacterBodyPartsLookup;

        private BufferLookup<SkinMatrix> m_SkinMatixLookup;
        private BufferLookup<LinkedEntityGroup> m_LinkedEntityGroupLookup;

        private NativeHashMap<BodyPartMeshKey, Entity> m_BodyPartsPrefabs;

        private EntityQuery m_SwapEventQuery;
        private EntityQuery m_RenderMeshQuery;

        void OnCreate(ref SystemState state)
        {
            m_BodyPartsPrefabs = new NativeHashMap<BodyPartMeshKey, Entity>(100, Allocator.Persistent);
            m_AnimatedSkinnedMeshLookUp = state.GetComponentLookup<AnimatedSkinnedMeshComponent>();
            m_CharacterBodyPartsLookup = state.GetComponentLookup<CharacterBodyParts>();

            m_SkinMatixLookup = state.GetBufferLookup<SkinMatrix>();
            m_LinkedEntityGroupLookup = state.GetBufferLookup<LinkedEntityGroup>();

            m_SwapEventQuery = state.GetEntityQuery(typeof(SwapSkinnedMesh));
            m_RenderMeshQuery = state.GetEntityQuery(typeof(AnimatedRendererComponent));
        }

        void OnDestroy(ref SystemState state)
        {
        }


        unsafe void OnUpdate(ref SystemState state)
        {
            m_AnimatedSkinnedMeshLookUp.Update(ref state);
            m_CharacterBodyPartsLookup.Update(ref state);
            m_SkinMatixLookup.Update(ref state);
            m_LinkedEntityGroupLookup.Update(ref state);

            var ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>()
                .ValueRW
                .CreateCommandBuffer(state.WorldUnmanaged);

            var destroyedSkinnedEntity = new NativeArray<Entity>(1, Allocator.TempJob);
            UpdateBodyPartsDatabaseJob updateBodyPartsDatabaseJob = new UpdateBodyPartsDatabaseJob
            {
                ECB = ecb,
                BodyPartsPrefabs = m_BodyPartsPrefabs,
            };
            state.Dependency = updateBodyPartsDatabaseJob.Schedule(state.Dependency);



            var rendererEntities = m_RenderMeshQuery.ToEntityArray(Allocator.Temp);

            var entities = m_SwapEventQuery.ToEntityArray(Allocator.Temp);
            var swapEvents = m_SwapEventQuery.ToComponentDataArray<SwapSkinnedMesh>(Allocator.Temp);
            for (int i = 0; i < entities.Length; ++i)
            {
                var characterBodyParts = state.EntityManager.GetComponentData<CharacterBodyParts>(entities[i]);

                var skinnedMeshEntity = m_CharacterBodyPartsLookup[entities[i]].Top; //Only top for the test
                if (skinnedMeshEntity == Entity.Null) continue;
                var data = state.EntityManager.GetComponentData<AnimatedSkinnedMeshComponent>(skinnedMeshEntity);
                state.EntityManager.DestroyEntity(skinnedMeshEntity);


                Entity renderParent = Entity.Null;
                //Get render parent to affect it later
                // And delete old ones
                for (int j = 0; j < rendererEntities.Length; ++j)
                {
                    //Only one parent for the test
                    var renderMeshComponent = state.EntityManager.GetComponentData<AnimatedRendererComponent>(rendererEntities[j]);
                    if (renderMeshComponent.skinnedMeshEntity == skinnedMeshEntity)
                    {
                        renderParent = state.EntityManager.GetComponentData<Parent>(rendererEntities[j]).Value;
                        state.EntityManager.DestroyEntity(rendererEntities[j]);
                    }
                }


                var key = new BodyPartMeshKey { Category = swapEvents[i].Category, Index = swapEvents[i].Index };
                var prefab = m_BodyPartsPrefabs[key];

                var newEntity = state.EntityManager.Instantiate(prefab);
                var newData = state.EntityManager.GetComponentData<AnimatedSkinnedMeshComponent>(newEntity);
                newData.animatedRigEntity = data.animatedRigEntity;

                characterBodyParts.Top = newEntity;
                state.EntityManager.SetComponentData(entities[i], characterBodyParts);

                state.EntityManager.SetComponentData(newEntity, newData);
                state.EntityManager.AddComponentData(newEntity, new Parent {  Value = entities[i] });

                //Setup Renderers
                var linkedEntities = state.EntityManager.GetBuffer<LinkedEntityGroup>(newEntity);
                for (int j = 0; j < linkedEntities.Length; ++j)
                {
                    state.EntityManager.SetComponentData(linkedEntities[j].Value, new Parent { Value = renderParent });
                }
                state.EntityManager.RemoveComponent<SwapSkinnedMesh>(entities[i]);
            }
            entities.Dispose();
            swapEvents.Dispose();
            rendererEntities.Dispose();

           
        }

        [BurstCompile]
        partial struct UpdateBodyPartsDatabaseJob : IJobEntity
        {
            public EntityCommandBuffer ECB;
            public NativeHashMap<BodyPartMeshKey, Entity> BodyPartsPrefabs;

            void Execute(Entity entity, DynamicBuffer<MeshArray> newPrefabs)
            {
                for (int i = 0; i < newPrefabs.Length; ++i)
                {
                    var key = new BodyPartMeshKey
                    {
                        Category = newPrefabs[i].Category,
                        Index = newPrefabs[i].Index
                    };

                    if (!BodyPartsPrefabs.ContainsKey(key))
                        BodyPartsPrefabs.Add(
                            key,
                            newPrefabs[i].Value
                        );
                }
                ECB.RemoveComponent<MeshArray>(entity);
            }
        }
    }
}
