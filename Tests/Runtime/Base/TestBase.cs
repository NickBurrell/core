using ReactUnity.Components;
using ReactUnity.StyleEngine;
using UnityEngine;

namespace ReactUnity.Tests
{
    public class TestBase
    {
        public const string TestPath = "Packages/com.reactunity.core/Tests/Runtime/.scripts/tests/index.js";
        protected GameObject Canvas => GameObject.Find("REACT_CANVAS");
        protected ReactUnity Component => Canvas.GetComponent<ReactUnity>();
        protected UGUIContext Context => Component.Context;
        protected IMediaProvider MediaProvider => Context.MediaProvider;
        protected HostComponent Host => Context.Host as HostComponent;
        internal ReactUnityBridge Bridge => ReactUnityBridge.Instance;
    }
}
