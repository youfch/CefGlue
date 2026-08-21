using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xilium.CefGlue.Common.Shared.RendererProcessCommunication;
using Xilium.CefGlue.Common.Shared.Serialization;

namespace Xilium.CefGlue.BrowserProcess.ObjectBinding
{
    internal class V8FunctionHandler : CefV8Handler
    {
        private readonly string _objectName;
        private readonly Func<Messages.NativeObjectCallRequest, PromiseHolder> _functionCallHandler;

        public V8FunctionHandler(string objectName, Func<Messages.NativeObjectCallRequest, PromiseHolder> functionCallHandler)
        {
            _objectName = objectName;
            _functionCallHandler = functionCallHandler;
        }

        protected override bool Execute(string name, CefV8Value obj, CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
        {
            var message = new Messages.NativeObjectCallRequest()
            {
                ObjectName = _objectName,
                MemberName = name
            };

            // 兼容性：无二进制参数时，走旧的单 JSON 字符串路径
            if (arguments.Length == 1 && arguments[0].IsString)
            {
                message.ArgumentsAsJson = arguments[0].GetStringValue();
            }
            else if (arguments.Length > 0)
            {
                // 新路径：arguments[0] 是 JSON 字符串，其后可能跟随 N 个 Uint8Array 二进制参数
                message.ArgumentsAsJson = arguments[0]?.GetStringValue() ?? string.Empty;

                var binaryArgs = new List<byte[]>();
                for (int i = 1; i < arguments.Length; i++)
                {
                    var arg = arguments[i];
                    if (arg != null && arg.IsArrayBuffer)
                    {
                        var data = ExtractArrayBufferBytes(arg);
                        binaryArgs.Add(data);
                    }
                }

                if (binaryArgs.Count > 0)
                {
                    message.BinaryArguments = binaryArgs.ToArray();
                }
            }
            else
            {
                message.ArgumentsAsJson = string.Empty;
            }

            var promiseHolder = _functionCallHandler(message);

            if (promiseHolder != null)
            {
                returnValue = promiseHolder.Promise;
                exception = null;
            }
            else
            {
                returnValue = null;
                exception = "Failed to create promise";
            }

            return true;
        }

        /// <summary>
        /// 从 CEF V8 ArrayBuffer 中提取原始字节，避免 base64 编解码。
        /// </summary>
        private static byte[] ExtractArrayBufferBytes(CefV8Value arg)
        {
            var length = (int)arg.GetArrayBufferByteLength();
            if (length <= 0) return Array.Empty<byte>();

            var dataPtr = arg.GetArrayBufferData();
            if (dataPtr == IntPtr.Zero) return Array.Empty<byte>();

            var buffer = new byte[length];
            Marshal.Copy(dataPtr, buffer, 0, length);
            return buffer;
        }
    }
}
