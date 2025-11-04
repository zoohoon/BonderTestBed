using System;

namespace LoaderParameters
{
    /// <summary>
    /// ArrayExtension 을 정의합니다.
    /// </summary>
    public static class ArrayExtension
    {
        /// <summary>
        /// ICloneable의 Array Collection의 복사본을 반환합니다.
        /// </summary>
        /// <typeparam name="T">ICloneable을 구현한 오브젝트 타입</typeparam>
        /// <param name="collection">ICloneable을 갖는 배열</param>
        /// <returns>복사된 배열</returns>
        public static T[] CloneFrom<T>(this T[] collection)
         where T : ICloneable
        {
            T[] coll = new T[collection.Length];
            for (int i = 0; i < collection.Length; i++)
            {
                coll[i] = (T)collection[i].Clone();
            }
            return coll;
        }
    }
}
