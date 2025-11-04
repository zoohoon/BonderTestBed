using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberDevelopPackWindow.Tab.Run.Gem
{
    public class SecsMessageHelper
    {
        // long nObjectID의 경우, 기존 파싱 로직을 그대로 이용하기 위함.

        private Queue<SecsItem> itemsQueue;
        private bool isFirstListCall = true;

        public SecsMessageHelper(SecsItem data)
        {
            if (data is SecsItemList itemList)
            {
                this.itemsQueue = new Queue<SecsItem>(itemList.Items);
            }
            else
            {
                throw new ArgumentException("Data is not a list.", nameof(data));
            }
        }

        public long GetList(long nObjectID, ref long pnItemCount)
        {
            // isFirstListCall 논리는 현재 문제와 관련이 없으므로 생략합니다.
            // 중첩된 리스트를 처리하는 더 구체적인 로직 필요

            if (isFirstListCall)
            {
                isFirstListCall = false;

                pnItemCount = this.itemsQueue.Count;
                return pnItemCount; // 최초 호출 시, 전체 리스트 항목의 개수 반환
            }
            else
            {
                if (this.itemsQueue.Count > 0 && this.itemsQueue.Peek() is SecsItemList list)
                {
                    this.itemsQueue.Dequeue(); // 현재 처리 중인 리스트를 큐에서 제거
                    this.itemsQueue = new Queue<SecsItem>(list.Items.Concat(this.itemsQueue)); // 내부 리스트 항목을 현재 큐 앞에 추가

                    pnItemCount = list.Items.Count;
                    return pnItemCount;
                }
                else
                {
                    throw new InvalidOperationException("Expected a list but found another item type or queue is empty.");
                }
            }
        }

        

        public string GetAscii(long nObjectID, ref string psValue)
        {
            if (itemsQueue.Count > 0 && itemsQueue.Peek() is SecsItemAscii asciiItem)
            {
                itemsQueue.Dequeue();
                psValue = asciiItem.Value;
                return psValue;
            }
            throw new InvalidOperationException("Next item is not of type Ascii.");
        }

        public byte[] GetU1(long nObjectID, ref byte[] pnValue)
        {
            if (itemsQueue.Count > 0 && itemsQueue.Peek() is SecsItemU1 u1Item)
            {
                itemsQueue.Dequeue();
                pnValue = u1Item.Value;
                return pnValue;
            }
            throw new InvalidOperationException("Next item is not of type U1.");
        }
        public uint[] GetU2(long nObjectID, ref uint[] prValue)
        {
            if (itemsQueue.Count > 0 && itemsQueue.Peek() is SecsItemU2 u2Item)
            {
                itemsQueue.Dequeue();

                prValue = u2Item.Value;
                return prValue;
            }
            throw new InvalidOperationException("Next item is not of type U2.");
        }

        public uint[] GetU4(long nObjectID, ref uint[] prValue)
        {
            if (itemsQueue.Count > 0 && itemsQueue.Peek() is SecsItemU4 u4Item)
            {
                itemsQueue.Dequeue();

                prValue = u4Item.Value;
                return prValue;
            }
            throw new InvalidOperationException("Next item is not of type U4.");
        }

        public long MakeObject(ref long pnMsgId)
        {
            pnMsgId = 0;
            return pnMsgId;
        }

        public long SetListItem(long nMsgId, long nItemCount)
        {
            return 0;
        }

        public long SetBinaryItem(long nMsgId, byte nValue)
        {
            return 0;
        }

        public long SetBoolItem(long nMsgId, bool nValue)
        {
            return 0;
        }
    }
}
