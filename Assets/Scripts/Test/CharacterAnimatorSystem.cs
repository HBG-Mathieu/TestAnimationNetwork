using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

using Rukhanka;

namespace Unity.Template.CompetitiveActionMultiplayer
{
    public struct CharacterControl : IComponentData
    {
        public float MoveSpeed;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [BurstCompile]
    public partial struct CharacterAnimatorSystem : ISystem
    {
        private FastAnimatorParameter speedParam;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameResources>();
            state.RequireForUpdate(SystemAPI.QueryBuilder()
                .WithAll<FirstPersonCharacterControl>().Build());

            speedParam = new FastAnimatorParameter("MoveSpeed");
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            UpdateAnimatorJob updateAnimatorJob = new UpdateAnimatorJob
            {
                speedParam = speedParam,
            };
            state.Dependency = updateAnimatorJob.Schedule(state.Dependency);
        }

        [BurstCompile]
        partial struct UpdateAnimatorJob : IJobEntity
        {
            public FastAnimatorParameter speedParam;

            void Execute(Entity entity, in CharacterControl controller,
                AnimatorParametersAspect animatorAspect)
            {
                animatorAspect.SetFloatParameter(speedParam,
                    controller.MoveSpeed);
            }
        }
    }
}