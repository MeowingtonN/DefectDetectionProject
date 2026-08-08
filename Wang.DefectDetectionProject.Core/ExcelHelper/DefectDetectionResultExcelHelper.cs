using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wang.DefectDetectionProject.Core.Models;

namespace Wang.DefectDetectionProject.Core.ExcelHelper
{
    public static class DefectDetectionResultExcelHelper
    {
        /// <summary>
        /// 导出所有图像列表中的缺陷检测结果到 Excel
        /// </summary>
        /// <param name="imageListItems">图像列表集合</param>
        /// <param name="filePath">保存路径（如 @"D:\Results.xlsx"）</param>
        /// <param name="sheetName">工作表名称（可选）</param>
        public static void ExportDefectDetectionResults(
            ObservableCollection<ImageListItem> imageListItems,
            string filePath,
            string sheetName = "DetectionResults")
        {
            if (imageListItems == null)
                return;

            // 1. 收集所有 DefectDetectionResult，跳过空集合
            var allResults = imageListItems
                .Where(item => item != null && item.DefectDetectionResults != null && item.DefectDetectionResults.Count > 0)
                .SelectMany(item => item.DefectDetectionResults!)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            // 2. 写表头
            worksheet.Cell(1, 1).Value = "图像名";
            worksheet.Cell(1, 2).Value = "缺陷名称";
            worksheet.Cell(1, 3).Value = "标记颜色";
            worksheet.Cell(1, 4).Value = "缺陷总面积(px)";
            worksheet.Cell(1, 5).Value = "缺陷个数";
            worksheet.Cell(1, 6).Value = "检测结果";

            // 3. 写数据
            for (int i = 0; i < allResults.Count; i++)
            {
                var result = allResults[i];
                int row = i + 2;

                worksheet.Cell(row, 1).Value = result.FileName ?? string.Empty;
                worksheet.Cell(row, 2).Value = result.DefectName ?? string.Empty;
                // HTuple? 类型处理：ToString() 可将基本值转为字符串，null 则留空
                worksheet.Cell(row, 3).Value = result.MarkingColor?.ToString() ?? string.Empty;
                worksheet.Cell(row, 4).Value = result.Area?.ToString() ?? string.Empty;
                worksheet.Cell(row, 5).Value = result.Count?.ToString() ?? string.Empty;
                worksheet.Cell(row, 6).Value = result.DetectionResult ?? string.Empty;
            }

            // 4. 美化与保存
            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        }
    }
}
