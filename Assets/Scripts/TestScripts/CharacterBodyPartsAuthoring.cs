using Unity.Entities;
using UnityEngine;

namespace Unity.Template.CompetitiveActionMultiplayer
{
    public struct CharacterBodyParts : IComponentData
    {
        public Entity Head_Face;
        public Entity Top;
        public Entity Hands;
        public Entity Bottom;
        public Entity Feet;
    }
    public class CharacterBodyPartsAuthoring : MonoBehaviour
    {
        public GameObject Head_Face;
        public GameObject Top;
        public GameObject Hands;
        public GameObject Bottom;
        public GameObject Feet;

        public class CharacterBodyPartsBaker : Baker<CharacterBodyPartsAuthoring>
        {
            public override void Bake(CharacterBodyPartsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent(entity, new CharacterBodyParts
                {
                    Head_Face = GetEntity(authoring.Head_Face, TransformUsageFlags.Renderable),
                    Top = GetEntity(authoring.Top, TransformUsageFlags.Renderable),
                    Hands = GetEntity(authoring.Hands, TransformUsageFlags.Renderable),
                    Bottom = GetEntity(authoring.Bottom, TransformUsageFlags.Renderable),
                    Feet = GetEntity(authoring.Feet, TransformUsageFlags.Renderable),
                });

                //Test
                AddComponent(entity, new SwapSkinnedMesh
                {
                    Category = E_CharacterEditorCategory.Top,
                    Index = 0,
                });
            }
        }
    }
}
