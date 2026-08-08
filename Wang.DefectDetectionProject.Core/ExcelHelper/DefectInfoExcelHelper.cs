using ClosedXML.Excel;
using System.Collections.ObjectModel;
using System.IO;
using Wang.DefectDetectionProject.Core.DefectDetection.Models;

namespace Wang.DefectDetectionProject.Core.ExcelHelper
{
    public static class DefectInfoExcelHelper
    {
        /// <summary>
        /// 将 DefectInfo 集合保存为 Excel 文件
        /// </summary>
        /// <param name="defects">缺陷数据集合</param>
        /// <param name="filePath">保存路径（如 @"C:\Defects.xlsx"）</param>
        /// <param name="sheetName">工作表名称（可选）</param>
        public static void SaveDefectInfoToExcel(
            ObservableCollection<DefectInfo> defects,
            string filePath,
            string sheetName = "DefectInfos")
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            // 1. 写表头
            worksheet.Cell(1, 1).Value = "类别名称";
            worksheet.Cell(1, 2).Value = "类别ID";

            // 2. 写数据
            for (int i = 0; i < defects.Count; i++)
            {
                var defect = defects[i];
                int row = i + 2; // 数据从第2行开始

                // 处理可空字符串，避免空引用
                worksheet.Cell(row, 1).Value = defect.ClassName ?? string.Empty;
                worksheet.Cell(row, 2).Value = defect.ClassID;
            }

            // 3. 简单美化（可选）
            var headerRange = worksheet.Range(1, 1, 1, 2);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            worksheet.Columns().AdjustToContents(); // 自动列宽

            // 4. 保存文件
            workbook.SaveAs(filePath);
        }

        /// <summary>
        /// 从 Excel 文件中读取缺陷信息
        /// </summary>
        /// <param name="filePath">Excel 文件路径</param>
        /// <param name="sheetName">工作表名称（默认为 "Defects"）</param>
        /// <returns>缺陷信息集合</returns>
        public static ObservableCollection<DefectInfo> LoadDefectInfoFromExcel(
            string filePath,
            string sheetName = "DefectInfos")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"文件不存在: {filePath}");

            var defects = new ObservableCollection<DefectInfo>();

            using var workbook = new XLWorkbook(filePath);
            if (!workbook.Worksheets.TryGetWorksheet(sheetName, out var worksheet))
                throw new InvalidOperationException($"工作表 '{sheetName}' 不存在");

            // 第一行是表头，数据从第二行开始
            if(worksheet == null || worksheet.RangeUsed() == null) return defects;
            var rows = worksheet.RangeUsed()!.RowsUsed();
            bool isFirstRow = true;

            foreach (var row in rows)
            {
                // 跳过表头
                if (isFirstRow)
                {
                    isFirstRow = false;
                    continue;
                }

                // 获取单元格值，注意处理空值
                var classNameCell = row.Cell(1).GetValue<string>();
                var classIDCell = row.Cell(2);

                // 跳过全空的行（可选）
                if (string.IsNullOrWhiteSpace(classNameCell) && classIDCell.IsEmpty())
                    continue;

                var defect = new DefectInfo
                {
                    ClassName = string.IsNullOrWhiteSpace(classNameCell) ? null : classNameCell,
                    // 尝试解析数值，失败则给默认值或抛出异常
                    ClassID = classIDCell.TryGetValue<int>(out var id) ? id : 0
                };

                defects.Add(defect);
            }

            return defects;
        }
    }
}
