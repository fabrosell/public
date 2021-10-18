using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PostClient
{
    class Program
    {
        const string uri = "http://localhost:7071/api/upload";

        static async Task Main(string[] args)
        {
            try
            {
                Console.Write("Press enter to POST to api");
                Console.ReadLine();

                NameValueCollection nvc = new NameValueCollection();
                //For adding other json data
                nvc.Add("id", "TTR");
                nvc.Add("btn-submit-photo", "Upload");

                // Upload files at once (suggested)
                var files = new List<String>()
                {
                    "Catastrophe-106.jpeg",
                    "Catastrophe-106 - Copy.jpeg",
                };

                // File per file upload
                //foreach (var file in files)
                //{
                //    //HttpUploadSingleFile(uri, file, nvc);
                //}
                
                HttpUploadMultipleFiles(uri, files);

                Console.Write("Press enter to exit");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.ToString()}");
            }
        }


        static void HttpUploadMultipleFiles(string url, List<string> files, string paramName = "File", NameValueCollection nvc = null)
        {
            /*
             
             DASHES STUFF

            Header and keys (data or file keys) are separated in request by a user-defined boundary.

            In web, boundaries also start with some dashes. This is just coincidential but keep on mind. 

            E.g.: boundary = "boundary_[someuniqueid]";

            Default request separator includes extra dashes (-) in boundary.
           
            For all boundaries, 2 extra dashes must be added at beggining
        
            E.g: boundary will be "--boundary_[someuniqueid]"            

            For last boundary, 2 extra dashes also must be added at the end of the string

            E.g: last boundary will be "--boundary_[someuniqueid]--"
            
            --> Failing to instate boundaries in proper place will make requests fail. 
             
             */

            Console.WriteLine($"Uploading {string.Join(',', files.ToArray())} to {url}");


            string boundary = $"---------------------------{DateTime.Now.Ticks.ToString("x")}";
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes($"\r\n--{boundary}\r\n");

            // Header
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = $"multipart/form-data; boundary={boundary}";
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            // Key-pair values for non files
            if (nvc != null)
                foreach (string key in nvc.Keys)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes($"Content-Disposition: form-data; name=\"{key}\"\r\n\r\n{nvc[key]}");
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }
            
            foreach (var file in files)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);

                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes($"Content-Disposition: form-data; name=\"{paramName}\"; filename=\"{file}\"\r\n\r\n");
                rs.Write(headerbytes, 0, headerbytes.Length);

                FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    rs.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();
            }

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes($"\r\n--{boundary}--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                Console.WriteLine($"File uploaded, server response is: {reader2.ReadToEnd()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file : {ex.ToString()}");
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }

        // Origian code but simplified by using inline formatting options (shorter code, easier to read)
        static void HttpUploadSingleFile(string url, string file, string paramName = "File", NameValueCollection nvc = null)
        {
            Console.WriteLine($"Uploading {file} to {url}");
            string boundary = $"---------------------------{DateTime.Now.Ticks.ToString("x")}";
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes($"\r\n--{boundary}\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = $"multipart/form-data; boundary={boundary}";
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            if (nvc != null)
                foreach (string key in nvc.Keys)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes($"Content-Disposition: form-data; name=\"{paramName}\"\r\n\r\n");
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }

            rs.Write(boundarybytes, 0, boundarybytes.Length);

            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes($"Content-Disposition: form-data; name=\"{paramName}\"; filename=\"{file}\"\r\n\r\n");
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes($"\r\n--{boundary}--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                Console.WriteLine($"File uploaded, server response is: {reader2.ReadToEnd()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file : {ex.ToString()}");
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }

        // Original code from https://stackoverflow.com/questions/566462/upload-files-with-httpwebrequest-multipart-form-data/2996904#2996904
        static void HttpUploadSingleFile_Original(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            Console.WriteLine(string.Format("Uploading {0} to {1}", file, url));
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                Console.WriteLine(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file : {ex.ToString()}");
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }
    }
}
