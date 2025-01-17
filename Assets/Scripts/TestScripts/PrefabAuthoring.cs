using UnityEngine;
using Unity.Entities;

namespace Unity.Template.CompetitiveActionMultiplayer
{


    public class PrefabAuthoring : MonoBehaviour
    {

        class PrefabBaker : Baker<PrefabAuthoring>
        {

            public override void Bake(PrefabAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Prefab());
            }
        }
    }
}