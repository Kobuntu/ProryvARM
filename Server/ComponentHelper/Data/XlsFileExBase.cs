using FlexCel.Core;
using FlexCel.XlsAdapter;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Section;
using Proryv.AskueARM2.Server.WCF;
using Proryv.Servers.Calculation.DBAccess.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Server.VisualCompHelpers.Data
{
    public class XlsFileExBase : XlsFile
    {
        public int NoDecimalBoldFormat;
        public int NoDecimalFormat;

        public int ProfileBoldFormat;
        public int ProfileFormat;

        public int IntegralBoldFormat;
        public int IntegralFormat;

        public int CenterFormatThin;
        public int LeftFormatThin;
        public int StartCol;
        public int StartRow;
        public int CenterRow;

        public int LeftFormat;
        public int CenterBoldRow;
        public int CenterBoldThinFormat;
        public int BoldLeftFormatThin;

        public int Row;
        public int Col;

        public Dictionary<string, List<FormulaRowsRange>> FormulasSum;

        public bool Need0;
        public bool SetPercisionAsDisplayed;
        public int DoublePrecisionProfile;
        public int DoublePrecisionIntegral;

        internal readonly Color _noDrumsColor = Color.DarkOrange;
        internal readonly Color _offsetFromMoscowEnbledForDrumsColor =  Color.Yellow;
        internal TCommentProperties _commentProps = new TCommentProperties
        {
            Anchor = new TClientAnchor(TFlxAnchorType.DontMoveAndDontResize, 1, 30, 7, 502, 5, 240, 8, 900),
            ShapeFill = new TShapeFill(true, new TSolidFill(Colors.White))
        };

        #region Конструкторы

        public XlsFileExBase() : base()
        {
        }
        public XlsFileExBase(bool aAllowOverwritingFiles) : base(aAllowOverwritingFiles)
        {
        }
        public XlsFileExBase(string aFileName):base (aFileName)
        {
        }
        public XlsFileExBase(string aFileName, bool aAllowOverwritingFiles) :base(aFileName, aAllowOverwritingFiles)
        {
        }
        public XlsFileExBase(int aSheetCount, bool aAllowOverwritingFiles): base(aSheetCount, aAllowOverwritingFiles)
        {
        }
        public XlsFileExBase(Stream aStream, bool aAllowOverwritingFiles) : base(aStream, aAllowOverwritingFiles)
        {
        }
        public XlsFileExBase(int aSheetCount, TExcelFileFormat aFileFormat, bool aAllowOverwritingFiles): base (aSheetCount, aFileFormat, aAllowOverwritingFiles)
        {
        }

        #endregion
        
        public void WriteAnnotate(bool _showAnnotateOvRepl, Color ovReplacedColor)
        {
            if (_showAnnotateOvRepl)
            {
                Row++;
                Row++;
                SetCellValue(Row, StartCol + 3, "Примечание : ");
            }

            if (_showAnnotateOvRepl)
            {
                Row++;
                SetCellValue(Row, StartCol + 4, 0);
                SetCellFontColor(Row, StartCol + 4, ovReplacedColor);
                SetCellValue(Row, StartCol + 5, " - обходной выключатель");
                MergeCells(Row, StartCol + 5, Row, ColCount);
            }

            //var f = GetDefaultFormat;
            //f.WrapText = true;
            //f.HAlignment = THFlxAlignment.center;
            //f.Borders.Bottom.Color = Color.Black;
            //f.Borders.Left.Color = Color.Black;
            //f.Borders.Right.Color = Color.Black;
            //f.Borders.Top.Color = Color.Black;
            //f.Borders.Bottom.Style = TFlxBorderStyle.Thin;
            //f.Borders.Left.Style = TFlxBorderStyle.Thin;
            //f.Borders.Right.Style = TFlxBorderStyle.Thin;
            //f.Borders.Top.Style = TFlxBorderStyle.Thin;
            //var ff = AddFormat(f);
            //SetCellFormat(StartRow, StartCol, StartRow, ColCount, ff);
        }

        public int SetCellFontColor(int row, int col, Color fc)
        {
            var ff = GetCellFormat(row, col);
            var fx = GetFormat(ff);
            fx.Font.Color = fc;
            ff = AddFormat(fx);
            SetCellFormat(row, col, ff);
            return ff;
        }

        public void SetCellFloatValue(int row, int col, double value, bool isIntegral = false, bool isBold = false, bool? need0 = null)
        {
            if (col >= 16384) return;

            int format;

            if (!need0.HasValue)
            {
                need0 = Need0;
            }

            if (!need0.Value && (value == 0 || value % 1 == 0))
            {
                format = isBold ? NoDecimalBoldFormat : NoDecimalFormat;
                if (SetPercisionAsDisplayed)
                {
                    value = Math.Round(value, 0, MidpointRounding.AwayFromZero);
                }
            }
            else if (!isIntegral)
            {
                format = isBold ? ProfileBoldFormat : ProfileFormat;
                if (SetPercisionAsDisplayed)
                {
                    value = Math.Round(value, DoublePrecisionProfile, MidpointRounding.AwayFromZero);
                }
            }
            else
            {
                format = isBold ? IntegralBoldFormat : IntegralFormat;
                if (SetPercisionAsDisplayed)
                {
                    value = Math.Round(value, DoublePrecisionIntegral, MidpointRounding.AwayFromZero);
                }
            }

            SetCellValue(row, col, value, format);
        }

        internal int SetCellBkColor(int row, int col, Color bk)
        {
            int FF = GetCellFormat(row, col);
            TFlxFormat FX = GetFormat(FF);
            FX.FillPattern.Pattern = TFlxPatternStyle.Solid;
            //FX.FillPattern.FgColorIndex = Xls.NearestColorIndex(bk);
            FX.FillPattern.FgColor = bk;

            FF = AddFormat(FX);
            SetCellFormat(row, col, FF);
            return FF;
        }

        internal int SetCellWrapText(int row, int col, bool Wrap)
        {
            int ff = GetCellFormat(row, col);
            TFlxFormat FX = GetFormat(ff);
            FX.WrapText = Wrap;
            ff = AddFormat(FX);
            SetCellFormat(row, col, ff);
            return ff;
        }

        
    }
}
