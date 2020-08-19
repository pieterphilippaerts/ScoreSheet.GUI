using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PieterP.Shared;

namespace PieterP.ScoreSheet.Model.Database {
    public abstract class AbstractDatabase<T> where T : new() {
        public AbstractDatabase(string filename, bool isFullPath = false) {
            if (isFullPath) {
                var fi = new FileInfo(filename);
                Filename = fi.Name;
                FullPath = filename;
            } else {
                Filename = filename;
                FullPath = Path.Combine(DatabaseManager.Current.ActiveProfilePath, filename);
            }

            if (File.Exists(FullPath)) {
                using (var tr = new StreamReader(FullPath)) {
                    var json = tr.ReadToEnd();
                    this.Database = DataSerializer.Deserialize<T>(json);
                }
            } else {
                Database = new T();
            }
            Initialize();
        }
        public void Save() {
            var jsonBytes = DataSerializer.SerializeToBytes(Database);
            using (var stream = File.Create(FullPath)) {
                stream.Write(jsonBytes, 0, jsonBytes.Length);
            }
        }

#region GetProp
        protected string GetProp(Func<string?> f, string def = "") {
            var p = f();
            if (p == null)
                return def;
            else
                return p;
        }
        protected bool GetProp(Func<bool?> f, bool def = false) {
            var p = f();
            if (p == null)
                return def;
            else
                return p.Value;
        }
        protected int GetProp(Func<int?> f, int def = 0) {
            var p = f();
            if (p == null)
                return def;
            else
                return p.Value;
        }
        protected float GetProp(Func<float?> f, float def = 0) {
            var p = f();
            if (p == null)
                return def;
            else
                return p.Value;
        }
#endregion

#region SetProp
        protected void SetProp(string value, Action<string> f) {
            f(value);
            Save();
        }
        protected void SetProp(bool value, Action<bool> f) {
            f(value);
            Save();
        }
        protected void SetProp(int value, Action<int> f) {
            f(value);
            Save();
        }
        protected void SetProp(float value, Action<float> f) {
            f(value);
            Save();
        }
#endregion

        internal T Database { get; private set; }
        protected string Filename { get; private set; }
        protected string FullPath { get; private set; }

        internal void Update(T data) {
            this.Database = data;
            Initialize();
            Save();
            RaiseDataUpdated();
        }
        protected virtual void Initialize() { }
        protected void RaiseDataUpdated() {
            DataUpdated?.Invoke(this);
        }

        public event Action<AbstractDatabase<T>> DataUpdated;
    }
}