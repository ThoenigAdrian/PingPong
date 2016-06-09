namespace PingPongClient.InputLayer
{
    class PlayerInput
    {
        public int ID { get; set; }

        public InputInterface Input { get; set; }

        public PlayerInput(InputInterface input)
        {
            Input = input;
        }
    }
}
