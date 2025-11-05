using LogModule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProberDevelopPackWindow.Tab.Run.Gem
{
    public class SecsMessage : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _Stream;
        public int Stream
        {
            get { return _Stream; }
            set
            {
                _Stream = value;
                RaisePropertyChanged();
            }
        }

        private int _Function;
        public int Function
        {
            get { return _Function; }
            set
            {
                _Function = value;
                RaisePropertyChanged();
            }
        }

        private bool _WBit;
        public bool WBit
        {
            get { return _WBit; }
            set
            {
                _WBit = value;
                RaisePropertyChanged();
            }
        }

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set
            {
                _Description = value;
                RaisePropertyChanged();
            }
        }

        private string _CommandName;
        public string CommandName
        {
            get { return _CommandName; }
            set
            {
                _CommandName = value;
                RaisePropertyChanged();
            }
        }

        private SecsItem _Data;
        public SecsItem Data
        {
            get { return _Data; }
            set
            {
                _Data = value;
                RaisePropertyChanged();
            }
        }

        public static SecsMessage Parse(string message)
        {
            var secsMessage = new SecsMessage();

            try
            {
                message = message.Trim();

                // CommandName
                var lines = message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.Contains("/* RCMD */"))
                    {
                        // Extract the command name from this line
                        var commandNameMatch = Regex.Match(trimmedLine, @"<A\s+\""(.*?)\"">\s*/\*\s*RCMD\s*\*/");
                        if (commandNameMatch.Success)
                        {
                            secsMessage.CommandName = commandNameMatch.Groups[1].Value;
                            break; // Assuming only one RCMD per message
                        }
                    }
                }

                var firstLineEndIndex = message.IndexOf('\n');
                if (firstLineEndIndex != -1)
                {
                    var firstLine = message.Substring(0, firstLineEndIndex).Trim();
                    ParseHeader(firstLine, out int stream, out int function, out bool wBit, out string description);

                    secsMessage.Stream = stream;
                    secsMessage.Function = function;
                    secsMessage.WBit = wBit;
                    secsMessage.Description = description;
                }

                var startIndex = message.IndexOf("<L");
                if (startIndex != -1)
                {
                    var listContent = message.Substring(startIndex);
                    secsMessage.Data = ParseItems(listContent);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return secsMessage;
        }
        private static void ParseHeader(string headerLine, out int stream, out int function, out bool wBit, out string description)
        {
            stream = 0;
            function = 0;
            wBit = false;
            description = string.Empty;

            try
            {
                var match = Regex.Match(headerLine, @"S(\d+)F(\d+)(W)?");

                if (match.Success)
                {
                    stream = int.Parse(match.Groups[1].Value);
                    function = int.Parse(match.Groups[2].Value);
                    wBit = match.Groups[3].Value == "W";
                }

                var descriptionMatch = Regex.Match(headerLine, @"/\*(.*?)\*/");
                description = descriptionMatch.Success ? descriptionMatch.Groups[1].Value.Trim() : "";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public static SecsItem ParseItems(string input)
        {
            int index = 0;
            return ParseItemsInternal(input, ref index);
        }
        private static SecsItem ParseItemsInternal(string input, ref int index)
        {
            while (index < input.Length)
            {
                SkipWhitespaceAndComments(input, ref index);

                if (index < input.Length && input[index] == '<')
                {
                    if (input[index + 1] == 'L')
                    {
                        index += 2; // '<L' 넘기기
                        var itemList = SecsItem.Create(SecsItemType.List);
                        SkipWhitespaceAndComments(input, ref index);

                        while (index < input.Length && input[index] != '>')
                        {
                            var item = ParseItemsInternal(input, ref index);

                            (itemList as SecsItemList).AddItem(item);
                            SkipWhitespaceAndComments(input, ref index);
                        }

                        index++; // '>' 넘기기

                        return itemList;
                    }
                    else
                    {
                        index++; // '<' 넘기기

                        var endTagIndex = input.IndexOf(' ', index);
                        if (endTagIndex == -1) // 공백이 없는 경우 '>' 검색
                        {
                            endTagIndex = input.IndexOf('>', index);
                        }
                        if (endTagIndex == -1)
                        {
                            throw new Exception("Invalid format: '>' not found.");
                        }

                        var tag = input.Substring(index, endTagIndex - index);
                        index = endTagIndex;
                        SkipWhitespaceAndComments(input, ref index);

                        if (tag.StartsWith("A"))
                        {
                            var value = ExtractValue(input, ref index);
                            return SecsItem.Create(SecsItemType.Ascii, value);
                        }
                        else if (tag.StartsWith("U1"))
                        {
                            SkipWhitespaceAndComments(input, ref index); // 숫자 값 시작 전 공백 및 주석 건너뛰기

                            var valueEndIndex = input.IndexOf('>', index);
                            if (valueEndIndex == -1)
                            {
                                throw new Exception("Invalid format: '>' not found for U1 value.");
                            }

                            var valueStr = input.Substring(index, valueEndIndex - index).Trim(); // 숫자 값 추출 및 공백 제거
                            if (!byte.TryParse(valueStr, out byte value))
                            {
                                throw new FormatException($"Unable to parse '{valueStr}' as a uint.");
                            }

                            index = valueEndIndex + 1; // '>' 넘기고 다음 위치로
                            return SecsItem.Create(SecsItemType.U1, value);
                        }
                        else if (tag.StartsWith("U2"))
                        {
                            SkipWhitespaceAndComments(input, ref index); // 숫자 값 시작 전 공백 및 주석 건너뛰기

                            var valueEndIndex = input.IndexOf('>', index);
                            if (valueEndIndex == -1)
                            {
                                throw new Exception("Invalid format: '>' not found for U2 value.");
                            }

                            var valueStr = input.Substring(index, valueEndIndex - index).Trim(); // 숫자 값 추출 및 공백 제거
                            if (!uint.TryParse(valueStr, out uint value))
                            {
                                throw new FormatException($"Unable to parse '{valueStr}' as a uint.");
                            }

                            index = valueEndIndex + 1; // '>' 넘기고 다음 위치로
                            return SecsItem.Create(SecsItemType.U2, value);
                        }
                        else if (tag.StartsWith("U4"))
                        {
                            SkipWhitespaceAndComments(input, ref index); // 숫자 값 시작 전 공백 및 주석 건너뛰기

                            var valueEndIndex = input.IndexOf('>', index);
                            if (valueEndIndex == -1)
                            {
                                throw new Exception("Invalid format: '>' not found for U4 value.");
                            }

                            var valueStr = input.Substring(index, valueEndIndex - index).Trim(); // 숫자 값 추출 및 공백 제거
                            if (!uint.TryParse(valueStr, out uint value))
                            {
                                throw new FormatException($"Unable to parse '{valueStr}' as a uint.");
                            }

                            index = valueEndIndex + 1; // '>' 넘기고 다음 위치로
                            return SecsItem.Create(SecsItemType.U4, value);
                        }
                    }
                }
                else
                {
                    // '<'가 없는 경우는 입력이 잘못되었거나 끝났을 경우임
                    break;
                }
            }

            throw new Exception("Invalid input format or end of input reached unexpectedly.");
        }
        private static void SkipWhitespaceAndComments(string input, ref int index)
        {
            while (index < input.Length)
            {
                if (char.IsWhiteSpace(input[index]))
                {
                    index++;
                }
                else if (input[index] == '/' && index + 1 < input.Length && input[index + 1] == '*')
                {
                    // 주석 시작 발견
                    index += 2; // 주석 시작 기호 건너뛰기
                    while (index + 1 < input.Length && !(input[index] == '*' && input[index + 1] == '/'))
                    {
                        index++; // 주석 끝까지 이동
                    }
                    index += 2; // 주석 종료 기호 건너뛰기
                }
                else
                {
                    break; // 공백이나 주석이 아닌 경우 루프 종료
                }
            }
        }
        private static string ExtractValue(string input, ref int index)
        {
            string retval = string.Empty;

            try
            {
                var startQuoteIndex = input.IndexOf('"', index) + 1;
                var endQuoteIndex = input.IndexOf('"', startQuoteIndex);

                retval = input.Substring(startQuoteIndex, endQuoteIndex - startQuoteIndex);
                index = endQuoteIndex + 2; // 다음 문자열로 이동
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public static string ToSecsMessageString(SecsMessage message)
        {
            string retval = string.Empty;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"S{message.Stream}F{message.Function}{(message.WBit ? "W" : "")}  /*{message.Description}*/");
                string dataString = ConvertSecsItemToString(message.Data, 0);
                sb.Append(dataString);

                retval = sb.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

            return retval;
        }
        private static string ConvertSecsItemToString(SecsItem item, int indentLevel)
        {
            string retval = string.Empty;

            try
            {
                var indent = new string(' ', indentLevel * 4);
                var sb = new StringBuilder();

                switch (item)
                {
                    case SecsItemList list:
                        sb.AppendLine($"{indent}<L");
                        foreach (var child in list.Items)
                        {
                            sb.Append(ConvertSecsItemToString(child, indentLevel + 1));
                        }
                        sb.AppendLine($"{indent}>");
                        break;
                    case SecsItemAscii ascii:
                        sb.AppendLine($"{indent}<A \"{ascii.Value}\">");
                        break;
                    case SecsItemU1 u1:
                        sb.AppendLine($"{indent}<U1 {u1.Value[0]}>");
                        break;
                    case SecsItemU2 u2:
                        sb.AppendLine($"{indent}<U2 {u2.Value[0]}>");
                        break;
                    case SecsItemU4 u4:
                        sb.AppendLine($"{indent}<U4 {u4.Value[0]}>");
                        break;
                        // Add cases for other SecsItem types
                }

                retval = sb.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        // Method to create a shallow copy of SecsMessage
        public void Copy(SecsMessage source)
        {
            try
            {
                this.Stream = source.Stream;
                this.Function = source.Function;
                this.WBit = source.WBit;
                this.Description = source.Description;

                if(string.IsNullOrEmpty(source.CommandName) == false)
                {
                    this.CommandName = source.CommandName;
                }

                // Assuming SecsItem also has a Clone method. If not, you need to implement it.
                this.Data = source.Data?.Clone();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class EnumRemoteActionItem
    {
        public string Name { get; set; }
        public bool IsIncluded { get; set; }
    }
}
