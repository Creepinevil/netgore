using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NetGore;
using NetGore.Graphics;
using NetGore.Graphics.GUI;
using NetGore.IO;

namespace DemoGame.Client
{
    public delegate void UseSkillHandler(SkillType skillType);

    public class SkillsForm : Form, IRestorableSettings
    {
        static readonly Vector2 _iconSize = new Vector2(32, 32);
        readonly int _lineSpacing;
        readonly ISkillCooldownManager _cooldownManager;

        /// <summary>
        /// Notifies listeners when a skill button is clicked.
        /// </summary>
        public event UseSkillHandler OnUseSkill;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillsForm"/> class.
        /// </summary>
        /// <param name="cooldownManager">The skill cooldown manager.</param>
        /// <param name="position">The position.</param>
        /// <param name="parent">The parent.</param>
        public SkillsForm(ISkillCooldownManager cooldownManager, Vector2 position, Control parent) : base(parent, position, new Vector2(150, 100))
        {
            _cooldownManager = cooldownManager;

            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            var fontLineSpacing = Font.LineSpacing;
            // ReSharper restore DoNotCallOverridableMethodsInConstructor

            // Find the spacing to use between lines
            _lineSpacing = (int)Math.Max(fontLineSpacing, _iconSize.Y);

            // Create all the skills
            var allSkillTypes = SkillTypeHelper.Values;
            Vector2 offset = Vector2.Zero;
            foreach (SkillType skillType in allSkillTypes)
            {
                CreateSkillEntry(offset, skillType);
                offset += new Vector2(0, _lineSpacing);
            }
        }

        public ISkillCooldownManager CooldownManager { get { return _cooldownManager; } }

        void CreateSkillEntry(Vector2 position, SkillType skillType)
        {
            var skillInfo = SkillInfoManager<SkillType>.Instance.GetSkillInfo(skillType);

            PictureBox pb = new SkillPictureBox(this, skillInfo, position);
            pb.OnClick += SkillPicture_OnClick;

            SkillLabel skillLabel = new SkillLabel(this, skillInfo, position + new Vector2(_iconSize.X + 4, 0));
            skillLabel.OnClick += SkillLabel_OnClick;
        }

        /// <summary>
        /// Sets the default values for the <see cref="Control"/>. This should always begin with a call to the
        /// base class's method to ensure that changes to settings are hierchical.
        /// </summary>
        protected override void SetDefaultValues()
        {
            base.SetDefaultValues();

            Text = "Skills";
        }

        void SkillLabel_OnClick(object sender, MouseClickEventArgs e)
        {
            if (OnUseSkill != null)
            {
                SkillLabel source = (SkillLabel)sender;
                OnUseSkill((SkillType)source.SkillInfo.Value);
            }
        }

        void SkillPicture_OnClick(object sender, MouseClickEventArgs e)
        {
            if (OnUseSkill != null)
            {
                SkillPictureBox source = (SkillPictureBox)sender;
                OnUseSkill((SkillType)source.SkillInfo.Value);
            }
        }

        #region IRestorableSettings Members

        /// <summary>
        /// Loads the values supplied by the <paramref name="items"/> to reconstruct the settings.
        /// </summary>
        /// <param name="items">NodeItems containing the values to restore.</param>
        public void Load(IDictionary<string, string> items)
        {
            Position = new Vector2(items.AsFloat("X", Position.X), items.AsFloat("Y", Position.Y));
            IsVisible = items.AsBool("IsVisible", IsVisible);
        }

        /// <summary>
        /// Returns the key and value pairs needed to restore the settings.
        /// </summary>
        /// <returns>The key and value pairs needed to restore the settings.</returns>
        public IEnumerable<NodeItem> Save()
        {
            return new NodeItem[]
            { new NodeItem("X", Position.X), new NodeItem("Y", Position.Y), new NodeItem("IsVisible", IsVisible) };
        }

        #endregion

        sealed class SkillLabel : Label
        {
            public SkillLabel(Control parent, SkillInfoAttribute skillInfo, Vector2 position)
                : base(parent, position)
            {
                SkillInfo = skillInfo;
                Text = SkillInfo.DisplayName;
            }

            public SkillInfoAttribute SkillInfo { get; private set; }
        }

        sealed class SkillPictureBox : PictureBox
        {
            readonly ISkillCooldownManager _cooldownManager;

            public SkillPictureBox(SkillsForm parent, SkillInfoAttribute skillInfo, Vector2 position) : base(parent, position, _iconSize)
            {
                SkillInfo = skillInfo;
                Sprite = new Grh(GrhInfo.GetData(SkillInfo.Icon));
                _cooldownManager = parent.CooldownManager;
            }

            public SkillInfoAttribute SkillInfo { get; private set; }

            bool _isCoolingDown = false;

            protected override void UpdateControl(int currentTime)
            {
                _isCoolingDown = _cooldownManager.IsCoolingDown(SkillInfo.CooldownGroup, currentTime);

                base.UpdateControl(currentTime);
            }

            /// <summary>
            /// Draws the <see cref="Control"/>.
            /// </summary>
            /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to draw to.</param>
            protected override void DrawControl(SpriteBatch spriteBatch)
            {
                base.DrawControl(spriteBatch);

                if (_isCoolingDown)
                {
                    var pos = ScreenPosition + new Vector2(Border.LeftWidth, Border.TopHeight);
                    Rectangle r = new Rectangle((int)pos.X, (int)pos.Y, (int)ClientSize.X, (int)ClientSize.Y);
                    XNARectangle.Draw(spriteBatch, r, new Color(0, 0, 0, 150));
                }
            }

            /// <summary>
            /// Sets the default values for the <see cref="Control"/>. This should always begin with a call to the
            /// base class's method to ensure that changes to settings are hierchical.
            /// </summary>
            protected override void SetDefaultValues()
            {
                base.SetDefaultValues();

                StretchSprite = false;
                Size = _iconSize;
            }
        }
    }
}