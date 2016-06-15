using NetworkLibrary.Utility;
using System.Collections.Generic;

namespace ConnectionTesting
{
    abstract class Module
    {
        public event InitFailHandler InitFailedEvent;
        public delegate void InitFailHandler(Module sender);

        public SafeStack<string> CommandStack { get; private set; }
        protected LogWriter Logger;

        bool m_stopModule;

        public Module(LogWriter logger)
        {
            Logger = logger;
            CommandStack = new SafeStack<string>();
            m_stopModule = false;
        }

        public void Run()
        {
            Initialize();

            while (!m_stopModule)
            {
                string[] cmds = GetCommandos();
                foreach (string cmd in cmds)
                    ExecuteCommand(cmd);
                ExecuteModuleActions();
            }
        }

        public void Shutdown()
        {
            m_stopModule = true;
            ShutdownActions();
        }


        protected abstract void Initialize();
        protected abstract void ExecuteCommand(string cmd);
        protected abstract void ExecuteModuleActions();
        protected abstract void ShutdownActions();

        private string[] GetCommandos()
        {
            List<string> allCmds = new List<string>();

            string cmd;
            while ((cmd = CommandStack.Read()) != null)
            {
                allCmds.Add(cmd);
            }

            return allCmds.ToArray();
        }

        protected void RaiseInitFailEvent()
        {
            if (InitFailedEvent != null)
                InitFailedEvent.Invoke(this);
        }
    }
}
