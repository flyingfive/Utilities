using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MyDBAssistant
{
    public class ExcelExporter
    {
        /// <summary>
        /// 获取系统颜色相对于NPOI excel 的颜色
        /// </summary>
        /// <param name="systemColour">系统颜色</param>
        /// <param name="workbook">导出的excel</param>
        /// <returns></returns>
        public static short GetExcelColor(Color systemColour, IWorkbook workbook = null)
        {
            if (systemColour == Color.Green) { return NPOI.HSSF.Util.HSSFColor.Green.Index; }//NPOI.HSSF.Util.HSSFColor.GREEN.index; }
            if (systemColour == Color.Yellow) { return NPOI.HSSF.Util.HSSFColor.Yellow.Index;}//NPOI.HSSF.Util.HSSFColor.YELLOW.index; }
            if (systemColour == Color.Red) { return NPOI.HSSF.Util.HSSFColor.Red.Index;}//NPOI.HSSF.Util.HSSFColor.RED.index; }
            if (systemColour == Color.White) { return NPOI.HSSF.Util.HSSFColor.White.Index;}//NPOI.HSSF.Util.HSSFColor.WHITE.index; }
            short s = 0;
            if (workbook == null) { workbook = new HSSFWorkbook(); }
            HSSFWorkbook book = workbook as HSSFWorkbook;
            if (book == null) { book = new HSSFWorkbook(); }
            HSSFPalette XlPalette = book.GetCustomPalette();
            HSSFColor XlColour = XlPalette.FindColor(systemColour.R, systemColour.G, systemColour.B);
            if (XlColour == null)
            {
                if (NPOI.HSSF.Record.PaletteRecord.STANDARD_PALETTE_SIZE < 255)
                {
                    if (NPOI.HSSF.Record.PaletteRecord.STANDARD_PALETTE_SIZE < 64)
                    {
                        //NPOI.HSSF.Record.PaletteRecord.STANDARD_PALETTE_SIZE = 64;
                        //NPOI.HSSF.Record.PaletteRecord.STANDARD_PALETTE_SIZE += 1;
                        XlColour = XlPalette.AddColor(systemColour.R, systemColour.G, systemColour.B);
                    }
                    else
                    {
                        XlColour = XlPalette.FindSimilarColor(systemColour.R, systemColour.G, systemColour.B);
                    }
                    s = XlColour.Indexed;//.GetIndex();
                }
            }
            else
            {
                s = XlColour.Indexed;//GetIndex();
            }
            return s;
        }
        private static HSSFDataFormat format = null;

        

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="templateFile">模板文件</param>
        /// <param name="sheetName">样板工作薄名称</param>
        /// <param name="data">导出数据</param>
        /// <param name="bindingRowIndex">绑定行索引</param>
        /// <returns></returns>
        public static MemoryStream Export2Excel<T>(string templateFile, string sheetName, IList<T> data, int bindingRowIndex = 0)
        {
            if (!File.Exists(templateFile)) { throw new ArgumentException(string.Format("服务器上模板文件:{0}不存在!", templateFile)); }
            if (bindingRowIndex < 0) { throw new ArgumentException("绑定行索引不能小于0"); }
            HSSFWorkbook templateBook = null;
            var exportBook = new HSSFWorkbook();
            var exportSheet = exportBook.CreateSheet(sheetName);
            IDictionary<int, string> headerColumns = new Dictionary<int, string>();                         //要导出的所有列名称
            ISheet templateSheet = null;
            IList<ICellStyle> dataRowCellStyles = new List<ICellStyle>();
            using (var stream = System.IO.File.OpenRead(templateFile))
            {
                var template = new NPOI.POIFS.FileSystem.POIFSFileSystem(stream);
                templateBook = new HSSFWorkbook(template);
                templateSheet = templateBook.GetSheet(sheetName);
                if (templateSheet == null) { throw new ArgumentException(string.Format("服务器模板文件:{0}不存在指定的工作薄名称:{1}!", template, sheetName)); }
                format = templateBook.CreateDataFormat() as HSSFDataFormat;
                var bindingRow = templateSheet.GetRow(bindingRowIndex);                                                //模板文件中的绑定行
                foreach (var cell in bindingRow.Cells)
                {
                    if (string.IsNullOrEmpty(cell.StringCellValue)) { continue; }
                    headerColumns.Add(cell.ColumnIndex, cell.StringCellValue.Replace("#", string.Empty));
                    var cellStyle = exportBook.CreateCellStyle();
                    cellStyle.CloneStyleFrom(cell.CellStyle);
                    dataRowCellStyles.Add(cellStyle);
                    exportSheet.SetColumnWidth(cell.ColumnIndex, templateSheet.GetColumnWidth(cell.ColumnIndex));
                }
            }
            var properties = typeof(T).GetProperties();
            //var headerCellStyle = exportBook.CreateCellStyle();
            //var dataCellStyle = exportBook.CreateCellStyle();
            //dataCellStyle.CloneStyleFrom(templateSheet.GetRow(bindingRowIndex).Cells[0].CellStyle);
            //headerCellStyle.CloneStyleFrom(templateSheet.GetRow(bindingRowIndex < 1 ? bindingRowIndex : bindingRowIndex - 1).Cells[0].CellStyle);
            for (int headerRowIndex = 0; headerRowIndex < bindingRowIndex; headerRowIndex++)
            {
                var templateRow = templateSheet.GetRow(headerRowIndex);
                IRow row = exportSheet.CreateRow(headerRowIndex);
                row.Height = templateRow.Height;
                foreach (var columnIndex in headerColumns.Keys)
                {
                    var templateHeadCell = templateSheet.GetRow(bindingRowIndex < 1 ? bindingRowIndex : bindingRowIndex - 1).Cells[columnIndex];
                    var headerCellStyle = exportBook.CreateCellStyle();
                    headerCellStyle.CloneStyleFrom(templateHeadCell.CellStyle);
                    ICell cell = row.CreateCell(columnIndex);
                    cell.CellStyle = headerCellStyle;
                    var templateCell = templateRow.GetCell(columnIndex);
                    cell.SetCellValue(templateCell.StringCellValue);
                }
            }

            int startRowIndex = bindingRowIndex;
            foreach (T item in data)
            {
                IRow row = exportSheet.CreateRow(startRowIndex);
                var cellIndex = 0;
                foreach (var columnIndex in headerColumns.Keys)
                {
                    var prop = properties.SingleOrDefault(p => p.Name.Equals(headerColumns[columnIndex], StringComparison.CurrentCultureIgnoreCase));
                    if (prop == null) { continue; }
                    object value = GetExportValue<T>(prop.Name, item);          //导出的值
                    ICell cell = row.CreateCell(columnIndex);
                    cell.CellStyle = dataRowCellStyles[cellIndex];
                    if (value == null || value == DBNull.Value)
                    {
                        cell.SetCellValue(" ");
                    }
                    else
                    {
                        if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                        {
                            cell.SetCellValue(Convert.ToDateTime(value).ToString("yyyy/MM/dd"));
                        }
                        else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?)
                             || prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                        {
                            cell.SetCellValue(Convert.ToDouble(value));
                        }
                        else
                        {
                            cell.SetCellValue(Convert.ToString(value));
                        }
                    }
                    cellIndex++;
                }
                startRowIndex++;
            }
            //for (int i = 0; i < headerColumns.Count; i++)
            //{
            //    exportSheet.AutoSizeColumn(i);
            //}
            var ms = new MemoryStream();
            exportBook.Write(ms);
            return ms;
        }

        /// <summary>
        /// 泛型集合导出Excel
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="templateFile">导出模板文件</param>
        /// <param name="destFile">导出目标文件</param>
        /// <param name="headerRowNum">excel模板文件的列头位置索引(0开始),最多支持两行合并的复杂表头</param>
        /// <param name="lst">数据源集合</param>
        /// <param name="sheetName">模板文件中的工作薄名称</param>
        /// <param name="custCellStyle">外部提供自定义单元格样式功能的方法</param>
        /// <returns></returns>
        public static bool Export2Excel2003<T>(string templateFile, string destFile, int headerRowNum,
            IList<T> lst, string sheetName = "Sheet1", Func<T, string, Color> custCellStyle = null)
        {
            HSSFWorkbook book = null;
            using (var stream = System.IO.File.OpenRead(templateFile))
            {
                var template = new NPOI.POIFS.FileSystem.POIFSFileSystem(stream);
                book = new HSSFWorkbook(template);
                var sheet = book.GetSheet(sheetName);
                if (sheet == null)
                {
                    throw new ArgumentException(string.Concat(sheetName, "不存在!"));
                }
                format = book.CreateDataFormat() as HSSFDataFormat;
                var styles = MakeStyles(book);
                if (lst != null && lst.Count > 0)
                {
                    //IList<Type> propTypes = new List<Type>();
                    var exportedProperties = typeof(T).GetProperties()
                    //    .Where(p =>       //数据类型中要导出的属性
                    //{
                    //    var attributes = p.GetCustomAttributes(typeof(ExcelColumnAttribute), false);
                    //    return attributes != null && attributes.Length > 0 && attributes[0] is ExcelColumnAttribute;
                    //})
                    .ToList();
                    //exportedProperties.ToList().ForEach(prop => propTypes.Add(prop.PropertyType));
                    var headerRow = sheet.GetRow(headerRowNum);                         //模板文件中的表头行
                    IList<string> headerColumns = new List<string>();                   //要导出的所有列名称
                    int cellIndex = 0;
                    foreach (var cell in headerRow.Cells)
                    {
                        if (!string.IsNullOrEmpty(cell.StringCellValue))
                        {
                            headerColumns.Add(cell.StringCellValue);
                        }
                        else                                                            //找上一行的表头列名称
                        {
                            var preHeaderCell = sheet.GetRow(headerRowNum - 1).Cells[cellIndex];
                            if (preHeaderCell != null) { headerColumns.Add(preHeaderCell.StringCellValue); }
                        }
                        cellIndex++;
                    }
                    int startRowIndex = headerRowNum + 1;
                    foreach (T item in lst)                                     //循环写行
                    {
                        IRow row = sheet.CreateRow(startRowIndex);
                        int columnIndex = 0;
                        foreach (string column in headerColumns)                //循环写列
                        {
                            ICell cell = row.CreateCell(columnIndex);
                            string columnName = column.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");
                            var prop = exportedProperties
                            //    .Where(p => {          //要导出的属性
                            //    var attribute = p.GetCustomAttributes(typeof(ExcelColumnAttribute), false)[0] as ExcelColumnAttribute;
                            //    return attribute != null && columnName.Equals(attribute.Name.Replace(" ", ""), StringComparison.CurrentCultureIgnoreCase);
                            //})
                            .SingleOrDefault();
                            var style = styles[0];//GetCellStyle(book, prop, item, custCellStyle);
                            WriteCellValue(cell, style, prop, item);
                            columnIndex++;
                        }
                        startRowIndex++;
                    }
                    for (int i = 0; i < headerColumns.Count; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }
                }
            }
            using (var data = File.Create(destFile))
            {
                book.Write(data);
                data.Close();
                return true;
            }
        }


        private static IList<ICellStyle> MakeStyles(HSSFWorkbook book)
        {
            var styles = new List<ICellStyle>();
            ICellStyle defaultStyle = book.CreateCellStyle();
            defaultStyle.BorderBottom = BorderStyle.Thin;
            defaultStyle.BorderTop = BorderStyle.Thin;
            defaultStyle.BorderLeft = BorderStyle.Thin;
            defaultStyle.BorderRight = BorderStyle.Thin;
            defaultStyle.Alignment = HorizontalAlignment.Left;
            defaultStyle.DataFormat = format.GetFormat("@");
            styles.Add(defaultStyle);

            ICellStyle greenStyle = book.CreateCellStyle();
            greenStyle.BorderBottom = BorderStyle.Thin;
            greenStyle.BorderTop = BorderStyle.Thin;
            greenStyle.BorderLeft = BorderStyle.Thin;
            greenStyle.BorderRight = BorderStyle.Thin;
            greenStyle.Alignment = HorizontalAlignment.Left;
            greenStyle.DataFormat = format.GetFormat("@");
            greenStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Green.Index;//.GREEN.index;                  //此三个属性同时设置单元格背景色.
            greenStyle.FillPattern = FillPattern.Squares;//FillPatternType.SQUARES;        //一定要注意FillForegroundColor,FillPattern,FillBackgroundColor设置顺序,
            greenStyle.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.Green.Index;//.GREEN.index;                  //否则意想不到的后果...
            styles.Add(greenStyle);

            ICellStyle yellowStyle = book.CreateCellStyle();
            yellowStyle.BorderBottom = BorderStyle.Thin;
            yellowStyle.BorderTop = BorderStyle.Thin;
            yellowStyle.BorderLeft = BorderStyle.Thin;
            yellowStyle.BorderRight = BorderStyle.Thin;
            yellowStyle.Alignment = HorizontalAlignment.Left;
            yellowStyle.DataFormat = format.GetFormat("@");
            yellowStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;//.YELLOW.index;                  //此三个属性同时设置单元格背景色.
            yellowStyle.FillPattern = FillPattern.Squares;//FillPatternType.SQUARES;        //一定要注意FillForegroundColor,FillPattern,FillBackgroundColor设置顺序,
            yellowStyle.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;//.YELLOW.index;                  //否则意想不到的后果...
            styles.Add(yellowStyle);

            ICellStyle redStyle = book.CreateCellStyle();
            redStyle.BorderBottom = BorderStyle.Thin;
            redStyle.BorderTop = BorderStyle.Thin;
            redStyle.BorderLeft = BorderStyle.Thin;
            redStyle.BorderRight = BorderStyle.Thin;
            redStyle.Alignment = HorizontalAlignment.Left;
            redStyle.DataFormat = format.GetFormat("@");
            redStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Red.Index;//.RED.index;                  //此三个属性同时设置单元格背景色.
            redStyle.FillPattern = FillPattern.Squares; //FillPatternType.SQUARES;        //一定要注意FillForegroundColor,FillPattern,FillBackgroundColor设置顺序,
            redStyle.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.Red.Index;//.RED.index;                  //否则意想不到的后果...
            styles.Add(redStyle);
            return styles;

        }

        //private static ICellStyle GetCellStyle<T>(HSSFWorkbook book, PropertyInfo prop, T item, Func<T, string, Color> custCellStyle)
        //{
        //    ICellStyle style = styles[0];
        //    if (custCellStyle != null && prop != null)
        //    {
        //        var color = custCellStyle(item, prop.Name);
        //        if (color == Color.Green)
        //        {
        //            style = styles[1];
        //        }
        //        if (color == Color.Yellow)
        //        {
        //            style = styles[2];
        //        }
        //        if (color == Color.Red)
        //        {
        //            style = styles[3];
        //        }
        //    }
        //    var type = typeof(String);
        //    if (prop != null)
        //    {
        //        type = prop.PropertyType;
        //    }
        //    if (type == typeof(DateTime) || type == typeof(DateTime?))
        //    {
        //        style.Alignment = HorizontalAlignment.CENTER;
        //    }
        //    if (type == typeof(decimal) || type == typeof(decimal?))
        //    {
        //        style.Alignment = HorizontalAlignment.RIGHT;
        //        style.DataFormat = format.GetFormat("#,##0.00000");
        //    }
        //    return style;
        //}

        /// <summary>
        /// 写入单元格值
        /// </summary>
        /// <typeparam name="T">写入行对象类型</typeparam>
        /// <param name="cell">要写入的单元格</param>
        /// <param name="cellStyle">单元格样式</param>
        /// <param name="prop">对象的导出属性</param>
        /// <param name="item">写入行对象</param>
        /// <param name="custCellEdition"></param>
        private static void WriteCellValue<T>(ICell cell, ICellStyle cellStyle, PropertyInfo prop, T item)
        {
            cell.CellStyle = cellStyle;
            if (prop == null)
            {
                cell.SetCellValue(" ");
            }
            else
            {
                object value = GetExportValue<T>(prop.Name, item);    //导出的值
                if (value == null)
                {
                    cell.SetCellValue(" ");
                }
                else
                {
                    if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                    {
                        cell.SetCellValue(Convert.ToDateTime(value).ToString("yyyy/MM/dd"));
                    }
                    else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?)
                         || prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                    {
                        cell.SetCellValue(Convert.ToDouble(value));
                    }
                    else
                    {
                        cell.SetCellValue(Convert.ToString(value));
                    }
                }
            }
        }

        private static Hashtable compliedMethodGetter = new Hashtable();

        /// <summary>
        /// 获取要导出的值
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="propName">要读取的属性名</param>
        /// <param name="data">实体数据</param>
        /// <returns>实体属性值</returns>
        public static object GetExportValue<T>(string propName, T data)
        {
            string key = string.Format("{0}:{1}", typeof(T).FullName, propName);
            Func<T, object> fn = null;
            if (compliedMethodGetter.ContainsKey(key))
            {
                fn = compliedMethodGetter[key] as Func<T, object>;
            }
            if (fn == null)
            {
                var arg = Expression.Parameter(typeof(T));
                Expression prop = Expression.Property(arg, propName);
                fn = Expression.Lambda<Func<T, object>>(Expression.Convert(prop, typeof(object)), arg).Compile();
                compliedMethodGetter.Add(key, fn);
            }
            if (fn != null)
            {
                return fn(data);
            }
            return null;
        }

        ///// <summary>
        ///// 生成单元格样式与数据类型的数据字典
        ///// </summary>
        ///// <param name="styleDict">数据字典</param>
        ///// <param name="workbook">Excel Workbook</param>
        ///// <param name="types">数据类型列表</param>
        //private static IDictionary<Type, ICellStyle> GetStyleDict(HSSFWorkbook workbook, IList<Type> types)
        //{
        //    IDictionary<Type, ICellStyle> styleDict = new Dictionary<Type, ICellStyle>();
        //    HSSFDataFormat format = workbook.CreateDataFormat() as HSSFDataFormat;
        //    foreach (Type type in types)
        //    {
        //        if (styleDict.ContainsKey(type)) { continue; }
        //        ICellStyle cellStyle = workbook.CreateCellStyle();
        //        cellStyle.BorderBottom = BorderStyle.THIN;
        //        cellStyle.BorderTop = BorderStyle.THIN;
        //        cellStyle.BorderLeft = BorderStyle.THIN;
        //        cellStyle.BorderRight = BorderStyle.THIN;
        //        cellStyle.Alignment = HorizontalAlignment.LEFT;
        //        cellStyle.DataFormat = format.GetFormat("@");
        //        if (type == typeof(DateTime) || type == typeof(DateTime?))
        //        {
        //            cellStyle.Alignment = HorizontalAlignment.CENTER;
        //        }
        //        if (type == typeof(decimal) || type == typeof(decimal?))
        //        {
        //            cellStyle.Alignment = HorizontalAlignment.RIGHT;
        //            cellStyle.DataFormat = format.GetFormat("#,##0.00000");
        //        }
        //        styleDict.Add(type, cellStyle);
        //    }
        //    return styleDict;
        //}

        ///// <summary>
        ///// 给单元格设置样式和值
        ///// </summary>
        ///// <param name="cell">单元格</param>
        ///// <param name="cellStyle">单元格样式</param>
        ///// <param name="colType">列数据类型</param>
        ///// <param name="value">数据值</param>
        //private static void SetCellValue(ICell cell, ICellStyle cellStyle, Type colType, object value, Action<ICell, object> custCellEdition = null)
        //{
        //    if (colType == typeof(DateTime) || colType == typeof(DateTime?))
        //    {
        //        cell.SetCellValue(Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss"));
        //    }
        //    else if (colType == typeof(decimal) || colType == typeof(decimal?)
        //         || colType == typeof(int) || colType == typeof(int?))
        //    {
        //        if (value == null) { cell.SetCellValue(string.Empty);}
        //        else { cell.SetCellValue(Convert.ToDouble(value)); }
        //    }
        //    else
        //    {
        //        cell.SetCellValue(Convert.ToString(value));
        //    }
        //    cell.CellStyle = cellStyle;
        //}
    }
}
