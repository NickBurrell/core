using Facebook.Yoga;
using ReactUnity.Editor.Events;
using ReactUnity.Editor.Renderer;
using ReactUnity.Editor.Styling;
using ReactUnity.Helpers;
using ReactUnity.Styling;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

namespace ReactUnity.Editor.Components
{
    public interface IEditorComponent<out T> : IReactComponent
    {
        T Element { get; }
    }

    public class EditorComponent<T> : BaseReactComponent<EditorContext>, IEditorComponent<T> where T : VisualElement, new()
    {
        public T Element { get; protected set; }
        public override string Name => Element.name;

        protected Dictionary<string, object> EventHandlers = new Dictionary<string, object>();
        protected Dictionary<Type, object> Manipulators = new Dictionary<Type, object>();
        private string currentCursor = null;

        protected EditorComponent(T element, EditorContext context, string tag) : base(context, tag, true)
        {
            Element = element;
            Element.userData = Data;
        }

        public EditorComponent(EditorContext context, string tag, bool isContainer = true) : base(context, tag, isContainer)
        {
            Element = new T();
            Element.userData = Data;
        }

        protected override void ApplyLayoutStylesSelf()
        {
            var computed = ComputedStyle;
            Element.style.flexDirection = StylingHelpers.GetStyleEnumCustom<FlexDirection>(computed, LayoutProperties.FlexDirection);
            Element.style.flexWrap = StylingHelpers.GetStyleEnumCustom<Wrap>(computed, LayoutProperties.Wrap);
            Element.style.flexGrow = StylingHelpers.GetStyleFloat(computed, LayoutProperties.FlexGrow);
            Element.style.flexShrink = StylingHelpers.GetStyleFloat(computed, LayoutProperties.FlexShrink);

            Element.style.width = StylingHelpers.GetStyleLength(computed, LayoutProperties.Width);
            Element.style.height = StylingHelpers.GetStyleLength(computed, LayoutProperties.Height);
            Element.style.flexBasis = StylingHelpers.GetStyleLength(computed, LayoutProperties.FlexBasis);

            Element.style.minWidth = StylingHelpers.GetStyleLength(computed, LayoutProperties.MinWidth);
            Element.style.minHeight = StylingHelpers.GetStyleLength(computed, LayoutProperties.MinHeight);
            Element.style.maxWidth = StylingHelpers.GetStyleLength(computed, LayoutProperties.MaxWidth);
            Element.style.maxHeight = StylingHelpers.GetStyleLength(computed, LayoutProperties.MaxHeight);

            Element.style.paddingBottom = StylingHelpers.GetStyleLengthDouble(computed, LayoutProperties.PaddingBottom, LayoutProperties.Padding);
            Element.style.paddingTop = StylingHelpers.GetStyleLengthDouble(computed, LayoutProperties.PaddingTop, LayoutProperties.Padding);
            Element.style.paddingLeft = StylingHelpers.GetStyleLengthDouble(computed, LayoutProperties.PaddingLeft, LayoutProperties.Padding);
            Element.style.paddingRight = StylingHelpers.GetStyleLengthDouble(computed, LayoutProperties.PaddingRight, LayoutProperties.Padding);

            Element.style.marginBottom = StylingHelpers.GetStyleLengthDouble(computed, LayoutProperties.MarginBottom, LayoutProperties.Margin);
            Element.style.marginTop = StylingHelpers.GetStyleLengthDouble(computed, LayoutProperties.MarginTop, LayoutProperties.Margin);
            Element.style.marginLeft = StylingHelpers.GetStyleLengthDouble(computed, LayoutProperties.MarginLeft, LayoutProperties.Margin);
            Element.style.marginRight = StylingHelpers.GetStyleLengthDouble(computed, LayoutProperties.MarginRight, LayoutProperties.Margin);

            Element.style.left = StylingHelpers.GetStyleLength(computed, LayoutProperties.Left);
            Element.style.right = StylingHelpers.GetStyleLength(computed, LayoutProperties.Right);
            Element.style.top = StylingHelpers.GetStyleLength(computed, LayoutProperties.Top);
            Element.style.bottom = StylingHelpers.GetStyleLength(computed, LayoutProperties.Bottom);

            Element.style.borderLeftWidth = StylingHelpers.GetStyleFloatDouble(computed, LayoutProperties.BorderLeftWidth, LayoutProperties.BorderWidth);
            Element.style.borderRightWidth = StylingHelpers.GetStyleFloatDouble(computed, LayoutProperties.BorderRightWidth, LayoutProperties.BorderWidth);
            Element.style.borderTopWidth = StylingHelpers.GetStyleFloatDouble(computed, LayoutProperties.BorderTopWidth, LayoutProperties.BorderWidth);
            Element.style.borderBottomWidth = StylingHelpers.GetStyleFloatDouble(computed, LayoutProperties.BorderBottomWidth, LayoutProperties.BorderWidth);

            Element.style.display = StylingHelpers.GetStyleEnumCustom<DisplayStyle>(computed, LayoutProperties.Display);
            Element.style.position = StylingHelpers.GetStyleEnumCustom<Position>(computed, LayoutProperties.PositionType);
            Element.style.overflow = StylingHelpers.GetStyleEnumCustom<Overflow>(computed, LayoutProperties.Overflow);

            Element.style.alignContent = StylingHelpers.GetStyleEnumCustom<Align>(computed, LayoutProperties.AlignContent);
            Element.style.alignItems = StylingHelpers.GetStyleEnumCustom<Align>(computed, LayoutProperties.AlignItems);
            Element.style.alignSelf = StylingHelpers.GetStyleEnumCustom<Align>(computed, LayoutProperties.AlignSelf);
            Element.style.justifyContent = StylingHelpers.GetStyleEnumCustom<Justify>(computed, LayoutProperties.JustifyContent);
        }

        protected override void ApplyStylesSelf()
        {
            var computed = ComputedStyle;
            Element.style.backgroundColor = StylingHelpers.GetStyleColor(computed, StyleProperties.backgroundColor);
            Element.style.color = StylingHelpers.GetStyleColor(computed, StyleProperties.color);
            Element.style.textOverflow = StylingHelpers.GetStyleEnumCustom<TextOverflow>(computed, StyleProperties.textOverflow);
            Element.style.visibility = StylingHelpers.GetStyleBoolToEnum(computed, StyleProperties.visibility, Visibility.Visible, Visibility.Hidden);
            Element.style.opacity = StylingHelpers.GetStyleFloat(computed, StyleProperties.opacity);
            Element.style.whiteSpace = StylingHelpers.GetStyleBoolToEnum(computed, StyleProperties.textWrap, WhiteSpace.Normal, WhiteSpace.NoWrap);

            if (computed.HasValue(StyleProperties.fontSize)) Element.style.fontSize = computed.fontSizeActual;
            else Element.style.fontSize = StyleKeyword.Null;

            Element.style.borderBottomLeftRadius = StylingHelpers.GetStyleBorderRadius(computed, StyleProperties.borderBottomLeftRadius);
            Element.style.borderBottomRightRadius = StylingHelpers.GetStyleBorderRadius(computed, StyleProperties.borderBottomRightRadius);
            Element.style.borderTopLeftRadius = StylingHelpers.GetStyleBorderRadius(computed, StyleProperties.borderTopLeftRadius);
            Element.style.borderTopRightRadius = StylingHelpers.GetStyleBorderRadius(computed, StyleProperties.borderTopRightRadius);

            Element.style.borderBottomColor = StylingHelpers.GetStyleBorderColor(computed, StyleProperties.borderBottomColor);
            Element.style.borderTopColor = StylingHelpers.GetStyleBorderColor(computed, StyleProperties.borderTopColor);
            Element.style.borderLeftColor = StylingHelpers.GetStyleBorderColor(computed, StyleProperties.borderLeftColor);
            Element.style.borderRightColor = StylingHelpers.GetStyleBorderColor(computed, StyleProperties.borderRightColor);

            if (computed.HasValue(StyleProperties.backgroundImage)) computed.backgroundImage?.Get(Context, tx => Element.style.backgroundImage = tx);
            else Element.style.backgroundImage = StyleKeyword.Null;

            if (computed.HasValue(StyleProperties.fontStyle) || computed.HasValue(StyleProperties.fontWeight))
                Element.style.unityFontStyleAndWeight = StylingHelpers.ConvertFontStyle(computed.fontStyle, computed.fontWeight);
            else Element.style.unityFontStyleAndWeight = StyleKeyword.Null;


            if (computed.HasValue(StyleProperties.backgroundImage) && computed.HasValue(StyleProperties.backgroundColor))
                Element.style.unityBackgroundImageTintColor = computed.backgroundColor;
            else Element.style.unityBackgroundImageTintColor = StyleKeyword.Null;


            if (computed.HasValue(StyleProperties.textAlign))
            {
                if (StylingHelpers.TextAlignMap.TryGetValue(computed.textAlign, out var value)) Element.style.unityTextAlign = value;
                else Element.style.unityTextAlign = TextAnchor.MiddleCenter;
            }
            else Element.style.unityTextAlign = StyleKeyword.Null;


            if (computed.HasValue(StyleProperties.fontFamily))
            {
                if (computed.fontFamily != null) computed.fontFamily?.Get(Context, x =>
                {
                    if (x?.sourceFontFile) Element.style.unityFont = x?.sourceFontFile;
                    else Element.style.unityFont = EditorResourcesHelper.DefaultFont;
                });
            }
            else Element.style.unityFont = StyleKeyword.Null;


            if (computed.HasValue(StyleProperties.cursor))
            {
                var cursor = EditorResourcesHelper.UtilityCursorClassPrefix + computed.cursor;
                if (currentCursor != cursor)
                {
                    if (currentCursor != null)
                    {
                        Element.RemoveFromClassList(currentCursor);
                        currentCursor = null;
                    }
                    if (cursor != null)
                    {
                        currentCursor = cursor;
                        Element.AddToClassList(currentCursor);
                    }
                }
            }
            else if (currentCursor != null)
            {
                Element.RemoveFromClassList(currentCursor);
                currentCursor = null;
            }

            // Transforms

            if (computed.HasValue(StyleProperties.scale)) Element.transform.scale = new Vector3(computed.scale.x, computed.scale.y, 1);
            else Element.transform.scale = Vector3.one;

            if (computed.HasValue(StyleProperties.rotate)) Element.transform.rotation = Quaternion.Euler(computed.rotate);
            else Element.transform.rotation = Quaternion.identity;



            Vector3 translate;

            var size = Element.layout.size;
            var rect = new Vector2(float.IsNaN(size.x) ? 0 : size.x, float.IsNaN(size.y) ? 0 : size.y);

            if (computed.HasValue(StyleProperties.translate))
            {
                var tran = computed.translate;

                var scale = new Vector2(tran.X.Unit == YogaUnit.Percent ? rect.x / 100 : 1, tran.Y.Unit == YogaUnit.Percent ? rect.y / 100 : 1);
                translate = new Vector2(tran.X.Value * scale.x, -tran.Y.Value * scale.y);
            }
            else translate = Vector3.zero;


            var hasPivot = computed.HasValue(StyleProperties.transformOrigin);

            if (hasPivot)
            {
                var origin = computed.transformOrigin;

                var pivotX = origin.X.Unit == YogaUnit.Percent ? (origin.X.Value / 100) : origin.X.Unit == YogaUnit.Point ? (origin.X.Value / rect.x) : 0.5f;
                var pivotY = origin.Y.Unit == YogaUnit.Percent ? (origin.Y.Value / 100) : origin.Y.Unit == YogaUnit.Point ? (origin.Y.Value / rect.y) : 0.5f;
                var pivot = new Vector3(pivotX, 1 - pivotY, 0);

                if (pivot == Vector3.zero) Element.transform.position = translate;
                else
                {
                    Vector3 deltaPosition = -pivot;    // get change in pivot
                    deltaPosition.Scale(rect);           // apply sizing
                    deltaPosition.Scale(Element.transform.scale);          // apply scaling
                    deltaPosition = Element.transform.rotation * deltaPosition; // apply rotation

                    var counter = new Vector3(pivot.x, pivot.y, 0);
                    counter.Scale(rect);

                    var pos = deltaPosition + translate + counter;
                    Element.transform.position = new Vector3(pos.x, pos.y, 0);
                }
            }
            else Element.transform.position = translate;
        }

        public override void DestroySelf()
        {
            base.DestroySelf();
            Element.RemoveFromHierarchy();
        }

        #region Setters

        public override void SetEventListener(string eventName, Callback fun)
        {
            var (register, unregister) = EditorEventHandlerMap.GetEventMethods(eventName);

            // Remove
            if (EventHandlers.TryGetValue(eventName, out var existingHandler))
            {
                unregister.Invoke(Element, new object[] { existingHandler, TrickleDown.NoTrickleDown });
                EventHandlers.Remove(eventName);
            }

            // No event to add
            if (fun == null) return;

            EventCallback<EventBase> callAction = (e) => fun.Call(e, this);

            register.Invoke(Element, new object[] { callAction, TrickleDown.NoTrickleDown });
            EventHandlers[eventName] = callAction;
        }

        public override void SetProperty(string property, object value)
        {
            switch (property)
            {
                case "name":
                    Element.name = value?.ToString();
                    return;
                case "focusable":
                    Element.focusable = Convert.ToBoolean(value);
                    return;
#if UNITY_EDITOR
                case "bind":
                    if (value is UnityEditor.SerializedObject so) Element.Bind(so);
                    else Element.Unbind();
                    return;
#endif
                default:
                    base.SetProperty(property, value);
                    return;
            }
        }

        public override void UpdateClasses(string oldClassName, HashSet<string> oldClassList)
        {
            foreach (var cls in oldClassList) Element.RemoveFromClassList(cls);
            foreach (var cls in ClassList) Element.AddToClassList(cls);
        }

        #endregion

        public override object GetComponent(Type type)
        {
            if (Manipulators.TryGetValue(type, out var val)) return val;
            return null;
        }

        public override object AddComponent(Type type)
        {
            var instance = Activator.CreateInstance(type);
            if (instance is IManipulator m)
            {
                Element.AddManipulator(m);
                Manipulators[type] = m;
            }

            return instance;
        }

        protected override bool InsertChild(IReactComponent child, int index = -1)
        {
            if (child is IEditorComponent<VisualElement> u)
            {
                if (index >= 0) Element.Insert(index, u.Element);
                else Element.Add(u.Element);
                return true;
            }
            return false;
        }

        protected override bool DeleteChild(IReactComponent child)
        {
            if (child is IEditorComponent<VisualElement> u)
            {
                Element.Remove(u.Element);
                return true;
            }
            return false;
        }

        public void CaptureMouse()
        {
            MouseCaptureController.CaptureMouse(Element);
        }

        public void ReleaseMouse()
        {
            MouseCaptureController.ReleaseMouse(Element);
        }

        public bool HasMouseCapture()
        {
            return MouseCaptureController.HasMouseCapture(Element);
        }
    }
}
