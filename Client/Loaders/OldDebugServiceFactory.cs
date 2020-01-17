using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Reflection;
using System.ServiceModel;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Proryv.AskueARM2.Client.ServiceReference.DeclaratorService;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.StimulReportService;
using Proryv.AskueARM2.Client.ServiceReference.UAService;

namespace Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service
{
    public interface IServiceRecreate
    {
        object Recreate();
        void SwitchToPort80();
    }

    public static class ServiceFactory
    {
        public delegate void ServiceNotFoundEventHandler(Exception e);

        public delegate void CustomExceptionEventHandler(Exception e);

        public static event ServiceNotFoundEventHandler OnServiceNotFoundException;
        public static event CustomExceptionEventHandler OnCustomException;

        public static volatile int IdleMinuteCounter = 0;
        public static bool dataNotActual = false;

        public static bool IsDesignMode = true;
        public static bool ThrowExceptionCallWcf = false;

        private static volatile bool _anyMethodSucceedCall = false;

        public static object InvokeMethod(this IServiceRecreate service, string remoteMethod, params object[] input)
        {
            //#if DEBUG
            //            if (IsDesignMode)
            //            {
            //                try
            //                {
            //                    throw new Exception();
            //                }
            //                catch (Exception ex)
            //                {
            //                    MessageBox.Show("Вызов WCF-метода в design-режиме! " + remoteMethod + "\nStack trace: "+ex.StackTrace);
            //                }
            //                return null;
            //            }
            //#endif
            IdleMinuteCounter = 0;
            retry:
            lock (Services)
            {
                if (!Services.ContainsKey(service))
                {
                    var serviceMethods = new Dictionary<string, MethodInfo>();
                    var methods = service.GetType().GetMethods();
                    foreach (var method in methods)
                        serviceMethods[method.Name] = method;
                    Services[service] = serviceMethods;
                }
            }
            var threadParams = new ThreadParams()
            {
                Service = service,
                MethodName = remoteMethod,
                Params = input
            };
            var thread = new Thread(new ParameterizedThreadStart(AsyncInvoke));
            thread.Start(threadParams);
            thread.Join();
            if (threadParams.Result is Exception)
            {
                if (threadParams.Result is SocketException)
                {
                    try
                    {
                        if (OnServiceNotFoundException != null && remoteMethod != "ALARM_Get_CurrentAlarms_Count" && remoteMethod != "GetServerMemory") //для корректного переключения на 80 порт

                        {
                            OnServiceNotFoundException((SocketException)threadParams.Result); /*throw new FaultException(threadParams.Result.ToString()); */
                        }
                    }
                    finally
                    {
                        lock (Services)
                        {
                            Services.Remove(service);
                            if (!_anyMethodSucceedCall) service.SwitchToPort80();
                            service = service.Recreate() as IServiceRecreate;
                        }
                    }
                    return null;
                }
                if ((threadParams.Result is CommunicationObjectFaultedException) /*||(threadParams.Result is System.ServiceModel.FaultException)*/)
                {
                    lock (Services)
                    {
                        Services.Remove(service);
                        if (!_anyMethodSucceedCall) service.SwitchToPort80();
                        service = service.Recreate() as IServiceRecreate;
                    }
                    goto retry;
                }
                if (OnCustomException != null) OnCustomException(threadParams.Result as Exception);
                if (ThrowExceptionCallWcf)
                    throw threadParams.Result as Exception;
            }
            else
            {
                if (!_anyMethodSucceedCall) _anyMethodSucceedCall = true; //прекращаем переключаться между портами
            }
            return threadParams.Result;

        }

        private class ThreadParams
        {
            public object Service;
            public string MethodName;
            public object[] Params;
            public object Result;
        }

        private static readonly Dictionary<object, Dictionary<string, MethodInfo>> Services = new Dictionary<object, Dictionary<string, MethodInfo>>();

        private static void AsyncInvoke(object threadParams)
        {
            var tp = (ThreadParams)threadParams;
            MethodInfo mi = null;
            lock (Services)
            {
                Dictionary<string, MethodInfo> md;
                if (Services.TryGetValue(tp.Service, out md) && md != null)
                {
                    md.TryGetValue(tp.MethodName, out mi);
                }
            }
            try
            {
                if (mi != null)
                {
                    tp.Result = mi.Invoke(tp.Service, tp.Params);
                }
            }
            catch (AggregateException ex)
            {
                tp.Result = ex.ToException();
            }
            catch (Exception e)
            {
                if (!Equals(tp.MethodName, "GetServerMemory")) tp.Result = e.GetBaseException();
            }
#if DEBUG
            // логирование вызываемых WCF-методов с параметрами и результатом.
            try
            {
                string debug = "=== WCF-метод: " + tp.MethodName + "; параметры: ";
                foreach (var par in tp.Params)
                    debug += ((par == null) ? "NULL" : par.ToString()) + ", ";
                debug += "; результат: " + ((tp.Result == null) ? "NULL" : tp.Result.ToString());
                Debug.WriteLine(debug);
            }
            catch (Exception)
            {
            }
#endif
        }


        private static readonly Dictionary<string, MethodInfo> ArmServiceMethodDict = new Dictionary<string, MethodInfo>();

        /// <summary>
        /// Синхронный вызов метода
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T ArmServiceInvokeSync<T>(string methodName, params object[] args) where T : class
        {
            using (var createdcontext = new ARM_ServiceClient())
            {
                //CancellationTokenSource token;
                //return WCFUltimateDataLoader.PartialLoaderSync<IServiceInvoke, T>(createdcontext, methodName, args, out token);

                createdcontext.Endpoint.Address = new EndpointAddress(ARM_Service.Service.Endpoint.Address.Uri);
                return InvokeSync<T, IARM_Service>(createdcontext, ArmServiceMethodDict, methodName, args);
            }
        }

        private static readonly Dictionary<string, MethodInfo> FreeHierarchyMethodDict = new Dictionary<string, MethodInfo>();

        /// <summary>
        /// Синхронный вызов метода
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T FreeHierarchyInvokeSync<T>(string methodName, params object[] args) where T : class
        {

            using (var createdcontext = new FreeHierarchyServiceClient())
            {
                //CancellationTokenSource token;
                //return WCFUltimateDataLoader.PartialLoaderSync<IServiceInvoke, T>(createdcontext, methodName, args, out token);
                createdcontext.Endpoint.Address = new EndpointAddress(FreeHierarchyService.FreeHierarchyService.Service.Endpoint.Address.Uri);
                return InvokeSync<T, IFreeHierarchyService>(createdcontext, FreeHierarchyMethodDict, methodName, args);
            }
        }


        private static readonly Dictionary<string, MethodInfo> StimulReportMethodDict = new Dictionary<string, MethodInfo>();

        /// <summary>
        /// Синхронный вызов метода
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T StimulReportInvokeSync<T>(string methodName, params object[] args) where T : class
        {
            using (var createdcontext = new StimulReportServiceClient())
            {


                Uri newUrl = new Uri(
                    createdcontext.Endpoint.Address.Uri.ToString()
                        .Replace(createdcontext.Endpoint.Address.Uri.Host, ARM_Service.Service.Endpoint.Address.Uri.Host));
                createdcontext.Endpoint.Address = new EndpointAddress(newUrl);


                return InvokeSync<T, IStimulReportService>(createdcontext, StimulReportMethodDict, methodName, args);
            }
        }


        private static readonly Dictionary<string, MethodInfo> DeclaratorServiceMethodDict = new Dictionary<string, MethodInfo>();

        /// <summary>
        /// Синхронный вызов метода
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T DeclaratorServiceInvokeSync<T>(string methodName, params object[] args) where T : class
        {
            using (var createdcontext = new DeclaratorServiceClient())
            {
                //CancellationTokenSource token;
                //return WCFUltimateDataLoader.PartialLoaderSync<IServiceInvoke, T>(createdcontext, methodName, args, out token);
                createdcontext.Endpoint.Address = new EndpointAddress(DeclaratorService.DeclaratorService.Service.Endpoint.Address.Uri);
                return InvokeSync<T, IDeclaratorService>(createdcontext, DeclaratorServiceMethodDict, methodName, args);
            }
        }

        private static T InvokeSync<T, TChannel>(ClientBase<TChannel> createdcontext, Dictionary<string, MethodInfo> methodDict, string methodName, object[] args)
            where TChannel : class
            where T : class
        {
            lock (methodDict)
            {
                if (methodDict.Count == 0)
                {
                    var methods = createdcontext.GetType().GetMethods();
                    foreach (var method in methods)
                        methodDict[method.Name] = method;
                }
            }

            int errCounter = 0;

            repeat:
            try
            {
                if (createdcontext.State == CommunicationState.Closed ||
                    createdcontext.State == CommunicationState.Closing ||
                    createdcontext.State == CommunicationState.Faulted)
                {
                    throw new Exception("Коммуникационный канал закрыт или занят");
                }

                MethodInfo mi;
                if (methodDict.TryGetValue(methodName, out mi) && mi != null)
                {
#if DEBUG
                    // логирование вызываемых WCF-методов с параметрами и результатом.
                    try
                    {
                        var debug = "=== WCF-метод: " + methodName + "; параметры: ";
                        foreach (var par in args)
                            debug += ((par == null) ? "NULL" : par.ToString()) + ", ";
                        Debug.WriteLine(debug);
                    }
                    catch (Exception)
                    {
                    }
#endif

                    return mi.Invoke(createdcontext, args) as T;
                }

                createdcontext.Close();
                throw new Exception("Вызываемый метод " + methodName + " не найден, или ошибка параметров");
            }
            catch (Exception exception)
            {
                Exception ex;
                if (exception is AggregateException)
                {
                    ex = (exception as AggregateException).ToException();
                }
                else
                {
                    ex = exception.GetBaseException();
                }

                if (ex is SocketException)
                {
                    if (errCounter++ < 5)
                    {
                        //Пытаемся еще несколько раз прочитать данные
                        Thread.Sleep(200);
                        goto repeat;
                    }

                    if (OnServiceNotFoundException != null)
                    {
                        OnServiceNotFoundException(ex as SocketException);
                    }
                }

                throw ex;
            }
        }
    }

    public enum EnumServiceType
    {
        ArmService = 0,
        FreeHierarchy = 1,
        StimulReport = 2,
    }
}
