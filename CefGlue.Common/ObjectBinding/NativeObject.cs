using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xilium.CefGlue.Common.Events;

namespace Xilium.CefGlue.Common.ObjectBinding
{
    internal class NativeObject
    {
        private readonly object _target;
        private readonly IDictionary<string, NativeMethod> _methods;
        private readonly object _methodHandlerTarget;
        private readonly NativeMethod _methodHandler;
        
        public NativeObject(string name, object target, MethodCallHandler methodHandler = null)
        {
            Name = name;
            _target = target;
            _methods = GetObjectMembers(target);

            if (methodHandler != null)
            {
                _methodHandler = new NativeMethod(methodHandler.Method);
                _methodHandlerTarget = methodHandler.Target;
            }
        }

        public string Name { get; }

        public IEnumerable<string> MethodsNames => _methods.Keys;

        public void ExecuteMethod(string methodName, object[] args, Action<object, Exception> handleResult)
        {
            InnerExecuteMethod(methodName, args, handleResult);
        }

        public void ExecuteMethod(string methodName, string argsAsJson, Action<object, Exception> handleResult)
        {
            InnerExecuteMethod(methodName, argsAsJson, handleResult);
        }

        public void ExecuteMethod(string methodName, string argsAsJson, byte[][] binaryArgs, Action<object, Exception> handleResult)
        {
            InnerExecuteMethod(methodName, argsAsJson, binaryArgs, handleResult);
        }

        private void InnerExecuteMethod<T>(string methodName, T args, Action<object, Exception> handleResult)
        {
            if (!_methods.TryGetValue(methodName ?? "", out var method))
            {
                handleResult(default, new Exception($"Object does not have a {methodName} method."));
                return;
            }

            if (_methodHandler == null)
            {
                method.Execute(_target, args, handleResult);
                return;
            }

            var innerMethod = method.MakeDelegate(_target, args);
            _methodHandler.Execute(_methodHandlerTarget, innerMethod, (result, exception) =>
            {
                if (result is Task task)
                {
                    task.ContinueWith(t =>
                    {
                        var taskResult = GenericTaskAwaiter.GetResultFrom(t);
                        handleResult(taskResult.Result, taskResult.Exception);
                    });
                    return;
                }

                handleResult(result, exception);
            });
        }

        /// <summary>
        /// 执行方法，支持二进制参数（原生 SetBinary 传输）。
        /// </summary>
        private void InnerExecuteMethod(string methodName, string argsAsJson, byte[][] binaryArgs, Action<object, Exception> handleResult)
        {
            if (!_methods.TryGetValue(methodName ?? "", out var method))
            {
                handleResult(default, new Exception($"Object does not have a {methodName} method."));
                return;
            }

            if (_methodHandler == null)
            {
                method.Execute(_target, argsAsJson, binaryArgs, handleResult);
                return;
            }

            // 如果用户自定义了 methodHandler，需要先将二进制参数以 base64 注入 JSON（MakeDelegate 闭包需要数据嵌入在 JSON 中）
            var replacedJson = NativeMethod.ReplaceBinaryPlaceholdersWithBase64(argsAsJson, binaryArgs);
            var innerMethod = method.MakeDelegate(_target, replacedJson);
            _methodHandler.Execute(_methodHandlerTarget, innerMethod, (result, exception) =>
            {
                if (result is Task task)
                {
                    task.ContinueWith(t =>
                    {
                        var taskResult = GenericTaskAwaiter.GetResultFrom(t);
                        handleResult(taskResult.Result, taskResult.Exception);
                    });
                    return;
                }

                handleResult(result, exception);
            });
        }

        private static IDictionary<string, NativeMethod> GetObjectMembers(object obj)
        {
            var methods = obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => !m.IsSpecialName);
            return methods.ToDictionary(m => ToJavascriptMemberName(m.Name), m => new NativeMethod(m));
        }

        private static string ToJavascriptMemberName(string name) =>
            name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
    }
}
