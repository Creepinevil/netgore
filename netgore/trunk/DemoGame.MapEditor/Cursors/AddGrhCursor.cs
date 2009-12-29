using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DemoGame.MapEditor.Properties;
using Microsoft.Xna.Framework;
using NetGore.EditorTools;
using NetGore.Graphics;

namespace DemoGame.MapEditor
{
    sealed class AddGrhCursor : MapEditorCursorBase<ScreenForm>
    {
        /// <summary>
        /// Gets the cursor's <see cref="Image"/>.
        /// </summary>
        public override Image CursorImage
        {
            get { return Resources.cursor_grhsadd; }
        }

        /// <summary>
        /// When overridden in the derived class, gets the name of the cursor.
        /// </summary>
        public override string Name
        {
            get { return "Add Grh"; }
        }

        /// <summary>
        /// Gets the priority of the cursor on the toolbar. Lower values appear first.
        /// </summary>
        /// <value></value>
        public override int ToolbarPriority
        {
            get { return 20; }
        }

        /// <summary>
        /// When overridden in the derived class, handles when the cursor has moved.
        /// </summary>
        /// <param name="screen">Screen that the cursor is on.</param>
        /// <param name="e">Mouse events.</param>
        public override void MouseMove(ScreenForm screen, MouseEventArgs e)
        {
            if (screen.chkSnapGrhGrid.Checked)
                MouseUp(screen, e);
        }

        /// <summary>
        /// When overridden in the derived class, handles when a mouse button has been released.
        /// </summary>
        /// <param name="screen">Screen that the cursor is on.</param>
        /// <param name="e">Mouse events.</param>
        public override void MouseUp(ScreenForm screen, MouseEventArgs e)
        {
            Vector2 cursorPos = screen.CursorPos;

            // On left-click place the Grh on the map
            if (e.Button == MouseButtons.Left)
            {
                // Check for a valid MapGrh
                if (screen.SelectedGrh.GrhData == null)
                    return;

                // Find the position the MapGrh will be created at
                Vector2 drawPos;
                if (screen.chkSnapGrhGrid.Checked)
                    drawPos = screen.Grid.AlignDown(cursorPos);
                else
                    drawPos = cursorPos;

                // Check if a MapGrh of the same type already exists at the location
                foreach (MapGrh grh in screen.Map.MapGrhs)
                {
                    if (grh.Position == drawPos && grh.Grh.GrhData.GrhIndex == screen.SelectedGrh.GrhData.GrhIndex)
                        return;
                }

                // Add the MapGrh to the map
                Grh g = new Grh(screen.SelectedGrh.GrhData.GrhIndex, AnimType.Loop, screen.GetTime());
                screen.Map.AddMapGrh(new MapGrh(g, drawPos, screen.chkForeground.Checked));
            }
            else if (e.Button == MouseButtons.Right)
            {
                // On right-click delete any Grhs under the cursor
                while (true)
                {
                    MapGrh mapGrh = screen.Map.Spatial.GetEntity<MapGrh>(cursorPos);
                    if (mapGrh == null)
                        break;

                    screen.Map.RemoveMapGrh(mapGrh);
                }
            }
        }
    }
}