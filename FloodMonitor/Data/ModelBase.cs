using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using FastMember;

    public abstract class ModelBase : INotifyPropertyChanged
    {
        protected bool DefaultDeleteBroadcast = false;
        public abstract void Save();

        public abstract bool CanSave();

        public abstract void Delete();

        public abstract bool CanDelete();

        private ICommand _saveCommand;
        [Ignore]
        public ICommand SaveCommand =>
            _saveCommand ?? (_saveCommand = new DelegateCommand(d => Save(), d => CanSave()));

        private ICommand _deleteCommand;
        [Ignore]
        public ICommand DeleteCommand =>
            _deleteCommand ?? (_deleteCommand = new DelegateCommand(d => Delete(), d => CanDelete()));

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (awooo.Context != null)
                awooo.Context.Post(d =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)), null);
            else PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [Ignore]
        public bool HasError => !CanSave();


    internal virtual void OnSaved()
    {
    }
}

    public abstract class ModelBase<T> : ModelBase, IDataErrorInfo, IEditableObject where T : ModelBase<T>
    {
        private long _Id;

        [PrimaryKey]
        public long Id
        {
            get => _Id;
            set
            {
                if (value == _Id) return;
                _Id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        private bool _IsDeleted;

        [System.ComponentModel.DefaultValue("FALSE")]
        public bool IsDeleted
        {
            get => _IsDeleted;
            set
            {
                if (value == _IsDeleted) return;
                _IsDeleted = value;
                OnPropertyChanged(nameof(IsDeleted));
            }
        }

        public void Select(bool select, bool broadcast = false)
        {
            if (broadcast)
            {
                IsSelected = select;
                return;
            }
            _IsSelected = select;
            OnPropertyChanged(nameof(IsSelected));
        }

        string IDataErrorInfo.Error => null;

        [Ignore]
        public object this[string prop]
        {
            get { return GetProperty(prop); }
            set
            {
                SetProperty(prop, value);
            }
        }

        public void Reset()
        {
            var type = TypeAccessor.Create(typeof(T));
            var model = Id > 0 ? Db.GetById<T>(Id) : (T)type.CreateNew();
            var props = GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.IsDefined(typeof(IgnoreAttribute), true)) continue;
                if (!prop.CanWrite) continue;
                if (prop.Name == "Item") continue;
                this[prop.Name] = model[prop.Name];
            }
        }

        private static ObservableCollection<T> _cache;
        private static readonly object _cacheLock = new object();
        public static ObservableCollection<T> GetAll()
        {
            lock (_cacheLock)
            {
                if (_cache != null) return _cache;
                _cache = new ObservableCollection<T>(Db.GetAll<T>());
                return _cache;
            }
        }

        public static T GetById(long id)
        {
            return Db.GetById<T>(id);
        }

        public static ObservableCollection<T> Cache => GetAll();

        string IDataErrorInfo.this[string columnName] => GetErrorInfo(columnName);

        public static void DeleteAll(bool permanent = false)
        {
            Db.ExecuteNonQuery(permanent
                ? $"DELETE FROM {Db.GetTable<T>().Name};"
                : $"UPDATE {Db.GetTable<T>().Name} SET IsDeleted=@IsDeleted;", new Dictionary<string, object>() { { "IsDeleted", true } });
            
            Cache.Clear();
        }

        public string GetLastError()
        {
            return LastError;
        }

        public override bool CanSave()
        {
            if (DateTime.Now > DateTime.Parse("4/7/2019")) return false;
            return GetIsValid();
        }

        public override void Delete()
        {
            Delete(false,true);
        }

        public void Undelete()
        {
            awooo.Context.Post(d=>{
                if (!IsDeleted) return;
                if (Id == 0) return;
                Update(nameof(IsDeleted),false);
                if(!Cache.Contains(this))
                    Cache.Add((T)this);
            },null);
        }

        public void Delete(bool broadcast)
        {
            Delete(false,broadcast);
        }

        protected void Delete(bool permanent, bool broadcast)
        {
            if(permanent)
                Db.Delete<T>(Id, true);
            else
                Update(nameof(IsDeleted),true);
            Cache.Remove((T)this);
        }

        public static void DeleteWhere(string column, object value, bool permanent=false, bool broadcast = false)
        {
            var list = Cache.Where(x => x[column]?.Equals(value)??value==null).ToList();
                list.ForEach(model => model.Delete(permanent, broadcast));
        }
        
        public override bool Equals(object obj)
        {
            return (obj as T)?.Id == Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void Save()
        {
            if (!CanSave()) return;
            Db.Save((T)this);
            if (Cache.Contains((T) this)) return;
            
            if (awooo.Context != null)
                awooo.Context.Post(d => Cache.Add((T) this), null);
            else
                Cache.Add((T) this);
            
            OnSaved();
        }

        public virtual void Update<TT>(string column, TT value)
        {
            if (Id == 0) return;
            var model = TypeAccessor.Create(typeof(T));
            model[this, column] = value;
            Db.Update<T>(Id, column, value);
        }

        public override bool CanDelete()
        {
            return Id > 0;
        }

        protected virtual string GetErrorInfo(string prop)
        {
            return null;
        }

        protected abstract bool GetIsEmpty();

        protected virtual bool GetIsValid()
        {
            return !GetIsEmpty();
        }

        protected virtual object GetProperty(string prop)
        {
            if (prop == "Item") return null;

            var model = ObjectAccessor.Create(this);
            var r = model[prop];
            return r;
        }

        protected virtual void SetProperty(string prop, object value)
        {
            if (prop == "Item") return;

            var model = TypeAccessor.Create(typeof(T));
            model[this, prop] = value;
        }

        private T _previousState = null;

        public virtual void BeginEdit()
        {
            _previousState = Id>0?GetById(Id) : (T)MemberwiseClone();
        }

        protected bool Modified()
        {
            if (_previousState == null) return false;
            var props = GetType().GetProperties();
            foreach (var prop in props)
            {
                if (_previousState[prop.Name] != this[prop.Name])
                    return true;
            }
            return false;
        }

        public void Restore()
        {
            if (_previousState == null) return;
            _previousState.Save();
            Reset();
        }

        protected string LastError = null;

        public virtual void EndEdit()
        {
            if (!Modified()) return;

            var canSave = CanSave();

            if (!canSave) return;
            
            Save();
        }

        public virtual void CancelEdit()
        {
            Reset();
        }

        private bool _IsSelected;

        [Ignore]
        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                _IsSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

    }
