using ReactUnity.Dispatchers;
using ReactUnity.Helpers;
using ReactUnity.Schedulers;
using ReactUnity.ScriptEngine;
using ReactUnity.StyleEngine;
using System;
using UnityEngine.UIElements;

namespace ReactUnity.Editor.Renderer
{
    public class ReactUnityElement : VisualElement
    {
        protected IDisposable ScriptWatchDisposable;
        public ReactUnityRunner runner { get; private set; }
        public EditorContext context { get; private set; }
        public IDispatcher dispatcher { get; private set; }
        public IMediaProvider MediaProvider { get; private set; }

        public ReactScript Script { get; }
        public GlobalRecord Globals { get; }
        public JavascriptEngineType EngineType { get; }

        public bool Debug = false;
        public bool AwaitDebugger = false;


        public ReactUnityElement(ReactScript script, GlobalRecord globals, IMediaProvider mediaProvider, JavascriptEngineType engineType = JavascriptEngineType.Auto, bool debug = false, bool awaitDebugger = false, bool autorun = true)
        {
            Script = script;
            Globals = globals;
            MediaProvider = mediaProvider;
            EngineType = engineType;
            Debug = debug;
            AwaitDebugger = awaitDebugger;
            AddToClassList("react-unity__host");
            if (autorun) Run();
        }

        public virtual void Run()
        {
            var src = Script;

            runner = new ReactUnityRunner();
            dispatcher = new EditorDispatcher();

            ScriptWatchDisposable = src.GetScript((sc, isDevServer) =>
            {
                context = new EditorContext(this, Globals, src, dispatcher, new UnityScheduler(dispatcher), MediaProvider, isDevServer, () => Restart());
                runner.RunScript(sc, context, EngineType, Debug, AwaitDebugger);
            }, dispatcher, true, true);
        }

        public void Destroy()
        {
            Clear();
            if (ScriptWatchDisposable != null) ScriptWatchDisposable.Dispose();
            context?.Dispose();
            dispatcher?.Dispose();
            runner = null;
            context = null;
            dispatcher = null;
            ScriptWatchDisposable = null;
        }

        public virtual void Restart()
        {
            Destroy();
            Run();
        }
    }
}
