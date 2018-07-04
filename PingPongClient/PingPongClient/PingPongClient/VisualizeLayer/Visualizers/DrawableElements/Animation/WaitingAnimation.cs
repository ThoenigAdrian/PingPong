using Microsoft.Xna.Framework;
using System;

namespace PingPongClient.VisualizeLayer.Visualizers.DrawableElements
{
    enum State
    {
        Waiting,
        Error,
        Success
    }

    class WaitingAnimation : DrawableElement
    {
        public State AnimationState { get; private set; }
        VisualizerInterface Visualizer { get; set; }
        Vector2 Center { get; set; }
        int ElementCount { get; set; }
        Color AnimationColor { get; set; }
        float Offset { get; set; }
        int ElementSize { get; set; }
        float Radius { get; set; }
        float MovementWavelength { get; set; }
        float MovementStep { get; set; }
        float WavelengthMinimum { get; set; }

        AnimationChange _animationChange;

        public WaitingAnimation()
        {
            ElementSize = 7;
            ElementCount = 8;
            WavelengthMinimum = 25;
            Radius = WavelengthMinimum;
           
            Offset = 0;

            MovementStep = 0;
            MovementWavelength = 30;

            ResetState();
        }

        public void ResetState()
        {
            AnimationState = State.Waiting;
            AnimationColor = new Color(255, 255, 255);
        }

        public void Update()
        {
            float offsetIncrement = 1.2F;

            if (_animationChange != null)
                _animationChange.Update(offsetIncrement);

            Offset += offsetIncrement;
            if (Offset >= 360)
                Offset -= 360;

            Radius = WavelengthMinimum + (MovementWavelength / 2) + ((float)Math.Sin(GetRadiant(Offset * 2F)) * MovementWavelength / 2);
        }

        public void Draw(VisualizerInterface Visualizer)
        {
            Center = Visualizer.GetCenter();

            for (int i = 0; i < ElementCount; i++)
            {
                if (AnimationState != State.Waiting && _animationChange != null && _animationChange.StartReached)
                    DrawSingleElementChanging(Visualizer, i, _animationChange);
                else
                    DrawSingleElement(Visualizer, i);
            }
        }

        private void DrawSingleElement(VisualizerInterface Visualizer, int index)
        {
            float angle = 360 / ElementCount * index + Offset;

            Vector2 elementPosition = new Vector2(
                Center.X + (float)Math.Cos(GetRadiant(angle)) * Radius,
                Center.Y + (float)Math.Sin(GetRadiant(angle)) * Radius);

            Visualizer.DrawCircle(ElementSize, elementPosition, AnimationColor);
        }

        private void DrawSingleElementChanging(VisualizerInterface Visualizer, int index, AnimationChange change)
        {
            int colorChange = 255 - (int)(255F / change.AngleDuration * change.OffsetSinceStart);

            if (AnimationState == State.Success)
                AnimationColor = new Color(colorChange, 255, colorChange);
            else if (AnimationState == State.Error)
                AnimationColor = new Color(255, colorChange, colorChange);

            DrawSingleElement(Visualizer, index);
        }

        public void AnimateSuccess()
        {
            ChangeState(State.Success);
        }

        public void AnimateError()
        {
            ChangeState(State.Error);
        }

        private void ChangeState (State newState)
        {
            _animationChange = new AnimationChange(Offset, Offset + 1, Offset + 91);
            AnimationState = newState;
        }


        private float GetRadiant(float angle)
        {
            return angle / 180 * (float)Math.PI;
        }
    }
}
