using System.Collections.Concurrent;
using System.Windows.Media.Effects;
using Microsoft.Win32.SafeHandles;
using Proryv.AskueARM2.Both.VisualCompHelpers.Data;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Proryv.AskueARM2.Client.ServiceReference.UAService;
using Proryv.AskueARM2.Server.DBAccess.Public.Common;
using Visifire.Charts;
using Visifire.Commons;

namespace Proryv.AskueARM2.Both.VisualCompHelpers
{
    public class VisifireChartHelper
    {
        public static void ChartButtonExport(object sender, RoutedEventArgs e)
        {
            var btn = sender as ChartButton;
            if (btn == null) return;
            var chart = btn.FindParent<Chart>();
            if (chart == null) return;
            //Пробуем взять подпись графика
            var title = String.Empty;
            try
            {
                var form = chart.FindParent<IFormingExportedFileName>();

                if (form != null)
                {
                    //Если на форме есть интерфэйс для взятия названия - берем название из формы
                    bool isExportCollapsedDetail;
                    title = form.FormingExportedFileName(chart.Name, out isExportCollapsedDetail);
                }
                else
                {
                    if (chart.Titles != null && chart.Titles.Count > 0)
                    {
                        title = chart.Titles[0].Text;
                    }
                    else
                    {
                        title = "График";
                    }
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            // ReSharper restore EmptyGeneralCatchClause
            { }

            ShowExportToJpegDialog(title, chart);
        }

        public static void ChartButtonPrint(object sender, RoutedEventArgs e)
        {
            var btn = sender as ChartButton;
            if (btn != null)
            {
                VisifireChartHelper.ShowPrintJpegDialog(btn.FindParent<Chart>());
            }
        }

        public static void ChartButtonView3D(object sender, RoutedEventArgs e)
        {
            var btn = sender as ChartButton;
            if (btn != null)
            {
                VisifireChartHelper.ToggleView3D(btn.FindParent<Chart>());
            }
        }

        public static void CreateAndFillChart(Chart vcChart, List<TConsumptionScheduleTypeRow> source, string seriesName)
        {
            vcChart.Series.Clear();

            Brush b = new SolidColorBrush(Color.FromArgb(0xFF, 0xAE, 0x7F, 0x2B));
            var currSeries = new DataSeries
            {
                Color = b, //Здесь присваиваем цвет
                LabelEnabled = false,
                RenderAs = RenderAs.StepLine, ////Линейный график
                XValueType = ChartValueTypes.Time,
                Name = seriesName,
                XValueFormatString = "HH:mm",
                MarkerType = Visifire.Commons.MarkerTypes.Circle,
                DataMappings = new DataMappingCollection
                {
                    new DataMapping {MemberName = "XValue", Path = "Time"},
                    new DataMapping {MemberName = "YValue", Path = "Value"},
                },
                DataSource = source,
            };


            vcChart.Series.Add(currSeries);
            //vcChart.BeginInit();
            //vcChart.EndInit();

            vcChart.Measure(new Size(vcChart.ActualWidth, vcChart.ActualHeight));
            vcChart.Arrange(new Rect(0, 0, vcChart.ActualWidth, vcChart.ActualHeight));
            vcChart.UpdateLayout();
        }

        private const int MinLightness = 1;
        private const int MaxLightness = 10;
        private const float MinLightnessCoef = 1f;
        private const float MaxLightnessCoef = 0.4f;

        //Максимальное значение тока, напряжения или еще чего для векторного графика 
        private const double MaxDoubleLimit = 1000000000000;
        
        /// <summary>
        /// Устаревшая версия, больше не использовать
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="valuesDict"></param>
        public static void UpdatePolarChart(Chart chart, Dictionary<enumArchTechParamType, double> valuesDict)
        {
            if (chart == null || valuesDict == null) return;
            foreach (var val in valuesDict)
             {
                Debug.WriteLine("[enumArchTechParamType." + val.Key+"]="+val.Value+";");
            }
            chart.Series.Clear();

            try
            {

                chart.BeginInit();

                var serieTemplate = chart.Resources["serieTemplate"] as DataTemplate;

                var voltages = new double[3];
                var currents = new double[3];

                valuesDict.TryGetValue(enumArchTechParamType.VoltageL1, out voltages[0]);
                valuesDict.TryGetValue(enumArchTechParamType.VoltageL2, out voltages[1]);
                valuesDict.TryGetValue(enumArchTechParamType.VoltageL3, out voltages[2]);
                valuesDict.TryGetValue(enumArchTechParamType.CurrentL1, out currents[0]);
                valuesDict.TryGetValue(enumArchTechParamType.CurrentL2, out currents[1]);
                valuesDict.TryGetValue(enumArchTechParamType.CurrentL3, out currents[2]);

                //if (voltage.Any(v=>v > 500000) 
                //    || current.Any(c=>c > 1000000)) return; //Не корректное значение

                var paramAnglePhaseVoltage = new[] {enumArchTechParamType.AnglePhaseVoltageL12, enumArchTechParamType.AnglePhaseVoltageL23};
                var paramsAngleUI = new[]
                    {enumArchTechParamType.AngleUIL1, enumArchTechParamType.AngleUIL2, enumArchTechParamType.AngleUIL3};
                var paramsCosFi = new[] {enumArchTechParamType.CosFiL1, enumArchTechParamType.CosFiL2, enumArchTechParamType.CosFiL3};
                var paramsCosFiExport = new[]
                    {enumArchTechParamType.CosFiL1Export, enumArchTechParamType.CosFiL2Export, enumArchTechParamType.CosFiL3Export};
                var paramsReactiveExport = new[]
                {
                    enumArchTechParamType.ReactiveL1Export, enumArchTechParamType.ReactiveL2Export, enumArchTechParamType.ReactiveL3Export
                };

                double maxU = voltages.Max(), maxI = currents.Max();

                if (maxU == 0 && maxI != 0) maxU = 1.0;

                double voltageAngel = 0;
                int brushIndex = 1;
                for (int i = 0; i < 3; i++)
                {
                    //Есть ли углы в базе, если нет рисуем классически каждые 120 градусов по каждой фазе
                    double v = 0;
                    if (i > 0 && (!valuesDict.TryGetValue(paramAnglePhaseVoltage[i - 1], out v) || v == 0))
                    {
                        if (i != 2 || (i == 2 && (!valuesDict.TryGetValue(enumArchTechParamType.AnglePhaseVoltageL13, out v) || v == 0)))
                        {
                            voltageAngel = i * 120;
                        }
                        else
                        {
                            voltageAngel = v;
                        }
                    }
                    else voltageAngel += v; //Наращиваем угол относительно 1 фазы

                    var text = "U" + (i + 1);
                    var serie = serieTemplate.LoadContent() as DataSeries;

                    var voltage = voltages[i];
                    if (voltage >= 0 && voltage < MaxDoubleLimit)
                    {
                        try
                        {
                            serie.BeginInit();
                            /// Цвета из ПУЭ 7 издания, 1 глава, пункт 1.1.29. и 1.1.30. 
                            var brushKey = "vector_brush_collection_" + i.ToString() + brushIndex.ToString();
                            serie.Color = Application.Current.Resources[brushKey] as Brush;
                            serie.DataContext = text;
                            serie.ZValueFormatString = "### ### ##0.###";
                            serie.Name = "U = " + voltage.ToString("# ##0.######") + " V"; //Отображаем значения через название серии

                            serie.DataPoints.Add(new DataPoint {XValue = voltageAngel, YValue = 0, ZValue = 0, LabelEnabled = false});
                            serie.DataPoints.Add(new DataPoint {XValue = voltageAngel, YValue = voltage, ZValue = voltage});
                            chart.Series.Add(serie);
                            brushIndex++;
                        }
                        finally
                        {
                            serie.EndInit();
                        }
                    }

                    text = "I" + (i + 1);
                    serie = serieTemplate.LoadContent() as DataSeries;

                    var current = currents[i];
                    if (current >= 0 && current < MaxDoubleLimit)
                    {

                        try
                        {
                            serie.BeginInit();
                            /// Цвета из ПУЭ 7 издания, 1 глава, пункт 1.1.29. и 1.1.30. 
                            var brushKey = "vector_brush_collection_" + i.ToString() + brushIndex.ToString();
                            serie.Color = Application.Current.Resources[brushKey] as Brush;
                            serie.DataContext = text;
                            serie.ZValueFormatString = "### ### ##0.###";
                            serie.Name = "I = " + current.ToString("# ##0.######") + " A";

                            //Проверяем есть в БД угол мужду напряжением и током, считаем на основе косинуса фи только если нет
                            double angelCurrent;
                            //В БД нет угла между напряжением и током, считаем на основе кос фи
                            if (!valuesDict.TryGetValue(paramsAngleUI[i], out angelCurrent) || angelCurrent == 0) 
                            {
                                double cf;
                                //Надо еще проверять cosFi[i] должеть быть от -1 до 1, пишем что векторную диаграмму построить невозможно
                                if (!valuesDict.TryGetValue(paramsCosFi[i], out cf) || cf > 1 || cf < -1)
                                {
                                    //Пытаемся смотреть отдачу
                                    if (!valuesDict.TryGetValue(paramsCosFiExport[i], out cf) || cf < -1 || cf > 1)
                                    {
                                        //Угол нельзя посчитать, считаем его 0.9
                                        cf = 0.9;
                                    }
                                }

                                //Если Q-, то считаем XValue = i * 120 + (360 - Math.Acos(CosFi[i]) / Math.PI * 180), т.е. поворачиваем в нужную квадранту
                                double reactiveExport;
                                if (valuesDict.TryGetValue(paramsReactiveExport[i], out reactiveExport) &&
                                    reactiveExport > 0)
                                {
                                    angelCurrent = 360 - Math.Acos(cf) / Math.PI * 180;
                                }
                                else
                                {
                                    angelCurrent = Math.Acos(cf) / Math.PI * 180;
                                }
                            }

                            var sum = Math.Round(voltageAngel + angelCurrent, 7);
                            var yVal = Math.Round(current / maxI * maxU, 7);

                            serie.DataPoints.Add(new DataPoint {XValue = voltageAngel, YValue = 0, ZValue = 0, LabelEnabled = false});
                            serie.DataPoints.Add(new DataPoint
                            {
                                XValue = sum,
                                YValue = yVal,
                                ZValue = current
                            });

                            chart.Series.Add(serie);
                            brushIndex++;
                        }
                        finally
                        {
                            serie.EndInit();
                        }
                    }
                }

                if (maxU > 0)
                {
                    chart.AxesY[0].Interval = maxU * 1.25;
                }
            }
            finally
            {
                chart.EndInit();
            }
            //chart.UpdateLayout();
        }

        /// <summary>
        /// Строим график по предварительно расчитанным на сервере данным
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="polarChartSource"></param>
        public static void UpdatePolarChart(Chart chart, Dictionary<byte, Dictionary<enumVectorChartParams, double>> polarChartSource)
        {
            if (chart == null || polarChartSource == null) return;
            
            chart.Series.Clear();

            try
            {

                chart.BeginInit();

                var serieTemplate = chart.Resources["serieTemplate"] as DataTemplate;

                int brushIndex = 1;
                for (byte i = 0; i < 3; i++)
                {
                    Dictionary<enumVectorChartParams, double> polarChartSourceByIndex;
                    if (!polarChartSource.TryGetValue(i, out polarChartSourceByIndex)) break;

                    var text = "U" + (i + 1);
                    var serie = serieTemplate.LoadContent() as DataSeries;

                    double voltage, voltageAngel;
                    if (polarChartSourceByIndex.TryGetValue(enumVectorChartParams.VoltageXValue, out voltageAngel)
                    && polarChartSourceByIndex.TryGetValue(enumVectorChartParams.VoltageYValue, out voltage)
                    && voltage >= 0 && voltage < MaxDoubleLimit)
                    {
                        try
                        {
                            serie.BeginInit();
                            /// Цвета из ПУЭ 7 издания, 1 глава, пункт 1.1.29. и 1.1.30. 
                            var brushKey = "vector_brush_collection_" + i.ToString() + brushIndex.ToString();
                            serie.Color = Application.Current.Resources[brushKey] as Brush;
                            serie.DataContext = text;
                            serie.ZValueFormatString = "### ### ##0.###";
                            serie.Name = "U = " + voltage.ToString("# ##0.######") + " V"; //Отображаем значения через название серии

                            serie.DataPoints.Add(new DataPoint { XValue = voltageAngel, YValue = 0, ZValue = 0, LabelEnabled = false });
                            serie.DataPoints.Add(new DataPoint { XValue = voltageAngel, YValue = voltage, ZValue = voltage });
                            chart.Series.Add(serie);
                            brushIndex++;
                        }
                        finally
                        {
                            serie.EndInit();
                        }
                    }

                    text = "I" + (i + 1);
                    serie = serieTemplate.LoadContent() as DataSeries;

                    double current, sum, yVal;
                    if (polarChartSourceByIndex.TryGetValue(enumVectorChartParams.CurrentXValue, out sum)
                         && polarChartSourceByIndex.TryGetValue(enumVectorChartParams.CurrentYValue, out yVal)
                         && polarChartSourceByIndex.TryGetValue(enumVectorChartParams.CurrentZValue, out current)
                         && current >= 0 && current < MaxDoubleLimit)
                    {

                        try
                        {
                            serie.BeginInit();
                            /// Цвета из ПУЭ 7 издания, 1 глава, пункт 1.1.29. и 1.1.30. 
                            var brushKey = "vector_brush_collection_" + i.ToString() + brushIndex.ToString();
                            serie.Color = Application.Current.Resources[brushKey] as Brush;
                            serie.DataContext = text;
                            serie.ZValueFormatString = "### ### ##0.###";
                            serie.Name = "I = " + current.ToString("# ##0.######") + " A";

                            serie.DataPoints.Add(new DataPoint { XValue = voltageAngel, YValue = 0, ZValue = 0, LabelEnabled = false });
                            serie.DataPoints.Add(new DataPoint
                            {
                                XValue = sum,
                                YValue = yVal,
                                ZValue = current
                            });

                            chart.Series.Add(serie);
                            brushIndex++;
                        }
                        finally
                        {
                            serie.EndInit();
                        }
                    }
                }

                //if (maxU > 0)
                //{
                //    chart.AxesY[0].Interval = maxU * 1.25;
                //}
            }
            finally
            {
                chart.EndInit();
            }
            //chart.UpdateLayout();
        }

        public static void ShowPrintJpegDialog(Chart chart)
        {
            try
            {
                chart.Print();
            }
            catch (Exception)
            {
                //TODO
            }
        }

        public static void ToggleView3D(Chart chart)
        {
            chart.View3D = !chart.View3D;
        }

        public static void ShowExportToJpegDialog(string fileName, Chart chart)
        {
            try
            {
                using (System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog())
                {

                    //saveDialog.DefaultExt = "jpg";
                    //saveDialog.Filter = "JPG Files (*.jpg)|*.jpg";
                    saveDialog.FilterIndex = 1;
                    saveDialog.FileName = fileName.removeBadChar();

                    saveDialog.OverwritePrompt = true;
                    saveDialog.AddExtension = false;
                    saveDialog.SupportMultiDottedExtensions = true;

                    if (saveDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                    //ExportToJpeg(saveDialog.FileName, chart);
                    if (!string.IsNullOrEmpty(saveDialog.FileName)) chart.Export(saveDialog.FileName, ExportType.Jpg);
                }
            }
            catch (Exception)
            {
                //TODO
            }
        }

        public static void ExportToJpeg(string path, Chart surface)
        {
            if (String.IsNullOrEmpty(path)) return;

            var background = surface.Background;

            Transform transform = surface.LayoutTransform;
            surface.LayoutTransform = null;
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
            (int)surface.ActualWidth,
            (int)surface.ActualHeight,
             (Double)PixelsPerInch(Orientation.Horizontal),
             (Double)PixelsPerInch(Orientation.Vertical),
            PixelFormats.Pbgra32);
            renderBitmap.Render(surface);

            using (FileStream outStream = new FileStream(path, FileMode.Create))
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(outStream);
            }

            //Восстанавливаем настройки
            surface.LayoutTransform = transform;
        }

        
        public static void CreateAndFillUaDatas(Chart vcChart, List<TUAData> archive, List<TUAData> selecteDatas, IVisualDataRequestObjectsNames getNameInterface, string timeZoneId)
        {
            vcChart.Series.Clear();

            if (archive == null || archive.Count == 0) return;

            int i = 0;
            var currTimeZoneInfo = timeZoneId.GeTimeZoneInfoById();

            foreach (var tuaDataGroupByNode in archive.GroupBy(a => a.UANode_ID))
            {
                var titleName = getNameInterface.GetUaNodeName(tuaDataGroupByNode.Key);
                var currSeries = new DataSeries
                                 {
                                     Color = GetColorByIndexSeries(i++),
                                     LabelEnabled = false,
                                     RenderAs = RenderAs.Line,
                                     XValueType = ChartValueTypes.DateTime,
                                     Name = titleName,
                                     XValueFormatString = "dd.MM.yyyy HH:mm",
                                     MarkerType = Visifire.Commons.MarkerTypes.Circle,
                                     //Legend = titleName,
                                     //ShowInLegend = true,
                                     //DataSource = source,
                                 };



                foreach (var point in tuaDataGroupByNode.OrderBy(a=>a.SourceTimeStamp))
                {
                    if (point == null) continue;

                    decimal v;
                    if (!point.TryGetDecimal(out v))
                    {
                        continue;
                    }
                    var dt = TimeZoneInfo.ConvertTimeFromUtc(point.SourceTimeStamp, currTimeZoneInfo);
                    //Это новая серия просто заполняем точками
                    DataPoint dp = new DataPoint
                                   {
                                       XValue = dt,
                                       YValue = (double)v,
                                       ToolTipText = titleName + "\n" + dt.ToString("dd.MM.yyyy HH:mm") + "  " + v.ToString("#,##0.######"),
                                   };

                    if (selecteDatas.Contains(point))
                    {
                        dp.MarkerType = MarkerTypes.Square;
                        dp.MarkerScale = 1.3;
                    }

                    currSeries.DataPoints.Add(dp);
                }

                vcChart.Series.Add(currSeries);
            }

            vcChart.Measure(new Size(vcChart.ActualWidth, vcChart.ActualHeight));
            vcChart.Arrange(new Rect(0, 0, vcChart.ActualWidth, vcChart.ActualHeight));
            vcChart.UpdateLayout();
        }

        public static void CreateAndFillOldTelescopeDatas(Chart vcChart, List<TOldTelescope_UniversalData> archive, List<TOldTelescope_UniversalData> selecteDatas, string timeZoneId)
        {
            vcChart.Series.Clear();

            if (archive == null || archive.Count == 0) return;

            int i = 0;
            var currTimeZoneInfo = timeZoneId.GeTimeZoneInfoById();

            foreach (var tuaDataGroupByNode in archive.GroupBy(a => new {a.ID}))
            {
                var titleName = archive.First(n => n.ID == tuaDataGroupByNode.Key.ID).TreeNode.StringName;

                var currSeries = new DataSeries
                {
                    Color = GetColorByIndexSeries(i++),
                    LabelEnabled = false,
                    RenderAs = RenderAs.Line,
                    XValueType = ChartValueTypes.DateTime,
                    Name = titleName,
                    XValueFormatString = "dd.MM.yyyy HH:mm",
                    MarkerType = Visifire.Commons.MarkerTypes.Circle,
                    //Legend = titleName,
                    //ShowInLegend = true,
                    //DataSource = source,
                };



                foreach (var point in tuaDataGroupByNode.OrderBy(a => a.EventDateTime))
                {
                    if (point == null) continue;
                    
                    if (point.EventDateTime == null)
                        continue;

                    decimal v;
                    if (!point.TryGetDecimal(out v))
                    {
                        continue;
                    }

                    var dt = TimeZoneInfo.ConvertTimeFromUtc(point.EventDateTime.Value, currTimeZoneInfo);
                    //Это новая серия просто заполняем точками
                    DataPoint dp = new DataPoint
                    {
                        XValue = dt,
                        YValue = (double)v,
                        ToolTipText = titleName + "\n" + dt.ToString("dd.MM.yyyy HH:mm") + "  " + v.ToString("#,##0.######"),
                    };

                    if (selecteDatas.Contains(point))
                    {
                        dp.MarkerType = MarkerTypes.Square;
                        dp.MarkerScale = 1.3;
                    }

                    currSeries.DataPoints.Add(dp);
                
                }

                vcChart.Series.Add(currSeries);
            }

            vcChart.Measure(new Size(vcChart.ActualWidth, vcChart.ActualHeight));
            vcChart.Arrange(new Rect(0, 0, vcChart.ActualWidth, vcChart.ActualHeight));
            vcChart.UpdateLayout();
        }


        public static Int32 PixelsPerInch(Orientation orientation)
        {
            Int32 capIndex = (orientation == Orientation.Horizontal) ? 0x58 : 90;
            using (DCSafeHandle handle = UnsafeNativeMethods.CreateDC("DISPLAY"))
            {
                return (handle.IsInvalid ? 0x60 : UnsafeNativeMethods.GetDeviceCaps(handle, capIndex));
            }
        }
        internal sealed class DCSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private DCSafeHandle() : base(true) { }

            protected override Boolean ReleaseHandle()
            {
                return UnsafeNativeMethods.DeleteDC(base.handle);
            }
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class UnsafeNativeMethods
        {
            [DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern Boolean DeleteDC(IntPtr hDC);

            [DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern Int32 GetDeviceCaps(DCSafeHandle hDC, Int32 nIndex);

            [DllImport("gdi32.dll", EntryPoint = "CreateDC", CharSet = CharSet.Auto)]
            public static extern DCSafeHandle IntCreateDC(String lpszDriver,
                String lpszDeviceName, String lpszOutput, IntPtr devMode);

            public static DCSafeHandle CreateDC(String lpszDriver)
            {
                return UnsafeNativeMethods.IntCreateDC(lpszDriver, null, null, IntPtr.Zero);
            }
        }

        
        // список цветов для серий
        private static ConcurrentStack<Brush> ColorDefined = null;
        private static Brush GetColorByIndexSeries(int index)
        {
            if (ColorDefined == null)
            {
                ColorDefined = new ConcurrentStack<Brush>();

                Action<Color> action = (Color c) =>
                                    {
                                        var b = new SolidColorBrush(c);
                                        b.Freeze();
                                        ColorDefined.Push(b);
                                    };

                action(Color.FromRgb(64, 10, 10));
                action(Color.FromRgb(10, 10, 128));
                action(Color.FromRgb(245, 245, 5));
                action(Color.FromRgb(0, 0, 255));
                action(Color.FromRgb(0, 255, 0));
                action(Color.FromRgb(128, 0, 128));
                action(Color.FromRgb(0, 255, 255));
                action(Color.FromRgb(255, 128, 64));
                action(Color.FromRgb(255, 0, 255));
                action(Color.FromRgb(128, 0, 0));
                action(Color.FromRgb(0, 128, 0));
                action(Color.FromRgb(128, 128, 0));
                action(Color.FromRgb(255, 0, 0));
                action(Color.FromRgb(64, 0, 128));
                action(Color.FromRgb(0, 128, 128));
            }


            if (index >= ColorDefined.Count)
            {
                var R = new Random(DateTime.Now.GetHashCode());
                while (ColorDefined.Count <= index)
                {
                    var b = new SolidColorBrush(Color.FromRgb((byte) R.Next(255), (byte) R.Next(255), (byte) R.Next(255)));
                    b.Freeze();
                    ColorDefined.Push(b);
                }
            }

            return ColorDefined.ElementAtOrDefault(index);
        }

        public static void ExecuteGroupTPPowerControl(Chart vcChart, GroupTPFactory GroupTPPower, IVisualDataRequestObjectsNames getNameInterface, Func<enumPowerFlag, string> GetPowerFlagMessage)
        {
            vcChart.Series.Clear();

            if (GroupTPPower == null || GroupTPPower.GroupTPResult == null) return;

            int c = 0;

            vcChart.BeginInit();
            //Когда много серий, необходимо ускорить отрисовывание
            bool isManySeries = GroupTPPower.GroupTPResult.Count > 10;
            foreach (var groupTPResult in GroupTPPower.GroupTPResult.Take(20)) //Ограничиваем 20, больше не влезет на экран
            {
                AddPowerToChart(vcChart, groupTPResult, getNameInterface, c++, GetPowerFlagMessage, isManySeries, true);

                //if (groupTPResult.PowerMax.HasValue)
                //{
                //    AddPowerMaxToChart(vcChart, groupTPResult, c++, groupTPResult.PowerHoursList.Min(p=>p.EventDateTime), groupTPResult.PowerHoursList.Max(p => p.EventDateTime));
                //}
            }

            vcChart.EndInit();

            vcChart.Measure(new Size(vcChart.ActualWidth, vcChart.ActualHeight));
            vcChart.Arrange(new Rect(0, 0, vcChart.ActualWidth, vcChart.ActualHeight));
            vcChart.UpdateLayout();
        }

        static void AddPowerToChart(Chart vcChart, TGroupTPResult groupTpResult,
            IVisualDataRequestObjectsNames getNameInterface, int c,
            Func<enumPowerFlag, string> getPowerFlagMessage, bool isManySeries, bool isShowMaxPowerLine = false)
        {
            if (groupTpResult.PowerHoursList == null || !groupTpResult.PowerHoursList.Any(p => p != null))
                return; //нет нормальных значений, выходим

            var titleName = string.Empty;
            if (groupTpResult.HierarchyObject != null)
            {
                titleName = groupTpResult.HierarchyObject.ToString();
            }
            else if (groupTpResult.ID.TypeHierarchy == enumTypeHierarchy.Section)
            {

                titleName = getNameInterface.GetSectionName(groupTpResult.ID.ID);
            }
            else if (groupTpResult.ID.TypeHierarchy == enumTypeHierarchy.Info_TP)
            {
                titleName = getNameInterface.GetTPName(groupTpResult.ID.ID);
            }

            Brush colorMax = new SolidColorBrush(Color.FromArgb(0xFF, 0xAE, 0x7F, 0x2B)),
                colorMaxExcess = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                colorMin = new SolidColorBrush(Color.FromRgb(0, 0, 255));

            var ser = new DataSeries
            {
                Color = GetColorByIndexSeries(c), //Здесь присваиваем цвет
                LabelEnabled = false,
                RenderAs = isManySeries ? RenderAs.QuickLine : RenderAs.Line,
                XValueType = ChartValueTypes.DateTime,
                Name = titleName,
                XValueFormatString = "dd.MM.yyyy HH:mm",
                //MarkerType = MarkerTypes.Circle,
                MarkerEnabled = false,
                LineThickness = 1,
                //Effect = effect,
                //DataSource = source,
            };

            DataSeries maxPowerSerie = null;

            if (isShowMaxPowerLine && groupTpResult.PowerMax.HasValue)
            {
                maxPowerSerie = new DataSeries
                {
                    Color = GetColorByIndexSeries(c + 1), //Здесь присваиваем цвет
                    LabelEnabled = false,
                    RenderAs = RenderAs.Line,
                    XValueType = ChartValueTypes.DateTime,
                    Name = @"Pmax - " + titleName,
                    XValueFormatString = "dd.MM.yyyy HH:mm",
                    //MarkerType = MarkerTypes.Circle,
                    MarkerEnabled = false,
                    LineThickness = 1,
                    BorderStyle = BorderStyles.Dotted,
                    //Effect = effect,
                    //DataSource = source,
                    //ToolTip = titleName,
                };
            }

            ser.BeginInit();
            try
            {
                foreach (var av in groupTpResult.PowerHoursList)
                {
                    if (av == null) return;
                    //Это новая серия просто заполняем точками
                    var dp = new DataPoint()
                    {
                        XValue = av.EventDateTime,
                        YValue = av.F_VALUE,

                        //AxisXLabel = Mar
                    };

                    if (getPowerFlagMessage != null)
                    {
                        dp.ToolTipText = titleName + "\n" + av.EventDateTime.ToString("dd.MM.yyyy HH:mm") + "  " +
                                         av.F_VALUE.ToString("#,##0.######") + "\n" + getPowerFlagMessage(av.PowerFlag);
                    }

                    if (av.PowerFlag.HasFlag(enumPowerFlag.MaxPowerExcess))
                    {
                        //превышение максимальной мощности
                        dp.MarkerType = MarkerTypes.Triangle;
                        dp.MarkerScale = 2;
                        dp.Color = colorMaxExcess;
                        dp.MarkerEnabled = true;
                    }
                    else if (av.PowerFlag.HasFlag(enumPowerFlag.IsMaxPowerInDay) ||
                             av.PowerFlag.HasFlag(enumPowerFlag.IsMaxPowerInDayInSystemOperatorHour))
                    {
                        dp.MarkerType = MarkerTypes.Circle;
                        dp.MarkerScale = 2;
                        dp.Color = colorMax;
                        dp.MarkerEnabled = true;
                    }
                    else if (av.PowerFlag.HasFlag(enumPowerFlag.IsMinPowerInDay) ||
                             av.PowerFlag.HasFlag(enumPowerFlag.IsMinPowerInDayInSystemOperatorHour))
                    {
                        dp.MarkerType = MarkerTypes.Square;
                        dp.MarkerScale = 2;
                        dp.Color = colorMin;
                        dp.MarkerEnabled = true;
                    }

                    ser.DataPoints.Add(dp);

                    if (isShowMaxPowerLine && groupTpResult.PowerMax.HasValue)
                    {
                        var tt = titleName + "\n" +
                                 "P max: " + groupTpResult.PowerMax.Value.ToString("#,##0.######");

                        if (av.PowerFlag.HasFlag(enumPowerFlag.MaxPowerExcess))
                        {
                            tt += "\n" + "▲ на : " +
                                  (av.F_VALUE - groupTpResult.PowerMax.Value).ToString("#,##0.######");
                        }

                        maxPowerSerie.DataPoints.Add(new DataPoint
                        {
                            XValue = av.EventDateTime,
                            YValue = groupTpResult.PowerMax.Value,
                            ToolTip = tt,
                            ToolTipText = tt
                            //AxisXLabel = Mar
                        });
                    }
                } //foreach
            }
            finally
            {
                ser.EndInit();
            }

            vcChart.Series.Add(ser);

            if (isShowMaxPowerLine && groupTpResult.PowerMax.HasValue)
            {
                vcChart.Series.Add(maxPowerSerie);
            }
        }

        static void AddPowerMaxToChart(Chart vcChart, TGroupTPResult groupTpResult, int c, DateTime dtStart, DateTime dtEnd)
        {
            if (!groupTpResult.PowerMax.HasValue) return;

            var titleName = "Pmax - ";
            if (groupTpResult.HierarchyObject != null)
            {
                titleName += groupTpResult.HierarchyObject.ToString();
            }
            
            var ser = new DataSeries
            {
                Color = GetColorByIndexSeries(c),//Здесь присваиваем цвет
                LabelEnabled = false,
                RenderAs = RenderAs.Line,
                XValueType = ChartValueTypes.DateTime,
                Name = titleName,
                XValueFormatString = "dd.MM.yyyy HH:mm",
                //MarkerType = MarkerTypes.Circle,
                MarkerEnabled = false,
                LineThickness = 1,
                //Effect = effect,
                //DataSource = source,
                //ToolTip = titleName,
            };

            ser.BeginInit();
            try
            {
                ser.DataPoints.Add(new DataPoint
                {
                    XValue = dtStart,
                    YValue = groupTpResult.PowerMax.Value,
                    ToolTip = titleName + "\n" + dtStart.ToString("dd.MM.yyyy HH:mm") + "  " + groupTpResult.PowerMax.Value.ToString("#,##0.######")
                    //AxisXLabel = Mar
                });

                ser.DataPoints.Add(new DataPoint
                {
                    XValue = dtEnd,
                    YValue = groupTpResult.PowerMax.Value,
                    ToolTip = titleName + "\n" + dtEnd.ToString("dd.MM.yyyy HH:mm") + "  " + groupTpResult.PowerMax.Value.ToString("#,##0.######")
                    //AxisXLabel = Mar
                });
            }
            finally
            {
                ser.EndInit();
            }

            vcChart.Series.Add(ser);
        }
    }
}
