using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Xilium.CefGlue.Common.Shared.Serialization;

namespace Xilium.CefGlue.Common.ObjectBinding
{
    internal class NativeMethod
    {
        private readonly MethodInfo _methodInfo;
        private readonly Type[] _parameterTypes;
        private readonly int _mandatoryParametersCount;
        private readonly bool _hasOptionalParameters;
        
        public NativeMethod(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;

            var parameters = methodInfo.GetParameters();
            _hasOptionalParameters = IsParamArray(parameters.LastOrDefault());
            _mandatoryParametersCount = parameters.Length - (_hasOptionalParameters ? 1 : 0);
            var parameterTypes = parameters
                .Take(_mandatoryParametersCount)
                .Select(p => p.ParameterType);
            if (_hasOptionalParameters)
            {
                parameterTypes = parameterTypes.Append(parameters.Last().ParameterType.GetElementType());
            }
            _parameterTypes = parameterTypes.ToArray();
        }
        
        public Func<object> MakeDelegate<T>(object targetObj, T args)
        {
            var convertedArgs = ConvertArguments(args);
            return () => ExecuteMethod(targetObj, convertedArgs);
        }

        public void Execute<T>(object targetObj, T args, Action<object, Exception> handleResult)
        {
            Execute(targetObj, ConvertArguments(args), handleResult);
        }

        /// <summary>
        /// 执行方法，支持二进制参数（通过 __BINARY_N__ 占位符替换）。
        /// 将占位符替换为 null 走 JSON 反序列化，再注入真实 byte[]，避免 base64 编解码。
        /// </summary>
        public void Execute(object targetObj, string argsJson, byte[][] binaryArgs, Action<object, Exception> handleResult)
        {
            var args = InnerConvertArguments(argsJson, binaryArgs);
            Execute(targetObj, args, handleResult);
        }

        public void Execute(object targetObj, Func<object> innerMethod, Action<object, Exception> handleResult)
        {
            Execute(targetObj, new[] { innerMethod }, handleResult);
        }

        public void Execute(object targetObj, object[] args, Action<object, Exception> handleResult)
        {
            object result = null;
            Exception exception = null;
            try
            {
                result = ExecuteMethod(targetObj, args);
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (result is Task task)
            {
                task.ContinueWith(t =>
                {
                    var taskResult = GenericTaskAwaiter.GetResultFrom(t);
                    handleResult(taskResult.Result, taskResult.Exception);
                });
                return;
            }

            // convertedArgumentsWithOptionals/exception is available
            handleResult(result, exception);
        }

        private object ExecuteMethod(object targetObj, object[] args)
        {
            try
            {
                // TODO improve call perf
                return _methodInfo.Invoke(targetObj, args);
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException ?? e).Throw();
                return null;
            }
        }

        private object[] ConvertArguments<T>(T args)
        {
            var argsAsObject = (object)args;
            
            if (typeof(T) == typeof(string)) {
                return InnerConvertArguments((string)argsAsObject);
            }

            return ConvertArgumentsWithOptionals((object[])argsAsObject);
        }

        private object[] InnerConvertArguments(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                var convertedArguments = Array.Empty<object>();

                ValidateMandatoryArguments(convertedArguments);
                return convertedArguments;
            }

            var originalArguments = Deserializer.Deserialize(args, _parameterTypes);

            return ConvertArgumentsWithOptionals(originalArguments);
        }

        /// <summary>
        /// 反序列化 JSON 参数，并将 __BINARY_N__ 占位符替换为真实 byte[] 数据。
        /// 避免 base64 编解码：将占位符替换为 null 走 JSON 反序列化，再注入真实 byte[]。
        /// </summary>
        private object[] InnerConvertArguments(string args, byte[][] binaryArgs)
        {
            if (string.IsNullOrEmpty(args))
            {
                var convertedArguments = Array.Empty<object>();
                ValidateMandatoryArguments(convertedArguments);
                return convertedArguments;
            }

            // 将 __BINARY_N__ 占位符替换为 null，避免 base64 编解码
            var cleanedJson = ReplaceBinaryPlaceholders(args, binaryArgs);
            var originalArguments = Deserializer.Deserialize(cleanedJson, _parameterTypes);

            // 将 null 替换为真实的 byte[] 数据（按占位符出现顺序注入）
            if (binaryArgs != null && binaryArgs.Length > 0)
            {
                int binaryIdx = 0;
                for (int i = 0; i < originalArguments.Length && binaryIdx < binaryArgs.Length; i++)
                {
                    if (originalArguments[i] == null)
                    {
                        originalArguments[i] = binaryArgs[binaryIdx++];
                    }
                }
            }

            return ConvertArgumentsWithOptionals(originalArguments);
        }

        /// <summary>
        /// 将 JSON 中的 __BINARY_N__ 占位符替换为 null，用于无 base64 的二进制参数注入路径。
        /// 替换后由调用方在反序列化结果中注入真实 byte[] 数据。
        /// </summary>
        internal static string ReplaceBinaryPlaceholders(string argsJson, byte[][] binaryArgs)
        {
            if (binaryArgs == null || binaryArgs.Length == 0 || string.IsNullOrEmpty(argsJson))
                return argsJson;

            if (!argsJson.Contains("__BINARY_"))
                return argsJson;

            for (int i = 0; i < binaryArgs.Length; i++)
            {
                var placeholder = $"\"__BINARY_{i}__\"";
                argsJson = argsJson.Replace(placeholder, "null");
            }
            return argsJson;
        }

        /// <summary>
        /// 将 JSON 中的 __BINARY_N__ 占位符替换为 B&lt;base64&gt; 格式。
        /// 仅在 _methodHandler 路径中使用（MakeDelegate 闭包需要数据嵌入 JSON）。
        /// </summary>
        internal static string ReplaceBinaryPlaceholdersWithBase64(string argsJson, byte[][] binaryArgs)
        {
            if (binaryArgs == null || binaryArgs.Length == 0 || string.IsNullOrEmpty(argsJson))
                return argsJson;

            if (!argsJson.Contains("__BINARY_"))
                return argsJson;

            for (int i = 0; i < binaryArgs.Length; i++)
            {
                var placeholder = $"\"__BINARY_{i}__\"";
                var replacement = $"\"B{Convert.ToBase64String(binaryArgs[i])}\"";
                argsJson = argsJson.Replace(placeholder, replacement);
            }
            return argsJson;
        }

        private object[] ConvertArgumentsWithOptionals(object[] originalArguments)
        {
            ValidateMandatoryArguments(originalArguments);

            if (!_hasOptionalParameters)
            {
                return originalArguments;
            }

            // the optionalParameterType is always a ParamArray of the last type in the ParameterTypes (eg int[])
            var convertedArgumentsWithOptionals = new object[_parameterTypes.Length];
            Array.Copy(originalArguments, convertedArgumentsWithOptionals, _mandatoryParametersCount);
            
            var optionalArgsCount = originalArguments.Length - _mandatoryParametersCount;
            var optionalParamType = _parameterTypes.Last();
            var optionalArgsArray = Array.CreateInstance(optionalParamType, optionalArgsCount);

            Array.Copy(originalArguments, _mandatoryParametersCount, optionalArgsArray, 0, optionalArgsCount);
            convertedArgumentsWithOptionals[_parameterTypes.Length - 1] = optionalArgsArray;

            return convertedArgumentsWithOptionals;
        }

        private void ValidateMandatoryArguments(object[] originalArguments)
        {
            if (originalArguments.Length < _mandatoryParametersCount)
            {
                throw new ArgumentException($"Number of original arguments provided does not match the number of {_methodInfo.Name} method required parameters.");
            }
        }

        private static bool IsParamArray(ParameterInfo paramInfo)
        {
            return paramInfo?.GetCustomAttribute(typeof(ParamArrayAttribute), false) != null;
        }
    }
}