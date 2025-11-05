using System;
using System.Collections.Generic;

namespace DBTableEditor
{
    using CsvUtil;
    using DBManagerModule;
    using DBManagerModule.Table;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Data;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;

    public enum EnumTable { SYSTEM, DEVICE, COMMON };
    public class UpdateTabViewModel : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> CurrentTable
        private EnumTable _CurrentTable;
        public EnumTable CurrentTable
        {
            get { return _CurrentTable; }
            set
            {
                if (value != _CurrentTable)
                {
                    _CurrentTable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ====> COLUMN FULL BACKUP

        #region ==> SelectAllColumnCommand
        private RelayCommand _SelectAllColumnCommand;
        public ICommand SelectAllColumnCommand
        {
            get
            {
                if (null == _SelectAllColumnCommand) _SelectAllColumnCommand = new RelayCommand(SelectAllColumnCommandFunc);
                return _SelectAllColumnCommand;
            }
        }
        public void SelectAllColumnCommandFunc()
        {
            foreach (ColumnChecker columnChecker in ColumnCheckerList)
            {
                columnChecker.IsChecked = true;
            }
        }
        #endregion

        #region ==> SelectNoneColumnCommand
        private RelayCommand _SelectNoneColumnCommand;
        public ICommand SelectNoneColumnCommand
        {
            get
            {
                if (null == _SelectNoneColumnCommand) _SelectNoneColumnCommand = new RelayCommand(SelectNoneColumnCommandFunc);
                return _SelectNoneColumnCommand;
            }
        }
        public void SelectNoneColumnCommandFunc()
        {
            foreach (ColumnChecker columnChecker in ColumnCheckerList)
            {
                columnChecker.IsChecked = false;
            }
        }
        #endregion

        #region ==> FullBackupCommand
        private RelayCommand _FullBackupCommand;
        public ICommand FullBackupCommand
        {
            get
            {
                if (null == _FullBackupCommand) _FullBackupCommand = new RelayCommand(FullBackupCommandFunc);
                return _FullBackupCommand;
            }
        }
        public void FullBackupCommandFunc()
        {

        }
        #endregion

        #region ==> ColumnCheckerList
        public List<ColumnChecker> ColumnCheckerList { get; set; }
        private Dictionary<String, ColumnChecker> _ColumnCheckerDic;
        #endregion

        #endregion

        #region ====> TABLE

        #region ==> ChangeSystemTableButtonCommand
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
            ChangeTable(EnumTable.SYSTEM);
        }
        #endregion

        #region ==> ChangeDeviceTableButtonCommand
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
            ChangeTable(EnumTable.DEVICE);
        }
        #endregion

        #region ==> ChangeCommonTableButtonCommand
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
            ChangeTable(EnumTable.COMMON);
        }
        #endregion

        #region ==> SystemTableBtnEnable
        private bool _SystemTableBtnEnable;
        public bool SystemTableBtnEnable
        {
            get { return _SystemTableBtnEnable; }
            set
            {
                if (value != _SystemTableBtnEnable)
                {
                    _SystemTableBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> DeviceTableBtnEnable
        private bool _DeviceTableBtnEnable;
        public bool DeviceTableBtnEnable
        {
            get { return _DeviceTableBtnEnable; }
            set
            {
                if (value != _DeviceTableBtnEnable)
                {
                    _DeviceTableBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CommonTableBtnEnable
        private bool _CommonTableBtnEnable;
        public bool CommonTableBtnEnable
        {
            get { return _CommonTableBtnEnable; }
            set
            {
                if (value != _CommonTableBtnEnable)
                {
                    _CommonTableBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #endregion

        #region ====> EXPORT & OVERWRITE

        #region ==> CsvExportButtonCommand : DataGrid Export CSV File(C:\ProberSystem\DB)
        private RelayCommand _CsvExportButtonCommand;
        public ICommand CsvExportButtonCommand
        {
            get
            {
                if (null == _CsvExportButtonCommand) _CsvExportButtonCommand = new RelayCommand(CsvExportButtonCommandFunc);
                return _CsvExportButtonCommand;
            }
        }
        public void CsvExportButtonCommandFunc()
        {
            try
            {
                ChangeTable(EnumTable.COMMON);
                using (StreamWriter sw = new StreamWriter(UpdateCsvDic[CurrentTable].FilePath))
                {
                    String columnLine = String.Empty;

                    //==> -1 은 마지막 ColorColumn 제거
                    for (int i = 0; i < UpdateCsvDataTable.Columns.Count - 1; ++i)
                    {
                        columnLine += UpdateCsvDataTable.Columns[i].ToString() + ";";
                    }

                    //==> 마지막 Spliter 제거
                    columnLine.Remove(columnLine.Length - 1);
                    sw.WriteLine(columnLine);

                    foreach (DataRow row in UpdateCsvDataTable.Rows)
                    {
                        String recordLine = String.Empty;

                        //==> -1 은 마지막 ColorColumn 제거
                        for (int i = 0; i < row.ItemArray.Length - 1; ++i)
                        {
                            recordLine += row.ItemArray[i] + ";";
                        }

                        recordLine.Remove(recordLine.Length - 1);
                        sw.WriteLine(recordLine);
                    }
                }

                MessageBox.Show($"Export Success : {UpdateCsvDic[CurrentTable].FilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region ==> CsvOverwriteButtonCommand : Update CSV(Source) -> Current CSV(Target)
        private RelayCommand _CsvOverwriteButtonCommand;
        public ICommand CsvOverwriteButtonCommand
        {
            get
            {
                if (null == _CsvOverwriteButtonCommand) _CsvOverwriteButtonCommand = new RelayCommand(CsvOverwriteButtonCommandFunc);
                return _CsvOverwriteButtonCommand;
            }
        }
        public void CsvOverwriteButtonCommandFunc()
        {
            if (File.Exists(UpdateCsvFilePath) == false)
            {
                MessageBox.Show($"File is Not exist : {UpdateCsvFilePath}");
                return;
            }

            if (File.Exists(CurrentCsvFilePath) == false)
            {
                MessageBox.Show($"File is Not exist : {CurrentCsvFilePath}");
                return;
            }


            try
            {
                //==> File Backup
                String backupFilePath = Path.Combine(DBManager.DBDirectoryPath, Path.GetFileNameWithoutExtension(CurrentCsvFilePath) + $"_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}" + ".csv");
                File.Copy(CurrentCsvFilePath, backupFilePath);

                //==> File Overwrite
                File.Copy(UpdateCsvFilePath, CurrentCsvFilePath, true);
                MessageBox.Show($"File Overwrite success : [{UpdateCsvFilePath}] -> [{CurrentCsvFilePath}]");
            }
            catch (Exception)
            {
                MessageBox.Show($"File Copy Error");
            }


        }
        #endregion

        #region ==> UpdateCsvFilePath : Source, DB 폴더(C:\ProberSystem\DB)
        private String _UpdateCsvFilePath;
        public String UpdateCsvFilePath
        {
            get { return _UpdateCsvFilePath; }
            set
            {
                if (value != _UpdateCsvFilePath)
                {
                    _UpdateCsvFilePath = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CurrentCsvFilePath : Target, Solution Folder(환경 마다 다름.)
        private String _CurrentCsvFilePath;
        public String CurrentCsvFilePath
        {
            get { return _CurrentCsvFilePath; }
            set
            {
                if (value != _CurrentCsvFilePath)
                {
                    _CurrentCsvFilePath = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #endregion

        #region ====> EXPLORER & Backup

        #region ==> BackUpButtonCommand : Current CSV에 선택된 Row를 Update CSV로 Backup
        private RelayCommand _BackUpButtonCommand;
        public ICommand BackUpButtonCommand
        {
            get
            {
                if (null == _BackUpButtonCommand) _BackUpButtonCommand = new RelayCommand(BackUpButtonCommandFunc);
                return _BackUpButtonCommand;
            }
        }
        public void BackUpButtonCommandFunc()
        {
            if (CurrentCsvSelectedItem == null)
            {
                return;
            }
            
            //==> Current CSV에서 삭제
            String removeKey = CurrentCsvSelectedItem[PramElementColumn.PropertyPath] as String;
            var removedData = CurrentCsvDic[CurrentTable].RemoveData(removeKey);
            //==> Update CSV로 이동
            UpdateCsvDic[CurrentTable].InsertData(0, removedData);

            //==> DataTable에서 삭제
            //RemoveRowFromDataTable(CurrentCsvDataTable, CurrentCsvSelectedItem.Row);
            Update_CurrentCsvDataGrid();
            Update_UpdateCsvDataGrid();
        }
        #endregion

        #region ==> BackUpMetaDataButtonCommand : Meta Data를 Field 단위로 복사(PropertyPath 제외)
        private RelayCommand _BackUpMetaDataButtonCommand;
        public ICommand BackUpMetaDataButtonCommand
        {
            get
            {
                if (null == _BackUpMetaDataButtonCommand) _BackUpMetaDataButtonCommand = new RelayCommand(BackUpMetaDataButtonCommandFunc);
                return _BackUpMetaDataButtonCommand;
            }
        }
        public void BackUpMetaDataButtonCommandFunc()
        {
            if (UpdateCsvSelectedItem == null)
            {
                return;
            }

            if (CurrentCsvSelectedItem == null)
            {
                return;
            }

            //==> CSV에서 삭제
            String removeKey = CurrentCsvSelectedItem[PramElementColumn.PropertyPath] as String;
            var removedData = CurrentCsvDic[CurrentTable].RemoveData(removeKey);
            ////==> Update CSV로 이동
            UpdateCsvDic[CurrentTable].RemoveData(removeKey);
            UpdateCsvDic[CurrentTable].InsertData(0, removedData);

            //==> DataTable에서 삭제
            //RemoveRowFromDataTable(CurrentCsvDataTable, CurrentCsvSelectedItem.Row);
            Update_CurrentCsvDataGrid();
            Update_UpdateCsvDataGrid();
        }
        #endregion

        #region ==> TotalPageCount
        private int _TotalPageCount;
        public int TotalPageCount
        {
            get { return _TotalPageCount; }
            set
            {
                if (value != _TotalPageCount)
                {
                    _TotalPageCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CurPageIndex
        private int _CurPageIndex;
        public int CurPageIndex
        {
            get { return _CurPageIndex; }
            set
            {
                if (value != _CurPageIndex)
                {
                    _CurPageIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PrevPageCommand
        private RelayCommand _PrevPageCommand;
        public ICommand PrevPageCommand
        {
            get
            {
                if (null == _PrevPageCommand) _PrevPageCommand = new RelayCommand(PrevPageCommandFunc);
                return _PrevPageCommand;
            }
        }
        public void PrevPageCommandFunc()
        {
            CurPageIndex--;
            if (CurPageIndex < 1)
            {
                CurPageIndex = 1;
                return;
            }

            Update_CurrentCsvDataGrid();
        }
        #endregion

        #region ==> NextPageCommand
        private RelayCommand _NextPageCommand;
        public ICommand NextPageCommand
        {
            get
            {
                if (null == _NextPageCommand) _NextPageCommand = new RelayCommand(NextPageCommandFunc);
                return _NextPageCommand;
            }
        }
        public void NextPageCommandFunc()
        {
            CurPageIndex++;
            if (CurPageIndex > TotalPageCount)
            {
                CurPageIndex = TotalPageCount;
                return;
            }

            Update_CurrentCsvDataGrid();
        }
        #endregion

        #region ==> Prev10xPageCommand
        private RelayCommand _Prev10xPageCommand;
        public ICommand Prev10xPageCommand
        {
            get
            {
                if (null == _Prev10xPageCommand) _Prev10xPageCommand = new RelayCommand(Prev10xPageCommandFunc);
                return _Prev10xPageCommand;
            }
        }
        public void Prev10xPageCommandFunc()
        {
            CurPageIndex -= 10;
            if (CurPageIndex < 1)
            {
                CurPageIndex = 1;
            }

            Update_CurrentCsvDataGrid();
        }
        #endregion

        #region ==> Next10xPageCommand
        private RelayCommand _Next10xPageCommand;
        public ICommand Next10xPageCommand
        {
            get
            {
                if (null == _Next10xPageCommand) _Next10xPageCommand = new RelayCommand(Next10xPageCommandFunc);
                return _Next10xPageCommand;
            }
        }
        public void Next10xPageCommandFunc()
        {
            CurPageIndex += 10;
            if (CurPageIndex > TotalPageCount)
            {
                CurPageIndex = TotalPageCount;
            }

            Update_CurrentCsvDataGrid();
        }
        #endregion

        private readonly int _RecordCountPerPage = 30;

        #endregion

        #region ==> DeleteButtonCommand : Update CSV에서 선택된 Row를 제거
        private RelayCommand _DeleteButtonCommand;
        public ICommand DeleteButtonCommand
        {
            get
            {
                if (null == _DeleteButtonCommand) _DeleteButtonCommand = new RelayCommand(DeleteButtonCommandFunc);
                return _DeleteButtonCommand;
            }
        }
        public void DeleteButtonCommandFunc()
        {
            //==> Update CSV로 이동
            String removeKey = UpdateCsvSelectedItem[PramElementColumn.PropertyPath] as String;
            UpdateCsvDic[CurrentTable].RemoveData(removeKey);

            Update_CurrentCsvDataGrid();
            Update_UpdateCsvDataGrid();
        }
        #endregion

        #region ====> DATAGRID SELECTED ITEM

        #region ==> UpdateCsvSelectedItem
        private DataRowView _UpdateCsvSelectedItem;
        public DataRowView UpdateCsvSelectedItem
        {
            get { return _UpdateCsvSelectedItem; }
            set
            {
                if (value != _UpdateCsvSelectedItem)
                {
                    _UpdateCsvSelectedItem = value;
                    RaisePropertyChanged();

                    //==> CurrentCsv 창의 선택을 지운다.
                    _CurrentCsvSelectedItem = null;
                    RaisePropertyChanged(nameof(CurrentCsvSelectedItem));

                    ////==> Update CSV Data Gird에서 선택한 Row와 똑같은 키를 가진 Row를 Current CSV Data Grid에서 선택 한다.
                    //if (_UpdateCsvSelectedItem != null)
                    //{
                    //    String key = _UpdateCsvSelectedItem.Row[PramElementColumn.PropertyPath] as String;
                    //    if (key == null)
                    //    {
                    //        return;
                    //    }
                    //    if (DataRowViewMap[CurrentCsvDataTable].ContainsKey(key))
                    //    {
                    //        _CurrentCsvSelectedItem = DataRowViewMap[CurrentCsvDataTable][key];
                    //    }
                    //    else
                    //    {
                    //        _CurrentCsvSelectedItem = null;
                    //    }
                    //    RaisePropertyChanged(nameof(CurrentCsvSelectedItem));
                    //}
                }
            }
        }
        #endregion

        #region ==> CurrentCsvSelectedItem
        private DataRowView _CurrentCsvSelectedItem;
        public DataRowView CurrentCsvSelectedItem
        {
            get { return _CurrentCsvSelectedItem; }
            set
            {
                if (value != _CurrentCsvSelectedItem)
                {
                    _CurrentCsvSelectedItem = value;
                    RaisePropertyChanged();

                    //==> Current CSV Data Gird에서 선택한 Row와 똑같은 키를 가진 Row를 Update CSV Data Grid에서 선택 한다.
                    if (_CurrentCsvSelectedItem != null)
                    {
                        String key = _CurrentCsvSelectedItem.Row[PramElementColumn.PropertyPath] as String;
                        if (key == null)
                        {
                            return;
                        }
                        if (DataRowViewMap[UpdateCsvDataTable].ContainsKey(key))
                        {
                            _UpdateCsvSelectedItem = DataRowViewMap[UpdateCsvDataTable][key];
                        }
                        else
                        {
                            _UpdateCsvSelectedItem = null;
                        }
                        RaisePropertyChanged(nameof(UpdateCsvSelectedItem));
                    }
                }
            }
        }
        #endregion

        #endregion

        //==> Update CSV DataGrid의 DataContext
        public DataTable UpdateCsvDataTable { get; set; }
        //==> Current CSV DataGrid의 DataContext
        public DataTable CurrentCsvDataTable { get; set; }

        //==> Update CSV (System, Device, Common)의 CSV Reader
        private Dictionary<EnumTable, CsvDicTypeReader> UpdateCsvDic { get; set; }
        //==> Current CSV (System, Device, Common)의 CSV Reader
        private Dictionary<EnumTable, CsvDicTypeReader> CurrentCsvDic { get; set; }
        //==> DataRowView에 접근하기 위한 Dictionary 구조
        //==> Dictionary<TABLE, Dictionary<PROPERTY PATH, DataRowView>> 
        private Dictionary<DataTable, Dictionary<String, DataRowView>> DataRowViewMap { get; set; }

        private DataTable BuildCsvDataTable()
        {
            DataTable dataTable = new DataTable();

            //==> Column 구성 초기화
            foreach (String columnName in PramElementColumn.ColumnList)
            {
                dataTable.Columns.Add(columnName, typeof(String));
            }

            //==> Cell 색깔을 표현하기 위한 Column 추가
            dataTable.Columns.Add(DGColor.ColorColumnName, typeof(String));

            //==> Primary Key 선택
            dataTable.PrimaryKey = new DataColumn[1] { dataTable.Columns[PramElementColumn.PropertyPath] };

            return dataTable;
        }
        private Dictionary<DataTable, Dictionary<String, DataRowView>> BuildDataRowViewMap()
        {
            var dataRowViewMap = new Dictionary<DataTable, Dictionary<String, DataRowView>>();
            dataRowViewMap.Add(UpdateCsvDataTable, new Dictionary<String, DataRowView>());
            dataRowViewMap.Add(CurrentCsvDataTable, new Dictionary<String, DataRowView>());

            return dataRowViewMap;
        }
        private Dictionary<String, ColumnChecker> BuildColumnCheckerDic()
        {
            Dictionary<String, ColumnChecker> columnCheckerDic = new Dictionary<String, ColumnChecker>();
            foreach (String columnName in PramElementColumn.ColumnList)
            {
                columnCheckerDic.Add(columnName, new ColumnChecker(columnName, true));
            }

            return columnCheckerDic;
        }

        public UpdateTabViewModel()
        {
            //==> DataTable 구성 초기화
            UpdateCsvDataTable = BuildCsvDataTable();
            CurrentCsvDataTable = BuildCsvDataTable();

            //==> DataRowViewMap 초기화
            DataRowViewMap = BuildDataRowViewMap();

            //==> Column Checker 구성 초기화
            _ColumnCheckerDic = BuildColumnCheckerDic();
            ColumnCheckerList = new List<ColumnChecker>(_ColumnCheckerDic.Values);

            //==> Current CSV File Path
            //String currentCsvPath = Directory.GetCurrentDirectory();
            String currentCsvPath = AppDomain.CurrentDomain.BaseDirectory;

            currentCsvPath = Directory.GetParent(currentCsvPath).ToString();// ..\
            currentCsvPath = Directory.GetParent(currentCsvPath).ToString();// ..\..\
            currentCsvPath = Directory.GetParent(currentCsvPath).ToString();// ..\..\..\
            currentCsvPath = Path.Combine(currentCsvPath, @"CsvUtil");

            //==> Csv Reader 초기화
            UpdateCsvDic = new Dictionary<EnumTable, CsvDicTypeReader>();
            CurrentCsvDic = new Dictionary<EnumTable, CsvDicTypeReader>();

            UpdateCsvDic.Add(EnumTable.SYSTEM, new CsvDicTypeReader(Path.Combine(DBManager.DBDirectoryPath, CsvDicTypeReader.SYSPARAMETERDATA_FILENAME), PramElementColumn.PropertyPath));
            CurrentCsvDic.Add(EnumTable.SYSTEM, new CsvDicTypeReader(Path.Combine(currentCsvPath, CsvDicTypeReader.SYSPARAMETERDATA_FILENAME), PramElementColumn.PropertyPath));

            UpdateCsvDic.Add(EnumTable.DEVICE, new CsvDicTypeReader(Path.Combine(DBManager.DBDirectoryPath, CsvDicTypeReader.DEVPARAMETERDATA_FILENAME), PramElementColumn.PropertyPath));
            CurrentCsvDic.Add(EnumTable.DEVICE, new CsvDicTypeReader(Path.Combine(currentCsvPath, CsvDicTypeReader.DEVPARAMETERDATA_FILENAME), PramElementColumn.PropertyPath));

            UpdateCsvDic.Add(EnumTable.COMMON, new CsvDicTypeReader(Path.Combine(DBManager.DBDirectoryPath, CsvDicTypeReader.COMMONPARAMETERDATA_FILENAME), PramElementColumn.PropertyPath));
            CurrentCsvDic.Add(EnumTable.COMMON, new CsvDicTypeReader(Path.Combine(currentCsvPath, CsvDicTypeReader.COMMONPARAMETERDATA_FILENAME), PramElementColumn.PropertyPath));


            //==> Read CSV File
            if (UpdateCsvDic[EnumTable.SYSTEM].Import() && CurrentCsvDic[EnumTable.SYSTEM].Import())
            {
                SystemTableBtnEnable = true;
            }

            if (UpdateCsvDic[EnumTable.DEVICE].Import() && CurrentCsvDic[EnumTable.DEVICE].Import())
            {
                DeviceTableBtnEnable = true;
            }

            if (UpdateCsvDic[EnumTable.COMMON].Import() && CurrentCsvDic[EnumTable.COMMON].Import())
            {
                CommonTableBtnEnable = true;
            }
        }
        private void ChangeTable(EnumTable table)
        {
            CurrentTable = table;
            UpdateCsvFilePath = UpdateCsvDic[CurrentTable].FilePath;
            CurrentCsvFilePath = CurrentCsvDic[CurrentTable].FilePath;

            //==> Update UpdateCSV Data Grid
            Update_UpdateCsvDataGrid();

            //==> Update CurrentCSV Data Grid
            CurPageIndex = 1;
            Update_CurrentCsvDataGrid();
        }
        private bool AddRowToDataTable(DataTable dataTable, DataRow row, int index, String colorComboString)
        {
            if (index < 0)
            {
                index = 0;
            }
            if (dataTable.Rows.Contains(row[PramElementColumn.PropertyPath]))
            {
                return false;
            }

            DataRow newRow = dataTable.NewRow();
            newRow.ItemArray = row.ItemArray.Clone() as Object[];
            newRow[DGColor.ColorColumnName] = colorComboString;
            dataTable.Rows.InsertAt(newRow, index);

            String key = row[PramElementColumn.PropertyPath] as String;
            DataRowView drv = dataTable.DefaultView[dataTable.Rows.IndexOf(newRow)];
            DataRowViewMap[dataTable].Add(key, drv);

            return true;
        }
        private bool AddRowToDataTable(DataTable dataTable, String[] row, int index, String colorComboString)
        {
            DataRow newRow = dataTable.NewRow();
            newRow.ItemArray = row;

            return AddRowToDataTable(dataTable, newRow, index, colorComboString);
        }
        private bool AddRowToDataTable(DataTable dataTable, Object[] row, int index, String colorComboString)
        {
            DataRow newRow = dataTable.NewRow();
            newRow.ItemArray = row;

            return AddRowToDataTable(dataTable, newRow, index, colorComboString);
        }
        private void RemoveRowFromDataTable(DataTable dataTable, DataRow deleteRow)
        {
            String key = deleteRow[PramElementColumn.PropertyPath] as String;
            DataRowViewMap[dataTable].Remove(key);

            dataTable.Rows.Remove(deleteRow);
        }
        private void ClearDataTable(DataTable dataTable)
        {
            DataRowViewMap[dataTable].Clear();
            dataTable.Rows.Clear();
        }

        //==> [Update] Update CSV Data Grid
        private void Update_UpdateCsvDataGrid()
        {
            CsvDicTypeReader updateCsvReader = UpdateCsvDic[CurrentTable];

            ClearDataTable(UpdateCsvDataTable);

            foreach (String[] record in updateCsvReader.ArrayData)
            {
                AddRowToDataTable(UpdateCsvDataTable, record, UpdateCsvDataTable.Rows.Count, DGColor.BuildColorComboString(EnumDGColor.Orange, new List<String>()));
            }
        }
        //==> [Update] Curent CSV Data Grid
        private void Update_CurrentCsvDataGrid()
        {
            UpdateTotalPage();

            CsvDicTypeReader updateCsvReader = UpdateCsvDic[CurrentTable];
            CsvDicTypeReader currentCsvReader = CurrentCsvDic[CurrentTable];

            ClearDataTable(CurrentCsvDataTable);

            int startRecordIndex = (CurPageIndex - 1) * _RecordCountPerPage;
            int endRecordIndex = startRecordIndex + _RecordCountPerPage - 1;
            if (endRecordIndex > currentCsvReader.ArrayData.Count - 1)
            {
                endRecordIndex = currentCsvReader.ArrayData.Count - 1;
            }

            while (startRecordIndex < endRecordIndex + 1)
            {
                String[] record = currentCsvReader.ArrayData[startRecordIndex];
                String key = record[currentCsvReader.KeyColumnIdx];

                List<String> differentColumn = null;
                if (IsDeleteRecord(key, updateCsvReader))
                {
                    //==> 삭제되는 Record
                    AddRowToDataTable(CurrentCsvDataTable, record, CurrentCsvDataTable.Rows.Count, DGColor.BuildColorComboString(EnumDGColor.OrangeRed, null));
                }
                else if (IsModifyRecord(key, updateCsvReader, currentCsvReader, out differentColumn))
                {
                    //==> Meta data가 수정된 Record
                    AddRowToDataTable(CurrentCsvDataTable, record, CurrentCsvDataTable.Rows.Count, DGColor.BuildColorComboString(EnumDGColor.Orange, differentColumn));
                }
                ++startRecordIndex;
            }
        }

        private void UpdateTotalPage()
        {
            CsvDicTypeReader currentCsvReader = CurrentCsvDic[CurrentTable];
            int pageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(currentCsvReader.ArrayData.Count) / _RecordCountPerPage));
            if (pageCount < 1)
            {
                pageCount = 1;
            }

            TotalPageCount = pageCount;
        }

        private bool IsDeleteRecord(String key, CsvDicTypeReader csvReader)
        {
            return csvReader.Data.ContainsKey(key) == false;

        }
        private bool IsModifyRecord(String key, CsvDicTypeReader csvReader, CsvDicTypeReader compareReader, out List<String> differentColumn)
        {
            differentColumn = new List<String>();

            foreach (String columnName in PramElementColumn.ColumnList)
            {
                if (columnName == PramElementColumn.PropertyPath)
                {
                    continue;
                }

                if(csvReader.Data[key].ContainsKey(columnName))
                {
                    if (csvReader.Data[key][columnName] != compareReader.Data[key][columnName])
                    {
                        differentColumn.Add(columnName);
                    }
                }

            }

            //==> 차이점이 없으면 Current CSV에 표시하지 않는다.
            if (differentColumn.Count < 1)
            {
                return false;
            }

            return true;
        }

        #region ==> INNER CLASS
        public class ColumnChecker : INotifyPropertyChanged
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

            #region ==> IsChecked
            private bool _IsChecked;
            public bool IsChecked
            {
                get { return _IsChecked; }
                set
                {
                    if (value != _IsChecked)
                    {
                        _IsChecked = value;
                        RaisePropertyChanged();
                    }
                }
            }
            #endregion

            public ColumnChecker(String content, bool isChecked)
            {
                Content = content;
                IsChecked = isChecked;
            }
        }

        #endregion
    }
}
