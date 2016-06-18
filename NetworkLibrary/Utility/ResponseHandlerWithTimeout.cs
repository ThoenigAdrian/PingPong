using NetworkLibrary.DataPackages;

namespace NetworkLibrary.Utility
{
    public class ResponseRequest
    {
        public enum ResponseState
        {
            Pending,
            Timeout,
            Received
        }

        public PackageInterface ResponsePackage { get; private set; }

        OneShotTimer m_timer;

        private ResponseState m_state;
        public ResponseState State
        {
            get
            {
                if(m_state == ResponseState.Pending)
                {
                    if (m_timer == true)
                        m_state = ResponseState.Timeout;
                }
                return m_state;
            }
            private set
            {
                m_state = value;
            }
        }

        public PackageType ResponsePackageType { get; set; }

        public ResponseRequest(PackageType responseType, long milliseconds)
        {
            m_timer = new OneShotTimer(milliseconds * 1000);
            ResponsePackageType = responseType;
            State = ResponseState.Pending;
        }

        public bool InspectPackage(PackageInterface package)
        {
            if (State == ResponseState.Pending)
            {
                if (package != null && package.PackageType == ResponsePackageType)
                {
                    ResponsePackage = package;
                    State = ResponseState.Received;
                }
            }

            return ResponsePackage != null;
        }
    }
}
