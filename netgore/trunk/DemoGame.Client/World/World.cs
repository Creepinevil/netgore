using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.Xna.Framework.Graphics;
using NetGore;
using NetGore.Graphics;

namespace DemoGame.Client
{
    public delegate void WorldEventHandler(World world);

    public delegate void WorldEventHandler<T>(World world, T item);

    public class World : WorldBase
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        readonly ICamera2D _camera;

        /// <summary>
        /// Interface used to get the current time
        /// </summary>
        readonly IGetTime _getTime;

        /// <summary>
        /// Map currently being used
        /// </summary>
        Map _map;

        MapEntityIndex _usercharIndex;
        public event WorldEventHandler<Map> OnChangeMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="World"/> class.
        /// </summary>
        /// <param name="getTime">Interface to get the current time.</param>
        /// <param name="camera">Primary world view camera.</param>
        public World(IGetTime getTime, ICamera2D camera)
        {
            _getTime = getTime;
            _camera = camera;
        }

        /// <summary>
        /// Gets the camera used for the active view.
        /// </summary>
        public ICamera2D Camera
        {
            get { return _camera; }
        }

        /// <summary>
        /// Gets or sets if the <see cref="UserCharIndex"/> is set.
        /// </summary>
        public bool IsUserCharIndexSet { get; set; }

        /// <summary>
        /// Gets or sets the map currently being used.
        /// </summary>
        public Map Map
        {
            get { return _map; }
            set
            {
                if (_map == value)
                    return;

                _map = value;
                _camera.Map = _map;

                if (OnChangeMap != null)
                    OnChangeMap(this, _map);
            }
        }

        /// <summary>
        /// Gets the user's character, or null if the user's character is not yet set.
        /// </summary>
        public Character UserChar
        {
            get
            {
                if (_map == null || !IsUserCharIndexSet)
                    return null;

                return _map.GetDynamicEntity<Character>(UserCharIndex);
            }
        }

        /// <summary>
        /// Gets or sets the MapEntityIndex of the Character that belongs to the client's character.
        /// </summary>
        public MapEntityIndex UserCharIndex
        {
            get { return _usercharIndex; }
            set
            {
                _usercharIndex = value;
                IsUserCharIndexSet = true;
            }
        }

        /// <summary>
        /// Draws the world
        /// </summary>
        /// <param name="sb">SpriteBatch to draw to</param>
        public void Draw(SpriteBatch sb)
        {
            _map.Draw(sb, Camera);
        }

        /// <summary>
        /// Gets the current time
        /// </summary>
        /// <returns>Current time</returns>
        public override int GetTime()
        {
            return _getTime.GetTime();
        }

        public void SetMap(Map newMap)
        {
            if (newMap == null)
                throw new ArgumentNullException("newMap");

            // Check that the map actually needs to change
            if (Map == newMap)
            {
                const string errmsg = "Requested to change map to the map we are already on.";
                Debug.Fail(errmsg);
                if (log.IsWarnEnabled)
                    log.Warn(errmsg);
                return;
            }

            // Invalidate the user's character index until it is reset
            IsUserCharIndexSet = false;

            // Dispose of the old map
            if (Map != null)
                Map.Dispose();

            // Set the new map
            Map = newMap;
        }

        protected override void UpdateMaps(int deltaTime)
        {
            // Update the map
            if (_map != null)
                _map.Update(deltaTime);
        }
    }
}