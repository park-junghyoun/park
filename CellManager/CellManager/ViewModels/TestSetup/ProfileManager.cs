using System;
using System.Collections.ObjectModel;
using System.Linq;
using CellManager.ViewModels;

namespace CellManager.ViewModels.TestSetup
{
    public class ProfileManager<T> where T : class
    {
        private readonly ProfileKind _kind;
        private readonly Func<T> _factory;
        private readonly Func<ObservableCollection<T>> _getCollection;
        private readonly Func<T> _getSelected;
        private readonly Action<T> _setSelected;
        private readonly Action<T, int> _save;
        private readonly Action<T> _delete;
        private readonly Func<T, int> _getId;
        private readonly Func<T, string> _getName;
        private readonly Func<int> _getCellId;
        private readonly Action _reloadAll;
        private readonly Action<ProfileKind> _setActiveEditor;
        private readonly Action _notifyCanExecutes;

        public ProfileManager(
            ProfileKind kind,
            Func<T> factory,
            Func<ObservableCollection<T>> getCollection,
            Func<T> getSelected,
            Action<T> setSelected,
            Action<T, int> save,
            Action<T> delete,
            Func<T, int> getId,
            Func<T, string> getName,
            Func<int> getCellId,
            Action reloadAll,
            Action<ProfileKind> setActiveEditor,
            Action notifyCanExecutes)
        {
            _kind = kind;
            _factory = factory;
            _getCollection = getCollection;
            _getSelected = getSelected;
            _setSelected = setSelected;
            _save = save;
            _delete = delete;
            _getId = getId;
            _getName = getName;
            _getCellId = getCellId;
            _reloadAll = reloadAll;
            _setActiveEditor = setActiveEditor;
            _notifyCanExecutes = notifyCanExecutes;
        }

        public void Add()
        {
            var profile = _factory();
            _setSelected(profile);
            _getCollection().Add(profile);
            _setActiveEditor(_kind);
            _notifyCanExecutes();
        }

        public void Save()
        {
            var selected = _getSelected();
            var prevId = selected != null ? _getId(selected) : 0;
            var prevName = selected != null ? _getName(selected) : null;
            _save(selected, _getCellId());
            _reloadAll();

            var collection = _getCollection();
            if (prevId > 0) _setSelected(collection.FirstOrDefault(p => _getId(p) == prevId));
            if (_getSelected() == null && !string.IsNullOrWhiteSpace(prevName))
                _setSelected(collection.LastOrDefault(p => _getName(p) == prevName));
            if (_getSelected() == null) _setSelected(collection.LastOrDefault());

            _setActiveEditor(_kind);
            _notifyCanExecutes();
        }

        public void Delete()
        {
            var selected = _getSelected();
            if (selected == null) return;
            _delete(selected);
            _reloadAll();
            _setActiveEditor(_kind);
        }
    }
}
