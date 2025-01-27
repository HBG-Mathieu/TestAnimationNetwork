using Unity.Mathematics;
using Unity.Burst;
using Unity.Entities;

using Rukhanka;


namespace Unity.Template.CompetitiveActionMultiplayer
{

    public struct CharacterRoot : IComponentData
    {
        public Entity Entity;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(RukhankaAnimationSystemGroup))]
    [BurstCompile]
    public partial struct PlayerAnimatorSystem : ISystem
    {
        private FastAnimatorParameter MoveParam;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

            state.RequireForUpdate<GameResources>();

            MoveParam = new FastAnimatorParameter("MoveSpeed");
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            UpdateAnimatorJob updateAnimatorJob = new UpdateAnimatorJob
            {
                MoveParam = MoveParam,
                CharacterControlLookup = SystemAPI.GetComponentLookup<FirstPersonCharacterControl>(),
            };
            state.Dependency = updateAnimatorJob.Schedule(state.Dependency);
        }

        [BurstCompile]
        public partial struct UpdateAnimatorJob : IJobEntity
        {
            public FastAnimatorParameter MoveParam;

            public ComponentLookup<FirstPersonCharacterControl> CharacterControlLookup;

            void Execute(
                AnimatorParametersAspect animatorAspect,
                CharacterRoot characterRoot)
            {
                var controller = CharacterControlLookup[characterRoot.Entity];
                animatorAspect.SetFloatParameter(MoveParam, math.distance(float3.zero, controller.MoveVector));
            }
        }
    }
}