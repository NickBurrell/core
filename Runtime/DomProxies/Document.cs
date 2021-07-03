using System;
using System.Collections.Generic;

namespace ReactUnity.DomProxies
{
    public class DocumentProxy
    {
        public HeadProxy head;
        public string origin;
        public Action<string> execute;
        public ReactContext context;

        public DocumentProxy(ReactContext context, Action<string> execute, string origin)
        {
            head = new HeadProxy();
            this.execute = execute;
            this.origin = origin;
            this.context = context;
        }

        public IDomElementProxy createElement(string type)
        {
            if (type == "script") return new ScriptProxy(this);
            if (type == "style") return new StyleProxy(this);
            else return null;
        }

        public string createTextNode(string text)
        {
            return text;
        }

        public object querySelector(string query)
        {
            if (query == "head") return head;
            return null;
        }

        public object getElementById(string query)
        {
            return null;
        }

        public List<IDomElementProxy> getElementsByTagName(string tagName)
        {
            return new List<IDomElementProxy>();
        }

        public List<IDomElementProxy> querySelectorAll(string domString)
        {
            return null;
        }
    }

    public interface IDomElementProxy
    {
        void OnAppend();
        void OnRemove();

        void setAttribute(object key, object value);
        void removeAttribute(object key);

        void appendChild(string text);
        void removeChild(string text);
    }

    public abstract class DomElementProxyBase
    {
        public int nodeType => 1;
        public object nextSibling => null;

        Dictionary<string, object> attributes = new Dictionary<string, object>();

        public void setAttribute(object key, object value) => attributes[key?.ToString() ?? ""] = value;
        public void removeAttribute(object key) => attributes.Remove(key?.ToString() ?? "");
        public bool hasAttribute(object key) => attributes.ContainsKey(key?.ToString() ?? "");
        public object getAttribute(object key) => attributes.TryGetValue(key?.ToString() ?? "", out var val) ? val : default;
    }

    public class HeadProxy : DomElementProxyBase
    {
        public void appendChild(IDomElementProxy child)
        {
            child.OnAppend();
        }

        public void removeChild(IDomElementProxy child)
        {
            child.OnRemove();
        }

        public void insertBefore(IDomElementProxy child, object before)
        {
            child.OnAppend();
        }
    }

    public class ScriptProxy : DomElementProxyBase, IDomElementProxy
    {
        public string src { get; set; }
        public string charset { get; set; }
        public string crossOrigin { get; set; }
        public float timeout { get; set; }

        public Action<ScriptProxy> onload { get; set; }
        public Action<ScriptProxy> onerror { get; set; }

        public DocumentProxy document;
        public HeadProxy parentNode;

        public ScriptProxy(DocumentProxy document)
        {
            this.document = document;
            parentNode = document.head;
        }

        public void OnAppend()
        {
            var script = document.context.CreateStaticScript(src);
            var dispatcher = document.context.Dispatcher;

            Action<string> action = (sc) =>
            {
                document.execute(sc);
                onload?.Invoke(this);
            };

            Action<string> callback = (sc) =>
            {
                dispatcher.OnceUpdate(() => action(sc));
            };

            script.GetScript((sc, isDevServer) => callback(sc), dispatcher, false, true);
        }

        public void OnRemove()
        {
        }

        public void appendChild(string text)
        {
            throw new NotImplementedException();
        }

        public void removeChild(string text)
        {
            throw new NotImplementedException();
        }
    }

    public class StyleProxy : DomElementProxyBase, IDomElementProxy
    {
        private List<string> pendingNodes = new List<string>();
        private List<string> pendingRemoval = new List<string>();
        public List<string> childNodes = new List<string>();
        public string firstChild => childNodes.Count > 0 ? childNodes[0] : default;

        public bool enabled;

        public DocumentProxy document;
        public HeadProxy parentNode;

        public StyleProxy(DocumentProxy document)
        {
            this.document = document;
            parentNode = document.head;
        }

        public void OnAppend()
        {
            enabled = true;
            ProcessNodes();
        }

        public void OnRemove()
        {
            // TODO:
        }

        public void appendChild(string text)
        {
            pendingNodes.Add(text);
            childNodes.Add(text);

            if (enabled) ProcessNodes();
        }

        public void removeChild(string text)
        {
            pendingRemoval.Add(text);
            childNodes.Remove(text);

            if (enabled) ProcessNodes();
        }

        void ProcessNodes()
        {
            pendingNodes.ForEach(x => document.context.InsertStyle(x));
            pendingNodes.Clear();

            pendingRemoval.ForEach(x => document.context.RemoveStyle(x));
            pendingRemoval.Clear();
        }
    }
}
