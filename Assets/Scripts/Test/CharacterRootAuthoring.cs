using Unity.Entities;
using UnityEngine;

namespace Unity.Template.CompetitiveActionMultiplayer
{
    public class CharacterRootAuthoring : MonoBehaviour
    {
        public GameObject Root;

        class CharacterRootBaker : Baker<CharacterRootAuthoring>
        {
            public override void Bake(CharacterRootAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CharacterRoot { Entity = GetEntity(authoring.Root, TransformUsageFlags.Dynamic) });
            }
        }
    }
}
