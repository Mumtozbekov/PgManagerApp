﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xaml;

namespace PgManager.Extensions
{
  
    public class MethodBindingExtension : MarkupExtension
    {
        private static readonly List<DependencyProperty> s_storageProperties = new();

        private static readonly ConcurrentDictionary<(Type TargetType, string MethodName, int ArgCount), MethodInfo> s_singleMethodInfoCache = new();
        private static readonly ConcurrentDictionary<(Type TargetType, string MethodName, Type[] ArgTypes), MethodInfo> s_methodInfoCache = new(new MethodCacheEqualityComparer());

        private readonly object[] _arguments;
        private readonly List<DependencyProperty> _argumentProperties = new List<DependencyProperty>();

   
        public MethodBindingExtension(object method) : this(new[] { method }) { }


        public MethodBindingExtension(object arg0, object arg1) : this(new[] { arg0, arg1 }) { }


        public MethodBindingExtension(object arg0, object arg1, object arg2) : this(new[] { arg0, arg1, arg2 }) { }


        public MethodBindingExtension(object arg0, object arg1, object arg2, object arg3) : this(new[] { arg0, arg1, arg2, arg3 }) { }

        public MethodBindingExtension(object arg0, object arg1, object arg2, object arg3, object arg4) : this(new[] { arg0, arg1, arg2, arg3, arg4 }) { }


        public MethodBindingExtension(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5) : this(new[] { arg0, arg1, arg2, arg3, arg4, arg5 }) { }


        public MethodBindingExtension(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6) : this(new[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6 }) { }


        public MethodBindingExtension(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7) : this(new[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7 }) { }

    
        public MethodBindingExtension(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8) : this(new[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 }) { }

        private MethodBindingExtension(object[] arguments)
        {
            _arguments = arguments;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            if (provideValueTarget == null)
                return this;

            Type eventHandlerType = null;
            if (provideValueTarget.TargetProperty is EventInfo eventInfo)
            {
                eventHandlerType = eventInfo.EventHandlerType;
            }
            else if (provideValueTarget.TargetProperty is MethodInfo methodInfo)
            {
                var parameters = methodInfo.GetParameters();

                if (parameters.Length == 2)
                    eventHandlerType = parameters[1].ParameterType;
            }

            if (provideValueTarget.TargetObject is DependencyObject target && eventHandlerType != null)
            {
                foreach (object argument in _arguments)
                {
                    var argumentProperty = SetUnusedStorageProperty(target, argument);
                    _argumentProperties.Add(argumentProperty);
                }

                return CreateEventHandler(target, eventHandlerType);
            }
            else if (provideValueTarget.TargetObject is CommandBinding command && eventHandlerType != null)
            {
                IRootObjectProvider rootProvider = serviceProvider as IRootObjectProvider;
                target = rootProvider?.RootObject as DependencyObject;
                foreach (object argument in _arguments)
                {
                    var argumentProperty = SetUnusedStorageProperty(target, argument);
                    _argumentProperties.Add(argumentProperty);
                }
                return CreateEventHandler(target, eventHandlerType);
            }

            return this;
        }
        private Delegate CreateEventHandler(DependencyObject source, Type eventHandlerType)
        {
            EventHandler handler = (sender, eventArgs) =>
            {
                object arg0 = source.GetValue(_argumentProperties[0]);

                if (arg0 == null)
                {
                    Trace.TraceWarning($"[{nameof(MethodBindingExtension)}] First method binding argument is required and cannot resolve to null - method name or method target expected.");
                    return;
                }

                int methodArgsStart;
                object methodTarget;


                string methodName;
                Type methodTargetType;

                if ((methodName = arg0 as string) != null)
                {
                    switch (source)
                    {
                        case FrameworkElement element:
                            methodTarget = element.DataContext;
                            methodTargetType = element.DataContext?.GetType();
                            break;
                        case FrameworkContentElement contentElement:
                            methodTarget = contentElement.DataContext;
                            methodTargetType = contentElement.DataContext?.GetType();
                            break;
                        default:
                            Trace.TraceWarning($"[{nameof(MethodBindingExtension)}] Method target must be specified on element type '{source.GetType()}' because it is not a FrameworkElement and has no data context.");
                            return;
                            break;
                    }
                    if (methodTarget == null)
                    {
                        Trace.TraceWarning($"[{nameof(MethodBindingExtension)}] Null data context for method '{methodName}' on element '{source}' (type: '{source.GetType()}').");
                        return;
                    }

                    methodTargetType = methodTarget.GetType();
                    methodArgsStart = 1;
                }
                else if (_argumentProperties.Count >= 2)
                {
                    methodTarget = arg0;
                    methodTargetType = methodTarget.GetType();
                    methodArgsStart = 2;

                    object arg1 = source.GetValue(_argumentProperties[1]);

                    if (arg1 == null)
                    {
                        Trace.TraceWarning($"[{nameof(MethodBindingExtension)}] Method target type resolved to '{methodTargetType}', method name resolved to null.");
                        return;
                    }

                    if ((methodName = arg1 as string) == null)
                    {
                        Trace.TraceWarning($"[{nameof(MethodBindingExtension)}] Method target type resolved to '{methodTargetType}', method name must be type '{typeof(string)}' (actual type: '{arg1.GetType()}').");
                        return;
                    }
                }
                else
                {
                    Trace.TraceWarning($"[{nameof(MethodBindingExtension)}] Method name must be type '{typeof(string)}' (actual type: '{arg0.GetType()}').");
                    return;
                }

                object[] arguments = new object[_argumentProperties.Count - methodArgsStart];

                for (int i = methodArgsStart; i < _argumentProperties.Count; i++)
                {
                    object argValue = source.GetValue(_argumentProperties[i]);

                    if (argValue is EventSenderExtension)
                        argValue = sender;
                    else if (argValue is EventArgsExtension eventArgsEx)
                        argValue = eventArgsEx.GetArgumentValue(eventArgs, (source as FrameworkElement)?.Language);

                    arguments[i - methodArgsStart] = argValue;
                }

                (var methodInfo, bool convertStrings) = GetCachedMethod(methodTarget.GetType(), methodName, arguments);

                if (methodInfo == null)
                    return;

                if (convertStrings)
                {
                    var parameters = methodInfo.GetParameters();

                    for (int i = 0; i < arguments.Length; i++)
                    {
                        var paramType = parameters[i].ParameterType;

                        if (arguments[i] == null)
                        {
                            if (paramType.IsValueType && Nullable.GetUnderlyingType(paramType) == null)
                            {
                                Trace.TraceWarning($"[{nameof(MethodBindingExtension)}] Method '{methodName}' (target type '{methodTargetType}') parameter {i + 1} (name: '{parameters[i].Name}', type: '{paramType}') is not assignable to null.");
                                return;
                            }
                        }
                        else if (paramType != typeof(object) && paramType != typeof(string) && _arguments[i + methodArgsStart] is string stringArg)
                        {
                            // The original value provided for this argument was a XAML string so try to convert it
                            try
                            {
                                arguments[i] = TypeDescriptor.GetConverter(parameters[i].ParameterType).ConvertFromString(stringArg);
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceWarning($"[{nameof(MethodBindingExtension)}] Method '{methodName}' (target type '{methodTargetType}') parameter {i + 1} (name: '{parameters[i].Name}', type: '{paramType}') could not be assigned from XAML string argument '{stringArg}': {ex}.");
                                return;
                            }
                        }
                        else if (!parameters[i].ParameterType.IsInstanceOfType(arguments[i]))
                        {
                            Trace.TraceWarning($"[{nameof(MethodBindingExtension)}] Method '{methodName}' (target type '{methodTargetType}') parameter {i + 1} (name: '{parameters[i].Name}', type: '{paramType}') is not assignable from argument type '{arguments[i]!.GetType()}'.");
                            return;
                        }
                    }
                }

                methodInfo.Invoke(methodTarget, arguments);
            };

            return Delegate.CreateDelegate(eventHandlerType, handler.Target, handler.Method);
        }

        private static (MethodInfo Info, bool ConvertStrings) GetCachedMethod(Type methodTargetType, string methodName, object[] arguments)
        {
            if (s_singleMethodInfoCache.TryGetValue((methodTargetType, methodName, arguments.Length), out var methodInfo))
                return (methodInfo, true);

            var argumentTypes = Array.ConvertAll(arguments, a => a?.GetType());

            if (s_methodInfoCache.TryGetValue((methodTargetType, methodName, argumentTypes), out methodInfo))
            {
                return (methodInfo, false);
            }

            var methods = methodTargetType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .Where(m => m.Name == methodName)
                               .Select(m => (Info: m, Parameters: m.GetParameters()))
                               .Where(m => m.Parameters.Length == argumentTypes.Length)
                               .ToArray();

            if (methods.Length == 1)
            {
                methodInfo = methods[0].Info;
                s_singleMethodInfoCache[(methodTargetType, methodName, argumentTypes.Length)] = methodInfo;
                return (methodInfo, true);
            }
            else if (methods.Length > 1)
            {
                foreach (var method in methods)
                {
                    int i;
                    for (i = 0; i < argumentTypes.Length; i++)
                    {
                        var paramType = method.Parameters[i].ParameterType;

                        if (method.Parameters[i].IsOut || paramType.IsByRef || paramType.IsPointer)
                            break;

                        var argType = argumentTypes[i];

                        if (argType == null)
                        {
                            if (paramType.IsValueType && Nullable.GetUnderlyingType(paramType) == null)
                                break;
                        }
                        else if (!paramType.IsAssignableFrom(argType))
                        {
                            break;
                        }
                    }

                    if (i == argumentTypes.Length)
                    {
                        if (methodInfo != null)
                        {
                            Trace.TraceWarning($"[{nameof(MethodBindingExtension)}] Multiple matching methods '{methodName}' (target type '{methodTargetType}') that accept the provided arguments ({GetArgTypesString()}).");
                            return (null, false);
                        }

                        methodInfo = method.Info;

                        // First parameterless method is from most specific subclass so use that since we don't need to do any other overload matching logic.
                        if (i == 0)
                            break;
                    }
                }

                if (methodInfo != null)
                {
                    s_methodInfoCache[(methodTargetType, methodName, argumentTypes)] = methodInfo;
                    return (methodInfo, false);
                }
            }

            if (arguments.Length == 0)
                Trace.TraceWarning($"[{nameof(MethodBindingExtension)}] Could not find parameterless method '{methodName}' (target type '{methodTargetType}').");
            else
                Trace.TraceWarning($"[{nameof(MethodBindingExtension)}] Could not find method '{methodName}' (target type '{methodTargetType}') that accepts the provided arguments ({GetArgTypesString()}).");

            return (null, false);

            string GetArgTypesString() => string.Join(", ", argumentTypes.Select(a => a == null ? "null" : $"'{a}'"));
        }

        private static DependencyProperty SetUnusedStorageProperty(DependencyObject obj, object value)
        {
            lock (s_storageProperties)
            {
                var property = s_storageProperties.Find(p => obj.ReadLocalValue(p) == DependencyProperty.UnsetValue);

                if (property == null)
                {
                    property = DependencyProperty.RegisterAttached("Storage" + s_storageProperties.Count, typeof(object), typeof(MethodBindingExtension), new PropertyMetadata());
                    s_storageProperties.Add(property);
                }

                if (value is MarkupExtension markupExtension)
                {
                    object resolvedValue = markupExtension.ProvideValue(new ServiceProvider(obj, property));
                    obj.SetValue(property, resolvedValue);
                }
                else
                {
                    obj.SetValue(property, value);
                }

                return property;
            }
        }

        private class ServiceProvider : IServiceProvider, IProvideValueTarget
        {
            public object TargetObject { get; }

            public object TargetProperty { get; }

            public ServiceProvider(object targetObject, object targetProperty)
            {
                TargetObject = targetObject;
                TargetProperty = targetProperty;
            }

            public object GetService(Type serviceType) => serviceType.IsInstanceOfType(this) ? this : null;
        }

        private class MethodCacheEqualityComparer : EqualityComparer<(Type TargetType, string MethodName, Type[] ArgTypes)>
        {
            public override bool Equals((Type TargetType, string MethodName, Type[] ArgTypes) x, (Type TargetType, string MethodName, Type[] ArgTypes) y)
            {
                return x.TargetType == y.TargetType && x.ArgTypes.Length == y.ArgTypes.Length && x.MethodName == y.MethodName && x.ArgTypes.SequenceEqual(y.ArgTypes);
            }

            public override int GetHashCode((Type TargetType, string MethodName, Type[] ArgTypes) obj)
            {
                HashCode hashCode = default;
                hashCode.Add(obj.TargetType);
                hashCode.Add(obj.MethodName);
                hashCode.Add(obj.ArgTypes.Length);

                foreach (var value in obj.ArgTypes)
                    hashCode.Add(value);

                return hashCode.ToHashCode();
            }
        }
    }
}