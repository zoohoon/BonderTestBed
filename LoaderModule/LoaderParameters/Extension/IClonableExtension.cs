using System;

namespace LoaderParameters
{
    /// <summary>
    /// IClonableExtension 을 정의합니다.
    /// </summary>
    public static class IClonableExtension
    {
        /// <summary>
        /// ICloneable을 구현한 오브젝트의 복사본을 지정된 타입으로 반환합니다.
        /// </summary>
        /// <typeparam name="T">오브젝트 타입</typeparam>
        /// <param name="obj">오브젝트</param>
        /// <returns>복사본</returns>
        public static T Clone<T>(this ICloneable obj)
        {
            return (T)obj.Clone();
        }
    }
}
