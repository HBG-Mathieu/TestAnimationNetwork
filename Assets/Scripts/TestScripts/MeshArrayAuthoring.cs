using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.Template.CompetitiveActionMultiplayer
{
    public enum E_CharacterEditorCategory
    {
        Silhouette,
        Head_Face,
        Head_Hair,
        Head_Ears,
        Head_Brows,
        Top,
        Hands,
        Pants,
        Feet
    }
    public struct MeshArray : IBufferElementData
    {
        public E_CharacterEditorCategory Category;
        public int Index;
        public Entity Value;
    }

    public class MeshArrayAuthoring : MonoBehaviour
    {

        class MeshArrayBaker : Baker<MeshArrayAuthoring>
        {

            private string[] m_CategoriesPath =
            {
                "CharacterEditor/Heads/Faces",
                "CharacterEditor/Heads/Hair",
                "CharacterEditor/Heads/Ears",
                "CharacterEditor/Heads/Brows",
                "CharacterEditor/Tops",
                "CharacterEditor/Hands",
                "CharacterEditor/Bottoms",
                "CharacterEditor/Feet"
            };

            public override void Bake(MeshArrayAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var buffer = AddBuffer<MeshArray>(entity);

                for (int i = 0; i < m_CategoriesPath.Length; ++i)
                {
                    var prefabs = Resources.LoadAll<GameObject>(m_CategoriesPath[i]);
                    for (int j = 0; j < prefabs.Length; ++j)
                    {
                        var entityPrefab = GetEntity(prefabs[j].GetComponentInChildren<SkinnedMeshRenderer>(), TransformUsageFlags.Dynamic);
                        buffer.Add(new MeshArray { Value = entityPrefab, Index = j, Category = (E_CharacterEditorCategory)(i + 1) });
                    }
                }
            }
        }
    }
}
