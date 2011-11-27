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
    public abstract class PecoBase
    {
        private List<PropertyInfo> _propertyList;
        private Dictionary<string, int> _indexTable;

        public PecoBase()
        {
            PropertyInfo[] allProperties;

            //プロパティ一覧取得
            allProperties = this.GetType().GetProperties(BindingFlags.Instance |
                                                            BindingFlags.Public);

            this._propertyList = new List<PropertyInfo>();

            //プロパティリスト、プロパティ名と配列位置の関連付けハッシュを作成
            this._indexTable = new Dictionary<string, int>();

            foreach (PropertyInfo prop in allProperties)
            {
                //呼び出しにパラメータが必要なプロパティは対象外
                if (prop.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                this._propertyList.Add(prop);
                this._indexTable.Add(prop.Name, this._propertyList.Count - 1);
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
                var prop = this._getPropertyInfo(key);
                return prop.GetValue(this, null);
            }

            set
            {
                var prop = this._getPropertyInfo(key);
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
                var prop = this._getPropertyInfo(index);
                return prop.GetValue(this, null);
            }

            set
            {
                var prop = this._getPropertyInfo(index);
                prop.SetValue(this, value, null);
            }
        }

        /// <summary>
        /// イテレータ
        /// </summary>
        /// <returns>プロパティの値</returns>
        public IEnumerator<object> GetEnumerator()
        {
            foreach (var prop in this._propertyList)
            {
                yield return prop.GetValue(this, null);
            }
        }

        /// <summary>
        /// プロパティ取得
        /// </summary>
        /// <param name="index">PropertyInfo配列上の位置</param>
        /// <returns>指定された位置のPropertyInfo</returns>
        private PropertyInfo _getPropertyInfo(int index)
        {
            //インデックスが不正な場合、例外をスロー
            if (index < 0 || index > this._propertyList.Count - 1)
            {
                throw new IndexOutOfRangeException();
            }

            return this._propertyList[index];
        }

        /// <summary>
        /// プロパティ取得
        /// </summary>
        /// <param name="key">プロパティ名</param>
        /// <returns>指定されたプロパティのPropertyInfo</returns>
        private PropertyInfo _getPropertyInfo(string key)
        {
            //指定されたプロパティが存在しない場合、例外をスロー
            if (!this._indexTable.ContainsKey(key))
            {
                throw new KeyNotFoundException();
            }

            return this._propertyList[this._indexTable[key]];
        }

        /// <summary>
        /// 項目数取得
        /// </summary>
        /// <returns>項目の数</returns>
        public int ItemCount()
        {
            return this._propertyList.Count;
        }

        /// <summary>
        /// 項目名一覧取得
        /// </summary>
        /// <returns>項目名の配列</returns>
        public string[] ItemNames()
        {
            return this._indexTable.Keys.ToArray();
        }

        public Type GetItemType(int index)
        {
            return this._getPropertyInfo(index).PropertyType;
        }

        public Type GetItemType(string key)
        {
            return this._getPropertyInfo(key).PropertyType;
        }
    }
}
