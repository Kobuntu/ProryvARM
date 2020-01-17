using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Proryv.AskueARM2.Client.ServiceReference.Service;

namespace Proryv.AskueARM2.Client.ServiceReference.UAService
{
    /// <summary>
    ///     Класс может грузить любой объем данных с WCF и на WCF
    /// </summary>
    public static class WCFUltimateDataLoader
    {
        private static readonly object SyncInitializer = new object();
        private static bool _isInitializet;

        public static bool _useProtoSerializer;

        /// <summary>
        ///     Стадии выполнения
        /// </summary>
        public enum ServiceInvokeState
        {
            [Description("Инициализация")]
            Initialization,
            [Description("Отправка данных на сервер")]
            SendData,
            [Description("Выполнение операции на сервере")]
            WaitResult,
            [Description("Загрузка результата операции")]
            ReciveData,
            [Description("Выполнено")]
            Finish,
            [Description("Ошибка")]
            Error,
            [Description("Операция была отменена")]
            Canceled
        }

        /// <summary>
        ///     Размер байт после которого результат будет разбиваться на части
        /// </summary>
        private const long PartialThreshold = 1024; //1024 байт

        /// <summary>
        ///     Выполнить метод на сервере с загрузкой данных по частям и поддержкой отмены
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="context">ARM_Service_OPCUAClient</param>
        /// <param name="methodName">Метод в виде () => UAService.Service.SomeMethod(SomeArgument)</param>
        /// <param name="successMethod">Что делать с результатом</param>
        /// <param name="errorMethod">В случае если ошибка</param>
        /// <returns></returns>
        public static T PartialLoaderAsync<T, TResult>(this T context, Expression<Func<TResult>> methodName,
            Action<TResult> successMethod, Action<Exception> errorMethod)
            where T : IServiceInvoke
        {
            var cts = new CancellationTokenSource();
            return PartialLoaderAsync(context, methodName, successMethod, errorMethod,
                out cts, args => { });
        }


        /// <summary>
        ///     Выполнить метод на сервере с загрузкой данных по частям и поддержкой отмены
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="context">ARM_Service_OPCUAClient</param>
        /// <param name="methodName">Метод в виде () => UAService.Service.SomeMethod(SomeArgument)</param>
        /// <param name="successMethod">Что делать с результатом</param>
        /// <param name="errorMethod">В случае если ошибка</param>
        /// <param name="tokenSource">CancellationTokenSource для отмены выполнения операции</param>
        /// <returns></returns>
        public static T PartialLoaderAsync<T, TResult>(this T context, Expression<Func<TResult>> methodName,
            Action<TResult> successMethod, Action<Exception> errorMethod, out CancellationTokenSource tokenSource)
            where T : IServiceInvoke
        {
            return PartialLoaderAsync(context, methodName, successMethod, errorMethod,
                out tokenSource, args => { });
        }




        /// <summary>
        /// Выполнить метод на сервере с загрузкой данных по частям и поддержкой отмены
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="createdcontextervice_OPCUAClient
        /// </param>
        /// <param name="methodName">Метод в виде () => UAService.Service.SomeMethod(SomeArgument)</param>
        /// <param name="successMethod">Что делать с результатом</param>
        /// <param name="errorMethod">В случае если ошибка</param>
        /// <param name="tokenSource">CancellationTokenSource для отмены выполнения операции</param>
        /// <param name="statusChangedMethod">Обработка изменения статуса загрузки</param>
        /// <returns></returns>
        public static T PartialLoaderAsync<T, TResult>(this T context, Expression<Func<TResult>> methodName,
            Action<TResult> successMethod, Action<Exception> errorMethod, out CancellationTokenSource tokenSource,
            Action<ServiceInvokeStatusChangedArgs> statusChangedMethod) where T : IServiceInvoke
        {
            Init();

            tokenSource = null;
            tokenSource = new CancellationTokenSource();
            CancellationTokenSource cts = tokenSource;
            var invokeState = new ServiceInvokeStatusChangedArgs(tokenSource);

            IServiceInvoke createdcontext = context.GetClient();


            cts.Token.ThrowIfCancellationRequested();
            invokeState.State = ServiceInvokeState.Initialization;
            statusChangedMethod(invokeState);

            //Контекст синхронизации который потом понадобится для того что бы выполнить методы из этого контекста
            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            SynchronizationContext syncContext = SynchronizationContext.Current;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Highest;


                    //Получили название метода и аргументы
                    var body = methodName.Body as MethodCallExpression;
                    if (body == null) throw new Exception("Неизвестный метод службы UAService");
                    Debug.WriteLine("Start InvokeService " + body.Method.Name + " method");
                    //Получаем параметры
                    object[] parameters = body.Arguments.ToArray();


                    //Серилизуем парметры
                    List<KeyValuePair<string, string>> serializedParms = SerializeParamsToList(parameters);


                    //Сжимаем параметры
                    MemoryStream zippedparams =
                        CompressUtility.CompressGZip(
                            new MemoryStream(
                                Encoding.UTF8.GetBytes(
                                    serializedParms.SerializeToString<List<KeyValuePair<string, string>>>())));

                    cts.Token.ThrowIfCancellationRequested();
                    invokeState.State = ServiceInvokeState.SendData;
                    statusChangedMethod(invokeState);

                    //Тут проверяем нужно ли разбивать передаваемые аргументы 
                    if (zippedparams.Length > PartialThreshold)
                    {
                        //TODO: Надо разбить на части, затем загрузить на сервер и после этого выполнить метод
                    }
                    byte[] task;
                    cts.Token.ThrowIfCancellationRequested();
                    invokeState.State = ServiceInvokeState.WaitResult;
                    if (syncContext != null) syncContext.Send(d => statusChangedMethod(invokeState), null);

                    if (createdcontext.State == CommunicationState.Closed ||
                        createdcontext.State == CommunicationState.Closing ||
                        createdcontext.State == CommunicationState.Faulted)
                    {
                        createdcontext = createdcontext.Recreate() as IServiceInvoke;
                    }

                    cts.Token.ThrowIfCancellationRequested();
                    bool[] aborted = { false };
                    Task.Factory.StartNew(() =>
                    {
                        while (!aborted[0])
                        {
                            Thread.Sleep(1000);
                            if (cts.IsCancellationRequested)
                            {
                                createdcontext.Abort();
                                aborted[0] = true;
                                invokeState.State = ServiceInvokeState.Canceled;
                                if (syncContext != null) syncContext.Send(d => statusChangedMethod(invokeState), null);
                                if (syncContext != null)
                                    syncContext.Send(state => errorMethod(state as Exception),
                                        new OperationCanceledException());
                            }
                        }
                    });


                    try
                    {
                        byte[] arrofpar = zippedparams.ToArray();
                        task = createdcontext.InvokeAndCompress(body.Method.Name, _useProtoSerializer, arrofpar);
                        arrofpar = new byte[0];
                        arrofpar = null;
                    }
                    catch (Exception exception)
                    {
                        if (aborted[0])
                        {
                            invokeState.State = ServiceInvokeState.Canceled;
                            if (syncContext != null) syncContext.Send(d => statusChangedMethod(invokeState), null);
                            if (syncContext != null)
                                syncContext.Send(state => errorMethod(state as Exception), exception);
                            return;
                        }
                        throw exception;
                    }

                    if (task == null) throw new Exception("Вызываемый метод не определен в ARM_Service_OPCUAClient!");

                    aborted[0] = true;


                    zippedparams = new MemoryStream();
                    zippedparams = null;


                    Stream serializedobjstring = new MemoryStream();
                    invokeState.State = ServiceInvokeState.WaitResult;
                    statusChangedMethod(invokeState);
                    if (CompressUtility.IsCompressed(task))
                    {
                        serializedobjstring = CompressUtility.DecompressGZip(new MemoryStream(task));

                    }
                    else
                    {
                        serializedobjstring = new MemoryStream(task);
                    }
                    cts.Token.ThrowIfCancellationRequested();
                    //Значит большая часть нужно грузить отдельно
                    if (WaiterHelper.IsWaiterInfoStream(serializedobjstring))
                    {
                        var waiter = (WaiterInfo)serializedobjstring.DeserializeFromByteArray(typeof(WaiterInfo), false);


                        invokeState.State = ServiceInvokeState.ReciveData;
                        invokeState.DownloadCurrentPart = 1;
                        invokeState.DownloadTotalPartsCount = waiter.PartsCount;
                        if (syncContext != null) syncContext.Send(d => statusChangedMethod(invokeState), null);


                        var parts = new ConcurrentStack<byte[]>();
                        for (int i = 0; i < waiter.PartsCount; i++)
                        {
                            try
                            {
                                byte[] fileBytes = createdcontext.GetWaiterNextPart(waiter.WaiterId);

                                parts.Push(fileBytes);
                                if (cts.Token.IsCancellationRequested)
                                {
                                    createdcontext.CancelWaiterById(waiter.WaiterId);
                                    cts.Token.ThrowIfCancellationRequested();
                                }
                                invokeState.DownloadCurrentPart = i + 1;
                                if (syncContext != null) syncContext.Send(d => statusChangedMethod(invokeState), null);
                            }
                            catch (Exception exception)
                            {
                                invokeState.State = ServiceInvokeState.Error;
                                if (syncContext != null) syncContext.Send(d => statusChangedMethod(invokeState), null);
                                if (syncContext != null)
                                    syncContext.Send(state => errorMethod(state as Exception), exception);
                            }
                        }

                        using (var fs = new MemoryStream())
                        {
                            List<byte[]> s = parts.ToList();
                            foreach (var bytese in s)
                            {
                                fs.Write(bytese, 0, bytese.Length);
                            }

                            #region clearMemory

                            parts.Clear();
                            parts = new ConcurrentStack<byte[]>();
                            parts = null;
                            s.Clear();
                            s = new List<byte[]>();
                            s = null;
                            createdcontext.Close();

                            #endregion

                            fs.Position = 0;
                            if (CompressUtility.IsCompressed(fs))
                            {

                                serializedobjstring = CompressUtility.DecompressGZip(fs);
                                //  (Encoding.UTF8.GetString(, 0, decompressedparts.Length));
                            }
                            else
                            {
                                serializedobjstring = fs;
                            }
                        }


                        object normalizatedresult = serializedobjstring.DeserializeFromByteArray(typeof(TResult), _useProtoSerializer);
                        cts.Token.ThrowIfCancellationRequested();

                        invokeState.State = ServiceInvokeState.Finish;
                        if (syncContext != null) syncContext.Send(d => statusChangedMethod(invokeState), null);

                        if (syncContext != null)
                            syncContext.Send(state => successMethod((TResult)state), normalizatedresult);
                        serializedobjstring = new MemoryStream();

                        //waiter.
                    }
                    else
                    {
                        //Все вернулось нормально без разбивок
                        invokeState.State = ServiceInvokeState.ReciveData;
                        invokeState.DownloadCurrentPart = 1;
                        invokeState.DownloadTotalPartsCount = 1;
                        if (syncContext != null) syncContext.Send(d => statusChangedMethod(invokeState), null);
                        object normalizatedresult = serializedobjstring.DeserializeFromByteArray(typeof(TResult), _useProtoSerializer);

                        cts.Token.ThrowIfCancellationRequested();
                        invokeState.State = ServiceInvokeState.Finish;
                        if (syncContext != null) syncContext.Send(d => statusChangedMethod(invokeState), null);

                        if (syncContext != null)
                            syncContext.Send(state => successMethod((TResult)state), normalizatedresult);
                        createdcontext.Close();
                        serializedobjstring = new MemoryStream();
                    }
                    if (createdcontext != null) createdcontext.Close();
                    createdcontext = null;
                    methodName = null;
                    body = null;
                    parameters = null;
                    zippedparams = null;
                }
                catch (OperationCanceledException exception)
                {
                    invokeState.State = ServiceInvokeState.Canceled;
                    syncContext.Send(d => statusChangedMethod(invokeState), null);
                    syncContext.Send(state => errorMethod(state as Exception), exception);
                }
                catch (Exception exception)
                {
                    if (invokeState.State != ServiceInvokeState.Canceled)
                        invokeState.State = ServiceInvokeState.Error;
                    syncContext.Send(d => statusChangedMethod(invokeState), null);
                    syncContext.Send(state => errorMethod(state as Exception), exception);

                }
            }, new TaskCreationOptions());
            return context;
        }


        private static void Init()
        {
            lock (SyncInitializer)
            {
                if (_isInitializet) return;

                try
                {
                    _useProtoSerializer = true;
                }
                catch
                {
                }

                //if (Environment.Is64BitOperatingSystem)
                //{
                //    RegistryKey localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                //    RegistryKey rk = localMachine64.OpenSubKey(@"SOFTWARE");
                //    if (rk != null)
                //    {
                //        rk = rk.OpenSubKey("НПФ ПРОРЫВ");
                //        if (rk != null) rk = rk.OpenSubKey("Телескоп+");
                //        if (rk != null) rk = rk.OpenSubKey("4.0");
                //    }

                //    if (rk != null && rk.GetValue("UseProtoSerializer") != null)
                //    {
                //        try
                //        {
                //            _useProtoSerializer = true;
                //        }
                //        catch
                //        {
                //        }

                //    }
                //}
                //else
                //{
                //    RegistryKey localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
                //    RegistryKey rk = localMachine64.OpenSubKey(@"SOFTWARE");
                //    if (rk != null)
                //    {
                //        rk = rk.OpenSubKey("НПФ ПРОРЫВ");
                //        if (rk != null) rk = rk.OpenSubKey("Телескоп+");
                //        if (rk != null) rk = rk.OpenSubKey("4.0");
                //    }

                //    if (rk != null && rk.GetValue("UseProtoSerializer") != null)
                //    {
                //        try
                //        {
                //            _useProtoSerializer = true;
                //        }
                //        catch
                //        {
                //        }

                //    }
                //}


                _isInitializet = true;
            }
        }

        public static TResult PartialLoaderSync<T, TResult>(this T context, Expression<Func<TResult>> methodName, TimeSpan? wcfTimeout = null, EndpointAddress addr = null)
            where T : IServiceInvoke
        {
            var cts = new CancellationTokenSource();
            return PartialLoaderSync(context, methodName, out cts, wcfTimeout, addr);
        }

        public static void PartialLoaderSync<T>(this T context, Expression<Action> methodName, TimeSpan? wcfTimeout = null)
           where T : IServiceInvoke
        {
            var cts = new CancellationTokenSource();
            PartialLoaderSync(context, methodName, out cts, wcfTimeout);
        }



        public class TaskReturnValue<T>
        {
            public bool IsNull { get; set; }
            public T Value { get; set; }
        }

        /// <summary>
        /// Выполнить метод на сервере с загрузкой данных по частям и синхронно
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="context"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
       /// [System.Diagnostics.DebuggerStepThrough]
        public static TResult PartialLoaderSync<T, TResult>(this T context, Expression<Func<TResult>> methodName,
            out CancellationTokenSource tokenSource, TimeSpan? wcfTimeout = null, EndpointAddress addr = null) where T : IServiceInvoke
        {
            Init();

            tokenSource = null;
            tokenSource = new CancellationTokenSource();
            CancellationTokenSource cts = tokenSource;


            using (var resulTask = new Task<TaskReturnValue<TResult>>(() =>
            {
                var createdcontext = context.GetClient(wcfTimeout, addr);


                TResult normalizatedresult = default(TResult);
                try
                {
                    //Получили название метода и аргументы
                    var body = methodName.Body as MethodCallExpression;
                    if (body == null) throw new Exception("Неизвестный метод службы UAService");
#if DEBUG
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    Debug.WriteLine(body.Method.Name + " Start Invoke WCF method");

#endif
                    //Получаем параметры
                    object[] parameters = body.Arguments.ToArray();
                    //Серилизуем парметры
                    var serializedParms = SerializeParamsToList(parameters);
                    //Сжимаем параметры
                    var zippedparams =
                        CompressUtility.CompressGZip(
                            new MemoryStream(
                                Encoding.UTF8.GetBytes(
                                    serializedParms.SerializeToString<List<KeyValuePair<string, string>>>())));

                    byte[] task = default(byte[]);
                    if (createdcontext.State == CommunicationState.Closed ||
                        createdcontext.State == CommunicationState.Closing ||
                        createdcontext.State == CommunicationState.Faulted)
                    {
                        createdcontext = (T)createdcontext.Recreate();
                    }


                    //bool[] aborted = { false };
                    //Task.Factory.StartNew(() =>
                    //{
                    //    while (!aborted[0])
                    //    {
                    //        Thread.Sleep(1000);
                    //        if (cts.IsCancellationRequested)
                    //        {
                    //            createdcontext.Abort();
                    //            aborted[0] = true;
                    //        }
                    //    }
                    //});
                    try
                    {
                        byte[] arrofpar = zippedparams.ToArray();

                        task = createdcontext.InvokeAndCompress(body.Method.Name, _useProtoSerializer, arrofpar);
                        arrofpar = new byte[0];
                        arrofpar = null;
#if DEBUG

                        stopwatch.Stop();
                        Debug.WriteLine(body.Method.Name + " Elapsed Invoke Time:" + stopwatch.ElapsedMilliseconds.ToString("g"));
                        if (task != null)
                        {
                            Debug.WriteLine(body.Method.Name + "Proto:"+ _useProtoSerializer + " Result After Download Stream Len: " + task.Length + " bytes");
                        }
#endif
                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                    if (task == null) return new TaskReturnValue<TResult>() { IsNull = true };
                    //aborted[0] = true;
                    zippedparams = new MemoryStream();
                    zippedparams = null;

                    Stream serializedobjstring = new MemoryStream();
                    serializedobjstring = CompressUtility.IsCompressed(task) ? CompressUtility.DecompressGZip(new MemoryStream(task)) : new MemoryStream(task);
                    //Значит большая часть нужно грузить отдельно
                    if (WaiterHelper.IsWaiterInfoStream(serializedobjstring))
                    {
                        var waiter = (FreeHierarchyService.WaiterInfo)serializedobjstring.DeserializeFromByteArray(typeof(FreeHierarchyService.WaiterInfo), false);
                        var parts = new ConcurrentStack<byte[]>();
                        for (int i = 0; i < waiter.PartsCount; i++)
                        {
                            try
                            {
                                byte[] fileBytes = createdcontext.GetWaiterNextPart(waiter.WaiterId);
                                parts.Push(fileBytes);
                            }
                            catch (Exception exception)
                            {
                                throw exception;
                            }
                        }

                        using (var fs = new MemoryStream())
                        {
                            List<byte[]> s = parts.ToList();
                            foreach (var bytese in s)
                            {
                                fs.Write(bytese, 0, bytese.Length);
                            }

                            #region clearMemory

                            parts.Clear();
                            parts = new ConcurrentStack<byte[]>();
                            parts = null;
                            s.Clear();
                            s = new List<byte[]>();
                            s = null;
                            createdcontext.Close();
                            createdcontext = null;
                            #endregion

                            fs.Position = 0;
                            if (CompressUtility.IsCompressed(fs))
                            {
                                serializedobjstring = CompressUtility.DecompressGZip(fs);


                            }
                            else
                            {
                                serializedobjstring = fs;
                            }
                        }


                        normalizatedresult = (TResult)serializedobjstring.DeserializeFromByteArray(typeof(TResult), _useProtoSerializer);
                        cts.Token.ThrowIfCancellationRequested();

                        serializedobjstring = new MemoryStream();
                    }
                    else
                    {
                        //Все вернулось нормально без разбивок

                        normalizatedresult = (TResult)serializedobjstring.DeserializeFromByteArray(typeof(TResult), _useProtoSerializer);
                        cts.Token.ThrowIfCancellationRequested();
                        createdcontext.Close();
                        createdcontext = null;
                        serializedobjstring = new MemoryStream();
                    }

                    if (createdcontext != null) createdcontext.Close();
                    createdcontext = null;
                    methodName = null;
                    body = null;
                    parameters = null;
                    zippedparams = null;
                }

                catch (Exception exception)
                {
                    createdcontext = null;
                    methodName = null;
                    throw exception;
                }

                return new TaskReturnValue<TResult> { Value = normalizatedresult };
            }, cts.Token))
            {
                try
                {
                    resulTask.Start();
                    if (resulTask.Exception != null)
                    {

                        methodName = null;
                        throw resulTask.Exception;
                    }
                    var result = resulTask.Result;
                    return result.Value;
                }
                catch (AggregateException exception)
                {

                    methodName = null;
                    throw exception.InnerException;
                }
            }
        }




        public static TResult PartialLoaderSync<T, TResult>(this T context, string methodName, object[] args,
            out CancellationTokenSource tokenSource, TimeSpan? wcfTimeout = null, EndpointAddress addr = null) where T : IServiceInvoke
        {
            Init();

            tokenSource = null;
            tokenSource = new CancellationTokenSource();
            CancellationTokenSource cts = tokenSource;


            using (var resulTask = new Task<TaskReturnValue<TResult>>(() =>
            {
                var createdcontext = context.GetClient(wcfTimeout, addr);


                TResult normalizatedresult = default(TResult);
                try
                {
                    //Получили название метода и аргументы
                    //var body = methodName.Body as MethodCallExpression;
                    //if (body == null) throw new Exception("Неизвестный метод службы UAService");
#if DEBUG
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    Debug.WriteLine(methodName + "Start Invoke WCF method");

#endif
                    //Получаем параметры
                    object[] parameters = args;
                    //Серилизуем парметры
                    var serializedParms = SerializeParamsToList(parameters);
                    //Сжимаем параметры
                    var zippedparams =
                        CompressUtility.CompressGZip(
                            new MemoryStream(
                                Encoding.UTF8.GetBytes(
                                    serializedParms.SerializeToString<List<KeyValuePair<string, string>>>())));

                    byte[] task = default(byte[]);
                    if (createdcontext.State == CommunicationState.Closed ||
                        createdcontext.State == CommunicationState.Closing ||
                        createdcontext.State == CommunicationState.Faulted)
                    {
                        createdcontext = (T)createdcontext.Recreate();
                    }


                    //bool[] aborted = { false };
                    //Task.Factory.StartNew(() =>
                    //{
                    //    while (!aborted[0])
                    //    {
                    //        Thread.Sleep(1000);
                    //        if (cts.IsCancellationRequested)
                    //        {
                    //            createdcontext.Abort();
                    //            aborted[0] = true;
                    //        }
                    //    }
                    //});
                    try
                    {
                        byte[] arrofpar = zippedparams.ToArray();

                        task = createdcontext.InvokeAndCompress(methodName, _useProtoSerializer, arrofpar);
                        arrofpar = new byte[0];
                        arrofpar = null;
#if DEBUG

                        stopwatch.Stop();
                        Debug.WriteLine(methodName + " Elapsed Invoke Time:" + stopwatch.ElapsedMilliseconds.ToString("g"));
                        if (task != null)
                        {
                            Debug.WriteLine(methodName + "Proto:"+ _useProtoSerializer + " Result After Download Stream Len: " + task.Length + " bytes");
                        }
#endif
                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                    if (task == null) return new TaskReturnValue<TResult>() { IsNull = true };
                    //aborted[0] = true;
                    zippedparams = new MemoryStream();
                    zippedparams = null;

                    Stream serializedobjstring = new MemoryStream();
                    serializedobjstring = CompressUtility.IsCompressed(task) ? CompressUtility.DecompressGZip(new MemoryStream(task)) : new MemoryStream(task);
                    //Значит большая часть нужно грузить отдельно
                    if (WaiterHelper.IsWaiterInfoStream(serializedobjstring))
                    {
                        var waiter = (FreeHierarchyService.WaiterInfo)serializedobjstring.DeserializeFromByteArray(typeof(FreeHierarchyService.WaiterInfo), false);
                        var parts = new ConcurrentStack<byte[]>();
                        for (int i = 0; i < waiter.PartsCount; i++)
                        {
                            try
                            {
                                byte[] fileBytes = createdcontext.GetWaiterNextPart(waiter.WaiterId);
                                parts.Push(fileBytes);
                            }
                            catch (Exception exception)
                            {
                                throw exception;
                            }
                        }

                        using (var fs = new MemoryStream())
                        {
                            List<byte[]> s = parts.ToList();
                            foreach (var bytese in s)
                            {
                                fs.Write(bytese, 0, bytese.Length);
                            }

                            #region clearMemory

                            parts.Clear();
                            parts = new ConcurrentStack<byte[]>();
                            parts = null;
                            s.Clear();
                            s = new List<byte[]>();
                            s = null;
                            createdcontext.Close();
                            createdcontext = null;
                            #endregion

                            fs.Position = 0;
                            if (CompressUtility.IsCompressed(fs))
                            {
                                serializedobjstring = CompressUtility.DecompressGZip(fs);


                            }
                            else
                            {
                                serializedobjstring = fs;
                            }
                        }


                        normalizatedresult = (TResult)serializedobjstring.DeserializeFromByteArray(typeof(TResult), _useProtoSerializer);
                        cts.Token.ThrowIfCancellationRequested();

                        serializedobjstring = new MemoryStream();
                    }
                    else
                    {
                        //Все вернулось нормально без разбивок

                        normalizatedresult = (TResult)serializedobjstring.DeserializeFromByteArray(typeof(TResult), _useProtoSerializer);
                        cts.Token.ThrowIfCancellationRequested();
                        createdcontext.Close();
                        createdcontext = null;
                        serializedobjstring = new MemoryStream();
                    }

                    if (createdcontext != null) createdcontext.Close();
                    createdcontext = null;
                    methodName = null;
                   // body = null;
                    parameters = null;
                    zippedparams = null;
                }

                catch (Exception exception)
                {
                    createdcontext = null;
                    methodName = null;
                    throw exception;
                }

                return new TaskReturnValue<TResult> { Value = normalizatedresult };
            }, cts.Token))
            {
                try
                {
                    resulTask.Start();
                    if (resulTask.Exception != null)
                    {

                        methodName = null;
                        throw resulTask.Exception;
                    }
                    var result = resulTask.Result;
                    return result.Value;
                }
                catch (AggregateException exception)
                {

                    methodName = null;
                    throw exception.InnerException;
                }
            }


        }


        /// <summary>
        /// Выполнить метод на сервере с загрузкой данных по частям и синхронно
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="methodName"></param>
        /// <param name="tokenSource"></param>
        /// <param name="wcfTimeout"></param>
        /// <returns></returns>
        public static void PartialLoaderSync<T>(this T context, Expression<Action> methodName,
            out CancellationTokenSource tokenSource, TimeSpan? wcfTimeout = null) where T : IServiceInvoke
        {
            tokenSource = null;
            tokenSource = new CancellationTokenSource();
            CancellationTokenSource cts = tokenSource;


            using (var resulTask = new Task(() =>
            {
                var createdcontext = context.GetClient(wcfTimeout);
                try
                {

                    //Получили название метода и аргументы
                    var body = methodName.Body as MethodCallExpression;

#if DEBUG
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    Debug.WriteLine(body.Method.Name + "Start Invoke WCF method");

#endif
                    if (body == null) throw new Exception("Неизвестный метод службы UAService");
                    Debug.WriteLine("Start InvokeService " + body.Method.Name + " method");
                    //Получаем параметры
                    object[] parameters = body.Arguments.ToArray();
                    //Серилизуем парметры
                    List<KeyValuePair<string, string>> serializedParms = SerializeParamsToList(parameters);
                    //Сжимаем параметры
                    MemoryStream zippedparams =
                        CompressUtility.CompressGZip(
                            new MemoryStream(
                                Encoding.UTF8.GetBytes(
                                    serializedParms.SerializeToString<List<KeyValuePair<string, string>>>())));

                    byte[] task = default(byte[]);
                    if (createdcontext.State == CommunicationState.Closed ||
                        createdcontext.State == CommunicationState.Closing ||
                        createdcontext.State == CommunicationState.Faulted)
                    {
                        createdcontext = (T)createdcontext.Recreate();
                    }


                    //bool[] aborted = { false };
                    //Task.Factory.StartNew(() =>
                    //{
                    //    while (!aborted[0])
                    //    {
                    //        Thread.Sleep(1000);
                    //        if (cts.IsCancellationRequested)
                    //        {
                    //            createdcontext.Abort();
                    //            aborted[0] = true;
                    //        }
                    //    }
                    //});
                    try
                    {
                        byte[] arrofpar = zippedparams.ToArray();

                        task = createdcontext.InvokeAndCompress(body.Method.Name, _useProtoSerializer, arrofpar);

                        arrofpar = new byte[0];
                        arrofpar = null;
#if DEBUG

                        stopwatch.Stop();
                        Debug.WriteLine(body.Method.Name + " Elapsed Invoke Time:" + stopwatch.ElapsedMilliseconds.ToString("g"));
                        if (task != null)
                            Debug.WriteLine(body.Method.Name + "Proto:" + _useProtoSerializer + " Result After Download Stream Len: " + task.Length + " bytes");
#endif

                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                    // if (task == null) return null;
                    //aborted[0] = true;
                    zippedparams = new MemoryStream();
                    zippedparams = null;

                    if (task != null)
                    {
                        Stream serializedobjstring = new MemoryStream();
                        if (CompressUtility.IsCompressed(task))
                        {
                            serializedobjstring = CompressUtility.DecompressGZip(new MemoryStream(task));

                        }
                        else
                        {
                            serializedobjstring = new MemoryStream(task);
                        }

                        //Значит большая часть нужно грузить отдельно
                        if (WaiterHelper.IsWaiterInfoStream(serializedobjstring))
                        {
                            var waiter =
                                (FreeHierarchyService.WaiterInfo) serializedobjstring.DeserializeFromByteArray(
                                    typeof(FreeHierarchyService.WaiterInfo), false);
                            var parts = new ConcurrentStack<byte[]>();
                            for (int i = 0; i < waiter.PartsCount; i++)
                            {
                                try
                                {
                                    byte[] fileBytes = createdcontext.GetWaiterNextPart(waiter.WaiterId);
                                    parts.Push(fileBytes);
                                }
                                catch (Exception exception)
                                {
                                    throw exception;
                                }
                            }

                            using (var fs = new MemoryStream())
                            {
                                List<byte[]> s = parts.ToList();
                                foreach (var bytese in s)
                                {
                                    fs.Write(bytese, 0, bytese.Length);
                                }

                                #region clearMemory

                                parts.Clear();
                                parts = new ConcurrentStack<byte[]>();
                                parts = null;
                                s.Clear();
                                s = new List<byte[]>();
                                s = null;
                                createdcontext.Close();
                                createdcontext = null;

                                #endregion

                                fs.Position = 0;
                                if (CompressUtility.IsCompressed(fs))
                                {
                                    serializedobjstring = CompressUtility.DecompressGZip(fs);
                                }
                                else
                                {
                                    serializedobjstring = fs;
                                }
                            }


                            //normalizatedresult = (TResult)serializedobjstring.DeserializeFromByteArray(typeof(TResult));
                            cts.Token.ThrowIfCancellationRequested();

                            serializedobjstring = new MemoryStream();
                        }
                        else
                        {
                            //Все вернулось нормально без разбивок

                            //normalizatedresult = (TResult)serializedobjstring.DeserializeFromByteArray(typeof(TResult));
                            cts.Token.ThrowIfCancellationRequested();
                            createdcontext.Close();
                            createdcontext = null;
                            serializedobjstring = new MemoryStream();
                        }
                    }

                    if (createdcontext != null) createdcontext.Close();
                    createdcontext = null;
                    methodName = null;
                    body = null;
                    parameters = null;
                    zippedparams = null;
                }

                catch (Exception exception)
                {
                    createdcontext = null;
                    methodName = null;
                    throw exception;

                }

                //return (() => {});
            }, cts.Token))
            {
                try
                {
                    resulTask.Start();
                    Task.WaitAll(new Task[] { resulTask });
                    if (resulTask.Exception != null)
                    {

                        methodName = null;
                        throw resulTask.Exception;
                    }

                }
                catch (AggregateException exception)
                {

                    methodName = null;
                    throw exception.InnerException;
                }



            }


        }

        /// <summary>
        ///     Сериализуем массив параметров в лист KeyValuePair тип,обьект
        /// </summary>
        /// <param name="paramList"></param>
        /// <returns>Сериализуем параметры</returns>
        public static List<KeyValuePair<string, string>> SerializeParamsToList(object[] paramList)
        {
            var serializedParms = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < paramList.Count(); i++)
            {
                object obj = paramList[i];
                if (obj != null)
                {
                    if (obj is UnaryExpression)
                    {
                        serializedParms.Add(new KeyValuePair<string, string>("null", "null"));
                        continue;
                    }
                    if (obj is ConstantExpression)
                    {
                        var ce = obj as ConstantExpression;
                        Type type = ce.Type;
                        string serializedstring = ce.Value.SerializeToString(ce.Type);
                        serializedParms.Add(new KeyValuePair<string, string>(type.ToString(), serializedstring));
                    }

                    else
                    {
                        if (obj is Expression)
                        {
                            var me = obj as MemberExpression;

                            if (me != null)
                            {
                                Type type = me.Type;
                                string serializedstring = GetMemberExpressionValue(me).SerializeToString(me.Type);
                                serializedParms.Add(new KeyValuePair<string, string>(type.ToString(), serializedstring));
                            }
                            else
                            {
                                var ce = obj as MethodCallExpression;
                                if (ce != null)
                                {
                                    Type type = ce.Type;
                                    string serializedstring = GetMemberExpressionValue(ce).SerializeToString(ce.Type);
                                    serializedParms.Add(new KeyValuePair<string, string>(type.ToString(),
                                        serializedstring));
                                }
                            }
                        }
                        else
                        {
                            Type type = obj.GetType();
                            string serializedstring = obj.SerializeToString(type);
                            serializedParms.Add(new KeyValuePair<string, string>(type.ToString(), serializedstring));
                        }
                    }
                }
                else
                {
                    //аргументы могут быть null
                    serializedParms.Add(new KeyValuePair<string, string>("null", "null"));
                }
            }
            return serializedParms;
        }

        /// <summary>
        ///     Получить значение аргумента MemberExpression
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        private static object GetMemberExpressionValue(MemberExpression member)
        {
            UnaryExpression objectMember = Expression.Convert(member, typeof(object));

            Expression<Func<object>> getterLambda = Expression.Lambda<Func<object>>(objectMember);

            Func<object> getter = getterLambda.Compile();

            return getter();
        }

        /// <summary>
        ///     Получить значение аргумента MemberExpression
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        private static object GetMemberExpressionValue(MethodCallExpression member)
        {
            UnaryExpression objectMember = Expression.Convert(member, typeof(object));

            Expression<Func<object>> getterLambda = Expression.Lambda<Func<object>>(objectMember);

            Func<object> getter = getterLambda.Compile();

            return getter();
        }

        public class ServiceInvokeStatusChangedArgs : EventArgs
        {
            public ServiceInvokeStatusChangedArgs(CancellationTokenSource cts)
            {
                cancellationTokenSource = cts;
            }

            /// <summary>
            ///     Стадия выполнения
            /// </summary>
            public ServiceInvokeState State { get; internal set; }


            /// <summary>
            ///     Описание операции
            /// </summary>
            public string StateString
            {
                get { return State.GetString(); }
            }

            /// <summary>
            ///     CancelationToken
            /// </summary>
            public CancellationTokenSource cancellationTokenSource { get; internal set; }


            /// <summary>
            ///     Количество частей
            /// </summary>
            public long DownloadTotalPartsCount { get; internal set; }


            /// <summary>
            ///     Скачено частей
            /// </summary>
            public long DownloadCurrentPart { get; internal set; }
        }


        public static class WaiterHelper
        {
            public const string ws = @"WaiterInfo";
            public const int startBytePosition = 42;
            //public static bool IsWaiterInfoStream(byte[] bytes)
            //{

            //    if (bytes.Length > startBytePosition + 9
            //        && bytes[startBytePosition] == 87
            //        && bytes[startBytePosition + 1] == 97
            //        && bytes[startBytePosition + 2] == 105
            //        && bytes[startBytePosition + 3] == 116
            //        && bytes[startBytePosition + 4] == 101
            //        && bytes[startBytePosition + 5] == 114
            //        && bytes[startBytePosition + 6] == 73
            //        && bytes[startBytePosition + 7] == 110
            //        && bytes[startBytePosition + 8] == 102
            //        && bytes[startBytePosition + 9] == 111
            //        )
            //        return true;

            //    //87 97 105 116 101 114 73 110 102 111
            //    return false;
            //}

            public static bool IsWaiterInfoStream(Stream stream)
            {
                stream.Position = 0;
                byte[] bytes = new byte[startBytePosition + 9 + 1];
                stream.Read(bytes, 0, bytes.Length);
                stream.Position = 0;
                if (bytes.Length >= startBytePosition + 9
                    && bytes[startBytePosition] == 87
                    && bytes[startBytePosition + 1] == 97
                    && bytes[startBytePosition + 2] == 105
                    && bytes[startBytePosition + 3] == 116
                    && bytes[startBytePosition + 4] == 101
                    && bytes[startBytePosition + 5] == 114
                    && bytes[startBytePosition + 6] == 73
                    && bytes[startBytePosition + 7] == 110
                    && bytes[startBytePosition + 8] == 102
                    && bytes[startBytePosition + 9] == 111
                    )
                    return true;



                //87 97 105 116 101 114 73 110 102 111
                return false;
            }
        }
    }
}
