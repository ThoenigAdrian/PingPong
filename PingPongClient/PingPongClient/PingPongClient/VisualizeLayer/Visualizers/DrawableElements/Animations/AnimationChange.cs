namespace PingPongClient.VisualizeLayer.Visualizers.DrawableElements
{
    class AnimationChange
    {
        public float Offset { get; private set; }
        public float OffsetOnChange { get; private set; }
        public float OffsetSinceStart { get; private set; }
        public float OffsetTillFinish { get; private set; }

        private float _startingAngle;
        public float StartingAngle { get { return _startingAngle >= 360 ? _startingAngle - 360 : _startingAngle; } }

        private float _finishAngle;
        public float FinishAngle { get { return _finishAngle >= 360 ? _finishAngle - 360 : _finishAngle; } }

        public float AngleDuration { get { return _finishAngle - _startingAngle; } }

        public bool StartReached { get; private set; }
        public bool ChangeComplete { get; private set; }

        public AnimationChange(float offsetOnChange, float startingAngle, float finishAngle)
        {
            Offset = offsetOnChange;
            OffsetOnChange = offsetOnChange;
            OffsetSinceStart = 0;

            if (startingAngle < offsetOnChange)
            {
                _startingAngle = startingAngle + 360;
                _finishAngle = finishAngle + 360;
            }
            else
            {
                _startingAngle = startingAngle;
                _finishAngle = finishAngle;
            }

            while (_finishAngle < _startingAngle)
                _finishAngle += 360;

            OffsetTillFinish = _finishAngle - offsetOnChange;

            StartReached = false;
            ChangeComplete = false;
        }

        public void Update(float offsetIncrement)
        {
            Offset += offsetIncrement;
            OffsetTillFinish = _finishAngle - Offset;

            if(!StartReached && Offset >= _startingAngle)
                StartReached = true;

            if (StartReached)
                OffsetSinceStart += offsetIncrement;

            if (StartReached && Offset >= _finishAngle)
                ChangeComplete = true;
        }
    }
}
