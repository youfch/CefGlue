using System;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Xilium.CefGlue.Common.ObjectBinding;
using Xilium.CefGlue.Common.Shared.Serialization;

namespace CefGlue.Tests.Javascript
{
    public class NativeObjectMethodExecutorTests
    {
        class Token
        {
            public bool Executed;
        }
        
        class NativeTestObject
        {
            public object MethodWithParams(string param1, int param2)
            {
                return new object[] { param1, param2 };
            }

            public string MethodWithReturn()
            {
                return "this is the result";
            }
            
            public Task AsyncMethod(Token token)
            {
                token.Executed = true;
                return Task.CompletedTask;
            }
            
            public Task<string> AsyncMethodWithReturn()
            {
                return Task.FromResult("this is the result");
            }

            public object[] MethodWithOptionalParams(params string[] optionalParams)
            {
                return optionalParams;
            }

            public object[] MethodWithFixedAndOptionalParams(string fixedParam, params int[] optionalParams)
            {
                return new object[] { fixedParam, optionalParams };
            }

            public byte[] MethodWithBinaryParam(byte[] data)
            {
                return data;
            }

            public object[] MethodWithBinaryAndStringParam(string text, byte[] data)
            {
                return new object[] { text, data };
            }

            public object[] MethodWithTwoBinaryParams(byte[] first, byte[] second)
            {
                return new object[] { first, second };
            }
        }

        private NativeTestObject nativeTestObject = new NativeTestObject();
        private NativeObject nativeObject;

        [OneTimeSetUp]
        protected void Setup()
        {
            nativeObject = new NativeObject("test", nativeTestObject);
        }

        private object ExecuteMethod(string name, object[] args)
        {
            object result = null;
            Exception exception = null;
            nativeObject.ExecuteMethod(name, args, (r, e) =>
            {
                result = r;
                exception = e;
            });
            if (exception != null)
            {
                throw exception;
            }
            return result;
        }
        
        private Task<object> ExecuteAsyncMethod(string name, object[] args)
        {
            var tcs = new TaskCompletionSource<object>();
            nativeObject.ExecuteMethod(name, args, (r, e) =>
            {
                if (e != null)
                {
                    tcs.SetException(e);
                } 
                else
                {
                    tcs.SetResult(r);
                }
            });
            return tcs.Task;
        }

        [Test]
        public void MethodWithReturnIsExecuted()
        {
            var result = ExecuteMethod("methodWithReturn", new object[0]);
            Assert.AreEqual(nativeTestObject.MethodWithReturn(), result);
        }

        [Test]
        public void AsyncMethodIsExecuted()
        {
            var token = new Token();
            var result = ExecuteAsyncMethod("asyncMethod", new [] { token });
            Assert.IsNull(result.Result);
            Assert.IsTrue(token.Executed);
        }
        
        [Test]
        public void AsyncMethodWithReturnIsExecuted()
        {
            var result = ExecuteAsyncMethod("asyncMethodWithReturn", new object[0]);
            Assert.AreEqual(nativeTestObject.AsyncMethodWithReturn().Result, result.Result);
        }

        [Test]
        public void MethodWithParamsIsExecuted()
        {
            const string Arg1 = "arg1";
            const int Arg2 = 2;
            var result = (object[]) ExecuteMethod("methodWithParams", new object[] { Arg1, Arg2 } );

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(Arg1, result[0]);
            Assert.AreEqual(Arg2, result[1]);
        }

        [Test]
        public void MethodWithOptionalParamsIsExecuted()
        {
            const string Arg1 = "arg1";
            const string Arg2 = "arg2";
            var result = (object[])ExecuteMethod("methodWithOptionalParams", new object[] { Arg1, Arg2 });

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(Arg1, result[0]);
            Assert.AreEqual(Arg2, result[1]);

            result = (object[])ExecuteMethod("methodWithOptionalParams", new object[0]);

            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void MethodWithFixedAndOptionalParamsIsExecuted()
        {
            const string Arg1 = "arg1";
            var arg2 = new int[] { 1, 2 , 3 };
            var result = (object[])ExecuteMethod("methodWithFixedAndOptionalParams", new object[] { Arg1, (int)1, (int)2, (int)3 });

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(Arg1, result[0]);
            CollectionAssert.AreEqual(arg2, (IEnumerable) result[1]);

            result = (object[])ExecuteMethod("methodWithFixedAndOptionalParams", new object[] { Arg1 });

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(Arg1, result[0]);
            Assert.AreEqual(0, ((int[]) result[1]).Length);
        }

        // ---- Binary parameter tests ----

        [Test]
        public void MethodWithBinaryParamIsExecuted()
        {
            var data = new byte[] { 0, 1, 2, 255 };
            var argsJson = "[\"__BINARY_0__\"]";
            byte[][] binaryArgs = new byte[][] { data };

            object result = null;
            Exception exception = null;
            nativeObject.ExecuteMethod("methodWithBinaryParam", argsJson, binaryArgs, (r, e) => { result = r; exception = e; });
            if (exception != null) throw exception;

            CollectionAssert.AreEqual(data, (IEnumerable)result);
        }

        [Test]
        public void MethodWithBinaryAndStringParamIsExecuted()
        {
            var data = new byte[] { 10, 20, 30 };
            var argsJson = "[\"text\",\"__BINARY_0__\"]";
            byte[][] binaryArgs = new byte[][] { data };

            object result = null;
            Exception exception = null;
            nativeObject.ExecuteMethod("methodWithBinaryAndStringParam", argsJson, binaryArgs, (r, e) => { result = r; exception = e; });
            if (exception != null) throw exception;

            var arr = (object[])result;
            Assert.AreEqual("text", arr[0]);
            CollectionAssert.AreEqual(data, (IEnumerable)arr[1]);
        }

        [Test]
        public void MethodWithTwoBinaryParamsIsExecuted()
        {
            var first = new byte[] { 1, 2, 3 };
            var second = new byte[] { 4, 5, 6 };
            var argsJson = "[\"__BINARY_0__\",\"__BINARY_1__\"]";
            byte[][] binaryArgs = new byte[][] { first, second };

            object result = null;
            Exception exception = null;
            nativeObject.ExecuteMethod("methodWithTwoBinaryParams", argsJson, binaryArgs, (r, e) => { result = r; exception = e; });
            if (exception != null) throw exception;

            var arr = (object[])result;
            CollectionAssert.AreEqual(first, (IEnumerable)arr[0]);
            CollectionAssert.AreEqual(second, (IEnumerable)arr[1]);
        }

        [Test]
        public void MethodWithEmptyJsonStringIsExecuted()
        {
            // 模拟无参数调用时 ArgumentsAsJson 为 "" 的场景
            object result = null;
            Exception exception = null;
            nativeObject.ExecuteMethod("methodWithReturn", "", (r, e) => { result = r; exception = e; });
            if (exception != null) throw exception;
            Assert.AreEqual(nativeTestObject.MethodWithReturn(), result);
        }

        [Test]
        public void MethodWithNullJsonStringIsExecuted()
        {
            // 模拟极端情况：ArgumentsAsJson 为 null
            object result = null;
            Exception exception = null;
            nativeObject.ExecuteMethod("methodWithReturn", (string)null, (r, e) => { result = r; exception = e; });
            if (exception != null) throw exception;
            Assert.AreEqual(nativeTestObject.MethodWithReturn(), result);
        }
    }
}
