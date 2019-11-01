using Google.Cloud.Vision.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleVisionResultsLibrary
{
    public class GoogleVisionResults
    {
        public void googleVisionResults(string srcFilePath, string destDirPath = "sourceDirectory")
        {
            ImageAnnotatorClient client;
            try
            {
                client = ImageAnnotatorClient.Create();
            }
            catch
            {
                throw new InvalidOperationException("Google Credentials are not found or are incorrect"); //Correction: Set Environment Variable: GOOGLE_APPLICATION_CREDENTIALS
            }
            string srcDirectoryName = Path.GetDirectoryName(srcFilePath);
            if (!Directory.Exists(srcDirectoryName))
            {
                throw new DirectoryNotFoundException("The Source directory does not exist"); //Correction: Make sure source directory is valid.
            }

            if (!File.Exists(srcFilePath)) //Covers the case where directory is not accessible either.
            {
                throw new FileNotFoundException("Source File/Directory does not exist or you do not have access to it"); //Correction: Make sure file/directory is there and set permissions.
            }

            //if (new FileInfo(srcFilePath).Length == 0)
            //{
            //    return "Source file is empty";
            //}

            try
            {
                //System.Drawing.Image newImage = System.Drawing.Image.FromFile(srcFilePath);
                System.Drawing.Image newImage;
                using (var bmpTemp = new Bitmap(srcFilePath))
                {
                    newImage = new Bitmap(bmpTemp);
                }
            }
            catch
            {
                throw new ImageEmptyOrCorruptException("Source File is empty or corrupt. Could not be opened as an image."); //Correction: Replace image file.
            }
            string srcFileName = Path.GetFileName(srcFilePath);
            string srcFileNameNoExt = Path.GetFileNameWithoutExtension(srcFilePath);
            if (destDirPath.Equals("sourceDirectory"))
            {
                destDirPath = srcDirectoryName;
            }
            try
            {
                Directory.CreateDirectory(destDirPath); //Creates Directory if it does not exist. Does nothing otherwise.
            }
            catch
            {
                throw new UnauthorizedAccessException("You do not have permission to create/access this Destination directory"); //Correction: Set parent directory permissions.
            }
            try
            {
                using (StreamWriter sw = new StreamWriter(destDirPath + "\\temp.txt"))
                {

                }
                File.Delete(destDirPath + "\\temp.txt");
            }
            catch
            {
                throw new System.UnauthorizedAccessException("You do not have write access to this Destination directory or it is invalid"); //Correction: Make sure directory has access
            }
            DirectoryInfo di = new DirectoryInfo(destDirPath);
            if (di.Exists)
            {
                try
                {
                    var acl = di.GetAccessControl();
                }

                catch (UnauthorizedAccessException uae)
                {
                    throw uae;
                }
            }
            // Get the responses from Google API. 1)Serialize to JSON write to file. 2) Extract text only section and write it to file.
            try
            {
                Google.Cloud.Vision.V1.Image image = Google.Cloud.Vision.V1.Image.FromFile(srcFilePath);
                var responseText = client.DetectText(image);
                var responseDocument = client.DetectDocumentText(image);
                if(responseText.Count == 0 && responseDocument == null)
                {
                    throw new EmptyResultsException("Google Vision could not find any text from DetectText and DetectDocumentText methods.");
                }
                if (responseText.Count == 0)
                {
                    throw new EmptyDetectTextException("Google Vision could not find any text from DetectText method");
                }
                if (responseDocument == null)
                {
                    throw new EmptyDetectDocumentTextException("Google Vision could not find any text from DetectDocumentText method");
                }
                string responseTextJson = JsonConvert.SerializeObject(responseText);
                string responseDocumentJson = JsonConvert.SerializeObject(responseDocument);
                string responseTextOnlyText = "";
                foreach (var annotation in responseText)
                {
                    responseTextOnlyText = annotation.Description;
                    break;
                }
                string responseDocumentOnlyText = responseDocument.Text;
                // Write data to the four files
                using (StreamWriter sw = new StreamWriter(destDirPath + "\\" + srcFileNameNoExt + ".GTR"))
                {
                    sw.Write(responseTextJson);
                }
                using (StreamWriter sw = new StreamWriter(destDirPath + "\\" + srcFileNameNoExt + ".GTT"))
                {
                    sw.Write(responseTextOnlyText);
                }
                using (StreamWriter sw = new StreamWriter(destDirPath + "\\" + srcFileNameNoExt + ".GDR"))
                {
                    sw.Write(responseDocumentJson);
                }
                using (StreamWriter sw = new StreamWriter(destDirPath + "\\" + srcFileNameNoExt + ".GDT"))
                {
                    sw.Write(responseDocumentOnlyText);
                }
            }
            catch (AnnotateImageException e)
            {
                throw e;
            }

        }

        public void googleVisionResults(FileStream source, string destDirPath, string destFileName)
        {
            ImageAnnotatorClient client;
            try
            {
                client = ImageAnnotatorClient.Create();
            }
            catch
            {
                throw new InvalidOperationException("Google Credentials are not found or are incorrect"); //Correction: Set Environment Variable: GOOGLE_APPLICATION_CREDENTIALS
            }
            try
            {
                System.Drawing.Image newImage = System.Drawing.Image.FromStream(source);
                source.Seek(0, SeekOrigin.Begin);
            }
            catch
            {
                throw new ImageEmptyOrCorruptException("Source File is empty or corrupt. Could not be opened as an image."); //Correction: Replace image file.
            }

            try
            {
                Directory.CreateDirectory(destDirPath); //Creates Directory if it does not exist. Does nothing otherwise.
            }
            catch
            {
                throw new UnauthorizedAccessException("You do not have permission to access/create this Destination directory"); //Correction: Set parent directory permissions.
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(destDirPath + "\\temp.txt"))
                {

                }
                File.Delete(destDirPath + "\\temp.txt");
            }
            catch
            {
                throw new System.UnauthorizedAccessException("You do not have write access to this Destination directory or it is invalid"); //Correction: Make sure directory has access
            }
            DirectoryInfo di = new DirectoryInfo(destDirPath);
            if (di.Exists)
            {
                try
                {
                    var acl = di.GetAccessControl();
                }

                catch (UnauthorizedAccessException uae)
                {
                    throw uae;
                }
            }

            try
            {
                Google.Cloud.Vision.V1.Image image = Google.Cloud.Vision.V1.Image.FromStream(source);
                var responseText = client.DetectText(image);
                var responseDocument = client.DetectDocumentText(image);
                string responseTextJson = JsonConvert.SerializeObject(responseText);
                string responseDocumentJson = JsonConvert.SerializeObject(responseDocument);
                string responseTextOnlyText = "";
                foreach (var annotation in responseText)
                {
                    responseTextOnlyText = annotation.Description;
                    break;
                }
                string responseDocumentOnlyText = responseDocument.Text;

                //byte[] responseTextJsonBytes = new UTF8Encoding(true).GetBytes(responseTextJson);
                //destination.Write(responseTextJsonBytes, 0, responseTextJsonBytes.Length);
                //byte[] responseTextOnlyTextBytes = new UTF8Encoding(true).GetBytes(responseTextOnlyText);
                //destination.Write(responseTextOnlyTextBytes, responseTextJsonBytes.Length, responseTextOnlyTextBytes.Length);

                // Write data to the four files
                using (StreamWriter sw = new StreamWriter(destDirPath + "\\" + destFileName + ".GTR"))
                {
                    sw.Write(responseTextJson);
                }
                using (StreamWriter sw = new StreamWriter(destDirPath + "\\" + destFileName + ".GTT"))
                {
                    sw.Write(responseTextOnlyText);
                }
                using (StreamWriter sw = new StreamWriter(destDirPath + "\\" + destFileName + ".GDR"))
                {
                    sw.Write(responseDocumentJson);
                }
                using (StreamWriter sw = new StreamWriter(destDirPath + "\\" + destFileName + ".GDT"))
                {
                    sw.Write(responseDocumentOnlyText);
                }
            }
            catch (AnnotateImageException e)
            {
                throw e;
            }

        }
    }

    public class ImageEmptyOrCorruptException : Exception
    {
        public ImageEmptyOrCorruptException(string message) : base(message)
        {

        }
    }
    public class EmptyResultsException : Exception
    {
        public EmptyResultsException(string message) : base(message)
        {

        }
    }
    public class EmptyDetectTextException : Exception
    {
        public EmptyDetectTextException(string message) : base(message)
        {

        }
    }
    public class EmptyDetectDocumentTextException : Exception
    {
        public EmptyDetectDocumentTextException(string message) : base(message)
        {

        }
    }
}
