using System;
using System.Collections.Generic;

namespace DBTableEditor
{
    using DBManagerModule;
    using DBManagerModule.Table;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    /*
     * DB Table을 DataGrid로 수정 할 수 있다.
     */
    public class TableEditTabViewModel : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> ChangeSystemTableButtonCommand : System Table을 DataGrid로 Load 한다.
        private RelayCommand _ChangeSystemTableButtonCommand;
        public ICommand ChangeSystemTableButtonCommand
        {
            get
            {
                if (null == _ChangeSystemTableButtonCommand) _ChangeSystemTableButtonCommand = new RelayCommand(ChangeSystemTableButtonCommandFunc);
                return _ChangeSystemTableButtonCommand;
            }
        }
        public void ChangeSystemTableButtonCommandFunc()
        {
            CurrentTable = DBManager.SystemParameter;
            UpdateDataGridByTable();
        }
        #endregion

        #region ==> ChangeDeviceTableButtonCommand : Device Table을 DataGrid로 Load 한다.
        private RelayCommand _ChangeDeviceTableButtonCommand;
        public ICommand ChangeDeviceTableButtonCommand
        {
            get
            {
                if (null == _ChangeDeviceTableButtonCommand) _ChangeDeviceTableButtonCommand = new RelayCommand(ChangeDeviceTableButtonCommandFunc);
                return _ChangeDeviceTableButtonCommand;
            }
        }
        public void ChangeDeviceTableButtonCommandFunc()
        {
            CurrentTable = DBManager.DeviceParameter;
            UpdateDataGridByTable();
        }
        #endregion

        #region ==> ChangeCommonTableButtonCommand : Common Table을 DataGrid로 Load 한다.
        private RelayCommand _ChangeCommonTableButtonCommand;
        public ICommand ChangeCommonTableButtonCommand
        {
            get
            {
                if (null == _ChangeCommonTableButtonCommand) _ChangeCommonTableButtonCommand = new RelayCommand(ChangeCommonTableButtonCommandFunc);
                return _ChangeCommonTableButtonCommand;
            }
        }
        public void ChangeCommonTableButtonCommandFunc()
        {
            CurrentTable = DBManager.CommonParameter;
            UpdateDataGridByTable();
        }
        #endregion

        #region ==> SearchCommand
        private RelayCommand _SearchCommand;
        public ICommand SearchCommand
        {
            get
            {
                if (null == _SearchCommand) _SearchCommand = new RelayCommand(SearchCommandFunc);
                return _SearchCommand;
            }
        }
        public void SearchCommandFunc()
        {
            UpdateDataGridByTable();
        }
        #endregion


        #region ==> ClearColumnCommand
        private RelayCommand _ClearColumnCommand;
        public ICommand ClearColumnCommand
        {
            get
            {
                if (null == _ClearColumnCommand) _ClearColumnCommand = new RelayCommand(ClearColumnCommandFunc);
                return _ClearColumnCommand;
            }
        }
        public void ClearColumnCommandFunc()
        {
            foreach(ColumnSearcher columnSearcher in ColumnSearcherList)
            {
                columnSearcher.Text = String.Empty;
            }
        }
        #endregion

        #region ==> ColumnSearcherList
        public List<ColumnSearcher> ColumnSearcherList { get; set; }

        //==> KEY : Column Name
        private Dictionary<String, ColumnSearcher> _ColumnSearcherDic;
        #endregion

        //==> DataGrid Cell Edit Event Handler, Cell 을 수정하고 Focus를 잃는 순간 함수가 호출 된다.
        public void MainWindow_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (CurrentTable == null)
            {
                return;
            }

            DataGridColumn dataGridColumn = e.Column;
            String columnName = dataGridColumn.Header.ToString();

            //==> cell 수정하기 전 값.
            DataRowView row = e.Row.Item as DataRowView;
            String oldCellValue = row[columnName].ToString();

            //==> cell 수정하고 난 후 값.
            TextBox t = e.EditingElement as TextBox;
            String newCellValue = t.Text.ToString();

            if (newCellValue == oldCellValue)
            {
                return;
            }

            //==> cell 이 위치한 key 값
            String key = row[CurrentTable.PrimaryKeyColumn].ToString();

            FrameworkElement element = e.Column.GetCellContent(e.Row);
            DataGridCell dataGridCell = element.Parent as DataGridCell;//==> 수정한 cell에 대한 개체
            if (dataGridCell == null)
            {
                return;
            }

            //==> cell 위치에 해당하는 DB Table Field 값을 수정된 Cell 값으로 갱신(Send SQL Query)
            if (CurrentTable.UpdateField(key, columnName, CurrentTable.ColumTemplate[columnName].Type, newCellValue) == false)
            {
                //==> SQL Query Fail, 실패하면 Cell 배경 색을 Orange Red로 바꿈.
                dataGridCell.Background = Brushes.OrangeRed;
                return;
            }

            //==> SQL Query Ok, 성공하면 Cell 배경 색을 Light Greend으로 바꿈.
            dataGridCell.Background = Brushes.LightGreen;

            //==> 수정된 Cell을 
            if (DBManager.CustomizedElementListParam.Add(key))
            {
                DBManager.SaveCustomizedElementParameter();
            }
        }

        //==> DataGrid의 DataContext
        public DataTable DBDataTable { get; set; }

        //==> 현재 선택된 Table
        public ParamElementTable CurrentTable { get; set; }

        public TableEditTabViewModel()
        {
            // 컬럼 생성
            DBDataTable = new DataTable();
            foreach (String column in PramElementColumn.ColumnList)
            {
                DBDataTable.Columns.Add(column, typeof(String));
            }


            _ColumnSearcherDic = new Dictionary<String, ColumnSearcher>();
            foreach (String columnName in PramElementColumn.ColumnList)
            {
                _ColumnSearcherDic.Add(columnName, new ColumnSearcher(columnName, String.Empty));
            }
            ColumnSearcherList = new List<ColumnSearcher>(_ColumnSearcherDic.Values);
        }

        //==> DB Table애 있는 data를 DataGrid로 Upload
        public void UpdateDataGridByTable()
        {
            DBDataTable.Rows.Clear();

            using (DbDataReader dataReader = CurrentTable.GetDataReaderByQuery())
            {
                while (dataReader.Read())
                {
                    Object[] row = GetRowOrNullByDBReader(dataReader);
                    if(row == null)
                    {
                        continue;
                    }

                    DBDataTable.Rows.Add(row);
                }
            }
        }
        private Object[] GetRowOrNullByDBReader(DbDataReader dataReader)
        {
            Object[] row = new Object[PramElementColumn.ColumnList.Count];

            for (int i = 0; i < PramElementColumn.ColumnList.Count; ++i)
            {
                String columnName = PramElementColumn.ColumnList[i];
                row[i] = dataReader[columnName];

                if(CheckFilterMatch(columnName, row[i].ToString()) == false)
                {
                    row = null;
                    break;
                }
            }
            return row;
        }
        private bool CheckFilterMatch(String columnName, String word)
        {
            String filterkeyWord = _ColumnSearcherDic[columnName].Text;
            if (String.IsNullOrEmpty(filterkeyWord))
            {
                return true;
            }

            //==> 문자열을 포함하고 있지 않다면 찾고 있는 Record가 아니다.
            return word.ToString().Contains(filterkeyWord);
        }

        #region ==> INNER CLASS
        public class ColumnSearcher : INotifyPropertyChanged
        {
            #region ==> PropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;
            protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion

            #region ==> Content
            private String _Content;
            public String Content
            {
                get { return _Content; }
                set
                {
                    if (value != _Content)
                    {
                        _Content = value;
                        RaisePropertyChanged();
                    }
                }
            }
            #endregion

            #region ==> Text
            private String _Text;
            public String Text
            {
                get { return _Text; }
                set
                {
                    if (value != _Text)
                    {
                        _Text = value;
                        RaisePropertyChanged();
                    }
                }
            }
            #endregion

            public ColumnSearcher(String content, String text)
            {
                Content = content;
                Text = text;
            }
        }
        #endregion
    }
}
