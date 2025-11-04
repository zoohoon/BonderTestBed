using System;
using System.Collections.ObjectModel;

namespace LoaderParameters
{
    /// <summary>
    /// ObservableCollectionExtension
    /// </summary>
    public static class ObservableCollectionExtension
    {
        /// <summary>
        /// ICloneable의 ObservableCollection의 복사본을 반환합니다.
        /// </summary>
        /// <typeparam name="T">ICloneable을 구현한 오브젝트 타입</typeparam>
        /// <param name="collection">ICloneable을 갖는 ObservableCollection</param>
        /// <returns>복사본</returns>
        public static ObservableCollection<T> CloneFrom<T>(this ObservableCollection<T> collection)
         where T : ICloneable
        {
            ObservableCollection<T> coll = new ObservableCollection<T>();
            foreach (var item in collection)
            {
                coll.Add((T)item.Clone());
            }
            return coll;
        }
    }
}
