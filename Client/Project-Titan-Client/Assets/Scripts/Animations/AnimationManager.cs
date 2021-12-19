using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Data.Components.Textures;

public static class AnimationManager
{
    /// <summary>
    /// Dictionary containing all animations
    /// </summary>
    private static Dictionary<TextureData, Animation> animations = new Dictionary<TextureData, Animation>();

    /// <summary>
    /// Initializes all stored animation data
    /// </summary>
    public static void Init()
    {
        foreach (var o in GameData.objects.Values) // loop all objects
        {
            if (o.textures == null) continue;
            foreach (var textureData in o.textures) // loop the textures of each object
            {
                CreateAnimation(textureData);
            }
        }
    }

    private static void CreateAnimation(TextureData textureData)
    {
        switch (textureData)
        {
            case CharacterTextureData charTextureData:
                CreateCharacterAnimation(charTextureData);
                break;
            case EntityTextureData entityTextureData:
                CreateEntityAnimation(entityTextureData);
                break;
            case SequenceTextureData seqTextureData:
                CreateSequenceAnimation(seqTextureData);
                break;
        }
    }

    /// <summary>
    /// Creates a character animation and stores it in the animation dictionary
    /// </summary>
    /// <param name="textureData"></param>
    private static void CreateCharacterAnimation(CharacterTextureData textureData)
    {
        animations.Add(textureData, new CharacterAnimation(textureData));
    }

    /// <summary>
    /// Creates an entity animation and stores it in the animation dictionary
    /// </summary>
    /// <param name="textureData"></param>
    private static void CreateEntityAnimation(EntityTextureData textureData)
    {
        animations.Add(textureData, new EntityAnimation(textureData));
    }

    /// <summary>
    /// Creates a sequence animation and stores it in the animation dictionary
    /// </summary>
    /// <param name="textureData"></param>
    private static void CreateSequenceAnimation(SequenceTextureData textureData)
    {
        animations.Add(textureData, new SequenceAnimation(textureData));
    }

    /// <summary>
    /// Returns the created character animation for a given character texture data
    /// </summary>
    /// <param name="textureData"></param>
    /// <returns></returns>
    public static Animation GetAnimation(TextureData textureData)
    {
        return animations[textureData];
    }
}
