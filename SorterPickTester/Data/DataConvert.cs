using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SorterPickTester.Data
{
    /// <summary>
    /// データ形式変換関数群
    /// </summary>
    public class DataConvert
    {
        /// <summary>
        /// 整数型(int) → Byte配列 変換
        /// </summary>
        /// <param name="input">変換対象のint型データ(上限2byte分)</param>
        /// <returns>返還後のbyte型配列</returns>
        /// <remarks>通信仕様がリトルエンディアンの為、変換関数を用意。入力されたint型データの上位２Byte分は破棄されます。</remarks>
        static public byte[] Integer2Bytes_to_ByteArray(int input)
        {
            byte[] ret_array = new byte[2];

            ret_array[0] = (byte)(input & 0x000000FF);
            ret_array[1] = (byte)((input & 0x0000FF00) >> 8);

            return ret_array;
        }

        /// <summary>
        /// 整数型(short) → Byte配列 変換
        /// </summary>
        /// <param name="input">変換対象のshort型データ</param>
        /// <returns>返還後のbyte型配列</returns>
        /// <remarks>通信仕様がリトルエンディアンの為、変換関数を用意。</remarks>
        static public byte[] Integer2Bytes_to_ByteArray(short input)
        {
            byte[] ret_array = new byte[2];

            ret_array[0] = (byte)(input & 0x00FF);
            ret_array[1] = (byte)((input & 0xFF00) >> 8);

            return ret_array;

        }

        /// <summary>
        /// Byte配列 → 整数型(short) 変換
        /// </summary>
        /// <param name="data">変換対象のbyte型配列データ</param>
        /// <param name="offset">変換対象のbyte型配列データの開始位置</param>
        /// <returns>返還後のshort型データ</returns>
        /// <remarks>通信仕様がリトルエンディアンの為、変換関数を用意。</remarks>
        static public short ByteArray_to_Short(byte[] data, int offset)
        {
            short ret = (short)(data[offset] + (data[offset + 1] << 8));

            return ret;
        }

        /// <summary>
        /// Byte配列 → 整数型(ushort) 変換
        /// </summary>
        /// <param name="data">変換対象のbyte型配列データ</param>
        /// <param name="offset">変換対象のbyte型配列データの開始位置</param>
        /// <returns>返還後のshort型データ</returns>
        /// <remarks>通信仕様がリトルエンディアンの為、変換関数を用意。</remarks>
        static public ushort ByteArray_to_UShort(byte[] data, int offset)
        {
            ushort ret = (ushort)(data[offset] + (data[offset + 1] << 8));

            return ret;
        }

        /// <summary>
        /// SByteデータ Byte配列セット
        /// </summary>
        /// <param name="src">元のデータ</param>
        /// <param name="dst">コピー先のbyte型配列</param>
        /// <param name="offset">コピー先の位置</param>
        static public void Copy_SByte_to_ByteAllay(sbyte src, ref byte[] dst, int offset)
        {
            sbyte[] buf = new sbyte[1];

            buf[0] = src;

            Buffer.BlockCopy(buf, 0, dst, offset, 1);
        }

        /// <summary>
        /// Byte配列データ SByteデータ抽出
        /// </summary>
        /// <param name="src">元のデータ</param>
        /// <param name="offset">配列内の位置</param>
        /// <returns>抽出したSbyte型データ</returns>
        static public sbyte Convert_ByteAllay_to_SByte(byte[] src, int offset)
        {
            sbyte[] buf = new sbyte[1];

            Buffer.BlockCopy(src, offset, buf, 0, 1);

            return (buf[0]);
        }

        /// <summary>
        /// インプットデータセット処理
        /// </summary>
        /// <param name="input">入力文字列</param>
        /// <param name="now_value">現在値</param>
        /// <returns>true:インプットOK false:インプットNG(異常値入力)</returns>
        static public bool Check_UserInputData(string input, ref short now_value)
        {
            short temp = now_value;

            bool bRet = short.TryParse(input, out now_value);

            if (!bRet)
            {
                now_value = temp;
            }

            return bRet;
        }

        /// <summary>
        /// インプットデータセット処理
        /// </summary>
        /// <param name="input">入力文字列</param>
        /// <param name="order">有効小数桁指定</param>
        /// <param name="now_value">現在値</param>
        /// <returns>true:インプットOK false:インプットNG(異常値入力)</returns>
        static public bool Check_UserInputData(string input, byte order, ref short now_value)
        {
            int temp_value;
            double dTemp;
            string str_buf;
            string[] str_split_buf;
            short coefficient;
            short integer_part;
            short decimal_part;

            coefficient = (short)(Math.Pow(10, order));

            bool bRet = double.TryParse(input, out dTemp);

            if (bRet)
            {
                // 入力文字列に小数点が含まれるかチェック.
                if (input.IndexOf('.') > 0)
                {
                    // 処理簡略化の為、入力文字列の末尾を0でパディング.
                    str_buf = input.PadRight(7 + order, '0');

                    // 入力された文字列を小数点を境に分割.
                    str_split_buf = str_buf.Split('.');
                }
                else
                {
                    // インスタンス生成.
                    str_split_buf = new string[1];

                    // 小数点がないので、inputをそのまま整数部としてセット.
                    str_split_buf[0] = input;
                }

                // 整数部を整数型で取得.
                integer_part = short.Parse(str_split_buf[0]);

                // 小数部の文字列が存在するかチェック.
                if (str_split_buf.Length > 1)
                {
                    // 小数部を桁数指定分、整数型で取得.
                    decimal_part = short.Parse(str_split_buf[1].Substring(0, order));
                }
                else
                {
                    // 小数部がないため、小数部＝0.
                    decimal_part = 0;
                }

                // 正負を考慮して、値を変換する.
                if (integer_part >= 0)
                {
                    // 一度int型で値を算出.
                    temp_value = (integer_part * coefficient) + decimal_part;

                    // short型に収まるかチェック.
                    if (temp_value <= short.MaxValue)
                    {
                        // short型に収まる場合は値をセット.
                        now_value = (short)temp_value;
                    }
                    else
                    {
                        // short型に収まらない場合は異常値.
                        bRet = false;
                    }
                }
                else
                {
                    // 一度int型で値を算出.
                    temp_value = (integer_part * coefficient) - decimal_part;

                    // short型に収まるかチェック.
                    if (temp_value >= short.MinValue)
                    {
                        // short型に収まる場合は値をセット.
                        now_value = (short)temp_value;
                    }
                    else
                    {
                        // short型に収まらない場合は異常値.
                        bRet = false;
                    }
                }
            }

            return bRet;
        }

        /// <summary>
        /// インプットデータセット処理
        /// </summary>
        /// <param name="input">入力文字列</param>
        /// <param name="order">桁指定</param>
        /// <param name="now_value">現在値</param>
        /// <returns>true:インプットOK false:インプットNG(異常値入力)</returns>
        /// <summary>
        /// インプットデータセット処理
        /// </summary>
        /// <param name="input">入力文字列</param>
        /// <param name="order">有効小数桁指定</param>
        /// <param name="now_value">現在値</param>
        /// <returns>true:インプットOK false:インプットNG(異常値入力)</returns>
        static public bool Check_UserInputData(string input, byte order, ref sbyte now_value)
        {
            int temp_value;
            double dTemp;
            string str_buf;
            string[] str_split_buf;
            short coefficient;
            sbyte integer_part;
            sbyte decimal_part;

            coefficient = (short)(Math.Pow(10, order));

            bool bRet = double.TryParse(input, out dTemp);

            if (bRet)
            {
                // 入力文字列に小数点が含まれるかチェック.
                if (input.IndexOf('.') > 0)
                {
                    // 処理簡略化の為、入力文字列の末尾を0でパディング.
                    str_buf = input.PadRight(7 + order, '0');

                    // 入力された文字列を小数点を境に分割.
                    str_split_buf = str_buf.Split('.');
                }
                else
                {
                    // インスタンス生成.
                    str_split_buf = new string[1];

                    // 小数点がないので、inputをそのまま整数部としてセット.
                    str_split_buf[0] = input;
                }

                // 整数部を整数型で取得.
                integer_part = sbyte.Parse(str_split_buf[0]);

                if (str_split_buf.Length > 1)
                {
                    // 小数部を桁数指定分、整数型で取得.
                    decimal_part = sbyte.Parse(str_split_buf[1].Substring(0, order));
                }
                else
                {
                    // 小数部は0.
                    decimal_part = 0;
                }


                // 正負を考慮して、値を変換する.
                if (integer_part >= 0)
                {
                    // 一度int型で値を算出.
                    temp_value = (integer_part * coefficient) + decimal_part;

                    // short型に収まるかチェック.
                    if (temp_value <= sbyte.MaxValue)
                    {
                        // short型に収まる場合は値をセット.
                        now_value = (sbyte)temp_value;
                    }
                    else
                    {
                        // short型に収まらない場合は異常値.
                        bRet = false;
                    }
                }
                else
                {
                    // 一度int型で値を算出.
                    temp_value = (integer_part * coefficient) - decimal_part;

                    // short型に収まるかチェック.
                    if (temp_value >= sbyte.MinValue)
                    {
                        // short型に収まる場合は値をセット.
                        now_value = (sbyte)temp_value;
                    }
                    else
                    {
                        // short型に収まらない場合は異常値.
                        bRet = false;
                    }
                }
            }

            return bRet;
        }

        /// <summary>
        /// インプットデータセット処理
        /// </summary>
        /// <param name="input">入力文字列</param>
        /// <param name="now_value">現在値</param>
        /// <returns>true:インプットOK false:インプットNG(異常値入力)</returns>
        static public bool Check_UserInputData(string input, ref byte now_value)
        {
            byte temp = now_value;

            bool bRet = byte.TryParse(input, out now_value);

            if (!bRet)
            {
                now_value = temp;
            }

            return bRet;
        }

        /// <summary>
        /// インプットデータセット処理
        /// </summary>
        /// <param name="input">入力文字列</param>
        /// <param name="now_value">現在値</param>
        /// <returns>true:インプットOK false:インプットNG(異常値入力)</returns>
        static public bool Check_UserInputData(string input, ref sbyte now_value)
        {
            sbyte temp = now_value;

            bool bRet = sbyte.TryParse(input, out now_value);

            if (!bRet)
            {
                now_value = temp;
            }

            return bRet;
        }
    }
}
