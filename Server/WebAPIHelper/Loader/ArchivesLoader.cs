using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Proryv.AskueARM2.Server.DBAccess.Internal;

namespace Proryv.AskueARM2.Server.WCF.Loader
{
    /// <summary>
    /// Вспомогательный класс для подгрузки архивов частями
    /// </summary>
    /// <typeparam name="T1">Тип объектов, которые будут возвращены клиенту</typeparam>
    public class ArchivesLoader<T1>
        where T1 : class
    {
        /// <summary>
        /// Словарь ожидателей и идентификаторов
        /// </summary>
        public ConcurrentDictionary<Guid, ArchivesWaiter<T1>> ArchivesLoaderDict = new ConcurrentDictionary<Guid, ArchivesWaiter<T1>>();

        /// <summary>
        /// Получить новый ожидатель
        /// </summary>
        /// <param name="waiterID"></param>
        /// <returns></returns>
        public ArchivesWaiter<T1> GetWaiter(Guid waiterID, Task<T1> singleResultTask = null)
        {
            ArchivesWaiter<T1> waiter = null;

            // создаем сборку если новая, или берем из словаря
            if (!ArchivesLoaderDict.TryGetValue(waiterID, out waiter))
            {
                waiter = new ArchivesWaiter<T1>(RemoveWaiter, waiterID, singleResultTask);

                ArchivesLoaderDict.TryAdd(waiterID, waiter);
            }

            return waiter;
        }

        /// <summary>
        /// Отмена выполнения
        /// </summary>
        /// <param name="waiterID"></param>
        /// <returns></returns>
        public bool TryCancelWaiter(Guid waiterID)
        {
            ArchivesWaiter<T1> waiter = null;

            // создаем сборку если новая, или берем из словаря
            if (!ArchivesLoaderDict.TryGetValue(waiterID, out waiter) || waiter == null)
            {
                return false;
            }

            waiter.Cancel();

            return true;
        }

        public bool GetNextPart(Guid waiterID, out List<T1> nextPart, out string errors, int? requestNumber = null)
        {
            object result;
            return GetNextPart(requestNumber, waiterID, out nextPart, out errors, out result);
        }

        /// <summary>
        /// Получить следующую партию объектов для отправки клиенту
        /// </summary>
        /// <param name="lastRequestNumber">Номер пакета</param>
        /// <param name="waiterID">Идентификатор ожидателя</param>
        /// <param name="nextPart">Возвращаемый пакет архивов</param>
        /// <param name="errors">Критическая ошибка</param>
        /// <param name="result">Конечный результат продолжительных операций</param>
        /// <returns>Признак того что больше нет объектов для возврата</returns>
        public bool GetNextPart(int? requestNumber, Guid waiterID, out List<T1> nextPart, out string errors, out object result)
        {
            errors = string.Empty;
            nextPart = null;
            result = null;
            ArchivesWaiter<T1> waiter;
            // создаем сборку если новая, или берем из словаря
            if (!ArchivesLoaderDict.TryGetValue(waiterID, out waiter) || waiter == null)
            {
                //errors = "Анализ данных закончен.";
                return true;
            }

            bool isLastPacket;
            try
            {
                isLastPacket = !waiter.TryGetNextPart(requestNumber, out nextPart);
                if (isLastPacket)
                {
                    result = waiter.GetResult();
                }
            }
            catch (Exception ex)
            {
                isLastPacket = true;
                errors = ex.Message;
            }

            //Пытаемcя получить следующую часть
            if (isLastPacket)
            {
                //Не получилось, архив закончился больше нечего читать, удаляем
                RemoveWaiter(waiterID);
            }

            return isLastPacket;
        }

        public int GetArchiveCount(Guid waiterID)
        {
            ArchivesWaiter<T1> waiter;
            // создаем сборку если новая, или берем из словаря
            if (!ArchivesLoaderDict.TryGetValue(waiterID, out waiter) || waiter == null || waiter.Archive == null)
            {
                //errors = "Анализ данных закончен.";
                return 0;
            }

            return waiter.Archive.Count;
        }

        /// <summary>
        /// Получить следующую партию объектов для отправки клиенту
        /// </summary>
        /// <param name="lastRequestNumber">Номер пакета</param>
        /// <param name="waiterID">Идентификатор ожидателя</param>
        /// <param name="nextPart">Возвращаемый пакет архивов</param>
        /// <param name="errors">Критическая ошибка</param>
        /// <param name="result">Конечный результат продолжительных операций</param>
        /// <returns>Признак того что больше нет объектов для возврата</returns>
        public bool GetSingleResult(Guid waiterID, out T1 result)
        {
            result = null;
            ArchivesWaiter<T1> waiter;
            // создаем сборку если новая, или берем из словаря
            if (!ArchivesLoaderDict.TryGetValue(waiterID, out waiter) || waiter == null)
            {
                //errors = "Анализ данных закончен.";
                return true;
            }

            bool isReady;
            try
            {
                isReady = !waiter.TryGetSingleResult(out result);
            }
            catch (Exception ex)
            {
                isReady = true;
            }

            //Пытаемcя получить следующую часть
            if (isReady)
            {
                //Не получилось, архив закончился больше нечего читать, удаляем
                RemoveWaiter(waiterID);
            }

            return isReady;
        }

        /// <summary>
        /// Получить следующую партию объектов для отправки клиенту
        /// </summary>
        /// <param name="waiterID">Идентификатор ожидателя</param>
        /// <param name="maxCount">Максимальное ограничения числа возвращаемых объектов</param>
        /// <returns></returns>
        public KeyValuePair<bool, List<T1>> GetNextPartWithValidateToEmpty(Guid waiterID, int maxCount)
        {
            ArchivesWaiter<T1> waiter;
            List<T1> resultList = null;
            var isEmpty = true;

            // создаем сборку если новая, или берем из словаря
            if (ArchivesLoaderDict.TryGetValue(waiterID, out waiter) && waiter != null)
            {

                isEmpty = waiter.TryGetNextPartWithValidateToEmpty(out resultList, maxCount);

                //Пытаемя получить следующую часть
                if (isEmpty)
                {
                    //Не получилось, архив закончился больше нечего читать, удаляем
                    RemoveWaiter(waiterID);
                }
            }

            return new KeyValuePair<bool, List<T1>>(isEmpty, resultList);
        }

        /// <summary>
        /// Удаление ожидателя из общего списка, очистка памяти
        /// </summary>
        /// <param name="waiterID">Идентификатор ожидателя</param>
        private void RemoveWaiter(Guid waiterID)
        {
            //предотвращение падения сервиса
            //Task.Factory.StartNew(() =>
            //{
                try
                {
                    ArchivesWaiter<T1> waiter;
                    if (ArchivesLoaderDict.TryRemove(waiterID, out waiter) && waiter != null)
                    {
                        waiter.Dispose();
                    }
                }
                catch
                {
                }
            //});
        }
    }

    /// <summary>
    /// Класс ожидателя
    /// </summary>
    /// <typeparam name="T">Тип подгружаемых клиентов объектов</typeparam>
    public class ArchivesWaiter<T> : IDisposable
        where T : class
    {
        /// <summary>
        /// Процедура самоочистки
        /// </summary>
        private Action<Guid> _removeItself;

        /// <summary>
        /// Идентификатор
        /// </summary>
        private readonly Guid _waiterID;

        /// <summary>
        /// Список ожидаемых объектов
        /// </summary>
        public ConcurrentStack<T> Archive;

        /// <summary>
        /// Токен для отмены продолжительных операций
        /// </summary>
        public CancellationTokenSource ArchivesCancellationTokenSource;

        /// <summary>
        /// Еще один вариант реализации
        /// </summary>
        private readonly Task<T> SingleResultTask;

        //public ZipForge Ziper;
        //public MemoryStream Archive;
        private object _resultLocker;
        private ICloneable _calculatedResult;

        /// <summary>
        /// Таймер для ожидания запросов клиента, когда таймер отсчитает ожидатель самоликвидируется и очистит память
        /// </summary>
        private readonly Stopwatch _sw = new Stopwatch();

        /// <summary>
        /// Флаг на принудительную самоликвидацию ожидателя
        /// </summary>
        private volatile bool _isNeedToStop;

        /// <summary>
        /// Размер партии возвращаемой клиенту
        /// </summary>
        private int _packetSize;

        /// <summary>
        /// Признак того что необходимо еще подождать, перед самоликвидацией
        /// </summary>
        private volatile bool _isNeedWait;

        /// <summary>
        /// Фактория по динамическому добавлению архивов
        /// </summary>
        private IDisposable _fillFactory;

        private readonly object _syncLock;

        /// <summary>
        /// Критические ошибки, продолжение невозможно
        /// </summary>
        private readonly StringBuilder _criticalErrors;


        private int _lastRequestNumber = -1;

        /// <summary>
        /// Последний запрошенный пакет, на случай если придется его перезапрашивать (ошибки сети)
        /// </summary>
        private List<T> _lastArchives;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="removeItself">Процедура самоочистки</param>
        /// <param name="waiterID">Идентификатор</param>
        public ArchivesWaiter(Action<Guid> removeItself, Guid waiterID, Task<T> singleResultTask = null)
        {
            Archive = new ConcurrentStack<T>();
            ArchivesCancellationTokenSource = new CancellationTokenSource();
            _removeItself = removeItself;
            _waiterID = waiterID;
            _packetSize = Settings.MaxListStringParams;
            _syncLock = new object();
            _criticalErrors = new StringBuilder();
            _resultLocker = new object();
            SingleResultTask = singleResultTask;
            if (singleResultTask != null) _isNeedWait = true;
        }

        /// <summary>
        /// Указываем архив и запускаем таймер на самоликвидацию
        /// </summary>
        /// <param name="archive">Архив</param>
        /// <param name="isNeedWait">Признак того что необходимо еще подождать, перед самоликвидацией</param>
        public void FeelArchivesAndStartWatch(ConcurrentStack<T> archive, int packetSize = 0, bool isNeedWait = false, IDisposable fillFactory = null)
        {
            _isNeedWait = isNeedWait;
            _fillFactory = fillFactory;
            //Набираем очередь, которую будем отправлять частями клиенту
            //int maxCount = Settings.MaxListStringParams;
            if (packetSize > 0) _packetSize = packetSize;

            //Наполняем массив 
            Archive = archive;

            //Стартуем ожидание
            if (!isNeedWait)
            {
                _sw.Start();
            }

            //Запускаем поток на ожидание чтение пакетов
            TaskHelper.LoggedExceptions(Task.Factory.StartNew(WaitForClear, TaskCreationOptions.AttachedToParent));
        }

        /// <summary>
        /// Берем следующюю партию архивов
        /// </summary>
        /// <param name="lastRequestNumber">Номер пакета</param>
        /// <param name="range">Возвращаемая партия</param>
        /// <returns>Признак того что еще есть архивы, которые нужно вернуть</returns>
        public bool TryGetNextPart(int? requestNumber, out List<T> range)
        {
            range = null;
            lock (_syncLock)
            {
                //Критическая ошибка продолжать нельзя
                if (_criticalErrors.Length > 0) throw new Exception(_criticalErrors.ToString());
            }

            if (_isNeedWait || Archive.Count >_packetSize)
            {
                lock (_sw)
                {
                    _sw.Restart();
                }
            }

            if (requestNumber.HasValue && requestNumber.Value == _lastRequestNumber)
            {
                range = _lastArchives;
                return !Archive.IsEmpty || _isNeedWait;
            }

            if (!Archive.IsEmpty)
            {
                int count = Math.Min(Archive.Count, _packetSize);
                var mArray = new T[count];
                Archive.TryPopRange(mArray, 0, count);
                _lastArchives = range = mArray.ToList();
                _lastRequestNumber = requestNumber.GetValueOrDefault();
            }
            else
            {
                //Коллекция пуста
                return _isNeedWait;
            }
            return true;
        }

        /// <summary>
        /// Берем следующюю партию архивов
        /// </summary>
        /// <param name="lastRequestNumber">Номер пакета</param>
        /// <param name="range">Возвращаемая партия</param>
        /// <returns>Признак того что еще есть архивы, которые нужно вернуть</returns>
        public bool TryGetSingleResult(out T result)
        {
            result = null;

            if (SingleResultTask == null) return false;

            lock (_syncLock)
            {
                //Критическая ошибка продолжать нельзя
                if (_criticalErrors.Length > 0) throw new Exception(_criticalErrors.ToString());
            }

            if (SingleResultTask.IsCompleted || SingleResultTask.IsCanceled || SingleResultTask.IsFaulted)
            {
                result = SingleResultTask.Result;
                ResetNeedFlag();
            }
            else
            {
                lock (_sw)
                {
                    _sw.Restart();
                }
            }
            
            return _isNeedWait;
        }

        /// <summary>
        /// Берем следующюю партию архивов
        /// </summary>
        /// <param name="range">Возвращаемая партия</param>
        /// <param name="maxCount">Максимальные размер возвращаемых данных</param>
        /// <returns>Признак того что еще есть архивы, которые нужно вернуть</returns>
        public bool TryGetNextPartWithValidateToEmpty(out List<T> range, int maxCount)
        {
            range = null;
            try
            {
                if (!_isNeedWait || Archive.Count > _packetSize)
                {
                    lock (_sw)
                    {
                        _sw.Restart();
                    }
                }

                if (!Archive.IsEmpty)
                {
                    int count = Math.Min(Archive.Count, maxCount);
                    var mArray = new T[count];
                    Archive.TryPopRange(mArray, 0, count);
                    range = mArray.ToList();
                }
            }
            catch
            {
            }

            return Archive.IsEmpty;
        }

        public void SetResult(ICloneable calculatedResult)
        {
            lock (_resultLocker)
            {
                _calculatedResult = calculatedResult;
            }
        }

        public object GetResult()
        {
            object calculatedResult = null;
            lock (_resultLocker)
            {
                if (_calculatedResult != null)
                {
                    calculatedResult = _calculatedResult.Clone();
                }
            }
            return calculatedResult;
        }


        public void ResetNeedFlag()
        {
            _isNeedToStop = true;
            _isNeedWait = false;
            lock (_sw)
            {
                _sw.Stop();
            }
        }

        public void OnCriticalError(string message)
        {
            lock (_syncLock)
            {
                if (_criticalErrors != null)
                {
                    _criticalErrors.Append(message);
                }
            }
        }

        /// <summary>
        /// Задача самоликвидации ожидателя
        /// </summary>
        private void WaitForClear()
        {
            //Выставляем наименьший приоритет
            //Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            while (true)
            {
                Thread.Sleep(10000);
                try
                {

                    lock (_sw)
                    {
                        //1 часа должно хватать на все операции
                        if (_sw.ElapsedMilliseconds > 1800000 || (_isNeedToStop && Archive.Count == 0))
                        {
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }

            //Раз до сюда дошли, то надо освободить память
            try
            {
                if (ArchivesCancellationTokenSource != null) ArchivesCancellationTokenSource.Cancel();
            }
            catch
            {
            }

            Thread.Sleep(5000);

            if (_removeItself != null)
            {
                _removeItself(_waiterID);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// Отмена выполнения
        /// </summary>
        public void Cancel()
        {
            if (ArchivesCancellationTokenSource != null)
            {
                ArchivesCancellationTokenSource.Cancel();
                if (_fillFactory != null)
                {
                    _fillFactory.Dispose();
                }
                _isNeedWait = false;
                _isNeedToStop = true;
            }
        }

        /// <summary>
        /// Освобождаем все что можно освободить
        /// </summary>
        public void Dispose()
        {
            _isNeedToStop = true;

            lock (_sw)
            {
                _sw.Stop();
            }

            //Thread.Sleep(100);

            if (!Archive.IsEmpty)
            {
                Archive.Clear();
            }

            if (_fillFactory != null)
            {
                _fillFactory.Dispose();
            }

            if (ArchivesCancellationTokenSource != null)
            {
                ArchivesCancellationTokenSource.Dispose();
                ArchivesCancellationTokenSource = null;
            }

            _removeItself = null;
        }
    }
}
