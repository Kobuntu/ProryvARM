using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Proryv.AskueARM2.Both.VisualCompHelpers
{
    public class FileAdapter
    {
        public static bool SaveXLSFile(XLSExportSingleObjectResult result, Action<string> onError, Action<object, string> onSaveDialog)
        {
            string fileName = "Таблица";
            if (!string.IsNullOrEmpty(result.FileName))
            {
                if (result.FileName.Length > 150)
                {
                    fileName = result.FileName.Substring(0, 150).Replace(".xls", "");
                }
                else
                {
                    fileName = result.FileName.Replace(".xls", "");
                }
            }
            return SaveFile(result.XLSStream, fileName, "xls", onError, onSaveDialog);
        }


        public static bool SaveXLSXFile(XLSExportSingleObjectResult _Result, Action<string> onError, Action<object, string> onSaveDialog)
        {
            string fileName = "Таблица";
            if (!string.IsNullOrEmpty(_Result.FileName))
            {
                if (_Result.FileName.Length > 150)
                {
                    fileName = _Result.FileName.Substring(0, 150).Replace(".xlsx", "");
                }
                else
                {
                    fileName = _Result.FileName.Replace(".xlsx", "");
                }
            }
            return SaveFile(_Result.XLSStream, fileName, "xlsx", onError, onSaveDialog);
        }



        public static bool SaveFile(Stream stream, string fileName, string ext, Action<string> onError, Action<object, string> onSaveDialog) 
        {
            if (stream == null) return true;
            try
            {
                using (System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog())
                {

                    saveDialog.DefaultExt = ext.ToLower();
                    saveDialog.Filter = ext.ToUpper() + " Files (*." + ext.ToLower() + ")|*." + ext.ToLower();
                    saveDialog.FilterIndex = 1;
                    saveDialog.FileName = RemoveBadChar(fileName) + "." + saveDialog.DefaultExt;

                    saveDialog.OverwritePrompt = true;
                    saveDialog.AddExtension = true;
                    saveDialog.SupportMultiDottedExtensions = true;

                    if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        System.IO.FileStream fs = null;
                        try
                        {
                            if (VerifyFileIsInUse(saveDialog.FileName, out fileName))
                            {
                                //перезапываем
                                if (File.Exists(fileName))
                                    File.Delete(fileName);

                                fs = new System.IO.FileStream(fileName, System.IO.FileMode.OpenOrCreate);

                                byte[] buf = new byte[50000];
                                int bytestoread = 0;
                                do
                                {
                                    bytestoread = stream.Read(buf, 0, buf.Length);
                                    fs.Write(buf, 0, bytestoread);

                                } while (bytestoread > 0);
                            }
                            else
                            {
                                throw new Exception("Не удается перезаписать файл!\n Возможно он уже открыт. Закройте его.");
                            }
                        }
                        catch (Exception ex)
                        {
                            if (onError != null) onError(ex.Message);
                            //Manager.UI.ShowMessage(ex.Message);
                            return false;
                        }
                        finally
                        {
                            if (fs != null)
                            {
                                fs.Close();
                            }
                        }

                        string dialogMesage;

                        if (fileName != saveDialog.FileName)
                        {
                            dialogMesage = "Файл не доступен для записи.\nЗаписан как\n" + Path.GetFileName(fileName) + "\nОткрыть?";
                        }
                        else
                        {
                            dialogMesage = "Открыть файл " + Path.GetFileName(fileName) + " ?";
                        }

                        if (onSaveDialog != null) onSaveDialog(dialogMesage, fileName);

                        return true;

                    }

                }

            }
            finally
            {
                stream.Dispose();
            }
            return true;
        }


        public static bool SaveFile(Stream stream, string fileName, ref string ext, bool openFile, bool openSaveDialog,
            ref string initialDirectory, Action<string> onError, Action<object, string> onSaveDialog)
        {

            if (stream == null) return true;
            try
            {
                string fullFileName = "";
                stream.Position = 0;

                if (openSaveDialog)
                {
                    using (System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog())
                    {
                        saveDialog.DefaultExt = ext.ToLower();
                        saveDialog.Filter = ext.ToUpper() + " Files (*." + ext.ToLower() + ")|*." + ext.ToLower();
                        saveDialog.FilterIndex = 1;
                        saveDialog.FileName = RemoveBadChar(fileName) + "." + saveDialog.DefaultExt;
                        if (initialDirectory != "")
                            saveDialog.InitialDirectory = initialDirectory;

                        saveDialog.OverwritePrompt = true;
                        saveDialog.AddExtension = true;
                        saveDialog.SupportMultiDottedExtensions = true;

                        if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            initialDirectory = Path.GetDirectoryName(saveDialog.FileName);
                            fullFileName = saveDialog.FileName;
                            ext = saveDialog.DefaultExt;
                        }
                        else
                            return false;
                    }

                }
                else
                {
                    fullFileName = string.Format("{0}\\{1}.{2}", initialDirectory, fileName, ext.ToLower());
                }



                System.IO.FileStream fs = null;
                try
                {
                    if (VerifyFileIsInUse(fullFileName, out fileName))
                    {
                        //перезапываем
                        if (File.Exists(fileName))
                            File.Delete(fileName);

                        fs = new System.IO.FileStream(fileName, System.IO.FileMode.OpenOrCreate);

                        byte[] buf = new byte[50000];
                        int bytestoread = 0;
                        do
                        {
                            bytestoread = stream.Read(buf, 0, buf.Length);
                            fs.Write(buf, 0, bytestoread);

                        } while (bytestoread > 0);
                    }
                    else
                    {
                        throw new Exception("Не удается перезаписать файл!\n Возможно он уже открыт. Закройте его.");
                    }
                }
                catch (Exception ex)
                {
                    if (onError!=null) onError(ex.Message);
                    return false;
                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Close();
                    }
                }

                string dialogMesage = "Открыть файл " + Path.GetFileName(fileName) + " ?";


                if (openFile && onSaveDialog != null)
                {
                    onSaveDialog(dialogMesage, fileName);
                }

                return true;


            }
            finally
            {
                stream.Dispose();
            }
        }

        public static bool VerifyFileIsInUse(string fileName, out string newFileName)
        {
            newFileName = fileName;
            int number = 1;
            bool result = false;
            FileStream fs = null;
            do
            {
                try
                {
                    fs = new FileStream(newFileName, System.IO.FileMode.OpenOrCreate);
                    result = true;
                }
                catch (Exception)
                {
                    result = false;
                    newFileName = Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "(" + number++ + ")" + Path.GetExtension(fileName);
                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Close();
                    }
                }

            } while (!result && number < 100);

            return result;
        }

        /// <summary>
        /// Убираем недопустимые символы в имени файла
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="charLimit">Ограничение символов</param>
        /// <param name="replacedChar">Символ, которым замещаем недопустимые</param>
        /// <returns></returns>
        public static string RemoveBadChar(string fileName, int charLimit = 200, string replacedChar = "_")
        {
            //string result = string.Concat(FileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            string result = FileNameCleaner.Replace(fileName, replacedChar);
            if (result.Length <= charLimit) return result;

            else return result.Substring(1, charLimit); //Ограничиваем размер файла в 200 символов
        }

        private static readonly string InvalidFileNameChars = string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars())));
        private static readonly Regex FileNameCleaner = new Regex(InvalidFileNameChars, RegexOptions.Compiled);
    }

    public struct XLSExportSingleObjectResult
    {

        public string FileName;
        public Stream XLSStream;
    }
}
