using System;
using InputFunctionality.KeyboardAdapter;
using GameLogicLibrary;
using PingPongClient.InputLayer.InputTranslation;

namespace PingPongClient.InputLayer
{
    class KeyboardInput : InputInterface
    {
        static KeyboardAdvanced KeyboardAdapter = new KeyboardAdvanced();

        KeyboardControlTranslation Translation { get; set; }

        public KeyboardInput(KeyboardControlTranslation translation)
        {
            Translation = translation;
        }

        public override void Initialize()
        {
            KeyboardAdapter.Initialize();
        }

        public override void Update()
        {
            KeyboardAdapter.UpdateState();
        }

        public override ClientMovement GetMovementInput()
        {
            if (Translation == null)
                return ClientMovement.NoInput;

            foreach (ClientMovement movement in Enum.GetValues(typeof(ClientMovement)))
            {
                if (KeyboardAdapter.KeyNowPressed(Translation.GetMovementKey(movement)))
                    return movement;
            }

            if (KeyboardAdapter.KeyNowReleased(Translation.GetMovementKey(ClientMovement.Up)) 
                || KeyboardAdapter.KeyNowReleased(Translation.GetMovementKey(ClientMovement.Down)))
                return ClientMovement.StopMoving;

            return ClientMovement.NoInput;
        }

        public override ClientControls GetControlInput()
        {
            if (Translation == null)
                return ClientControls.NoInput;

            foreach (ClientControls control in Enum.GetValues(typeof(ClientControls)))
            {
                if (KeyboardAdapter.KeyNowPressed(Translation.GetControlKey(control)))
                    return control;
            }

            return ClientControls.NoInput;
        }
    }
}
