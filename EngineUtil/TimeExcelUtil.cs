//Summary
//EPPlus.dll解决xlsx文件
//ExcelLibrary.dll解决xls文件

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using ExcelLibrary.SpreadSheet;
using OfficeOpenXml;

namespace EngineUtil
{
    /// <summary>
    /// Excel文档类型
    /// </summary>
    public enum ExcelType
    {
        /// <summary>
        /// Excel 97-2003工作簿
        /// </summary>
        XlsDocument = 0,

        /// <summary>
        /// Excel 工作簿
        /// </summary>
        XlsxDocument = 1
    }

    /// <summary>
    /// Excel操作类
    /// </summary>
    public class TimeExcelUtil : IDisposable
    {
        private ExcelLibrary.SpreadSheet.Workbook _xlsWorkbook; //Excel 97-2003工作簿对象
        private readonly ExcelPackage _excelPackage; //Excel 工作簿对象

        /// <summary>
        /// 
        /// </summary>
        public ExcelType ExcelType { get; set; }

        private int StartIndex
        {
            get { return ExcelType == ExcelType.XlsDocument ? 0 : 1; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="excelPath">Excel文件路径</param>
        public TimeExcelUtil(string excelPath)
        {
            var s = Path.GetExtension(excelPath);
            if (s == null) return;
            switch (s.ToUpper())
            {
                case ".XLS":
                    _xlsWorkbook = Workbook.Load(excelPath);
                    ExcelType = ExcelType.XlsDocument;
                    break;
                case ".XLSX":
                    _excelPackage = new ExcelPackage(new FileInfo(excelPath));
                    ExcelType = ExcelType.XlsxDocument;
                    break;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="excelPath">Excel文件路径</param>
        /// <param name="excelType">Excel文件类型</param>
        public TimeExcelUtil(string excelPath, ExcelType excelType)
        {
            switch (excelType)
            {
                case ExcelType.XlsDocument:
                    {
                        _xlsWorkbook = Workbook.Load(excelPath);
                        ExcelType = excelType;
                        break;
                    }
                case ExcelType.XlsxDocument:
                    {
                        _excelPackage = new ExcelPackage(new FileInfo(excelPath));
                        ExcelType = excelType;
                        break;
                    }
            }
        }

        /// <summary>
        /// 读取表格时间记录
        /// </summary>
        /// <returns></returns>
        public DataTable ReadTimeDataAsDataTable()
        {
            DataTable table = null;
            switch (ExcelType)
            {
                case ExcelType.XlsDocument:
                    {
                        var sheet = _xlsWorkbook.Worksheets.SingleOrDefault(_ => _.Name.Equals("统计时间比表"));
                        if (sheet != null)
                        {
                            table = new DataTable {TableName = sheet.Name};

                            var colCount = sheet.Cells.LastColIndex;
                            colCount = 2;
                            var rowCount = sheet.Cells.LastRowIndex;

                            for (ushort j = 0; j <= colCount; j++)
                                table.Columns.Add(new DataColumn(sheet.Cells[0, j].Value == null ? "NULL" : sheet.Cells[0, j].Value.ToString()));

                            for (ushort i = 1; i <= rowCount; i++)
                            {
                                var row = table.NewRow();
                                for (ushort j = 0; j <= colCount; j++)
                                    row[j] = sheet.Cells[i, j].Value;

                                table.Rows.Add(row);
                            }
                        }
                        break;
                    }
                case ExcelType.XlsxDocument:
                    {
                        var sheet = _excelPackage.Workbook.Worksheets.SingleOrDefault(_ => _.Name.Equals("统计时间比表"));
                        if (sheet != null)
                        {
                            table = new DataTable {TableName = sheet.Name};

                            var colCount = sheet.Dimension.End.Column;
                            colCount = 3;
                            var rowCount = sheet.Dimension.End.Row;

                            for (ushort j = 1; j <= colCount; j++)
                                table.Columns.Add(new DataColumn(sheet.Cells[1, j].Value == null ? "NULL" : sheet.Cells[1, j].Value.ToString()));

                            for (ushort i = 2; i <= rowCount; i++)
                            {
                                var row = table.NewRow();
                                for (ushort j = 1; j <= colCount; j++)
                                    row[j - 1] = sheet.Cells[i, j].Value;
                                table.Rows.Add(row);
                            }
                        }

                        break;
                    }

            }

            return table;
        }

        /// <summary>
        /// 存储时间数据
        /// </summary>
        /// <param name="table"></param>
        public void WriteTimeTable(DataTable table)
        {
            switch (ExcelType)
            {
                case ExcelType.XlsDocument:
                {
                    var sheet = _xlsWorkbook.Worksheets.SingleOrDefault(_ => _.Name.Equals("统计时间比表"));
                    if (sheet != null)
                    {
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            var row = table.Rows[i];
                            sheet.Cells[i, 0].Value = row[0].ToString();
                            sheet.Cells[i, 1].Value = row[1].ToString();
                        }
                    }
                    break;
                }
            }
        }


        /// <summary>
        /// 生成关于时间统计的excel表格
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="table"></param>
        /// <param name="type"></param>
        public static void CreateTimeExcel(string filePath, DataTable table,ExcelType type)
        {
            switch (type)
            {
                case ExcelType.XlsDocument:
                {
                    var workbook = new Workbook();
                    var worksheet = new Worksheet("统计时间比表");
                    for (var i = 0; i < table.Rows.Count; i++)
                    {
                        var row = table.Rows[i];
                        worksheet.Cells[i, 0] = new Cell(row[0].ToString());
                        worksheet.Cells[i, 1] = new Cell(row[1].ToString());
                    }
                    worksheet.Cells.ColumnWidth[0, 1] = 3000;
                    workbook.Worksheets.Add(worksheet);
                    workbook.Save(filePath);
                    break;
                }
                case ExcelType.XlsxDocument:
                {
                    using (var fs = File.OpenRead(string.Format("{0}\\时间统计.xlsx", AppDomain.CurrentDomain.BaseDirectory)))
                    {
                        using (var excelPackage = new ExcelPackage(fs))
                        {
                            var excelWorkBook = excelPackage.Workbook;
                            var excelWorksheet = excelWorkBook.Worksheets.SingleOrDefault(_ => _.Name.Equals("统计时间比表"));
                           
                            for (int i = 0; i < table.Columns.Count; i++)
                                excelWorksheet.Cells[1, i + 1].Value = table.Columns[i].ColumnName;

                            for (var i = 0; i < table.Rows.Count; i++)
                            {
                                var row = table.Rows[i];
                                for (int j = 0; j < table.Columns.Count; j++)
                                {
                                    double res;
                                    if (double.TryParse(row[j].ToString(), out res))
                                        excelWorksheet.Cells[i + 2, j + 1].Value = res;
                                    else
                                        excelWorksheet.Cells[i + 2, j + 1].Value = row[j].ToString();

                                }
                                //var time = double.Parse(row[1].ToString());
                                
                                //excelWorksheet.Cells[i+2, 1].Value = row[0].ToString();
                                //excelWorksheet.Cells[i+2, 2].Value = time;
                            }
                            excelPackage.SaveAs(new FileInfo(filePath)); // This is the important part.
                        }
                    }

                    break;
                }
            }

        }
        
        /// <summary>
        /// 生成普通的EXCEL表格-即将dataset数据拷贝纸excel表中
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dataset"></param>
        /// <param name="type"></param>
        public static void CreateCommonExcel(string filePath, DataSet dataset, ExcelType type)
        {
            switch (type)
            {
                case ExcelType.XlsDocument:
                    {
                        var workbook = new Workbook();

                        foreach (DataTable table in dataset.Tables)
                        {
                            var worksheet = new Worksheet(table.TableName);
                            for (var i = 0; i < table.Rows.Count; i++)
                            {
                                var row = table.Rows[i];
                                worksheet.Cells[i, 0] = new Cell(row[0].ToString());
                                worksheet.Cells[i, 1] = new Cell(row[1].ToString());
                            }
                            workbook.Worksheets.Add(worksheet);
                            workbook.Save(filePath);
                        }
                      
                        break;
                    }
                case ExcelType.XlsxDocument:
                    {
                        using (var fs = File.Create(filePath))
                        {
                            using (var excelPackage = new ExcelPackage(fs))
                            {
                                var excelWorkBook = excelPackage.Workbook;

                                foreach (DataTable table in dataset.Tables)
                                {
                                    var excelWorksheet = excelWorkBook.Worksheets.Add(table.TableName);

                                    for (int i = 0; i < table.Columns.Count; i++)
                                        excelWorksheet.Cells[1, i + 1].Value = table.Columns[i].ColumnName;

                                    for (var i = 0; i < table.Rows.Count; i++)
                                    {
                                        var row = table.Rows[i];
                                        for (int j = 0; j < table.Columns.Count; j++)
                                        {
                                            double res;
                                            if (double.TryParse(row[j].ToString(), out res))
                                                excelWorksheet.Cells[i + 2, j + 1].Value = res;
                                            else
                                                excelWorksheet.Cells[i + 2, j + 1].Value = row[j].ToString();

                                        }
                                    }
                                }

                                excelPackage.Save();
                            }
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// 读取图标信息,有问题
        /// </summary>
        public void ReadGraphic()
        {
            switch (ExcelType)
            {
                case ExcelType.XlsDocument:
                {
                    var sheet = _xlsWorkbook.Worksheets.FirstOrDefault(_ => _.SheetType == SheetType.Chart);
                   
                    break;
                }
                case ExcelType.XlsxDocument:
                {
                    var sheet = _excelPackage.Workbook.Worksheets;
                    var chart = sheet.SingleOrDefault(_ =>  _.Drawings !=null);

                    break;
                }
            }
        }

        public string GetName()
        {
            
            return _excelPackage.File.Name;

        }

        public void Dispose()
        {
            if (_excelPackage != null)
                _excelPackage.Dispose();
        }
    }
}
