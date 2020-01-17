using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.ElectroARM.Controls.Controls.Menu.Data;
using Proryv.ElectroARM.Controls.Controls.GlobalSet.Converters;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Data.FreeHierarchy;

namespace Proryv.AskueARM2.Client.Visual
{
    public partial class SetsManager : IDisposable
    {
        private ListCollectionView _lcv;
        private const string _voidSetName = "<без названия>";
        private EnumGlobalSetModuleType? _moduleNameForUse; //Фильтр наборов, зависит от того, где наборы применяются 
        private EnumNameRequestModalType _modalType; //Тип сообщений и диалогов, глобальные или только с блокировкой отдельного родителя
        private FrameworkElement _modalParent; //Родитель в рамках которого будет блокироваться диалог

        private string _setName;

        public SetsManager()
        {
            InitializeComponent();
            btnReload.Visibility = Visibility.Collapsed;

            _lcv = new ListCollectionView(_dict);
            _lcv.SortDescriptions.Add(new SortDescription("IsGlobalSet", ListSortDirection.Ascending));
            _lcv.SortDescriptions.Add(new SortDescription("SetName", ListSortDirection.Ascending));
            _lcv.SortDescriptions.Add(new SortDescription("UserId", ListSortDirection.Ascending));
            _lcv.GroupDescriptions.Add(new PropertyGroupDescription("IsGlobalSet", new GlobalToStringConverter()));

            lvSets.ItemsSource = _lcv;
            //cbSets.ItemsSource = _dict;
        }

        private readonly ObservableCollection<TSetView> _dict = new ObservableCollection<TSetView>();
        private readonly object _dictSyncLock = new object();

        /// <summary>
        /// Старая версия инициализации, надо уходить от нее
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="globalSets"></param>
        public void Init(Dictionary<string, List<string>> dict, List<Expl_User_Global_Set> globalSets = null)
        {
            lock (_dictSyncLock)
            {
                _dict.Clear();

                var set = new TSetView(_voidSetName, true, isSelected: true);

                _dict.Add(set);

                tbSelectSet.DataContext = set;
            }

            if (dict != null)
            {
                lock (_dictSyncLock)
                {
                    var t = dict.Select(d => new TSetView(d.Key, false, null,
                        d.Value == null || d.Value.Count == 0 ? new List<string>() : d.Value)).ToList();
                    foreach (var d in t)
                    {
                        _dict.Add(d);
                    }
                }
            }

            if (globalSets != null && globalSets.Count > 0)
            {
                foreach (var explUserGlobalSet in globalSets
                    .OrderByDescending(g => g.IsGlobal)
                    .ThenBy(g => g.User_ID)
                    .ThenBy(g => g.StringName))
                {
                    //if (explUserGlobalSet.List == null) continue;
                    List<string> existSet = null;
                    if (!string.IsNullOrEmpty(explUserGlobalSet.List))
                    {
                        existSet = explUserGlobalSet.UseProtoSerializer.GetValueOrDefault()
                            ? explUserGlobalSet.List.ProtoDeserializeFromString<List<string>>()
                            : explUserGlobalSet.List.DeserializeFromString<List<string>>();
                    }

                    if (existSet == null)
                    {
                        existSet = new List<string>();
                    }

                    var d = _dict.FirstOrDefault(s =>
                        string.Equals(s.SetName, explUserGlobalSet.StringName, StringComparison.OrdinalIgnoreCase));

                    if (d == null || d.Set == null)
                    {
                        d = new TSetView(explUserGlobalSet.StringName, explUserGlobalSet.IsReadOnly,
                            explUserGlobalSet.UserGlobalSet_ID, existSet,
                            true, explUserGlobalSet.User_ID, explUserGlobalSet.VersionNumber);
                        lock (_dictSyncLock)
                        {
                            _dict.Add(d);
                        }

                        d.Dict = dict;
                    }
                    else
                    {
                        //Переводим наборы в отдельную таблицу Expl_User_Global_Sets
                        if (dict != null)
                        {
                            dict.Remove(explUserGlobalSet.StringName);
                        }

                        d.Set.UnionWith(existSet);
                        //SaveSet(explUserGlobalSet.StringName, false); //Сохраняем в отдельной таблице
                    }

                    d.IsGlobalSet = explUserGlobalSet.IsGlobal;
                    d.IsUseGlobalTable = true;
                }
            }
        }


        /// <summary>
        /// Новая версия инициализации из таблицы Expl_User_Global_Set
        /// </summary>
        /// <param name="globalSets"></param>
        private void InitGlobal(Dictionary<string, List<string>> dict, List<Expl_User_Global_Set> globalSets)
        {
            lock (_dictSyncLock)
            {
                _dict.Clear();

                var set = new TSetView(_voidSetName, true, isSelected: true);

                _dict.Add(set);

                tbSelectSet.DataContext = set;
            }

            if (globalSets == null || globalSets.Count == 0) return;

            foreach (var explUserGlobalSet in globalSets
                .OrderBy(g => g.IsGlobal)
                .ThenBy(g => g.User_ID)
                .ThenBy(g => g.StringName))
            {
                List<string> existSet = null;
                string protoSerializetSelected = null;

                try
                {
                    if (explUserGlobalSet.VersionNumber < 3)
                    {
                        if (!string.IsNullOrEmpty(explUserGlobalSet.List))
                        {
                            existSet = explUserGlobalSet.UseProtoSerializer.GetValueOrDefault()
                                ? explUserGlobalSet.List.ProtoDeserializeFromString<List<string>>()
                                : explUserGlobalSet.List.DeserializeFromString<List<string>>();
                        }

                        if (existSet == null)
                        {
                            existSet = new List<string>();
                        }
                    }
                    else
                    {
                        protoSerializetSelected = explUserGlobalSet.List;
                    }
                }
                catch
                {

                }

                //Считаем что все наборы перевели в глобальную таблицу
                var d = new TSetView(explUserGlobalSet.StringName, explUserGlobalSet.IsReadOnly,
                        explUserGlobalSet.UserGlobalSet_ID, existSet,
                        true, explUserGlobalSet.User_ID, explUserGlobalSet.VersionNumber,
                        isGlobal: explUserGlobalSet.IsGlobal, protoSerializetSelected: protoSerializetSelected);

                lock (_dictSyncLock)
                {
                    _dict.Add(d);
                }
            }

            if (dict != null)
            {
                lock (_dictSyncLock)
                {
                    //var t = dict.Select(d => ).ToList();

                    foreach (var d in dict)
                    {
                        if (d.Value == null || _dict.Any(s=>string.Equals(s.SetName, d.Key))) continue;

                        var set = new TSetView(d.Key, false, null,
                            d.Value == null || d.Value.Count == 0 ? new List<string>() : d.Value, false,
                            versionNumber: 0);

                        set.Dict = dict;

                        _dict.Add(set);
                    }
                }
            }
        }

        private void SaveSet(bool isReadOnly = false, Action onError = null)
        {
            var set = tbSelectSet.DataContext as TSetView;
            if (set == null) return;

            if (string.IsNullOrEmpty(set.SetName))
            {
                ShowMessage("Название набора пустое. Сохранение отменено.");
                return;
            }

            //if (set.Set == null)
            //{
            //    ShowMessage("Набор пустой, или не найден. Сохранение отменено.");
            //    return;
            //}

            if (set.IsReadOnly)
            {
                ShowMessage("Набор недоступен для сохранения.");
                return;
            }

            if (OnSaveNew!=null)
            {
                set.ProtoSerializetSelected = OnSaveNew(null);
                set.VersionNumber = 3;
            }

            var newGlobalSet = new Expl_User_Global_Set
            {
                //Здесь только пользователь создавший набор!!!
                User_ID = string.IsNullOrEmpty(set.UserId) || !set.IsGlobalSet ? Manager.User.User_ID : set.UserId,
                UserGlobalSet_ID = set.UserGlobalSetsId ?? Guid.Empty,
                StringName = set.SetName,
                List = set.ProtoSerializetSelected,
                IsGlobal = set.IsGlobalSet,
                IsReadOnly = isReadOnly,
                UseProtoSerializer = true,
                ModuleNameForUse = _moduleNameForUse,
                VersionNumber = 3,
            };

            Manager.UI.RunUILocked(() =>
            {
                Guid userGlobalSetsId;

                try
                {
                    userGlobalSetsId = ARM_Service.EXPL_Save_GlobalSet(newGlobalSet);
                    if (userGlobalSetsId == default(Guid)) //Ошибка WCF
                    {
                        ShowMessage("Набор не удалось сохранить");
                        if (onError != null) onError();
                        return;
                    }

                    set.UserGlobalSetsId = userGlobalSetsId;

                    //Если это старый набор, не из глобальной таблицы, то удаляем его
                    if (!set.IsUseGlobalTable && set.Dict != null &&
                        set.Dict.Remove(set.SetName))
                    {
                        Manager.SaveConfig(true);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage("Набор не сохранен!\n" + ex.Message);
                    if (onError != null) onError();
                    return;
                }

                //set.UserGlobalSetsId = userGlobalSetsId;

                ShowMessage("Набор сохранен!");
                _lcv.Refresh();
            });
        }

        //Dictionary<string, Expl_User_Global_Set> _data;

        public void InitGlobal(string setName, EnumGlobalSetModuleType? moduleNameForUse = EnumGlobalSetModuleType.HierarchyTree,
            EnumNameRequestModalType modalType = EnumNameRequestModalType.GlobalModal, FrameworkElement source = null)
        {
            _setName = setName;

            _moduleNameForUse = moduleNameForUse;
            _modalParent = source;
            _modalType = modalType;

            btnReload.Visibility = Visibility.Visible;

            //Окончательно переводим наборы в глобальную таблицу
            Dictionary<string, List<string>> d = null;
            if (Manager.Config.FreeSets != null && !string.IsNullOrEmpty(setName))
            {
                Manager.Config.FreeSets.TryGetValue(setName, out d);
                //if (!Manager.Config.FreeSets.TryGetValue(setName, out d) || d == null)
                //{
                //    d = new Dictionary<string, List<string>>();
                //    Manager.Config.FreeSets[setName] = d;
                //}
            }

            cbIsGlobalSet.Visibility = Visibility.Visible;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var globalSet = ARM_Service.EXPL_Get_GlobalSets(Manager.User.User_ID, _moduleNameForUse);
                    if (globalSet == null) return;
                    Dispatcher.BeginInvoke((Action) (() => InitGlobal(d, globalSet)), DispatcherPriority.Background);
                }
                catch
                {
                    // ignored
                }
            });
        }

        //bool _isGlobal = false;

        private void butAdd_Click(object sender, RoutedEventArgs e)
        {
            AddNew("");
        }

        private void AddNew(string newName, string dialogHeader = "Введите имя набора")
        {
            List<string> names;
            lock (_dictSyncLock)
            {
                names = _dict.Select(s => s.SetName).ToList();
            }

            var nr = new NameRequest(names, newName, _modalType, _modalParent) ;
            nr.OnOK += OnOkPressed;

            if (_modalType == EnumNameRequestModalType.SimpleModal && _modalParent != null)
            {
                _modalParent.ShowModal(nr, "Введите имя набора");
            }
            else
            {
                Manager.UI.ShowGlobalModal(nr, dialogHeader);
            }
        }

        private void OnOkPressed(string name)
        {
            var isReadOnly = cbIsReadOnly.IsChecked.GetValueOrDefault();

            short versionNumber = 0;

            var list = new HashSet<string>();

            if (OnSave != null)
            {
                versionNumber = OnSave(list);
            }

            var isGlobalSet = cbIsGlobalSet.IsChecked.GetValueOrDefault();

            string protoSerializetSelected = null;

            if (OnSaveNew != null)
            {
                protoSerializetSelected = OnSaveNew(null);
                versionNumber = 3;
            }
            else
            {
                protoSerializetSelected = list.ProtoSerializeToString();
            }

            var newGlobalSet = new Expl_User_Global_Set
            {
                User_ID = Manager.User.User_ID,
                StringName = name,
                List = protoSerializetSelected,
                IsGlobal = isGlobalSet,
                IsReadOnly = isReadOnly,
                ModuleNameForUse = _moduleNameForUse,
                VersionNumber = versionNumber,
                UseProtoSerializer = true,
            };

            try
            {
                var userGlobalSetsId = ARM_Service.EXPL_Save_GlobalSet(newGlobalSet);
                if (userGlobalSetsId == default(Guid)) //Ошибка WCF
                {
                    ShowMessage("Набор не удалось сохранить");
                    return;
                }

                var set = new TSetView(name, isReadOnly, userGlobalSetsId, list, true, Manager.User.User_ID,
                    versionNumber,
                    true, isGlobalSet, protoSerializetSelected: protoSerializetSelected);

                lock (_dictSyncLock)
                {
                    _dict.Add(set);
                }

                tbSelectSet.DataContext = set;

                cbIsReadOnly.IsEnabled =
                    set.IsGlobalSet && string.Equals(newGlobalSet.User_ID, Manager.User.User_ID);
            }
            catch (Exception ex)
            {
                ShowMessage("Набор не сохранен!\n" + ex.Message);
                return;
            }
        }
        private void delete_Click(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe == null) return;

            var set = tbSelectSet.DataContext as TSetView;
            if (set == null || set.SetName == _voidSetName) return;

            if (!set.UserGlobalSetsId.HasValue && !set.IsUseGlobalTable && set.Dict != null &&
                set.Dict.Remove(set.SetName))
            {
                //Если это старый набор, не из глобальной таблицы
                Manager.SaveConfig(true);
                return;
            }

            Manager.UI.ShowYesNoDialog("Вы действительно хотите удалить набор \"" + set.SetName + "\"?", delegate
            {
                //if (set.IsUseGlobalTable)
                {
                    try
                    {
                        var res = ARM_Service.Expl_Delete_GlobalSet(set.UserGlobalSetsId.Value);
                        if (res == null) return;

                        if (res != string.Empty)
                        {
                            ShowMessage(res);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("Ошибка удаления набора:\n" + ex.Message);
                        return;
                    }
                }

                //Если это старый набор, не из глобальной таблицы
                if (!set.IsUseGlobalTable && set.Dict != null && set.Dict.Remove(set.SetName))
                {
                    Manager.SaveConfig(true);
                }

                lock (_dictSyncLock)
                {
                    _dict.Remove(set);
                }

                cbIsGlobalSet.IsChecked = cbIsReadOnly.IsChecked = cbIsReadOnly.IsEnabled = false;

                lock (_dictSyncLock)
                {
                    set = _dict.FirstOrDefault(d => d.SetName == _voidSetName);
                }

                if (set != null)
                {
                    set.IsSelected = true;
                    tbSelectSet.DataContext = set;
                }

                //cbSets.SelectedIndex = 0;
                if (Changed != null) Changed(null, 0);
            });
        }

        public event Action<object, short> Changed;

        public event Func<HashSet<string>, short> OnSave;

        public event Func<HashSet<EnumFreeHierarchyItemType>, string> OnSaveNew;

        private void save_Click(object sender, RoutedEventArgs e)
        {
            var set = tbSelectSet.DataContext as TSetView;
            if (set == null || set.SetName == _voidSetName)
            {
                butAdd_Click(null, null);
                return;
            }

            //if (set.Set == null || string.IsNullOrEmpty(set.ProtoSerializetSelected)) return; //Не сохраняем набор без выбранных объектов

            set.IsGlobalSet = cbIsGlobalSet.IsChecked.GetValueOrDefault();

            bool isReadOnly;
            if (set.IsGlobalSet)
            {
                isReadOnly = set.IsReadOnly = cbIsReadOnly.IsEnabled && cbIsReadOnly.IsChecked.GetValueOrDefault();
            }
            else
            {
                isReadOnly = false;
                cbIsReadOnly.IsChecked = false;
                cbIsReadOnly.IsEnabled = false;
            }

            //if (OnSave != null)
            //{
            //    set.VersionNumber = OnSave(set.Set);
            //}

            //if (set.IsUseGlobalTable)
            {
                SaveSet(isReadOnly);
            }
            //else
            //{
                //Manager.SaveConfig(true);
            //}
        }

        public void Reset()
        {
            TSetView set;

            lock (_dictSyncLock)
            {
                set = _dict.FirstOrDefault(d => d.SetName == _voidSetName);
            }

            if (set != null)
            {
                set.IsSelected = true;
                tbSelectSet.DataContext = set;
            }

            if (Changed != null) Changed(null, 0);
        }

        public string GetSelectedSet()
        {
            var set = tbSelectSet.DataContext as TSetView;
            if (set == null) return string.Empty;

            return set.SetName;

            //if (cbSets.SelectedIndex == 0)
            //{
            //    return null;
            //}
            //else
            //{
            //    if (cbSets.SelectedValue != null)
            //        return cbSets.SelectedValue.ToString();
            //    return "";
            //}
        }

        private bool _isUserClicked = true;

        private void isGlobalSet_Click(object sender, RoutedEventArgs e)
        {
            if (!_isUserClicked)
            {
                return;
            }

            var set = tbSelectSet.DataContext as TSetView;
            if (set == null || set.IsReadOnly) return;

            var cb = sender as CheckBox;
            if (cb == null) return;

            var isNewGlobalStatus = cb.IsChecked.GetValueOrDefault();
            if (set.IsGlobalSet == isNewGlobalStatus) return; //Состояние не изменилось

            if (set.SetName == _voidSetName)
            {
                cbIsReadOnly.IsChecked = cbIsReadOnly.IsEnabled = false;
                return;
            }

            if (isNewGlobalStatus)
            {
                cbIsReadOnly.IsEnabled = true;
            }
            else
            {
                cbIsReadOnly.IsChecked = cbIsReadOnly.IsEnabled = false;
            }

            var oldValue = set.IsGlobalSet;
            var oldUserId = set.UserId;

            Action onError = () =>
            {
                _isUserClicked = false;
                set.UserId = oldUserId;
                //Отмена сохранения 
                cbIsGlobalSet.IsChecked = set.IsGlobalSet = oldValue;
                _isUserClicked = true;
            };

            //Делаем глобальным, или просто пересохраняем свой
            if (isNewGlobalStatus || set.UserId == Manager.User.User_ID)
            {
                set.IsGlobalSet = isNewGlobalStatus;
                SaveSet(set.IsReadOnly, onError);
            }
            else
            {
                var name = string.Empty;
                List<string> names;
                lock (_dictSyncLock)
                {
                    names = _dict.Select(s => s.SetName).ToList();
                }

                for (var i = 1; i <= 10; i++)
                {
                    name = set.SetName + "_" + i;
                    if (!names.Any(n => string.Equals(name, n, StringComparison.InvariantCultureIgnoreCase))) break;
                }

                //Убираем признак локального с чужого диалога
                AddNew(name, "Создать локальную копию. Введите новое название");
            }
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            lock (_dictSyncLock)
            {
                _dict.Clear();
            }

            InitGlobal(_setName, _moduleNameForUse, _modalType, _modalParent);
        }

        public void Dispose()
        {
            lock (_dictSyncLock)
            {
                lvSets.ItemsSource = null;

                if (_lcv != null)
                {
                    _lcv.SortDescriptions.Clear();
                    _lcv = null;
                }

                if (_dict != null)
                {
                    foreach (var d in _dict)
                    {
                        d.Dispose();
                    }

                    _dict.Clear();
                }
            }

            _modalParent = null;
            Changed = null;
            OnSave = null;
        }

        #region Работа с выбором набора

        private volatile bool _isPopupOpen;

        private void selectType_Click(object sender, RoutedEventArgs e)
        {
            if (_isPopupOpen)
            {
                _isPopupOpen = false;
                Manager.UI.CloseAllPopups();
                return;
            }

            popup.OpenAndRegister(false);
            //if (founded != null && founded.Count == 1 && isFirst)
            //{
            //    FreeHierarchyTree_OnLoaded(null, null);
            //    hierarchyTypes.ItemContainerGenerator.ExpandAndSelect(founded.Values.First().Clone() as Stack);
            //}

            _isPopupOpen = true;
        }

        private void PopupOnClosed(object sender, EventArgs e)
        {
            if (!_isPopupOpen) return;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(200);
                _isPopupOpen = false;
            });
        }

        private void OnChangedSet(object sender, MouseButtonEventArgs e)
        {
            if (Changed == null || !IsLoaded || _dict == null) return;

            var fe = sender as FrameworkElement;
            if (fe == null) return;

            var set = fe.DataContext as TSetView;
            tbSelectSet.DataContext = set;

            if (set == null || (set.Set == null && string.IsNullOrEmpty(set.ProtoSerializetSelected)))
            {
                cbIsGlobalSet.IsChecked = cbIsReadOnly.IsChecked = cbIsReadOnly.IsEnabled = false;
                if (set != null) set.IsSelected = true;
                Changed(null, 3);
            }
            else
            {
                set.IsSelected = true;
                cbIsGlobalSet.IsEnabled = bDelete.IsEnabled = bSave.IsEnabled = !set.IsReadOnly;

                var newName = set.SetName;
                //if (string.IsNullOrEmpty(newName)) return;

                set.IsSelected = true;

                if (set.IsUseGlobalTable)
                {
                    Manager.UI.RunAsync(
                        arg => ARM_Service.EXPL_Get_GlobalSet(Manager.User.User_ID, newName,
                            EnumGlobalSetModuleType.HierarchyTree),
                        globalSet =>
                        {
                            if (globalSet == null) return;

                            if (globalSet.VersionNumber < 3)
                            {
                                List<string> existSet;
                                if (!string.IsNullOrEmpty(globalSet.List))
                                {
                                    existSet = globalSet.UseProtoSerializer.GetValueOrDefault()
                                        ? globalSet.List.ProtoDeserializeFromString<List<string>>()
                                        : globalSet.List.DeserializeFromString<List<string>>();
                                }
                                else
                                {
                                    existSet = new List<string>();
                                }

                                cbIsReadOnly.IsChecked = set.IsReadOnly = globalSet.IsReadOnly;
                                set.Set = new HashSet<string>(existSet);
                                Changed(set.Set, set.VersionNumber);
                            }
                            else
                            {
                                Changed(globalSet.List, set.VersionNumber);
                            }
                            
                            cbIsGlobalSet.IsChecked = set.IsGlobalSet;

                            cbIsReadOnly.IsEnabled =
                                set.IsGlobalSet && string.Equals(globalSet.User_ID, Manager.User.User_ID);
                        });
                }
                else
                {
                    if (set.VersionNumber < 3)
                    {
                        Changed(set.Set, set.VersionNumber);
                    }
                    else
                    {
                        Changed(set.ProtoSerializetSelected, set.VersionNumber);
                    }

                    cbIsGlobalSet.IsChecked = false;
                }
            }

            Manager.UI.CloseAllPopups(popup);
            _isPopupOpen = false;
        }

        #endregion

        private void ShowMessage(string message)
        {
            if (_modalType == EnumNameRequestModalType.SimpleModal && _modalParent!=null)
            {
                Manager.UI.ShowLocalMessage(message, _modalParent);
            }
            else
            {
                Manager.UI.ShowMessage(message);
            }
        }
    }
}
