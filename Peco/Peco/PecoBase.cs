using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Util.Data.Peco
{
    /// <summary>
    /// プロパティ列挙可能抽象クラス
    /// </summary>
    /// <remarks>
    /// Property Enumerable CLR Object
    /// 派生クラスにPublicなプロパティを列挙する機能を提供する
    /// </remarks>
    public abstract class PecoBase<T>
        where T:new()
    {
        private static List<PropertyInfo> _propertyList;
        private static Dictionary<string, int> _indexTable;

        static PecoBase()
        {
            PropertyInfo[] allProperties;

            //プロパティ一覧取得
            allProperties = typeof(T).GetProperties(BindingFlags.Instance |
                                                            BindingFlags.Public);

            _propertyList = new List<PropertyInfo>();

            //プロパティリスト、プロパティ名と配列位置の関連付けハッシュを作成
            _indexTable = new Dictionary<string, int>();

            foreach (PropertyInfo prop in allProperties)
            {
                //呼び出しにパラメータが必要なプロパティは対象外
                if (prop.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                _propertyList.Add(prop);
                _indexTable.Add(prop.Name, _propertyList.Count - 1);
            }
        }

        /// <summary>
        /// インデクサ(string)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                var prop = _getPropertyInfo(key);
                return prop.GetValue(this, null);
            }

            set
            {
                var prop = _getPropertyInfo(key);
                prop.SetValue(this, value, null);
            }
        }

        /// <summary>
        /// インデクサ(int)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object this[int index]
        {
            get
            {
                var prop = _getPropertyInfo(index);
                return prop.GetValue(this, null);
            }

            set
            {
                var prop = _getPropertyInfo(index);
                prop.SetValue(this, value, null);
            }
        }

        /// <summary>
        /// イテレータ
        /// </summary>
        /// <returns>プロパティの値</returns>
        public IEnumerator<object> GetEnumerator()
        {
            foreach (var prop in _propertyList)
            {
                yield return prop.GetValue(this, null);
            }
        }

        /// <summary>
        /// プロパティ取得
        /// </summary>
        /// <param name="index">PropertyInfo配列上の位置</param>
        /// <returns>指定された位置のPropertyInfo</returns>
        private static PropertyInfo _getPropertyInfo(int index)
        {
            //インデックスが不正な場合、例外をスロー
            if (index < 0 || index > _propertyList.Count - 1)
            {
                throw new IndexOutOfRangeException();
            }

            return _propertyList[index];
        }

        /// <summary>
        /// プロパティ取得
        /// </summary>
        /// <param name="key">プロパティ名</param>
        /// <returns>指定されたプロパティのPropertyInfo</returns>
        private static  PropertyInfo _getPropertyInfo(string key)
        {
            //指定されたプロパティが存在しない場合、例外をスロー
            if (!_indexTable.ContainsKey(key))
            {
                throw new KeyNotFoundException();
            }

            return _propertyList[_indexTable[key]];
        }

        /// <summary>
        /// 項目数取得
        /// </summary>
        /// <returns>項目の数</returns>
        public int ItemCount()
        {
            return _propertyList.Count;
        }

        /// <summary>
        /// 項目名一覧取得
        /// </summary>
        /// <returns>項目名の配列</returns>
        public string[] ItemNames()
        {
            return _indexTable.Keys.ToArray();
        }

        public Type GetItemType(int index)
        {
            return _getPropertyInfo(index).PropertyType;
        }

        public Type GetItemType(string key)
        {
            return _getPropertyInfo(key).PropertyType;
        }
    }
}
