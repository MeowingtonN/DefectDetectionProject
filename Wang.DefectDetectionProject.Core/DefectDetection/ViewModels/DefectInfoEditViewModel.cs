using HalconDotNet;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wang.DefectDetectionProject.Core.DefectDetection.Models;
using Wang.DefectDetectionProject.Core.ExcelHelper;

namespace Wang.DefectDetectionProject.Core.DefectDetection.ViewModels
{
    public class DefectInfoEditViewModel : NavigationViewModel
    {
        public DefectInfoEditViewModel(ObservableCollection<DefectInfo>? defectInfoCollection)
        {
            DefectInfoCollection = defectInfoCollection;

            AddDefectCommand = new DelegateCommand(AddDefect);
            DeleteDefectCommand = new DelegateCommand(DeleteDefect);
            ImportFromExcelCommand = new DelegateCommand(ImportFromExcel);
            ExportExcelCommand = new DelegateCommand(ExportExcel);
        }

        private void ExportExcel()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "导出Excel";
            dialog.Filter = "Excel文件|*.xlsx|所有文件|*.*";
            dialog.DefaultExt = ".xlsx";
            dialog.FileName = "DefectInfos.xlsx";
            var dialogResult = (bool)dialog.ShowDialog()!;
            if (dialogResult && DefectInfoCollection != null)
            {
                DefectInfoExcelHelper.SaveDefectInfoToExcel(DefectInfoCollection, dialog.FileName);
            }
        }

        private void ImportFromExcel()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "选择要导入的Excel";
            dialog.Filter = "Excel文件|*.xlsx|所有文件|*.*";
            var dialogResult = (bool)dialog.ShowDialog()!;
            if (dialogResult)
            {
                DefectInfoCollection = DefectInfoExcelHelper.LoadDefectInfoFromExcel(dialog.FileName);
            }
        }

        private void DeleteDefect()
        {
            if(SelectedDefectInfo != null)
            {
                DefectInfoCollection!.Remove(SelectedDefectInfo);
                SelectedDefectInfo = DefectInfoCollection.LastOrDefault();
            }
        }

        private void AddDefect()
        {
            DefectInfo defectInfo = new DefectInfo()
            {
                ClassName = "新缺陷"
            };
            DefectInfoCollection!.Add(defectInfo);
            SelectedDefectInfo = defectInfo;
        }

        private ObservableCollection<DefectInfo>? defectInfoCollection;
        public ObservableCollection<DefectInfo>? DefectInfoCollection 
        {
            get {  return defectInfoCollection; }
            set { defectInfoCollection = value; RaisePropertyChanged(); }
        }

        private DefectInfo? selectedDefectInfo;
        public DefectInfo? SelectedDefectInfo
        {
            get { return selectedDefectInfo; }
            set { selectedDefectInfo = value; RaisePropertyChanged(); }
        }

        public DelegateCommand AddDefectCommand { get; }
        public DelegateCommand DeleteDefectCommand { get; }
        public DelegateCommand ImportFromExcelCommand { get; }
        public DelegateCommand ExportExcelCommand { get; }
    }
}
