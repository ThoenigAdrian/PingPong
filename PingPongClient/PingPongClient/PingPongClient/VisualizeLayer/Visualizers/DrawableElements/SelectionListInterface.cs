using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace PingPongClient.VisualizeLayer.Visualizers.DrawableElements
{
    public abstract class SelectionListInterface : DrawableElement
    {
        public delegate void SelectionChangedHandler();
        public event SelectionChangedHandler SelectionChanged;

        public Vector2 TopLeft { get; set; }

        public Color Background { get; protected set; }
        public List<SelectionEntry> ListEntries { get; private set; }

        public Selector Selector { get; protected set; }

        int m_selected;
        public int Selection
        {
            get { return m_selected; }
            set
            {
                int previous = m_selected;

                m_selected = value;

                if (m_selected >= ListEntries.Count)
                    m_selected = ListEntries.Count - 1;

                if (m_selected < 0)
                    m_selected = 0;

                if (m_selected != previous)
                    RaiseSelectionChangedEvent();
            }
        }

        public SelectionListInterface()
        {
            TopLeft = Vector2.Zero;
            Background = Color.Transparent;
            ListEntries = new List<SelectionEntry>(CreateInitialListEntries());

            Selection = 0;
        }

        protected abstract SelectionEntry[] CreateInitialListEntries();

        private void RaiseSelectionChangedEvent()
        {
            if (SelectionChanged != null)
                SelectionChanged.Invoke();
        }

        public Vector2 GetMeasurements(SpriteFont font)
        {
            Vector2 measurement = Vector2.Zero;
            foreach (SelectionEntry entry in ListEntries)
            {
                Vector2 bottomRight = new Vector2(entry.GetMeasurements(font).X, entry.Position.Y + entry.GetMeasurements(font).Y);

                measurement.X = Math.Max(measurement.X, bottomRight.X);
                measurement.Y = Math.Max(measurement.Y, bottomRight.Y);
            }

            return measurement;
        }
    }

    public class SelectionEntry
    {
        public Selector Selector { get; set; }
        public DrawableString DrawString { get; set; }
        public Vector2 Position
        {
            get { return DrawString.Postion; }
            set { DrawString.Postion = value; }
        }

        protected float SpaceSelectorToString { get; set; }

        public SelectionEntry(DrawableString drawString, Selector selector)
        {
            Selector = selector;
            DrawString = drawString;
            SpaceSelectorToString = 5;
        }

        public Vector2 SelectorPosition(SpriteFont font)
        {
            return new Vector2(0, (GetMeasurements(font).Y - Selector.Size.Y) / 2);
        }

        public Vector2 StringPosition(SpriteFont font)
        {
            return new Vector2(Selector.Size.X + SpaceSelectorToString, (GetMeasurements(font).Y - DrawString.GetMeasurements(font).Y) / 2);
        }

        public Vector2 GetMeasurements(SpriteFont font)
        {
            Vector2 stringSize = DrawString.GetMeasurements(font);
            return new Vector2(Selector.Size.X + stringSize.X + SpaceSelectorToString, Math.Max(Selector.Size.Y, stringSize.Y));
        }
    }

    public class Selector
    {
        public enum SelectorTexture
        {
            Circle
        }

        public Color SelectorColor { get; set; }
        public SelectorTexture TextureType { get; set; }

        public Vector2 Size { get; set; }

        public Selector() : this(Color.Green, new Vector2(12, 12))
        {
        }

        public Selector(Color color, Vector2 size)
        {
            SelectorColor = color;
            Size = size;
        }
    }
}
