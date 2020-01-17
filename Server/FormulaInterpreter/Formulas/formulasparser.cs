using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;



namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{

    public class FormulasParser
    {
        public FormulasParser()
        {
        }

        //public FORMULAS_EXPRESSIONS Parse(string FilePath)
        //{
        //    PrvFileStream file_str = null;
        //    try
        //    {
        //        // открываем файл и копируем данные в память
        //        file_str = new PrvFileStream(FilePath, FileModeEx.OpenExisting, FileAccessEx.Read, FileShareEx.Read);
        //        byte[] data = new byte[(int)file_str.Length];
        //        file_str.Read(data, 0, data.Length);

        //        return Parse(data);
        //    }
        //    finally
        //    {
        //        if (file_str != null)
        //            file_str.Close();
        //    }

        //}

        public FORMULAS_EXPRESSIONS Parse(byte[] data)
        {
            XmlReaderSettings Xml_set;
            Xml_set = new XmlReaderSettings();
            Xml_set.ConformanceLevel = ConformanceLevel.Document;
            Xml_set.IgnoreWhitespace = true;
            Xml_set.IgnoreComments = true;

            XmlReader xml_reader = null;
            MemoryStream memory_str = null;

            try
            {
                memory_str = new MemoryStream();
                memory_str.Write(data, 0, data.Length);
                memory_str.Position = 0;

                xml_reader = XmlReader.Create(memory_str, Xml_set);

                //List<FORMULA> f_list = f_list = new List<FORMULA>(); // дабы не было сообщений от недовольных
                FORMULAS_EXPRESSIONS result;
                result.FORMULAS_LIST = new SortedList<string, FORMULA>();
                FORMULA fmla = null;
                while (xml_reader.Read())
                {
                    if (xml_reader.NodeType == XmlNodeType.Element && xml_reader.Name == "FORMULAS_EXPRESSIONS")
                        continue;
                    else if (xml_reader.NodeType == XmlNodeType.Element && xml_reader.Name == "FORMULA")
                    {
                        fmla = ReadFormulaData(xml_reader);
                        if (!result.FORMULAS_LIST.ContainsKey(fmla.F_ID)) result.FORMULAS_LIST.Add(fmla.F_ID, fmla);
                        //f_list.Add(ReadFormulaData(xml_reader));
                    }
                    else if (xml_reader.NodeType == XmlNodeType.EndElement && xml_reader.Name == "FORMULAS_EXPRESSIONS")
                        break;
                }


                //result.FORMULAS_LIST = f_list;

                return result;
            }
            finally
            {
                if (memory_str != null)
                    memory_str.Close();

                if (xml_reader != null)
                    xml_reader.Close();
            }
        }

        private int SortOperatorsByOrder(F_OPERATOR oper1, F_OPERATOR oper2)
        {
            if (oper1.OPER_ORDER > oper2.OPER_ORDER)
                return 1;
            if (oper1.OPER_ORDER == oper2.OPER_ORDER)
                return 0;
            return -1;
        }

        //public void Collect(FORMULAS_EXPRESSIONS F_excpressions, string FilePath)
        //{
        //    byte[] data = Collect(F_excpressions);
        //    PrvFileStream file_str = null;
        //    try
        //    {
        //        file_str = new PrvFileStream(FilePath, FileModeEx.CreateAlways, FileAccessEx.Write, FileShareEx.None);
        //        file_str.Write(data, 0, (int)data.Length);
        //    }
        //    finally
        //    {
        //        if (file_str != null)
        //            file_str.Close();
        //    }
        //}

        public byte[] Collect(FORMULAS_EXPRESSIONS F_excpressions)
        {
            MemoryStream memory_str = null;

            try
            {
                memory_str = new MemoryStream();
                memory_str.Position = 0;

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.ConformanceLevel = ConformanceLevel.Document;
                settings.Indent = true;
                XmlWriter xml_writer = XmlWriter.Create(memory_str, settings);

                xml_writer.WriteStartElement("FORMULAS_EXPRESSIONS");

                //List<FORMULA> f_list = F_excpressions.FORMULAS_LIST;

                //for (int i = 0; i < f_list.Count; i++)
                //    WriteFormulaData(f_list[i], xml_writer);

                foreach (var _key in F_excpressions.FORMULAS_LIST.Keys)
                    WriteFormulaData(F_excpressions.FORMULAS_LIST[_key], xml_writer);

                xml_writer.WriteEndElement();

                xml_writer.Close();
                memory_str.Position = 0;

                return memory_str.ToArray();
            }
            finally
            {
                if (memory_str != null)
                    memory_str.Close();
            }
        }

        private void WriteFormulaData(FORMULA f, XmlWriter xml_writer)
        {
            xml_writer.WriteStartElement("FORMULA");

            xml_writer.WriteAttributeString("F_CLASS", f.F_CLASS.ToString());
            xml_writer.WriteAttributeString("F_INFO", f.F_INFO);
            xml_writer.WriteAttributeString("F_TYPE", f.F_TYPE.ToString());
            xml_writer.WriteAttributeString("F_ID", f.F_ID);
            xml_writer.WriteAttributeString("F_RESULT_TYPE", f.F_RESULT_TYPE);
            xml_writer.WriteAttributeString("F_MODE", f.F_MODE.ToString());

            System.Globalization.CultureInfo provider = new System.Globalization.CultureInfo("en-US");
            provider.NumberFormat.NumberDecimalSeparator = ".";

            xml_writer.WriteAttributeString("HIGHT_LIMIT", f.HIGHT_LIMIT.ToString(provider));
            xml_writer.WriteAttributeString("LOWER_LIMIT", f.LOWER_LIMIT.ToString(provider));

            if (f.F_OPERATORS != null)
            {
                for (int i = 0; i < f.F_OPERATORS.Count; i++)
                    WriteFormulaOperator(xml_writer, f.F_OPERATORS[i]);
            }
            xml_writer.WriteEndElement();
        }

        private void WriteFormulaOperator(XmlWriter xml_writer, F_OPERATOR oper)
        {
            xml_writer.WriteStartElement("F_OPERATOR");

            xml_writer.WriteAttributeString("OPER_ORDER", oper.OPER_ORDER.ToString());
            if (oper.PRE_OPERANDS != null)
                xml_writer.WriteAttributeString("PRE_OPERANDS", oper.PRE_OPERANDS);
            xml_writer.WriteAttributeString("OPER_TYPE", ((int)oper.OPER_TYPE).ToString());
            xml_writer.WriteAttributeString("OPER_ID", oper.OPER_ID);
            if (oper.TI_CHANNEL != null)
                xml_writer.WriteAttributeString("TI_CHANNEL", ((int)oper.TI_CHANNEL.Value).ToString());
            if (oper.AFTER_OPERANDS != null)
                xml_writer.WriteAttributeString("AFTER_OPERANDS", oper.AFTER_OPERANDS);

            xml_writer.WriteEndElement();
        }

        private FORMULA ReadFormulaData(XmlReader xml_reader)
        {
            var fclass = Convert.ToInt32(xml_reader.GetAttribute("F_CLASS"));
            var f = new FORMULA(xml_reader.GetAttribute("F_ID"));
            f.F_CLASS = fclass;
            f.F_RESULT_TYPE = xml_reader.GetAttribute("F_RESULT_TYPE");
            f.F_TYPE = Convert.ToInt32(xml_reader.GetAttribute("F_TYPE"));
            f.F_INFO = xml_reader.GetAttribute("F_INFO");

            string temp_str = xml_reader.GetAttribute("F_MODE");
            if (!string.IsNullOrEmpty(temp_str))
                f.F_MODE = Convert.ToInt32(temp_str);

            System.Globalization.CultureInfo provider = new System.Globalization.CultureInfo("en-US");
            provider.NumberFormat.NumberDecimalSeparator = ".";


            temp_str = xml_reader.GetAttribute("HIGHT_LIMIT");
            if (temp_str != null)
                f.HIGHT_LIMIT = Convert.ToSingle(temp_str, provider);

            temp_str = xml_reader.GetAttribute("LOWER_LIMIT");
            if (temp_str != null)
                f.LOWER_LIMIT = Convert.ToSingle(temp_str, provider);


            if (xml_reader.IsEmptyElement == true)
                return f;

            while (xml_reader.Read())
            {
                if (xml_reader.NodeType == XmlNodeType.Element && xml_reader.Name == "F_OPERATOR")
                    f.F_OPERATORS.Add(ReadOperator(xml_reader, f));
                else if (xml_reader.NodeType == XmlNodeType.EndElement && xml_reader.Name == "FORMULA")
                    break;
            }
            f.F_OPERATORS.Sort(SortOperatorsByOrder);
            return f;
        }

        private F_OPERATOR ReadOperator(XmlReader xml_reader, FORMULA f)
        {
            var oper = new F_OPERATOR
            {
                AFTER_OPERANDS = xml_reader.GetAttribute("AFTER_OPERANDS"),
                OPER_ID = xml_reader.GetAttribute("OPER_ID"),
                OPER_ORDER = Convert.ToInt32(xml_reader.GetAttribute("OPER_ORDER")),
                OPER_TYPE = (F_OPERATOR.F_OPERAND_TYPE)Convert.ToInt32(xml_reader.GetAttribute("OPER_TYPE")),
                PRE_OPERANDS = xml_reader.GetAttribute("PRE_OPERANDS"),
            };

            if (oper.OPER_TYPE == F_OPERATOR.F_OPERAND_TYPE.TI_channel)
                oper.TI_CHANNEL = Convert.ToByte(xml_reader.GetAttribute("TI_CHANNEL"));
            else
                oper.TI_CHANNEL = null;

            oper.INNER_FORMULA = f;

            return oper;
        }

    }


    public struct FORMULAS_EXPRESSIONS
    {

        public SortedList<string, FORMULA> FORMULAS_LIST;


        // функции ....
    }



    public struct F_OPERATOR
    {
        /// <summary>
        /// Положение оператора в формуле
        /// </summary>
        public int OPER_ORDER;
        /// <summary>
        /// Операторы до узла
        /// </summary>
        public string PRE_OPERANDS;
        /// <summary>
        /// Тип операнда
        /// </summary>
        public F_OPERAND_TYPE OPER_TYPE;
        /// <summary>
        /// Либо идентификатор формулы или идентификатор точки измерения или значение константы
        /// </summary>
        public string OPER_ID;
        /// <summary>
        /// Тип канала точки измерения (может отсутствовать)
        /// </summary>
        public byte? TI_CHANNEL;

        /// <summary>
        /// Идентификатор закрытия
        /// </summary>
        public Guid? ClosedPeriod_ID;
        /// <summary>
        /// Операторы после узла
        /// </summary>
        public string AFTER_OPERANDS;

        /// <summary>
        /// Формула в которой учавствует оператор
        /// </summary>
        public FORMULA INNER_FORMULA;

        /// <summary>
        /// Типы операндов
        /// </summary>
        public enum F_OPERAND_TYPE
        {
            None = 0,
            Formula = 1,
            TI_channel = 2,
            Function = 3,
            TP_channel = 4,
            Section = 5,
            ContrTI_Chanel = 6,
            Integral_Channel = 7,
            Constanta = 8,
            FormulaConstant = 9,
            UANode = 10
        }

        /// <summary>
        /// Вспомогательная информация
        /// </summary>
        public object Tag;

        private string _name;
        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(_name)) return _name;

                if (TI_CHANNEL.HasValue)
                {
                    _name = OPER_ID + "_" + TI_CHANNEL.Value + "_" + OPER_TYPE;
                }
                else
                {
                    _name = OPER_ID;
                }

                return _name;//ti_id.ToUpper() + "_" + ChType + "_" + ((int) type) + "_" + type;
            }
        }
    } 
}
