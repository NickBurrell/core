using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ReactUnity.Types
{
    public enum AssetReferenceType
    {
        None = 0,
        Auto = 1,
        Object = 2,
        Resource = 3,
        File = 4,
        Url = 5,
        Global = 6,
        Procedural = 7,
        Data = 8,
        Path = 9,
    }

    public class AssetReference<AssetType> : IDisposable where AssetType : class
    {
        public static AssetReference<AssetType> None = new AssetReference<AssetType>(AssetReferenceType.None, null);
        private static Regex HttpRegex = new Regex("^https?://");
        private static Regex FileRegex = new Regex("^file?://");

        public AssetReferenceType type { get; private set; } = AssetReferenceType.None;
        public object value { get; private set; }

        protected bool IsCached;
        protected AssetType CachedValue;

        public AssetReference(AssetReferenceType type, object value)
        {
            this.type = type;
            this.value = value;
        }

        public AssetReference(Url url)
        {
            value = url.NormalizedUrl;
            switch (url.Protocol)
            {
                case UrlProtocol.Contextual:
                    type = AssetReferenceType.Auto;
                    break;
                case UrlProtocol.Web:
                    type = AssetReferenceType.Url;
                    break;
                case UrlProtocol.Resource:
                    type = AssetReferenceType.Resource;
                    break;
                case UrlProtocol.File:
                    type = AssetReferenceType.File;
                    break;
                case UrlProtocol.Data:
                    type = AssetReferenceType.Data;
                    break;
                case UrlProtocol.Global:
                    type = AssetReferenceType.Global;
                    break;
                case UrlProtocol.None:
                default:
                    type = AssetReferenceType.None;
                    value = null;
                    break;
            }
        }

        public void Get(ReactContext context, System.Action<AssetType> callback)
        {
            if (IsCached)
            {
                callback(CachedValue);
                return;
            }


            var realType = type;
            var realValue = value;
            if (realType == AssetReferenceType.Auto || realType == AssetReferenceType.Path)
            {
                var path = context.ResolvePath(realValue as string);
                if (path == null) realType = AssetReferenceType.None;
                else if (HttpRegex.IsMatch(path))
                {
                    realType = AssetReferenceType.Url;
                    realValue = path;
                }
                else if (FileRegex.IsMatch(path))
                {
                    realType = AssetReferenceType.File;
                    realValue = path;
                }
                else
                {
                    realType = context.Script.ScriptSource == ScriptSource.File ? AssetReferenceType.File : AssetReferenceType.Resource;
                    realValue = path;
                }
            }


            Get(context, realType, realValue, (val) =>
            {
                IsCached = true;
                CachedValue = val;
                callback(val);
            });
        }

        protected virtual void Get(ReactContext context, AssetReferenceType realType, object realValue, Action<AssetType> callback)
        {
            switch (realType)
            {
                case AssetReferenceType.Resource:
                    callback(Resources.Load(realValue as string, typeof(AssetType)) as AssetType);
                    break;
                case AssetReferenceType.Global:
                    if (context.Globals.TryGetValue(realValue as string, out var res)) callback(res as AssetType);
                    else callback(null);
                    break;
                case AssetReferenceType.Object:
                    callback(realValue as AssetType);
                    break;
                case AssetReferenceType.File:
                case AssetReferenceType.Url:
                case AssetReferenceType.None:
                case AssetReferenceType.Procedural:
                case AssetReferenceType.Data:
                default:
                    callback(null);
                    break;
            }
        }

        public virtual void Dispose()
        {
        }
    }
}
