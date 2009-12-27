using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NetGore.Graphics
{
    /// <summary>
    /// Interface for a class that controls a Character's sprite.
    /// </summary>
    public interface ICharacterSprite
    {
        /// <summary>
        /// Gets the character this <see cref="ICharacterSprite"/> is drawing the sprite for.
        /// </summary>
        Entity Character { get; }

        /// <summary>
        /// Adds a sprite body modifier that alters some, but not all, of the body. <see cref="ICharacterSprite"/>s
        /// that do not support dynamic sprites treat this the same as <see cref="ICharacterSprite.SetBody"/>.
        /// </summary>
        /// <param name="bodyModifierName">The name of the sprite body modifier.</param>
        void AddBodyModifier(string bodyModifierName);

        /// <summary>
        /// Draws the <see cref="ICharacterSprite"/>.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to draw with.</param>
        /// <param name="position">The position to draw the sprite.</param>
        /// <param name="heading">The character's heading.</param>
        void Draw(SpriteBatch spriteBatch, Vector2 position, Direction heading);

        /// <summary>
        /// Sets the sprite's body, which describes the components to use to draw a Set.
        /// </summary>
        /// <param name="bodyName">The name of the sprite body.</param>
        void SetBody(string bodyName);

        /// <summary>
        /// Sets the sprite's paper doll layers.
        /// </summary>
        /// <param name="layers">The name of the paper doll layers.</param>
        void SetPaperDollLayers(IEnumerable<string> layers);

        /// <summary>
        /// Sets the Set that describes how the sprite is laid out.
        /// </summary>
        /// <param name="setName">The name of the Set.</param>
        /// <param name="bodySize">The size of the body.</param>
        void SetSet(string setName, Vector2 bodySize);

        /// <summary>
        /// Updates the <see cref="ICharacterSprite"/>.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        void Update(int currentTime);
    }
}