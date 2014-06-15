using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Rohmote
{
    public static class RpcProcessorExtensions
    {
        public static void On<TResult>(this RpcProcessor rpc, string method, Func<Task<TResult>> handler)
        {
            rpc.Register(method, async (sender, parameters) =>
            {
                CheckParameters(method, parameters, 0);
                var result = await handler();
                return ToReturn(method, result);
            });
        }

        public static void On<TResult>(this RpcProcessor rpc, string method, Func<TResult> handler)
        {
            rpc.On(method, async () => await Task.Run(handler));
        }

        public static void On<T, TResult>(this RpcProcessor rpc, string method, Func<T, Task<TResult>> handler)
        {
            rpc.Register(method, async (sender, parameters) =>
            {
                CheckParameters(method, parameters, 1);

                var result = await handler(
                    FromParameters<T>(method, parameters, 0));

                return ToReturn(method, result);
            });
        }

        public static void On<T, TResult>(this RpcProcessor rpc, string method, Func<T, TResult> handler)
        {
            rpc.On(method, async (T arg1) => await Task.Run(() => handler(arg1)));
        }

        public static void On<T1, T2, TResult>(this RpcProcessor rpc, string method, Func<T1, T2, Task<TResult>> handler)
        {
            rpc.Register(method, async (sender, parameters) =>
            {
                CheckParameters(method, parameters, 2);

                var result = await handler(
                    FromParameters<T1>(method, parameters, 0),
                    FromParameters<T2>(method, parameters, 1));

                return ToReturn(method, result);
            });
        }

        public static void On<T1, T2, TResult>(this RpcProcessor rpc, string method, Func<T1, T2, TResult> handler)
        {
            rpc.On(method, async (T1 arg1, T2 arg2) => await Task.Run(() => handler(arg1, arg2)));
        }

        public static void On<T1, T2, T3, TResult>(this RpcProcessor rpc, string method, Func<T1, T2, T3, Task<TResult>> handler)
        {
            rpc.Register(method, async (sender, parameters) =>
            {
                CheckParameters(method, parameters, 3);

                var result = await handler(
                    FromParameters<T1>(method, parameters, 0),
                    FromParameters<T2>(method, parameters, 1),
                    FromParameters<T3>(method, parameters, 2));

                return ToReturn(method, result);
            });
        }

        public static void On<T1, T2, T3, TResult>(this RpcProcessor rpc, string method, Func<T1, T2, T3, TResult> handler)
        {
            rpc.On(method, async (T1 arg1, T2 arg2, T3 arg3) => await Task.Run(() => handler(arg1, arg2, arg3)));
        }

        public static void On<T1, T2, T3, T4, TResult>(this RpcProcessor rpc, string method, Func<T1, T2, T3, T4, Task<TResult>> handler)
        {
            rpc.Register(method, async (sender, parameters) =>
            {
                CheckParameters(method, parameters, 4);

                var result = await handler(
                    FromParameters<T1>(method, parameters, 0),
                    FromParameters<T2>(method, parameters, 1),
                    FromParameters<T3>(method, parameters, 2),
                    FromParameters<T4>(method, parameters, 3));

                return ToReturn(method, result);
            });
        }

        public static void On<T1, T2, T3, T4, TResult>(this RpcProcessor rpc, string method, Func<T1, T2, T3, T4, TResult> handler)
        {
            rpc.On(method, async (T1 arg1, T2 arg2, T3 arg3, T4 arg4) => await Task.Run(() => handler(arg1, arg2, arg3, arg4)));
        }

        public static async Task<TResult> Call<TResult>(this RpcProcessor rpc, string method)
        {
            var result = await rpc.Invoke(method, new JToken[0]);
            return FromReturn<TResult>(method, result);
        }

        public static async Task<TResult> Call<T, TResult>(this RpcProcessor rpc, string method, T arg1)
        {
            var parameters = new[]
            {
                ToParameter(method, arg1)
            };

            var result = await rpc.Invoke(method, parameters);
            return FromReturn<TResult>(method, result);
        }

        public static async Task<TResult> Call<T1, T2, TResult>(this RpcProcessor rpc, string method, T1 arg1, T2 arg2)
        {
            var parameters = new[]
            {
                ToParameter(method, arg1),
                ToParameter(method, arg2)
            };

            var result = await rpc.Invoke(method, parameters);
            return FromReturn<TResult>(method, result);
        }

        public static async Task<TResult> Call<T1, T2, T3, TResult>(this RpcProcessor rpc, string method, T1 arg1, T2 arg2, T3 arg3)
        {
            var parameters = new[]
            {
                ToParameter(method, arg1),
                ToParameter(method, arg2),
                ToParameter(method, arg3)
            };

            var result = await rpc.Invoke(method, parameters);
            return FromReturn<TResult>(method, result);
        }

        public static async Task<TResult> Call<T1, T2, T3, T4, TResult>(this RpcProcessor rpc, string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var parameters = new[]
            {
                ToParameter(method, arg1),
                ToParameter(method, arg2),
                ToParameter(method, arg3),
                ToParameter(method, arg4)
            };

            var result = await rpc.Invoke(method, parameters);
            return FromReturn<TResult>(method, result);
        }

        private static void CheckParameters(string method, JToken[] parameters, int expectedLength)
        {
            if (parameters.Length != expectedLength)
                throw new InvalidOperationException(string.Format("Method '{0}' requires {1} arguments", method, expectedLength));
        }

        // for On
        private static T FromParameters<T>(string method, JToken[] parameters, int index)
        {
            try
            {
                return parameters[index].ToObject<T>();
            }
            catch
            {
                throw new InvalidOperationException(string.Format("Argument {0} for method '{1}' must be of type '{2}'", index + 1, method, typeof(T).FullName));
            }
        }
        
        // for On
        private static JToken ToReturn<T>(string method, T value)
        {
            return JToken.FromObject(value); // TODO: should this be error checked?
        }

        // for Call
        private static JToken ToParameter<T>(string method, T value)
        {
            return JToken.FromObject(value); // TODO: should this be error checked?
        }

        // for Call
        private static T FromReturn<T>(string method, JToken value)
        {
            try
            {
                return value.ToObject<T>();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Return value for method '{0}' must be of type '{1}'", method, typeof(T).FullName));
            }
        }
    }
}
