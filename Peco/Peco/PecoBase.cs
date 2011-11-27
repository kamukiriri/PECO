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
        private delegate object GetMethod(object target);
        private delegate void SetMethod(object target, object value);

        private static List<GetMethod> _getMethodList;
        private static List<SetMethod> _setMethodList;
        private static List<Type> _typeList;
        private static Dictionary<string, int> _indexTable;

        private static int _propertyCount;

        static PecoBase()
        {
            PropertyInfo[] allProperties;

            //プロパティ一覧取得
            allProperties = typeof(T).GetProperties(BindingFlags.Instance |
                                                            BindingFlags.Public);

            //プロパティ情報保持リスト初期化
            _getMethodList = new List<GetMethod>();
            _setMethodList = new List<SetMethod>();
            _typeList = new List<Type>();

            //プロパティリスト、プロパティ名と配列位置の関連付けハッシュを作成
            _indexTable = new Dictionary<string, int>();
            _propertyCount = 0;

            foreach (PropertyInfo prop in allProperties)
            {
                //呼び出しにパラメータが必要なプロパティは対象外
                if (prop.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                //アクセサキャッシュメソッド生成
                MethodInfo getInfo = prop.GetGetMethod();
                Type getDelegateType = typeof(Func<,>).MakeGenericType(prop.DeclaringType, prop.PropertyType);
                Delegate getOrg = Delegate.CreateDelegate(getDelegateType, getInfo);
                
                MethodInfo setInfo = prop.GetSetMethod();
                Type setDelegateType = typeof(Action<,>).MakeGenericType(prop.DeclaringType, prop.PropertyType);
                Delegate setOrg = Delegate.CreateDelegate(setDelegateType, setInfo);

                //動的に生成したジェネリックメソッドを使用する為に、ジェネリッククラスとインターフェースで仲介する
                Type accessorType = typeof(AccessorCache<,>).MakeGenericType(prop.DeclaringType, prop.PropertyType);
                IAccessorCache accessor = (IAccessorCache)Activator.CreateInstance(accessorType, getOrg, setOrg);
                
                _getMethodList.Add(accessor.Get);
                _setMethodList.Add(accessor.Set);
                _typeList.Add(prop.PropertyType);
                _indexTable.Add(prop.Name, _propertyCount++);
            }

        }

        /// <summary>インデクサ(string)</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                var method = this._getListItem(_getMethodList, key);
                return method(this);
            }

            set
            {
                var method = this._getListItem(_setMethodList, key);
                method(this, value);
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
                var method = this._getListItem(_getMethodList, index);
                return method(this);
            }

            set
            {
                var method = this._getListItem(_setMethodList, index);
                method(this, value);
            }
        }

        /// <summary>
        /// イテレータ
        /// </summary>
        /// <returns>プロパティの値</returns>
        public IEnumerator<object> GetEnumerator()
        {
            foreach (var get in _getMethodList)
            {
                yield return get(this);
            }
        }

        /// <summary>
        /// リストからメソッドを取得
        /// </summary>
        /// <param name="index">PropertyInfo配列上の位置</param>
        /// <returns>指定された位置のPropertyInfo</returns>
        private TList _getListItem<TList>(List<TList> list, int index)
        {
            //インデックスが不正な場合、例外をスロー
            if (index < 0 || index > list.Count - 1)
            {
                throw new IndexOutOfRangeException();
            }

            return list[index];
        }

        /// <summary>
        /// リストからメソッドを取得
        /// </summary>
        /// <param name="key">プロパティ名</param>
        /// <returns>指定されたプロパティのPropertyInfo</returns>
        private TList _getListItem<TList>(List<TList> list, string key)
        {
            //指定されたプロパティが存在しない場合、例外をスロー
            if (!_indexTable.ContainsKey(key))
            {
                throw new KeyNotFoundException();
            }

            return list[_indexTable[key]];
        }

        /// <summary>
        /// 項目数取得
        /// </summary>
        /// <returns>項目の数</returns>
        public int ItemCount()
        {
            return _propertyCount;
        }

        /// <summary>
        /// 項目名一覧取得
        /// </summary>
        /// <returns>項目名の配列</returns>
        public string[] ItemNames()
        {
            return _indexTable.Keys.ToArray();
        }

        /// <summary>
        /// 項目の型を取得
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Type GetItemType(int index)
        {
            return _getListItem(_typeList, index);
        }

        /// <summary>
        /// 項目の型を取得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Type GetItemType(string key)
        {
            return _getListItem(_typeList, key);
        }
    }

    /// <summary>
    /// アクセサーキャッシュアクセス用インターフェース
    /// </summary>
    internal interface IAccessorCache
    {
        object Get(object target);
        void Set(object target, object value);
    }

    /// <summary>
    /// アクセサキャッシュクラス
    /// </summary>
    /// <typeparam name="TTarget">オブジェクトの型</typeparam>
    /// <typeparam name="TProperty">プロパティの型</typeparam>
    internal class AccessorCache<TTarget, TProperty> : IAccessorCache
    {
        private Func<TTarget, TProperty> _getMethod;
        private Action<TTarget, TProperty> _setMethod;

        public AccessorCache(Func<TTarget, TProperty> getMethod, Action<TTarget, TProperty> setMethod)
        {
            _getMethod = getMethod;
            _setMethod = setMethod;
        }

        public object Get(object target)
        {
            return (object)_getMethod((TTarget)target);
        }

        public void Set(object target, object value)
        {
            _setMethod((TTarget)target, (TProperty)value);
        }
    }
}
