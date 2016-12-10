using EditorUtil;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExcelLocalization
{
    class ExcelCell
    {
        public string Header { get; private set; }

        public string StringValue { get { return _cell.StringCellValue; } }
        public double DoubleValue { get { return _cell.NumericCellValue; } }
        public bool BoolValue { get { return _cell.BooleanCellValue; } }

        public ExcelCell(ICell cell, string header)
        {
            _cell = cell;
            Header = header;
        }

        private ICell _cell;
    }

    class ExcelRow
    {
        public int CellCount { get { return _row.Cells.Count; } }

        public ExcelRow(IRow row, string[] headers, int firstHeaderRow)
        {
            _row = row;
            _headers = headers;
            _firstHeaderRow = firstHeaderRow;
        }

        public ExcelCell GetCell(int index)
        {
            return new ExcelCell(_row.GetCell(index), _headers[index - _firstHeaderRow]);
        }

        public IEnumerable<ExcelCell> EnumerateCells()
        {
            for (var i = _firstHeaderRow; i < _headers.Length; ++i)
            {
                yield return new ExcelCell(_row.GetCell(i), _headers[i - _firstHeaderRow]);
            }
        }

        private IRow _row;
        private string[] _headers;
        private int _firstHeaderRow;
    }

    class ExcelSheet
    {
        public string Name { get { return _sheet.SheetName; } }
        public string[] Headers { get; private set; }

        public ExcelSheet(ISheet sheet)
        {
            _sheet = sheet;

            var headerRow = _sheet.GetRow(_sheet.FirstRowNum);
            Headers = new string[headerRow.LastCellNum - headerRow.FirstCellNum];
            for (var i = headerRow.FirstCellNum; i < headerRow.LastCellNum; ++i)
            {
                Headers[i - headerRow.FirstCellNum] = headerRow.GetCell(i).StringCellValue;
            }

            _firstHeaderRow = headerRow.FirstCellNum;
        }

        public IEnumerable<ExcelRow> EnumerateRows()
        {
            for (var i = _sheet.FirstRowNum + 1; i <= _sheet.LastRowNum; ++i)
            {
                yield return new ExcelRow(_sheet.GetRow(i), Headers, _firstHeaderRow);
            }
        }

        public Dictionary<string, Dictionary<TCellKey, TCellValue>> ToDictionary<TCellKey, TCellValue>(
            Func<ExcelCell, TCellKey> getCellKey,
            Func<ExcelCell, TCellValue> getCellValue)
        {
            var result = new Dictionary<string, Dictionary<TCellKey, TCellValue>>();
            foreach (var row in EnumerateRows())
            {
                var name = row.GetCell(0).StringValue;
                result[name] = row.EnumerateCells().Skip(1)
                    .ToDictionary(cell => getCellKey(cell), cell => getCellValue(cell));
            }
            return result;
        }

        private ISheet _sheet;
        private int _firstHeaderRow;
    }

    class ExcelReader : Disposable
    {
        public ExcelReader(string filepath)
        {
            _bookReader = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _book = new XSSFWorkbook(_bookReader);
            SetDisposingAction(() => Close());
        }

        public IEnumerable<ExcelSheet> EnumerateSheets()
        {
            for (var i = 0; i < _book.NumberOfSheets; ++i)
            {
                yield return new ExcelSheet(_book.GetSheetAt(i));
            }
        }

        private void Close()
        {
#if false
            // NPOIのバグ？ Close()したらファイルが壊れる
            if (_book != default(XSSFWorkbook))
            {
                _book.Close();
                _book = default(XSSFWorkbook);
            }
#else
            _book = default(XSSFWorkbook);

            if (_bookReader != null)
            {
                _bookReader.Dispose();
                _bookReader = null;
            }
#endif
        }

        private Stream _bookReader;
        private XSSFWorkbook _book;
    }
}
